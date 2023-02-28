

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Reflection.Adds;
using Debug = Microsoft.MetadataReader.Internal.Debug;
using System.IO;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using BindingFlags = System.Reflection.BindingFlags;
using Type = System.Reflection.Mock.Type;
using AssemblyName = System.Reflection.AssemblyName;
using AssemblyNameFlags = System.Reflection.AssemblyNameFlags;
using PortableExecutableKinds = System.Reflection.PortableExecutableKinds;
using ImageFileMachine = System.Reflection.ImageFileMachine;
using ProcessorArchitecture = System.Reflection.ProcessorArchitecture;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// Helpers for computing AssemblyName objects from metadata information.
    /// </summary>
    static class AssemblyNameHelper
    {
        private const int ProcessorArchitectureMask = 0xF0;
        private const int ReferenceAssembly = 0x70;

        /// <summary>
        /// This class helps build a System.Reflection.AssemblyName from the raw metadata structures.        
        /// Since the marshaling is very meticulous and involves unmanaged data structures and memory,  we
        /// wanted a single unified marshaling path for both the AssemblyRef and AssemblyDef paths.
        /// 
        /// Derived classes provide the actually call to the metadata APIs.
        /// </summary>
        abstract class AssemblyNameBuilder : IDisposable
        {
            // the underlying storage for the embedded pointers.
            readonly private MetadataFile m_storage;

            // The importer used by derived classes to fill out the fields.
            // We could fetch this from the storage, but that goes through a slow QI path, so we let it pass in separetely.
            readonly protected IMetadataAssemblyImport m_assemblyImport; 

            // These fields correspond to the underlying data to populate an AssemblyName
            // and the parameters to IMetadataAssemblyImport.GetAssemblyRefProps and GetAssemblyProps.
            // This leverages the fact that both metadata APIs have the same semantics on these properties.
            protected EmbeddedBlobPointer m_publicKey; 
            protected int m_cbPublicKey;
            protected int m_hashAlgId;
            protected StringBuilder m_szName;
            protected int m_chName;
            protected AssemblyNameFlags m_flags;
            protected AssemblyMetaData m_metadata;

            protected AssemblyNameBuilder(MetadataFile storage, IMetadataAssemblyImport assemblyImport)
            {
                this.m_storage = storage;
                this.m_assemblyImport = assemblyImport;
            }

            /// <summary>
            /// Derived class implements this to fetch the fields from the appropriate metadata API.
            /// </summary>
            protected abstract void Fetch();

            /// <summary>
            /// Calculate the AssemblyName object from the metadata structures.
            /// </summary>
            /// <returns>AssemblyName object</returns>
            public AssemblyName CalculateName()
            {
                // This is the resulting assembly name we'll build up.
                AssemblyName assemblyName = new AssemblyName();

                // Reset fields here (instead of in ctor) in case this is called multiple times on the same object. 
                m_metadata = new AssemblyMetaData();
                m_metadata.Init();
                this.m_szName = null;
                this.m_chName = 0;

                //
                // call first time to get sizes (both for szName and metadata fields)
                //
                this.Fetch();

                // Allocate storage
                this.m_szName = new StringBuilder();
                this.m_szName.Capacity = this.m_chName;
                                
                int countBytesLocale = (int)(m_metadata.cbLocale * sizeof(char));                
                m_metadata.szLocale = new UnmanagedStringMemoryHandle(countBytesLocale);


                // Clear metadata count fields that we're not using so that we don't request the data.
                m_metadata.ulProcessor = 0;
                m_metadata.ulOS = 0;

                //
                // Call 2nd time to fill in data.
                //
                this.Fetch();

                // Marshal culture data out from szLocale.
                assemblyName.CultureInfo = m_metadata.Locale;
                

                byte[] publicKeyArray = m_storage.ReadEmbeddedBlob(this.m_publicKey, this.m_cbPublicKey);

                assemblyName.HashAlgorithm = (System.Configuration.Assemblies.AssemblyHashAlgorithm)this.m_hashAlgId;

                assemblyName.Name = this.m_szName.ToString();
                assemblyName.Version = m_metadata.Version;
                // we cannot set assemblyName.Flags to this.m_flags directly because we will lose ContentType and ProcessorArchitecture.
                SetAssemblyNameFlags(assemblyName, m_flags);
                Debug.Assert(assemblyName.FullName != null);
                Debug.Assert(assemblyName.ToString() != null);

                // We may have either a PublicKey, or the PublicKeyToken. 
                if ((this.m_flags & AssemblyNameFlags.PublicKey) != 0)
                {
                    assemblyName.SetPublicKey(publicKeyArray);
                }
                else
                {
                    assemblyName.SetPublicKeyToken(publicKeyArray);
                }

                return assemblyName;
            }

            private static void SetAssemblyNameFlags(AssemblyName assemblyName, AssemblyNameFlags flags)
            {
                assemblyName.Flags = flags;

                // ProcessorArchitecture
                int arch = (((int)flags) & 0x70) >> 4;
                if (arch > 5) 
                    arch = 0;

                assemblyName.ProcessorArchitecture = (ProcessorArchitecture)arch;

                //ContentType
                int type = (((int)flags) & 0x00000E00) >> 9;
                if (type > 1)
                    type = 0;

                assemblyName.ContentType = (AssemblyContentType)type;
            }

            /// <summary>
            /// Exposes original assembly name flags read from metadata. Setting Flags property
            /// on AssemblyName masks some of the bits that are important for determining 
            /// assembly architecture.
            /// </summary>
            public AssemblyNameFlags AssemblyNameFlags
            {
                get
                {
                    return m_flags;
                }
            }

            #region Dispose
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            // The bulk of the clean-up code is implemented in Dispose(bool)
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // free managed resources
                    this.m_metadata.szLocale.Dispose();
                }
                // No native resources to free directly.
            }
            #endregion
        } // end class

        /// <summary>
        /// Derived class for getting an AssemblyName from an assembly definition.
        /// </summary>
        private class AssemblyNameFromDefitionBuilder : AssemblyNameBuilder
        {
            Token assemblyToken;

            public AssemblyNameFromDefitionBuilder(Token assemblyToken, MetadataFile storage, IMetadataAssemblyImport assemblyImport)
                : base(storage, assemblyImport)
            {
                this.assemblyToken = assemblyToken;
            }

            protected override void Fetch()
            {
                // call first time to get sizes (both for szName and metadata fields)
                m_assemblyImport.GetAssemblyProps(assemblyToken, out m_publicKey, out m_cbPublicKey, out m_hashAlgId,
                    this.m_szName, m_chName, out m_chName, ref m_metadata, out m_flags);
            }
        }

        /// <summary>
        /// Get an assembly name from the metadata for the manifest module
        /// </summary>
        /// <param name="module">the manifest module</param>
        /// <returns>AssemblyName object of the assembly.</returns>
        public static AssemblyName GetAssemblyName(MetadataOnlyModule module)
        {
            Token assemblyToken = MetadataOnlyAssembly.GetAssemblyToken(module);
            Debug.Assert(assemblyToken != Token.Nil, "Can't get name on a module without manifest.");
            Debug.Assert(assemblyToken.TokenType == TokenType.Assembly, "Token type must be for Assembly.");

            IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)module.RawImport;

            AssemblyNameFromDefitionBuilder an = new AssemblyNameFromDefitionBuilder(assemblyToken, module.RawMetadata, assemblyImport);

            AssemblyName assemblyName = an.CalculateName();
            assemblyName.CodeBase = MetadataOnlyAssembly.GetCodeBaseFromManifestModule(module);

            // Set the processor architecture. This is not stored in the manifest so we can't do this for AssemblyRefs.
            // For assemblies built against CLR v1.0 and v1.1, Reflection always reports None as proc architecture.
            if (!HasV1Metadata(module))
            {
                PortableExecutableKinds peKind;
                ImageFileMachine machine;
                module.GetPEKind(out peKind, out machine);
                // Note: explicitly use assembly name flags read from metadata instead ones from AssemblyName
                // since AssemblyName.Flags setter masks some important bits.
                ProcessorArchitecture arch = CalculateProcArchIndex(peKind, machine, an.AssemblyNameFlags);

                assemblyName.ProcessorArchitecture = arch;
            }
            else
            {
                assemblyName.ProcessorArchitecture = ProcessorArchitecture.None;
            }

            return assemblyName;
        }

        /// <summary>
        /// Determines if assembly as built against CLR v1.0 or v1.1.
        /// </summary>
        public static bool HasV1Metadata(MetadataOnlyModule module)
        {
            string versionString = module.GetRuntimeVersion();
            if (versionString.Length >= 2 && versionString[1] == '1')
                return true;
            return false;
        }

        /// <summary>
        /// Derived class to get an assembly name for an assembly Ref token.
        /// </summary>
        private class AssemblyNameFromRefBuilder : AssemblyNameBuilder
        {
            Token assemblyRefToken;

            public AssemblyNameFromRefBuilder(Token assemblyRefToken, MetadataFile storage, IMetadataAssemblyImport assemblyImport)
                : base(storage, assemblyImport)
            {
                if (assemblyRefToken.TokenType != TokenType.AssemblyRef)
                {
                    throw new ArgumentException(Resources.AssemblyRefTokenExpected);
                }

                this.assemblyRefToken = assemblyRefToken;
            }

            protected override void Fetch()
            {
                // Ignored in assembly name.
                UnusedIntPtr pHashValue;
                uint cbHashValue;

                // call first time to get sizes (both for szName and metadata fields)
                m_assemblyImport.GetAssemblyRefProps(assemblyRefToken, out m_publicKey, out m_cbPublicKey,
                    this.m_szName, m_chName, out m_chName,
                    ref m_metadata, out pHashValue, out cbHashValue, out m_flags);
            }
        }

        /// <summary>
        /// Get an AssemblyName for an assembly ref token in the given metadata scope
        /// </summary>
        /// <param name="assemblyRefToken">an assemblyRef token</param>
        /// <param name="assemblyImport">a metadata scope containing the token</param>
        /// <returns>AssemblyName object</returns>
        public static AssemblyName GetAssemblyNameFromRef(Token assemblyRefToken, MetadataOnlyModule module, IMetadataAssemblyImport assemblyImport)
        {
            AssemblyNameFromRefBuilder an = new AssemblyNameFromRefBuilder(assemblyRefToken, module.RawMetadata, assemblyImport);
            AssemblyName assemblyName = an.CalculateName();

            return assemblyName;
        }

        // This is taken directly from Reflector's view of CLR sources.
        // This is used to calculate AssemblyName.ProcessorArchitecture.
        private static ProcessorArchitecture CalculateProcArchIndex(PortableExecutableKinds pek, ImageFileMachine ifm, AssemblyNameFlags flags)
        {
            // 0x70 specifies "reference assembly", a new concept in v4.0. For these, CLR wants to return None as arch so they
            // can be always loaded, regardless of process type. 
            if (((uint)flags & ProcessorArchitectureMask) == ReferenceAssembly)
            {
                return ProcessorArchitecture.None;
            }
            else if ((pek & PortableExecutableKinds.PE32Plus) == PortableExecutableKinds.PE32Plus)
            {
                ImageFileMachine machine = ifm;
                if (machine == ImageFileMachine.I386)
                {
                    if ((pek & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly)
                    {
                        return ProcessorArchitecture.MSIL;
                    }
                }
                else
                {
                    if (machine == ImageFileMachine.IA64)
                    {
                        return ProcessorArchitecture.IA64;
                    }
                    if (machine == ImageFileMachine.AMD64)
                    {
                        return ProcessorArchitecture.Amd64;
                    }
                }
            }
            else if (ifm == ImageFileMachine.I386)
            {
                if (((pek & PortableExecutableKinds.Required32Bit) != PortableExecutableKinds.Required32Bit) && ((pek & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly))
                {
                    return ProcessorArchitecture.MSIL;
                }
                return ProcessorArchitecture.X86;
            }
            return ProcessorArchitecture.None;
        }


    } // end class AssemblyNameHelper
}
