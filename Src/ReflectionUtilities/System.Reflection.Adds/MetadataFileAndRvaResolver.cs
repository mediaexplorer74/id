// Wrap a metadata dispenser and raw importer.
//
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using Debug=Microsoft.MetadataReader.Internal.Debug;

namespace System.Reflection.Adds
{

    /// <summary>
    /// Metadata file wrapper with RVA resolver capabilities.
    /// </summary>
    internal class MetadataFileAndRvaResolver : MetadataFile
    {
        // Underlying file mapping. This keeps the memory that the importer operates on alive. 
        FileMapping m_file;

        bool m_disableRangeValidation = false;

        // Derived disposing function.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // m_file should be released after base.Dispose is called, 
            // which releases the IMetadaImport pointer for the metadata.
            // Otherwise, access violations may happen when using the pointer
            // to access the disposed metadata.
            // 
            m_file.Dispose();
        }

        public override string FilePath 
        {
            get { return m_file.Path; }
        }

        /// <summary>
        /// Bind a metadata importer scope together with a file mapping that can be used to fully resolve
        /// any RVAs.
        /// </summary>
        /// <param name="importer">a raw pointer to an IMetadataImporter object. The importer should be
        /// retrieved via Dispenser.OpenScopeOnMemory() for memory backed by the file mapping.</param>
        /// <param name="file">a file mapping object that owns the raw buffer that the importer operates on.</param>
        public MetadataFileAndRvaResolver(IntPtr importer, FileMapping file) :
            base(importer)
        {
            m_file = file;
        }

        /// <summary>
        /// Bind a metadata importer scope together with a file mapping that can be used to fully resolve
        /// any RVAs.
        /// </summary>
        /// <param name="importer">a raw pointer to an IMetadataImporter object. The importer should be
        /// retrieved via Dispenser.OpenScopeOnMemory() for memory backed by the file mapping.</param>
        /// <param name="file">a file mapping object that owns the raw buffer that the importer operates on.</param>
        /// <param name="disableRangeValidation">If true turns of range validation. This is required if the importer was opende without setting the NoTransform open flag.</param>
        public MetadataFileAndRvaResolver(IntPtr importer, FileMapping file, bool disableRangeValidation) :
            base(importer)
        {
            m_file = file;
            m_disableRangeValidation = disableRangeValidation;
        }

        /// <summary>
        /// Bind a metadata importer scope together with a file mapping that can be used to fully resolve
        /// any RVAs.
        /// </summary>
        /// <param name="importer">An RCW around an IMetadataImport interface.</param>
        /// <param name="file">a file mapping object that owns the raw buffer that the importer operates on.</param>
        public MetadataFileAndRvaResolver(object importer, FileMapping file) :
            base(importer)
        {
            m_file = file;
        }

        /// <summary>
        /// Resolve an RVA to its real virtual address within this process space.
        /// </summary>
        /// <param name="rva">rva from this metadata file.</param>
        /// <returns>the resolved address</returns>
        IntPtr ResolveRva(long rva)
        {
            var image = new ImageHelper(m_file.BaseAddress, m_file.Length);

            // Resolving RVAs in static disk images is complicated, so call out to a helper function.
            // This returns 0 if we can't resolve the Rva. 
            IntPtr realAddress = image.ResolveRva(rva);
            if (realAddress == IntPtr.Zero)
            {
                throw new InvalidOperationException(Resources.CannotResolveRVA);
            }
            return realAddress;
        }

        /// <summary>
        /// override base class definition.
        /// </summary>
        public override byte[] ReadRva(long rva, int countBytes)
        {
            Debug.Assert(countBytes > 0);

            IntPtr realAddress = ResolveRva(rva);

            // Read from target
            byte[] buffer = new byte[countBytes];
            Marshal.Copy(realAddress, buffer, 0, buffer.Length);

            return buffer;
        }

        protected override void ValidateRange(IntPtr ptr, int countBytes)
        {
            if(!m_disableRangeValidation) {
                long pStart = m_file.BaseAddress.ToInt64();
                long pEnd = pStart + m_file.Length;

                long p = ptr.ToInt64();

                if((p < pStart) || ((p + countBytes) >= pEnd)) 
                {
                    Debug.Assert(false, "Invalid embedded metadata range requested");
                    throw new InvalidOperationException();
                }
                // Region is ok to read.
            }
        }

        /// <summary>
        /// Reads managed resource based on a given offset. 
        /// </summary>
        /// <param name="offset">Offset on which a resource starts.</param>
        /// <returns>Resource content. Throws on any failures. Never returns null.</returns>
        public override byte[] ReadResource(long offset)
        {
            var image = new ImageHelper(m_file.BaseAddress, m_file.Length);

            // Get address of managed resources section.
            IntPtr resourcesStart = image.GetResourcesSectionStart();

            // Requested resrource is at an offset from the start.
            IntPtr resourceAddress = new IntPtr(resourcesStart.ToInt64() + offset);

            // First four bytes are resource size.
            uint resourceSize = (uint)Marshal.ReadInt32(resourceAddress);

            // Resource content immediately follows resource size.
            resourceAddress = new IntPtr(resourceAddress.ToInt64() + Marshal.SizeOf(resourceSize));
            byte[] buffer = new byte[resourceSize];
            Marshal.Copy(resourceAddress, buffer, 0, buffer.Length);

            return buffer;
        }

        public override Token ReadEntryPointToken()
        {
            var image = new ImageHelper(m_file.BaseAddress, m_file.Length);
            return image.GetEntryPointToken();
        }

        /// <summary>
        /// override base class definition.
        /// </summary>
        public override T ReadRvaStruct<T>(long rva)
        {
            IntPtr realAddress = ResolveRva(rva);

            // Read from target            
            var s = (T) Marshal.PtrToStructure(realAddress, typeof(T));
            return s;
        }
    }
}
