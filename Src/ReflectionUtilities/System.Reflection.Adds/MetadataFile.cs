// Wrap a metadata dispenser and raw importer.
// Cosnider if this should be moved from ReflectionAdds to LMR. 
//
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Debug=Microsoft.MetadataReader.Internal.Debug;

namespace System.Reflection.Adds
{
    /// <summary>
    /// Represents an live IntPtr that points into the loaded metadata blob. 
    /// This wraps the live pointer to protect access to it.
    /// The only way to actually read the contents from the pointer requires going through the MetadataFile,
    /// which can validate that the region of memory is valid.
    /// </summary>
    /// <remarks>The danger of exposing this pointer directly is that if the underlying storage was unloaded, the read 
    /// becomes invalid. For example, a bug would be caching the raw pointer in a global, and then the underlying metadata
    /// file may get unloaded, and then reading from the pointer may crash.
    /// By wrapping these embedded poitnters here, we force the read to go through the MetadataFile, which owns
    /// the storage and can thus vouch that the read is safe.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct EmbeddedBlobPointer
    {
        // The underlying storage. This is written to be the COM-marshaler when getting a 
        // EmbeddedBlobPointer from a COM-interop call.
        private IntPtr m_data;

        /// <summary>
        /// internal helper to get the pointer for reading. Caller should validate.
        /// </summary>
        internal IntPtr GetDangerousLivePointer
        {
            get { return m_data; }
        }

    }

    /// <summary>
    /// Represents a raw IMetadataImporter. 
    /// Metadata has RVAs to data. However, a raw importer object doesn't have a base address. Since the creator of the metadata
    /// almost always has a base address, so we couple the base address and metadata object together.
    /// This is also disposable, which will just Release the underlying native IMetadataImport pointer. That's important because it 
    /// may hold file locks.
    /// </summary>
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public class MetadataFile : IDisposable
    {
        // The underlying pointer to the IMetadataImport interface. This is a native resource, which 
        // may hold native resources such as file locks. This class implements IDisposable to manage this native resource.
        // We use a raw IntPtr to explicitly controll COM-marshalling and RCW usage, especially around appartments, because the CLR's 
        // implementation of IMetadataImport does not support the Free-threaded-marshaller.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        private IntPtr m_rawPointer;

        /// <summary>
        /// Create a wrapper around an existing imported Metadata reader object.  This is problematic
        /// because the importer may have thread affinity.
        /// </summary>
        /// <param name="importer">a RCW around an IMetadataImport interface</param>
        public MetadataFile(object importer)
        {
            if (importer == null)
            {
                throw new ArgumentNullException("importer");
            }
            // This will increase the reference count. We need to release in the finalizer.
            this.m_rawPointer = Marshal.GetIUnknownForObject(importer);
        }

        /// <summary>
        /// Create a wrapper around a raw intPtr to a raw IMetadataImport
        /// This will take a reference to importer, so the caller can release their reference to transfer
        /// ownership to this object.
        /// </summary>
        /// <param name="rawImporter">a direct pointer to a IMetadataImport interface </param>
        internal MetadataFile(IntPtr rawImporter)
        {
            if (rawImporter == IntPtr.Zero)
            {
                throw new ArgumentNullException("rawImporter");
            }

            // Do an AddRef here to balance the one in the finalizer.
            Marshal.AddRef(rawImporter);

            this.m_rawPointer = rawImporter;
        }

        ~MetadataFile()
        {
            // MetadataFile has a finalizer for one purpose only: 
            // to make sure IMDImport is eventually released even 
            // if LMR user forgets to call Dispose properly. So 
            // this is definitely not normal path for releasing this
            // pointer, just a safety feature.

            // Free native resources.
            Dispose(false);
        }
        #region IDisposable Members
                
        /// <summary>
        /// Implementation of IDisposable.Dispose. Explcitly release resources.        
        /// Caller is responsible for thread safey here and to not dispose while another thread is using.
        /// Caller should not use after this has been diposed.        
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            // if dispose==true, then free managed resources. We don't have any, so nothing to do here.
            
            // free native resources.
            if (m_rawPointer != IntPtr.Zero)
            {
                Marshal.Release(m_rawPointer);
            }
            m_rawPointer = IntPtr.Zero;
        }

        #endregion

        /// <summary>
        /// The raw IMetadataImport pointer. This may not yet have a RCW associated with it. 
        /// This class owns a reference on the ptr and will call Release().
        /// This uses a IntPtr instead of a System.Object to avoid having an RCW to avoid thread-affinity issues.
        /// See code:MetadataDispenser.OpenFile for details. 
        /// </summary>
        public IntPtr RawPtr {
            get
            {
                // Callers should not be calling this after dispose, so assert to help them out.
                // The pointer is nulled out after dispose, so a call would normally result in at least a null-ref exception.
                // Give them an object disposed exception to make it easier to track this down.
                Debug.Assert(m_rawPointer != IntPtr.Zero, "Using disposed metadata");                
                EnsureNotDispose();

                return m_rawPointer;
            }            
        }

