using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;
using Debug=Microsoft.MetadataReader.Internal.Debug;

namespace System.Reflection.Adds
{
    /// <summary>
    /// Map a file into memory and expose access. 
    /// This is internal and intended to support Metadata files.
    /// </summary>
    internal class FileMapping : IDisposable
    {
        // Name of the file being opened. This is useful for debugging purposes.
        private readonly string m_fileName;
        private readonly string m_filePath;
        private readonly long m_fileLength;
        private readonly IntPtr m_baseAddress;

        // These store the OS resources for the file mapping.
        // They're safe handles and will release resources, so we don't need the finalizer.
        private readonly SafeFileHandle m_fileHandle;
        private readonly NativeMethods.SafeWin32Handle m_fileMapping;
        private readonly NativeMethods.SafeMapViewHandle m_View;

        /// <summary>
        /// Create a file mapping around the given filename with already available 
        /// base address and file size. 
        /// </summary>
        /// <param name="fileName">Path to the mapped file; can be relative.</param>
        public FileMapping(IntPtr baseAddress, long fileLength, string fileName)
        {
            Debug.Assert(baseAddress != IntPtr.Zero, "Base address can't be zero.");
            Debug.Assert(!string.IsNullOrEmpty(fileName), "File name can't be empty.");

            m_fileName = fileName;
            m_filePath = System.IO.Path.GetFullPath(m_fileName);
            m_fileLength = fileLength;
            m_baseAddress = baseAddress;
        }

        /// <summary>
        /// Create a file mapping around the given filename
        /// </summary>
        /// <param name="fileName">filename to open; can be relative</param>
        public FileMapping(string fileName)
        {
            m_fileName = fileName;
            Debug.Assert(m_fileName != null);

            // Directly opens file for reading using Win32 API. We don't use File.OpenRead since it introduces
            // overhead of FileStream object, which we don't need. We only need a file handle to give to file
            // mapping APIs. 
            m_fileHandle = NativeMethods.SafeOpenFile(fileName);

            // Note: we never update file size if it changes. We should fix this if we need to support more 
            // dynamic scenarios. In Dev10 this is ok since LMR is used to analyze "static" binaries like FX assemblies. 
            m_fileLength = NativeMethods.FileSize(m_fileHandle);
            m_filePath = System.IO.Path.GetFullPath(m_fileName);

            // The dump file may be many megabytes large, so we don't want to
            // read it all at once. Instead, doing a mapping.
            m_fileMapping = NativeMethods.CreateFileMapping(m_fileHandle, IntPtr.Zero, NativeMethods.PageProtection.Readonly, 0, 0, null);
            if (m_fileMapping.IsInvalid)
            {
                int error = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(error);
            }

            // TODO: ensure that the OS is sharing the private pages of the file mappings. 
            const uint FILE_MAP_READ = 4;
            m_View = NativeMethods.MapViewOfFile(m_fileMapping, FILE_MAP_READ, 0, 0, IntPtr.Zero);
            if (m_View.IsInvalid)
            {
                int error = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(error);
            }

            m_baseAddress = m_View.BaseAddress;
        }


        /// <summary>
        /// Full path to the file.
        /// </summary>
        public string Path {
            get { return m_filePath; } 
        }

        /// <summary>
        /// Length of file in bytes.
        /// </summary>
        public long Length
        {
            get { return m_fileLength; }
        }

        /// <summary>
        /// Base address of file. This can be used in direct memory operations to read the file contents.
        /// </summary>
        public IntPtr BaseAddress
        {
            get { return m_baseAddress; }
        }

