using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;

namespace System.Reflection.Adds
{
    internal enum ImageType : ushort
    {
        Pe32bit = 0x010B,
        Pe64bit = 0x020B,
    }

    /// <summary>
    /// Internal support for cracking PE files.  
    /// The metadata import interfaces just give RVAs to key data (like method bodies), and so fully
    /// cracking the metadata requires resolving those RVAs.
    /// </summary>
    internal class ImageHelper
    {
        readonly private IntPtr m_baseAddress;
        readonly private long m_lengthBytes;

        readonly private uint m_idx;
        readonly private uint m_idxSectionStart;
        readonly private uint m_numSections;
        readonly private uint m_clrHeaderRva;

        public ImageHelper(IntPtr baseAddress, long lengthBytes)
        {
            m_baseAddress = baseAddress;
            m_lengthBytes = lengthBytes;

            //
            // Crack Dos, NT  headers to find section table
            // 
            IMAGE_DOS_HEADER dos = MarshalAt<IMAGE_DOS_HEADER>(0);
            if (!dos.IsValid)
            {
                throw new ArgumentException(Resources.InvalidFileFormat);
            }

            m_idx = dos.e_lfanew;

            // Determine if we have 32-bit or 64-bit image.
            IMAGE_NT_HEADERS_HELPER helper = MarshalAt<IMAGE_NT_HEADERS_HELPER>(m_idx);
            if (!helper.IsValid)
            {
                throw new ArgumentException(Resources.InvalidFileFormat);
            }

            if (helper.Magic == (ushort)ImageType.Pe32bit)
            {
                this.ImageType = ImageType.Pe32bit;

                IMAGE_NT_HEADERS_32 header = MarshalAt<IMAGE_NT_HEADERS_32>(m_idx);
                m_idxSectionStart = m_idx + (uint)Marshal.SizeOf(typeof(IMAGE_NT_HEADERS_32));
                m_numSections = (uint)header.FileHeader.NumberOfSections;
                m_clrHeaderRva = header.OptionalHeader.ClrHeaderTable.VirtualAddress;
            }
            else if (helper.Magic == (ushort)ImageType.Pe64bit)
            {
                this.ImageType = ImageType.Pe64bit;

                IMAGE_NT_HEADERS_64 header = MarshalAt<IMAGE_NT_HEADERS_64>(m_idx);
                m_idxSectionStart = m_idx + (uint)Marshal.SizeOf(typeof(IMAGE_NT_HEADERS_64));
                m_numSections = (uint)header.FileHeader.NumberOfSections;
                m_clrHeaderRva = header.OptionalHeader.ClrHeaderTable.VirtualAddress;
            }
            else
            {
                throw new ArgumentException(Resources.UnsupportedImageType);
            }
        }

        /// <summary>
        /// Defines image type: 32-bit or 64-bit.
        /// </summary>
        public ImageType ImageType { get; private set; }

        /// <summary>
        /// Finds real pointer to the start of managed resources section.
        /// </summary>
        public IntPtr GetResourcesSectionStart()
        {
            IMAGE_COR20_HEADER clrHeader = GetCor20Header();

            uint resourcesRva = clrHeader.Resources.VirtualAddress;

            // Resolve Resources RVA to a real address.
            return ResolveRva(resourcesRva);
        }

        /// <summary>
        /// Get the IMAGE_COR20_HEADER.
        /// </summary>
        /// <returns></returns>
        internal IMAGE_COR20_HEADER GetCor20Header()
        {
            // Find address of CLR header.
            IntPtr clrHeaderPtr = ResolveRva(m_clrHeaderRva);

            // Read CLR header that contains information about managed resources section.
            IMAGE_COR20_HEADER clrHeader = (IMAGE_COR20_HEADER)Marshal.PtrToStructure(clrHeaderPtr, typeof(IMAGE_COR20_HEADER));

            return clrHeader;
        }

