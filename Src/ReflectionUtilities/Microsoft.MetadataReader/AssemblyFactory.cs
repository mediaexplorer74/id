// Factory for creating LMR assemblies


using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection.Adds;
using Debug=Microsoft.MetadataReader.Internal.Debug;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using BindingFlags = System.Reflection.BindingFlags;
using Type = System.Reflection.Mock.Type;
using AssemblyName = System.Reflection.AssemblyName;
using AssemblyNameFlags = System.Reflection.AssemblyNameFlags;
#endif


namespace Microsoft.MetadataReader
{
    /// <summary>
    /// Public factory to allow callers to create an Assembly implementation without needing to expose the assembly
    /// implementation directly. 
    /// This is the most basic building block for allocation. Caller is reponsible for:
    /// - getting the MetadataFile (IMetadataImport) in the first place. 
    /// - updating the Universe with the new assembly. 
    /// MetadataFileLoader class layers on top of this to provide better universe integration.
    /// </summary>
    internal static class AssemblyFactory
    {
        /// <summary>
        /// Pass in TokenResolver (module) so that the caller can create a derived instance.
        /// This only supports creating single-module assemblies.
        /// </summary>
        public static Assembly CreateAssembly(MetadataOnlyModule manifestModule, string manifestFile)
        {
            MetadataOnlyAssembly a = new MetadataOnlyAssembly(manifestModule, manifestFile);

            return a;
        }

        /// <summary>
        /// Create a single-module assembly around the metadata importer.
        /// </summary>
        /// <param name="typeUniverse">Type universe in which types are resolved.</param>
        /// <param name="metadataImport">IMetadataImport representing single-module assembly.</param>
        /// <returns>Assembly object representing single-module assembly.</returns>
        public static Assembly CreateAssembly(ITypeUniverse typeUniverse, MetadataFile metadataImport, string manifestFile)
        {
            return CreateAssembly(typeUniverse, metadataImport, new DefaultFactory(), manifestFile);
        }

        /// <summary>
        /// Overload taking an IReflection factory
        /// </summary>
        /// <param name="typeUniverse">Type universe in which types are resolved.</param>
        /// <param name="metadataImport">IMetadataImport representing single-module assembly.</param>
        /// <param name="factory">reflection factory to use in assembly</param>
        /// <returns>Assembly object representing single-module assembly.</returns>
        public static Assembly CreateAssembly(ITypeUniverse typeUniverse, MetadataFile metadataImport, IReflectionFactory factory, string manifestFile)
        {
            return CreateAssembly(typeUniverse, metadataImport, null, factory, manifestFile, null);
        }

        /// <summary>
        /// Create a multi-module assembly around the metadata importer.
        /// </summary>
        /// <param name="typeUniverse">Type universe in which types are resolved.</param>
        /// <param name="manifestModuleImport">IMetadataImport representing module with manifest.</param>
        /// <param name="netModuleImports">Array of IMetadataImports representing netmodules. 
        /// Can be null or zero lenght in which case created assembly is a single-module assembly.</param>
        /// <returns>Assembly object representing multi-module assembly.</returns>
        public static Assembly CreateAssembly(
            ITypeUniverse typeUniverse,
            MetadataFile manifestModuleImport,
            MetadataFile[] netModuleImports,
            string manifestFile,
            string[] netModuleFiles)
        {
            return CreateAssembly(typeUniverse, manifestModuleImport, netModuleImports, new DefaultFactory(), manifestFile, netModuleFiles);
        }

        /// <summary>
        /// Create a multi-module assembly around the metadata importer.
        /// </summary>
        /// <param name="typeUniverse">Type universe in which types are resolved.</param>
        /// <param name="manifestModuleImport">IMetadataImport representing module with manifest.</param>
        /// <param name="netModuleImports">Array of IMetadataImports representing netmodules. 
        /// Can be null or zero lenght in which case created assembly is a single-module assembly.</param>
        /// <param name="factory">reflection factory to use in assembly</param>
        /// <returns>Assembly object representing multi-module assembly.</returns>
        public static Assembly CreateAssembly(
            ITypeUniverse typeUniverse,
            MetadataFile manifestModuleImport,
            MetadataFile[] netModuleImports,
            IReflectionFactory factory,
            string manifestFile,
            string[] netModuleFiles)
        {
            int numberOfModules = 1;
            if (netModuleImports != null)
            {
                numberOfModules += netModuleImports.Length;
            }

            MetadataOnlyModule[] modules = new MetadataOnlyModule[numberOfModules];

            MetadataOnlyModule manifestModule = new MetadataOnlyModule(typeUniverse, manifestModuleImport, factory, manifestFile);
            modules[0] = manifestModule;

            if (numberOfModules > 1)
            {
                for (int i = 0; i < netModuleImports.Length; i++)
                {
                    modules[i + 1] = new MetadataOnlyModule(typeUniverse, netModuleImports[i], factory, netModuleFiles[i]);
                }
            }

            Assembly a = new MetadataOnlyAssembly(modules, manifestFile);
            return a;
        }
    }

    #region AssemblyName support
    /// <summary>
    /// Class for test method used by Global DTar feature. LMR exposes this helper function to decrease
    /// coupling with DTar.
    /// </summary>
    internal static class CommonIdeHelper
    {
        /// <summary>
        /// Get an AssemblyName object for the given assembly at the specified path. This will crack the metadata.
        /// </summary>
        /// <param name="path">full path to local file assembly. File must exist</param>
        /// <returns>AssemblyName object parsed from assembly's metadata</returns>
        /// <remarks>Other dlls (GDTar) may depend on this signature, so be wary of changing it.</remarks>
        public static AssemblyName GetNameFromPath(string path)
        {
            // We just want to crack the assembly name in the metadata. We don't need to persist the actual
            // Assembly object.
            var e = new EmptyUniverse();
            MetadataFile file = new MetadataDispenser().OpenFile(path);
            Assembly a = AssemblyFactory.CreateAssembly(e, file, path);
            return a.GetName();
        }

        class EmptyUniverse : ITypeUniverse
        {
            #region ITypeUniverse Members

            public Type GetBuiltInType(CorElementType elementType)
            {
                throw new NotImplementedException();
            }

            public Type GetTypeXFromName(string fullName)
            {
                throw new NotImplementedException();
            }

            public Assembly GetSystemAssembly()
            {
                throw new NotImplementedException();
            }

            public Assembly ResolveAssembly(AssemblyName name)
            {
                throw new NotImplementedException();
            }

            public Assembly ResolveAssembly(AssemblyName name, bool throwOnError)
            {
                throw new NotImplementedException();
            }

            public Assembly ResolveAssembly(Module scope, Token tokenAssemblyRef)
            {
                throw new NotImplementedException();
            }

            public bool WouldResolveToAssembly(AssemblyName name, Assembly assembly)
            {
                throw new NotImplementedException();
            }

            public Module ResolveModule(Assembly containingAssembly, string moduleName)
            {
                throw new NotImplementedException();
            }

            public Type ResolveWindowsRuntimeType(string typeName, bool throwOnError, bool ignoreCase)
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
    
    #endregion
}