        /// <summary>
        /// The path of the metadata file.
        /// </summary>
        public virtual string FilePath 
        {
            get { return null; }
        }

        /// <summary>
        /// Read a region of memory specified by a Relative Virtual Address (RVA) in the given metadata scope.
        /// Fails if the data can't be read. This can
        /// include cases where the scope may not have a base address (dynamic modules, enc, etc)
        /// Never returns null.
        /// This can be used to read the raw bytes of RVA based static fields, such as in the case of the
        /// opaque struct blobs used in arrayliteral intializers, as well as MethodBody data.
        /// </summary>
        /// <param name="rva">rva encoded in metadata. </param>
        /// <param name="countBytes">number of bytes to read at</param>
        /// <returns>an array of bytes for the rva data. Throws on any failures. Never returns null.</returns>
        public virtual byte[] ReadRva(long rva, int countBytes)
        {
            throw new NotSupportedException(Resources.RVAUnsupported);
        }

        /// <summary>
        /// Reads managed resource based on a given offset. 
        /// </summary>
        /// <param name="offset">Offset on which a resource starts.</param>
        /// <returns>Content of resource. Throws on any failures. Never returns null.</returns>
        public virtual byte[] ReadResource(long offset)
        {
            throw new NotSupportedException(Resources.RVAUnsupported);
        }

        /// <summary>
        /// Read a blob embedded in the metadata
        /// </summary>
        /// <param name="pointer">live pointer into the metadata blob</param>
        /// <param name="countBytes">count of bytes to read</param>
        /// <returns>array of bytes of requested length copied from metadata blob.</returns>
        public byte[] ReadEmbeddedBlob(EmbeddedBlobPointer pointer, int countBytes)
        {
            EnsureNotDispose();

            if (countBytes == 0)
            {
                // If count of bytes is 0, the raw pointer may be random.
                return new byte[0] {  };
            }

            IntPtr p = pointer.GetDangerousLivePointer;
            this.ValidateRange(p, countBytes);

            byte[] blob = new byte[countBytes];
            Marshal.Copy(p, blob, 0, countBytes);
            return blob;
        }

        /// <summary>
        /// Derived class (which knows about the underlying storage of the IMDI) can validate that the region
        /// of memory is valid to read from.
        /// </summary>
        /// <param name="ptr">live pointer to within metadata blob</param>
        /// <param name="countBytes">count of bytes to read</param>
        protected virtual void ValidateRange(IntPtr ptr, int countBytes)
        {
            // derive class should provide validation.
        }

        /// <summary>
        /// Get the entry point token from the IMAGE_COR20_HEADER in the PE file. 
        /// See Ecma II 24.3.3.2 for details. 
        /// This can be a MethodDEf or a File token. Although in C# apps, this will usually be the methodDef for Main().
        /// This requires RVA resolution.
        /// </summary>
        /// <returns>entry point token from the headers. Nil token if no entry point specified (such as with dlls).</returns>
        public virtual Token ReadEntryPointToken()
        {
            throw new NotSupportedException(Resources.RVAUnsupported);
        }

        /// <summary>
        /// Read a unmanaged structure at the region memory at the given RVA.
        /// </summary>
        /// <typeparam name="T">type of structure to be read</typeparam>
        /// <param name="rva">rva encoded in metadata. </param>
        /// <returns>an instance of T marshaled from the given RVA</returns>
        public virtual T ReadRvaStruct<T>(long rva) where T : new()
        {
            EnsureNotDispose();

            // Default implementation builds on the byte[] reading version. Derived classes can override to
            // provide more efficient reading.

            // This will get the size of the unmanaged struct.
            int cb = Marshal.SizeOf(typeof(T));
            byte[] b = ReadRva(rva, cb);
            GCHandle g = GCHandle.Alloc(b, GCHandleType.Pinned);
            T obj;
            try
            {
                IntPtr p = g.AddrOfPinnedObject();
                obj = (T)Marshal.PtrToStructure(p, typeof(T));
            }
            finally
            {
                g.Free();
            }

            return obj;
        }

        /// <summary>
        /// internal helper to throw if the object is disposed. Nop if class is ok.
        /// </summary>
        protected void EnsureNotDispose()
        {
            if (this.m_rawPointer == IntPtr.Zero)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
        
    }
}