        /// <summary>
        /// Get the token that specifies the method  where execution in this module should start (as defined in ecma II.25.3.3.2 )
        /// or a null token if no such method.
        /// </summary>
        /// <returns></returns>
        public Token GetEntryPointToken()
        {
            // Entry point token is specified by the ".entry" directive in ILAsm.
            // This can not be in a generic class.
            // See II.25.3.3.2 for entry Point token:
            //  The entry point token (§15.4.1.2) is always a MethodDef token (§22.26) or File token 
            //  (§22.19 ) when the entry point for a multi-module assembly is not in the manifest assembly.  
            //  The signature and implementation flags in metadata for the method indicate how the entry is run.
             

            IMAGE_COR20_HEADER clrHeader = GetCor20Header();
            if ((clrHeader.Flags & CorHdrNumericDefines.COMIMAGE_FLAGS_NATIVE_ENTRYPOINT) != 0)
            {
                return Token.Nil;
            }
            uint raw = clrHeader.EntryPoint;
            return new Token(raw);
        }

        /// <summary>
        /// Resolve an Relative-Virtual-Address (RVA) within a file to a real address.
        /// The RVA is likely obtained as data embedded in the PE image, and used to refer to data elsewhere
        /// in the PE image.
        /// </summary>
        /// <param name="rva">rva within the PE image</param>
        /// <returns>a real address within the process, or 0 if the RVA can't be resolved. </returns>
        /// <remarks>You can't just resolve an RVA by adding it to the base address because RVAs the loaded
        /// image and the on-disk image are different. RVAs baked into the image are designed to be used in
        /// the loaded case, in which case it can just be added to the base address. But the on-disk image
        /// may have the sections in a compressed form. So we need to walk the sections to determine where
        /// the RVA would go. </remarks>
        public IntPtr ResolveRva(long rva)
        {
            //
            // Loop through each section to determine if the RVA is in this section.
            // 
            var idxSection = m_idxSectionStart;
            for (int i = 0; i < m_numSections; i++)
            {
                IMAGE_SECTION_HEADER section = MarshalAt<IMAGE_SECTION_HEADER>((uint)idxSection);

                if ((rva >= section.VirtualAddress) &&
                    (rva < (section.VirtualAddress + section.SizeOfRawData)))
                {
                    // RVA is in this seciton, we can now resolve it.
                    long address = this.m_baseAddress.ToInt64() + rva - section.VirtualAddress + section.PointerToRawData;


                    return new IntPtr(address);
                }

                // Move to next section
                idxSection += (uint)Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER));

            }