        public override string ToString()
        {
            if (m_baseAddress != IntPtr.Zero)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0} Addr=0x{1}, Length=0x{2}",
                    m_fileName, m_baseAddress.ToString("x"), m_fileLength.ToString("x", CultureInfo.InvariantCulture));
            }

            if ((m_View != null) && m_View.IsInvalid)
            {
                return m_fileName + " (closed)";
            }

            return m_fileName;
        }

        public void Dispose()
        {
            if (m_View != null)
            {
                m_View.Close();
            }

            if (m_fileMapping != null)
            {
                m_fileMapping.Close();
            }

            if (m_fileHandle != null)
            {
                m_fileHandle.Close();
            }

            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Import of native definitions supporting FileMapping class.
    /// </summary>
    internal static class NativeMethods
    {
        private const string Kernel32LibraryName = "kernel32.dll";
        // From WinBase.h
        private const int FILE_TYPE_DISK = 0x0001;
        private const int GENERIC_READ = unchecked((int)0x80000000);

        [DllImport(Kernel32LibraryName, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        private static extern SafeFileHandle CreateFile(String lpFileName,
                    int dwDesiredAccess, FileShare dwShareMode,
                    IntPtr securityAttrs, FileMode dwCreationDisposition,
                    int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport(Kernel32LibraryName)]
        private static extern int GetFileType(SafeFileHandle handle);

        /// <summary>
        /// Wrapper method to directly get a file handle from Win32 API. 
        /// </summary>
        internal static SafeFileHandle SafeOpenFile(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            // Prevent access to your disk drives as raw block devices.
            if ((fileName.Length == 0) || fileName.StartsWith("\\\\.\\", StringComparison.Ordinal))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture, Resources.InvalidFileName, fileName));
            }

            // File.OpenRead implementation normalizes file path (with Path.NormalizePath) for security reasons. 
            // Since using LMR requires full trust we don't need to do that. 

            SafeFileHandle handle = CreateFile(fileName, GENERIC_READ, FileShare.Read,
                                IntPtr.Zero, FileMode.Open,
                                (int)FileOptions.None, IntPtr.Zero);

            if (handle.IsInvalid)
            {
                int hr = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hr);
            }
            else
            {
                int fileType = GetFileType(handle);
                if (fileType != FILE_TYPE_DISK)
                {
                    handle.Dispose();
                    throw new ArgumentException(Resources.UnsupportedImageType);
                }
            }

            return handle;
        }

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        private static extern int GetFileSize(SafeFileHandle hFile, out int highSize);

        /// <summary>
        /// Wrapper to get file size as long. 
        /// </summary>
        internal static long FileSize(SafeFileHandle handle)
        {
            int hi = 0, lo = 0;
            lo = GetFileSize(handle, out hi);

            if (lo == -1)
            {  
                int error = Marshal.GetLastWin32Error();
                Marshal.ThrowExceptionForHR(error);
            }

            long len = (((long)hi) << 32) | ((uint)lo);
            return len;
        }

        [DllImport(Kernel32LibraryName, SetLastError = true, PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr handle);

        public sealed class SafeWin32Handle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeWin32Handle() : base(true) { }

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        [Flags]
        public enum PageProtection : uint
        {
            NoAccess = 0x01,
            Readonly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            Guard = 0x100,
            NoCache = 0x200,
            WriteCombine = 0x400,
        }

        // Call CloseHandle to clean up.
        [DllImport(Kernel32LibraryName, SetLastError = true, BestFitMapping = false)]
        public static extern SafeWin32Handle CreateFileMapping(SafeFileHandle hFile,
           IntPtr lpFileMappingAttributes, PageProtection flProtect, uint dwMaximumSizeHigh,
           uint dwMaximumSizeLow, string lpName);

        // SafeHandle to call UnmapViewOfFile
        public sealed class SafeMapViewHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeMapViewHandle() : base(true) { }

            protected override bool ReleaseHandle()
            {
                return UnmapViewOfFile(handle);
            }

            // This is technically equivalent to DangerousGetHandle, but it's safer for file
            // mappings. In file mappings, the "handle" is actually a base address that needs
            // to be used in computations and RVAs.
            // So provide a safer accessor method.
            public IntPtr BaseAddress
            {
                get
                {
                    return handle;
                }
            }
        }

        // Call BOOL UnmapViewOfFile(void*) to clean up. 
        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern SafeMapViewHandle MapViewOfFile(SafeWin32Handle hFileMappingObject, uint
           dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow,
           IntPtr dwNumberOfBytesToMap);

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnmapViewOfFile(IntPtr baseAddress);
    }

} // end namespace
