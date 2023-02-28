// Wrap a metadata dispenser and raw importer.
//
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;
using Debug=Microsoft.MetadataReader.Internal.Debug;

namespace System.Reflection.Adds
{
    // Flags passed to OpenScope
    [Flags]
    public enum CorOpenFlags : uint
    {
        Read = 0,
        Write = 1,
        ReadWriteMask = 1,

        CopyMemory = 2,

        // Obsolete and ignored
        // CacheImage = 4,
        // ManifestMetadata_OBSOLETE = 8

        ReadOnly = 0x10,
        TakeOwnership = 0x20,
        NoTypeLib = 0x80,
        NoTransform = 0x1000
    }

    /// <summary>
    /// Helper class to open up a Metadata scope from a file.
    /// This can be called on multiple threads.
    /// </summary>
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]    
    public class MetadataDispenser
    {
        /// <summary>
        /// Creates dispenser appropriately based on runtime version. 
        /// </summary>
        /// <remarks>
        /// In v4, interfaces in mscoree need to be created through indirection
        /// to make sure we'll get v4 implementation instead of v2. This is due
        /// to in-proc SxS changes introduced in CLR v4.
        /// </remarks>
        private static IMetaDataDispenserEx GetDispenserShim()
        {
            // If there happens to be a stand-alone CLR metadata DLL next to our DLL, use that (for testing)
            string ourDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists(Path.Combine(ourDir, k_standaloneMetadataDllName)))
            {
                Guid clsid = typeof(CorMetaDataDispenserExClass).GUID;
                Guid iid = typeof(IMetaDataDispenserEx).GUID;
                return StandaloneMetaDataGetDispenser(ref clsid, ref iid);
            }

#if !USE_CLR_V4
            return new MetaDataDispenserEx();
#else
            return (IMetaDataDispenserEx)RuntimeEnvironment.GetRuntimeInterfaceAsObject(
                typeof(CorMetaDataDispenserExClass).GUID, typeof(IMetaDataDispenserEx).GUID);
#endif
        }

        private const string k_standaloneMetadataDllName = "clrmd.dll";
        [DllImport(k_standaloneMetadataDllName, PreserveSig=false, EntryPoint="MetaDataGetDispenser")]
        private static extern IMetaDataDispenserEx StandaloneMetaDataGetDispenser(ref Guid clsID, ref Guid iid);

        #region Open from memory
        /// <summary>
        /// Open a metadata file from the given stream. The stream should contain the same contents as the file on disk.        
        /// This will read all bytes from the stream. 
        /// </summary>
        /// <param name="data">stream for metadata file. This should start with 'mz', not 'bsjb'.</param>
        /// <returns>a wrapper around a raw importer created on the raw bytes of data.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")] 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]
        public MetadataFile OpenFromByteArray(byte[] data)
        {
            // Immediately make a copy to protect against mutation.
            data = (byte[]) data.Clone();

            // This allocates the dispenser each time we open the file. We'd like to save the dispenser
            // across calls, but then dispenser is CoCreated with thread affinity and it does not aggregate
            // the Free Threaded Marshaller. 
            IMetaDataDispenserEx dispenser = MetadataDispenser.GetDispenserShim();

            Guid g = typeof(IMetadataImportDummy).GUID;

            // Explicitly marshal the return IUnknown as an IntPtr rather than a COM-interface to avoid the CLR
            // marshalling layer injecting any thread-affinity stubs.
            IntPtr pUnk = IntPtr.Zero;
            GCHandle h = new GCHandle();
            try
            {  
                // Pin the data array so that we can get a raw address to it to pass to the OpenScopeOnMemory call.
                // The MetadataFile will then own the GC handle to protect the memory.
                h = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
                uint cbData = (uint) data.Length;
                
                // The native dispenser has global caching, if we open the same string name multiple times, it will
                // hand back the same importer. 
                // This method won't give back the RVA of the metadata scope. 
                int res = dispenser.OpenScopeOnMemory(pData, cbData, m_openFlags, ref g, out pUnk);


                Marshal.ThrowExceptionForHR(res);

                // We need Dispenser to survive until after the OpenScope call so that it doesn't get released
                // by the GC in the middle of the call dispatch.
                GC.KeepAlive(dispenser);

                // Forcibly release now to make the release deterministic. 
                Marshal.FinalReleaseComObject(dispenser);
                dispenser = null;


                // This will take a reference to pUnk; so the release we call here in the finally just
                // releases this functions reference. MetadataFile then becomes the owner.
                var m = new MetadataFileOnByteArray(ref h, pUnk);
                Debug.Assert(!h.IsAllocated); // m took ownership and cleared our copy h so that we don't double-free.
                
                return m;
            }
            finally
            {
                if (h.IsAllocated)
                {
                    h.Free();
                }
                if (pUnk != IntPtr.Zero)
                {
                    Marshal.Release(pUnk);
                }
            }
        }

        /// <summary>
        /// MetadataFile for an in-memory importer. This protects the backing memory.
        /// </summary>
        class MetadataFileOnByteArray : MetadataFile
        {
            // The GC handle that pins the underlying data in place.
            GCHandle m_handle;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="h">gchandle pinning the memory pointed to by pUnk. If the ctor succeeds,
            /// it will clear h to indicate to caller that we've taken ownership of freeing the GC handle.</param>
            /// <param name="pUnk">pointer to raw metadata bytes</param>
            public MetadataFileOnByteArray(ref GCHandle h, IntPtr pUnk)
                : base(pUnk)
            {
                Debug.Assert(h.IsAllocated);
                m_handle = h;
                h = new GCHandle(); // we  now owns the GC handle and will free it.
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                // m_handle should be released after base.Dispose is called, 
                // which releases the IMetadaImport pointer for the metadata.
                // Otherwise, access violations may happen when using the pointer
                // to access the disposed metadata.
                // 
                m_handle.Free();
            }
        }

        #endregion // Open from memory

        /// <summary>
        /// Open file retrieving the RVA so that we have full access to metadata. 
        /// </summary>
        /// <param name="fileName">filename to open</param>
        /// <returns>a metadata file object with RVA resolution abilities</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")] 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")] 
        public MetadataFile OpenFileAsFileMapping(string fileName)
        {
            // Creat a file mapping so that we can get the base address so that we can resolve RVAs.
            var f = new FileMapping(fileName);

            // This allocates the dispenser each time we open the file. We'd like to save the dispenser
            // across calls, but then dispenser is CoCreated with thread affinity and it does not aggregate
            // the Free Threaded Marshaller. 
            IMetaDataDispenserEx dispenser = MetadataDispenser.GetDispenserShim();
            Guid g = typeof(IMetadataImportDummy).GUID;

            // Explicitly marshal the return IUnknown as an IntPtr rather than a COM-interface to avoid the CLR
            // marshalling layer injecting any thread-affinity stubs.
            IntPtr pUnk = IntPtr.Zero;
            try
            {
                IntPtr pData = f.BaseAddress;
                uint cbData = (uint) f.Length;

                // OpenScopeOnMemory takes in the base-address of the on-disk PE Image ('mz'), not
                // the address of a loaded image (which is different than disk), and not the pointer to the
                // metadata section ('bsjb'). 
                int res = dispenser.OpenScopeOnMemory(pData, cbData, m_openFlags, ref g, out pUnk);
                Marshal.ThrowExceptionForHR(res);

                // We need Dispenser to survive until after the OpenScope call so that it doesn't get released
                // by the GC in the middle of the call dispatch.
                GC.KeepAlive(dispenser);

                // Forcibly release now to make the release deterministic. 
                Marshal.FinalReleaseComObject(dispenser);
                dispenser = null;


                // This will take a reference to pUnk; so the release we call here in the finally just
                // releases this functions reference. MetadataFile then becomes the owner.
                return new MetadataFileAndRvaResolver(pUnk, f, 0 == (m_openFlags & CorOpenFlags.NoTransform));
            }
            finally
            {
                if (pUnk != IntPtr.Zero)
                {
                    Marshal.Release(pUnk);
                }
            }
        }

        /// <summary>
        /// Creates MetadataFile object over already available IMetadataImport interface (instead of opening 
        /// scope and getting it manually like in other overloads). E.g. this is useful when clients need to use
        /// IVsSmartOpenScope to faciliate sharing. It will also retrieve the RVA so that we have full access to metadata. 
        /// </summary>
        /// <param name="importer">An RCW around an IMetadataImport interface. Caller is responsible for importer's lifetime.</param>
        /// <param name="fileName">Full path to the opened file in case we need to fall back on mapping the file manually.
        /// E.g. this would happen when the file is in the GAC because CLR doesn't support it.</param>
        /// <returns>A metadata file object with RVA resolution abilities.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public MetadataFile OpenFileAsFileMapping(object importer, string fileName)
        {
            if (importer == null)
            {
                throw new ArgumentNullException("importer");
            }

            FileMapping fileMapping = null;
            IMetaDataInfo info = importer as IMetaDataInfo;
            if (info != null)
            {
                IntPtr baseAddress;
                long fileSize;
                CorFileMapping mappingType;
                int hr = info.GetFileMapping(out baseAddress, out fileSize, out mappingType);

                if ((hr == 0) && (mappingType == CorFileMapping.Flat))
                {
                    Debug.Assert(baseAddress != IntPtr.Zero, "Base address should not be zero if GetFileFlatMapping call succeeded.");
                    fileMapping = new FileMapping(baseAddress, fileSize, fileName);
                    return new MetadataFileAndRvaResolver(importer, fileMapping); 
                }
            }

            // Fall back on opening scope on memory and mapping the file manually if:
            //   1. passed in object doesn't implement IMetaDataInfo (older CLR version) or 
            //   2. CLR can't give us base address (file is in the GAC for example) or
            //   3. CLR mapped it as executable image (we don't support calculating RVAs on this type of mapping yet)
            return OpenFileAsFileMapping(fileName); 
        }

        /// <summary>
        /// Open a metadata scope from a file on disk for read-only.
        /// </summary>
        /// <param name="fileName">filename to pass to </param>
        /// <returns>a metadata scope object representing the file. It can't resolve RVAs.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]
        public MetadataFile OpenFile(string fileName)
        {
            // This allocates the dispenser each time we open the file. We'd like to save the dispenser
            // across calls, but then dispenser is CoCreated with thread affinity and it does not aggregate
            // the Free Threaded Marshaller. 
            IMetaDataDispenserEx dispenser = MetadataDispenser.GetDispenserShim();

            Guid g = typeof(IMetadataImportDummy).GUID;

            // Explicitly marshal the return IUnknown as an IntPtr rather than a COM-interface to avoid the CLR
            // marshalling layer injecting any thread-affinity stubs.
            IntPtr pUnk = IntPtr.Zero;
            try
            {
                // The native dispenser has global caching, if we open the same string name multiple times, it will
                // hand back the same importer. 
                // This method won't give back the RVA of the metadata scope. 
                // Note that in this version of LMR, we always want the raw view of WinMD files.  This should probably be an option.
                int res = dispenser.OpenScope(fileName, m_openFlags, ref g, out pUnk);

                Marshal.ThrowExceptionForHR(res);

                // We need Dispenser to survive until after the OpenScope call so that it doesn't get released
                // by the GC in the middle of the call dispatch.
                GC.KeepAlive(dispenser);

                // Forcibly release now to make the release deterministic. 
                Marshal.FinalReleaseComObject(dispenser);
                dispenser = null;


                // This will take a reference to pUnk; so the release we call here in the finally just
                // releases this functions reference. MetadataFile then becomes the owner.
                return new MetadataFile(pUnk);
            }
            finally
            {
                if (pUnk != IntPtr.Zero)
                {
                    Marshal.Release(pUnk);
                }
            }
        }
        
        /// <summary>
        /// The flags to use when opening a new importer
        /// In this version of LMR we always want the raw view of WinMD files (NoTransform).  For more general use we 
        /// probably want to give the host the option to choose.
        /// </summary>
        private CorOpenFlags m_openFlags = CorOpenFlags.ReadOnly | CorOpenFlags.NoTransform;

        public CorOpenFlags OpenFlags
        {
            get { return m_openFlags; }
            set { m_openFlags = value; }
        }

        #region COM interop
        // Flags returned from IMetaDataInfo.GetFileMapping
        internal enum CorFileMapping : uint
        {
            Flat            = 0,    // Flat file mapping - file is mapped as data file (code:SEC_IMAGE flag was not 
                                    // passed to code:CreateFileMapping).
            ExecutableImage = 1     // Executable image file mapping - file is mapped for execution 
                                    // (either via code:LoadLibrary or code:CreateFileMapping with code:SEC_IMAGE flag).
        } 

        [Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44"),
            InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
        ] // IID_IMetadataImport from cor.h
        interface IMetadataImportDummy
        {
            // We'll just pass this through to LMR as an opaque interface, 
            // we don't actually need to call any of the methods on it. 
        }

        // Importing a metadata dispenser.
        [ComImport, GuidAttribute("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IMetaDataDispenserEx
        {
            int DefineScope(ref Guid rclsid, uint dwCreateFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)]out object ppIUnk);
            
            // We'd like to marshal the return value as an 
            //    [MarshalAs(UnmanagedType.Interface)] out object ppIUnk
            // but interface marshaling has thread-affinity. See code:MetadataDispenser.OpenFile for details.
            [PreserveSig]
            int OpenScope([MarshalAs(UnmanagedType.LPWStr)]string szScope, CorOpenFlags dwOpenFlags, ref Guid riid, out IntPtr ppIUnk);

            [PreserveSig]
            int OpenScopeOnMemory(IntPtr pData, uint cbData, CorOpenFlags dwOpenFlags, ref Guid riid, out IntPtr ppIUnk);
            
            int SetOption(ref Guid optionid, [MarshalAs(UnmanagedType.Struct)]object value);

            [PreserveSig]
            int GetOption(ref Guid optionid, [MarshalAs(UnmanagedType.Struct)]out object pvalue);
            //uint OpenScopeOnITypeInfo([MarshalAs(UnmanagedType.Interface)]ITypeInfo pITI, uint dwOpenFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)]out object ppIUnk);
            int _OpenScopeOnITypeInfo();
            int GetCORSystemDirectory([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]char[] szBuffer, uint cchBuffer, out uint pchBuffer);
            int FindAssembly([MarshalAs(UnmanagedType.LPWStr)]string szAppBase, [MarshalAs(UnmanagedType.LPWStr)]string szPrivateBin, [MarshalAs(UnmanagedType.LPWStr)]string szGlobalBin, [MarshalAs(UnmanagedType.LPWStr)]string szAssemblyName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]char[] szName, uint cchName, out uint pcName);
            int FindAssemblyModule([MarshalAs(UnmanagedType.LPWStr)]string szAppBase, [MarshalAs(UnmanagedType.LPWStr)]string szPrivateBin, [MarshalAs(UnmanagedType.LPWStr)]string szGlobalBin, [MarshalAs(UnmanagedType.LPWStr)]string szAssemblyName, [MarshalAs(UnmanagedType.LPWStr)]string szModuleName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]char[] szName, uint cchName, out uint pcName);
        }

        [ComImport, GuidAttribute("E5CB7A31-7512-11D2-89CE-0080C792E5D8")]
        class CorMetaDataDispenserExClass { }

        [ComImport, GuidAttribute("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3"), CoClass(typeof(CorMetaDataDispenserExClass))]
        interface MetaDataDispenserEx : IMetaDataDispenserEx { }

        [ComImport]
        [Guid("7998EA64-7F95-48B8-86FC-17CAF48BF5CB")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMetaDataInfo
        {
            // MetaData scope is opened (there's a reference to a MetaData interface for this scope).
            // Returns S_OK, COR_E_NOTSUPPORTED, or E_INVALIDARG (if NULL is passed).
            // STDMETHOD(GetFileMapping)(
            //    const void ** ppvData,        // [out] Pointer to the start of the mapped file.
            //    ULONG * pcbData,              // [out] Size of the mapped memory region.
            //    DWORD * pdwMappingType) PURE; // [out] Type of file mapping (code:CorFileMapping).
            [PreserveSig]
            int GetFileMapping(out IntPtr ppvData, out long pcbData, out CorFileMapping pdwMappingType);
        }

    } // end class MetadataDispenser

    #endregion // COM interop
}