            // Can't resolve the RVA.
            return IntPtr.Zero;
        }

        #region Native imports of Image structures
        // See http://msdn.microsoft.com/msdnmag/issues/02/02/PE/default.aspx for more details

        // The only value of this is to get to at the IMAGE_NT_HEADERS.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        [StructLayout(LayoutKind.Explicit)]
        public class IMAGE_DOS_HEADER
        {
            // DOS .EXE header
            [System.Runtime.InteropServices.FieldOffset(0)]
            public short e_magic;                     // Magic number

            /// <summary>
            /// Determine if this is a valid DOS image. 
            /// </summary>
            public bool IsValid
            {
                get
                {
                    return e_magic == 0x5a4d;  // 'MZ'
                }
            }
            // This is the offset of the IMAGE_NT_HEADERS, which is what we really want.
            [System.Runtime.InteropServices.FieldOffset(0x3c)]
            public uint e_lfanew;                    // File address of new exe header
        }

        /// <summary>
        /// Native import for IMAGE_FILE_HEADER.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        [StructLayout(LayoutKind.Sequential)]
        internal class IMAGE_FILE_HEADER
        {
            public short Machine;
            public short NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public short SizeOfOptionalHeader;
            public short Characteristics;
        }

        /// <summary>
        /// Defines first part of IMAGE_NT_HEADERS that's processor architecture independant.
        /// Used to read "Magic" field that contains image type: 32-bit or 64-bit.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class IMAGE_NT_HEADERS_HELPER
        {
            // PE\0\0
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public ushort Magic;

            public bool IsValid
            {
                get { return Signature == 0x00004550; }  // 'PE\0\0'
            }
        }

        /// <summary>
        /// Native import for IMAGE_NT_HEADER on 32-bit architecture.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class IMAGE_NT_HEADERS_32
        {
            // PE\0\0
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER_32 OptionalHeader;
        }

        /// <summary>
        /// Native import for IMAGE_NT_HEADER on 64-bit architecture.
        /// </summary> 
        [StructLayout(LayoutKind.Sequential)]
        internal class IMAGE_NT_HEADERS_64
        {
            // PE\0\0
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER_64 OptionalHeader;
        }

        /// <summary>
        /// Native import for IMAGE_NT_HEADERs.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        class IMAGE_SECTION_HEADER
        {
            // BYTE Name[8];
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string name;

            // union {    DWORD PhysicalAddress;    DWORD VirtualSize;  } 
            public uint union;

            public uint VirtualAddress;
            public uint SizeOfRawData;
            public uint PointerToRawData;
            public uint PointerToRelocations;
            public uint PointerToLinenumbers;
            public ushort NumberOfRelocations;
            public ushort NumberOfLinenumbers;
            public uint Characteristics;
        }

        /// <summary>
        /// Managed definition for IMAGE_OPTIONAL_HEADER32 from Winnt.h.
        /// See Serge Lidin's book .NET IL assembler (page 47) for full description.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class IMAGE_OPTIONAL_HEADER_32
        {
            // Standard fields.
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public uint BaseOfData;

            // NT additional fields.
            public uint ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public uint SizeOfStackReserve;
            public uint SizeOfStackCommit;
            public uint SizeOfHeapReserve;
            public uint SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
            public IMAGE_DATA_DIRECTORY ExportTable;
            public IMAGE_DATA_DIRECTORY ImportTable;
            public IMAGE_DATA_DIRECTORY ResourceTable;
            public IMAGE_DATA_DIRECTORY ExceptionTable;
            public IMAGE_DATA_DIRECTORY CertificateTable;
            public IMAGE_DATA_DIRECTORY BaseRelocationTable;
            public IMAGE_DATA_DIRECTORY DebugData;
            public IMAGE_DATA_DIRECTORY ArchitectureData;
            public IMAGE_DATA_DIRECTORY GlobalPointer;
            public IMAGE_DATA_DIRECTORY TlsTable;
            public IMAGE_DATA_DIRECTORY LoadConfigTable;
            public IMAGE_DATA_DIRECTORY BoundImportTable;
            public IMAGE_DATA_DIRECTORY ImportAddressTable;
            public IMAGE_DATA_DIRECTORY DelayImportTable;
            public IMAGE_DATA_DIRECTORY ClrHeaderTable;
            public IMAGE_DATA_DIRECTORY Reserved;
        }

        /// <summary>
        /// Managed definition for IMAGE_OPTIONAL_HEADER64 from Winnt.h.
        /// See Serge Lidin's book .NET IL assembler (page 47) for full description.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class IMAGE_OPTIONAL_HEADER_64
        {
            // Standard fields.
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;

            // NT additional fields.
            public ulong ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public ulong SizeOfStackReserve;
            public ulong SizeOfStackCommit;
            public ulong SizeOfHeapReserve;
            public ulong SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
            public IMAGE_DATA_DIRECTORY ExportTable;
            public IMAGE_DATA_DIRECTORY ImportTable;
            public IMAGE_DATA_DIRECTORY ResourceTable;
            public IMAGE_DATA_DIRECTORY ExceptionTable;
            public IMAGE_DATA_DIRECTORY CertificateTable;
            public IMAGE_DATA_DIRECTORY BaseRelocationTable;
            public IMAGE_DATA_DIRECTORY DebugData;
            public IMAGE_DATA_DIRECTORY ArchitectureData;
            public IMAGE_DATA_DIRECTORY GlobalPointer;
            public IMAGE_DATA_DIRECTORY TlsTable;
            public IMAGE_DATA_DIRECTORY LoadConfigTable;
            public IMAGE_DATA_DIRECTORY BoundImportTable;
            public IMAGE_DATA_DIRECTORY ImportAddressTable;
            public IMAGE_DATA_DIRECTORY DelayImportTable;
            public IMAGE_DATA_DIRECTORY ClrHeaderTable;
            public IMAGE_DATA_DIRECTORY Reserved;
        }

        /// <summary>
        /// Managed definition for IMAGE_DATA_DIRECTORY from Winnt.h.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class IMAGE_DATA_DIRECTORY 
        {
            public uint VirtualAddress;
            public uint Size;
        }


        internal enum CorHdrNumericDefines : uint
        {
            // COM+ Header entry point flags.
            COMIMAGE_FLAGS_ILONLY = 0x00000001,
            COMIMAGE_FLAGS_32BITREQUIRED = 0x00000002,
            COMIMAGE_FLAGS_IL_LIBRARY = 0x00000004,
            COMIMAGE_FLAGS_STRONGNAMESIGNED = 0x00000008,
            COMIMAGE_FLAGS_NATIVE_ENTRYPOINT = 0x00000010,
            COMIMAGE_FLAGS_TRACKDEBUGDATA = 0x00010000,
            COMIMAGE_FLAGS_ISIBCOPTIMIZED = 0x00020000,    // NEW
        }

        /// <summary>
        /// Managed definition for CLR Header structure.
        /// See Serge Lidin's book .NET IL assembler (page 56) for full description.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class IMAGE_COR20_HEADER
        {
            // Header versioning
            public uint cb;              
            public ushort MajorRuntimeVersion;
            public ushort MinorRuntimeVersion;
            
            // Symbol table and startup information
            public IMAGE_DATA_DIRECTORY MetaData;
            public CorHdrNumericDefines Flags;           
          
            // The main program if it is an EXE (not used if a DLL?)
            // If COMIMAGE_FLAGS_NATIVE_ENTRYPOINT is not set, EntryPointToken represents a managed entrypoint.
            // If COMIMAGE_FLAGS_NATIVE_ENTRYPOINT is set, EntryPointRVA represents an RVA to a native entrypoint
            // (depricated for DLLs, use modules constructors intead). 
            // union { DWORD EntryPointToken; DWORD EntryPointRVA; };
            public uint EntryPoint;
            
            // This is the blob of managed resources. Fetched using code:AssemblyNative.GetResource and
            // code:PEFile.GetResource and accessible from managed code from
            // System.Assembly.GetManifestResourceStream.  The meta data has a table that maps names to offsets into
            // this blob, so logically the blob is a set of resources. 
            public IMAGE_DATA_DIRECTORY Resources;

            // IL assemblies can be signed with a public-private key to validate who created it.  The signature goes
            // here if this feature is used. 
            public IMAGE_DATA_DIRECTORY StrongNameSignature;

            // Depricated, not used
            public IMAGE_DATA_DIRECTORY CodeManagerTable;			 

            // Used for manged codee that has unmaanaged code inside it (or exports methods as unmanaged entry points)
            public IMAGE_DATA_DIRECTORY VTableFixups;
            public IMAGE_DATA_DIRECTORY ExportAddressTableJumps;

            // null for ordinary IL images.  NGEN images it points at a code:CORCOMPILE_HEADER structure
            public IMAGE_DATA_DIRECTORY ManagedNativeHeader;
        }

        #endregion // Native imports of Image structures


        /// <summary>
        /// Marshal an unmanaged data structure from the image
        /// </summary>
        /// <typeparam name="T">managed type for the data structure to marshal</typeparam>
        /// <param name="offset">offset within the image to copy from. This must be in the image's range. </param>
        /// <returns>a managed version of the data structure marshaled from the image.</returns>
        internal T MarshalAt<T>(uint offset)
        {
            // Validate that marshal request is in the image range            
            long start = m_baseAddress.ToInt64();
            long end = start + this.m_lengthBytes;

            var size = Marshal.SizeOf(typeof(T));
            if (offset + size > end)
            {
                throw new InvalidOperationException(Resources.CorruptImage);
            }

            IntPtr p2 = new IntPtr(start + offset);           
            T header = (T)Marshal.PtrToStructure(p2, typeof(T));
            return header;
        }
    }

} // end namespace System.Reflection.Adds

