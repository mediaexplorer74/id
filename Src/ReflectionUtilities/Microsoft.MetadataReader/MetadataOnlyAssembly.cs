// LMR Assembly


using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection.Adds;
using Debug = Microsoft.MetadataReader.Internal.Debug;

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
    /// Represent a System.Reflection.Assembly 
    /// </summary>
    internal class MetadataOnlyAssembly : Assembly, IAssembly2, IDisposable
    {
        /// <summary>
        /// m_modules[0] is always the manifest module. It's the only module for single-module 
        /// assemblies. For multi-module assemblies m_modules[1..n] contains netmodules.
        /// Modules have to be of type Module and not MetadataOnlyModule because Module resolver
        /// could return any Module type.
        /// </summary>
        readonly private Module[] m_modules;

        /// <summary>
        /// Same as m_modules[0]. Used to avoid casting in cases when we just need LMR
        /// specific information about manifest module. Manifest module always has to be
        /// of ManifestOnlyModule type.
        /// </summary>
        readonly private MetadataOnlyModule m_manifestModule;

        /// <summary>
        /// The file containing the manifest information for the assembly.
        /// </summary>
        readonly private string m_manifestFile;

        /// <summary>
        /// A profile of running Fib(20) using a naive recursive algorithm showed that caching the
        /// AssemblyName reduced execution speed by 40%. 
        /// </summary>
        readonly private AssemblyName m_name;


        /// <summary>
        /// Creates an instance of a single-module or multi-module assembly.
        /// </summary>
        /// <param name="manifestModule">Module containing manifest for an assembly.</param>
        /// <param name="manifestFile">File containing the manifest information.</param>
        internal MetadataOnlyAssembly(MetadataOnlyModule manifestModule, string manifestFile)
            : this(new MetadataOnlyModule[] { manifestModule }, manifestFile)
        {
        }

        /// <summary>
        /// Creates an instance of a multi-module assembly.
        /// </summary>
        /// <param name="modules">Array of modules that form a multi-module assembly. The first one
        /// must be the manifest module.</param>
        /// <param name="manifestFile">File containing the manifest information.</param>
        internal MetadataOnlyAssembly(MetadataOnlyModule[] modules, string manifestFile)
        {
            Debug.Assert(m_modules == null, "m_modules can be set only once.");

            MetadataOnlyAssembly.VerifyModules(modules);

            // We verified that manifest module is ok - save it.
            m_manifestModule = modules[0];
            m_name = AssemblyNameHelper.GetAssemblyName(m_manifestModule);
            m_manifestFile = manifestFile;

            // Ensure all modules passed in have their Assembly property set properly.
            foreach (MetadataOnlyModule module in modules)
            {
                module.SetContainingAssembly(this);
            }

            // Create temporary list of netmodules (including manifest module). This list will be
            // expanded if there are any netmodules that still need to be resolved.
            List<Module> currentModules = new List<Module>(modules);

            // Extract list of netmodule names from manifest.
            bool getResources = false;
            List<string> netModuleNames = MetadataOnlyAssembly.GetFileNamesFromFilesTable(m_manifestModule, getResources);

            // Load netmodules that are not passed in (if there are any).
            foreach (string netModuleName in netModuleNames)
            {
                if (currentModules.Find(i => i.Name.Equals(netModuleName, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    // Already loaded - skip.
                    continue;
                }
                else
                {
                    // Resolver can return non-LMR modules. These modules can have their own way of
                    // setting assembly property but they need to know what the containing assembly is. 
                    Module newModule = m_manifestModule.AssemblyResolver.ResolveModule(this, netModuleName);

                    if (newModule == null)
                    {
                        throw new InvalidOperationException(Resources.ResolverMustResolveToValidModule);
                    }

                    if (newModule.Assembly != this)
                    {
                        throw new InvalidOperationException(Resources.ResolverMustSetAssemblyProperty);
                    }

                    currentModules.Add(newModule);
                }
            }

            m_modules = currentModules.ToArray();
        }

        #region Disposing
        /// <summary>        
        /// This should only be called in the context of disposing the parent Universe.
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
            if (disposing)
            {
                // free managed resources
                // Dipsose any metadata objects held in this assembly.
                if (m_modules != null)
                {
                    foreach (Module m in m_modules)
                    {
                        IDisposable d = m as IDisposable;
                        if (d != null)
                        {
                            d.Dispose();
                        }
                    }
                }

            }
            // No native resources to free directly.

        }
        #endregion // Disposing



        /// <summary>
        /// Verifies that modules have these properties:
        ///     1) First module contains manifest.
        ///     2) All other modules (if provided) do not contain manifest.
        /// </summary>
        private static void VerifyModules(MetadataOnlyModule[] modules)
        {
            // TODO: it might be beneficial to special-case mscorlib here and skip all the checks.

            // Verify that there is manifest module passed.
            if ((modules == null) || modules.Length < 1)
            {
                throw new ArgumentException(Resources.ManifestModuleMustBeProvided);
            }

            // Verify that first module contains manifest.
            if (MetadataOnlyAssembly.GetAssemblyToken(modules[0]) == Token.Nil)
            {
                throw new ArgumentException(Resources.NoAssemblyManifest);
            }

            // Verify that all other modules (if provided) do not contain manifest.
            for (int i = 1; i < modules.Length; i++)
            {
                if (MetadataOnlyAssembly.GetAssemblyToken(modules[i]) != Token.Nil)
                {
                    throw new ArgumentException(Resources.ExtraAssemblyManifest);
                }
            }

            // All checks passed - verification is complete.
        }

        /// <summary>
        /// Gets list of names of all dependent files from Files table based on manifest module. 
        /// </summary>
        /// <remarks>
        /// The CLI spec is not clear on where exactly netmodules should be listed: in ModuleRefs table or in 
        /// Files table, or both. C# compiler lists them in both places. That's what Serge Lidin's book on IL
        /// Assembler describes as correct. Dynamic modules generated with Reflection.Emit have both of these
        /// tables empty - until dynamic assembly is persisted to disk, when it becomes single module assembly.
        /// The only thing stored in a manifest of a dynamic assembly is information about assembly level
        /// custom attributes.
        /// 
        /// ModuleRefs table is hard to use since, in addition to net modules, it lists native DLL dependencies.
        /// There is no way to distinguish between native and managed binaries. The table only contains names
        /// like mscoree.dll or moduleA.netmodule. ModuleRefs table doesn't list full closure of dependencies 
        /// either. It only lists direct dependencies. E.g. if manifest module has dependency on module A, and 
        /// module A in turn has dependency on module B, ModuleRefs table will only have module A listed.
        /// 
        /// In contrast, Files table lists the full closure of all dependent netmodules, direct and indirect. 
        /// </remarks>
        /// <param name="manifestModule">Module with manifest that needs to be inspected.</param>
        /// <param name="getResources">Specifies whether the result includes resource files.</param>
        private static List<string> GetFileNamesFromFilesTable(MetadataOnlyModule manifestModule, bool getResources)
        {
            HCORENUM hEnum = new HCORENUM();
            int fileTokenValue;
            int size;

            int chName;
            UnusedIntPtr pbHashValue;
            uint cbHashValue;
            CorFileFlags flags;

            List<string> fileNames = new List<string>();
            StringBuilder szName = StringBuilderPool.Get();
            IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)(manifestModule).RawImport;

            try
            {
                while (true)
                {
                    // Get next file token from the Files table.
                    assemblyImport.EnumFiles(ref hEnum, out fileTokenValue, 1, out size);
                    if (size == 0)
                        break;

                    // Get file name based on its token.
                    assemblyImport.GetFileProps(fileTokenValue, null, 0, out chName, out pbHashValue, out cbHashValue, out flags);

                    if (!getResources)
                    {
                        // Skip resource files.
                        if (flags == CorFileFlags.ContainsNoMetaData)
                            continue;
                    }

                    szName.Length = 0;
                    szName.EnsureCapacity(chName);

                    assemblyImport.GetFileProps(fileTokenValue, szName, szName.Capacity, out chName, out pbHashValue, out cbHashValue, out flags);
                    fileNames.Add(szName.ToString());
                }
            }
            finally
            {
                hEnum.Close(assemblyImport);
            }

            StringBuilderPool.Release(ref szName);
            return fileNames;
        }

        public override int GetHashCode()
        {
            // Just use hash of the manifest module. It's highly unlikely that
            // two multi-module assemblies would have the same manifest module hashes.
            return m_modules[0].GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // Can't check for specific type implementation since the assembly may be a proxy.
            // So compare using public properties.
            Assembly otherAssembly = obj as Assembly;
            if (otherAssembly == null)
                return false;

            // If two assmblies have the same manifest module they are the same.
            return this.ManifestModule.Equals(otherAssembly.ManifestModule);
        }

        #region Assembly Members

        /// <summary>
        /// Gets resource stream for a given resource name using type for namespace name.
        /// </summary>
        /// <remarks>We can't currently rely on Reflection to do this part since they
        /// call their internal API from their overload (instead of calling 
        /// GetManifestResourceStream(string name) overload.</remarks>
        public override Stream GetManifestResourceStream(Type type, String name)
        {
            StringBuilder resourceName = StringBuilderPool.Get();
            if (type == null)
            {
                if (name == null)
                {
                    throw new ArgumentNullException("type");
                }
            }
            else
            {
                String nameSpace = type.Namespace;
                if (nameSpace != null)
                {
                    resourceName.Append(nameSpace);
                    if (name != null)
                    {
                        resourceName.Append(Type.Delimiter);
                    }
                }
            }

            if (name != null)
            {
                resourceName.Append(name);
            }

            string text = resourceName.ToString();
            StringBuilderPool.Release(ref resourceName);

            return GetManifestResourceStream(text);
        }

        /// <summary>
        /// Gets resource stream for a given resource name.
        /// </summary>
        public override Stream GetManifestResourceStream(string name)
        {
            IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)(m_manifestModule).RawImport;

            int resourceTokenValue;
            assemblyImport.FindManifestResourceByName(name, out resourceTokenValue);

            Token resourceToken = new Token(resourceTokenValue);
            // If resource doesn't exist we just return null. That's how Reflection works too. 
            if (resourceToken.IsNil)
            {
                return null;
            }

            // We alrady have a name (and length). Use these variables to double check
            // IMDImport returns expected values.
            StringBuilder nameCheck = StringBuilderPool.Get(name.Length + 1);
            int nameLengthCheck;

            int implementationTokenValue;
            uint offset;
            CorManifestResourceFlags flags;

            assemblyImport.GetManifestResourceProps(
                resourceTokenValue,
                nameCheck,
                nameCheck.Capacity,
                out nameLengthCheck,
                out implementationTokenValue,
                out offset,
                out flags);
            Debug.Assert(nameLengthCheck == name.Length + 1, string.Format(CultureInfo.InvariantCulture, "Expected name lengths {0} and {1} to be same.", nameLengthCheck, name.Length + 1));
            Debug.Assert(name.Equals(nameCheck.ToString()), string.Format(CultureInfo.InvariantCulture, "Expected names: {0} and {1} to be same.", name, nameCheck));

            StringBuilderPool.Release(ref nameCheck);

            // TODO: check flags and add security check for Reflection permission

            Token implementationToken = new Token(implementationTokenValue);

            // TODO: can resource be in a netmodule?
            if (implementationToken.TokenType == TokenType.File)
            {
                // When implementation token is Nil, that means resource is in the current file.
                // From CLI spec, Partition II, section 22.24 (ManifestResource):
                //      Implementation can be null or non-null (if null, it means the resource is stored in the current file).
                //
                if (implementationToken.IsNil)
                {
                    byte[] resource = m_manifestModule.RawMetadata.ReadResource(offset);
                    return new MemoryStream(resource);
                }
                else
                {
                    // Get file properties for a file that contains resources.
                    int fileNameLength;
                    UnusedIntPtr ppbHashValue;
                    uint pcbHashValue;
                    CorFileFlags dwFileFlags;
                    assemblyImport.GetFileProps(
                        implementationToken.Value,
                        null,
                        0,
                        out fileNameLength,
                        out ppbHashValue,
                        out pcbHashValue,
                        out dwFileFlags);

                    StringBuilder fileName = StringBuilderPool.Get(fileNameLength);
                    assemblyImport.GetFileProps(
                        implementationToken.Value,
                        fileName,
                        fileName.Capacity,
                        out fileNameLength,
                        out ppbHashValue,
                        out pcbHashValue,
                        out dwFileFlags);

                    string path = Path.GetDirectoryName(this.Location);
                    string fullFileName = Path.Combine(path, fileName.ToString());

                    StringBuilderPool.Release(ref fileName);
                    return new FileStream(fullFileName, FileMode.Open);
                }

            }
            else if (implementationToken.TokenType == TokenType.AssemblyRef)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new ArgumentException(Resources.InvalidMetadata);
            }
        }

        public override string[] GetManifestResourceNames()
        {
            HCORENUM hEnum = new HCORENUM();
            int resourceToken;
            int size;

            int chName;
            int implementationToken;
            uint offset;
            CorManifestResourceFlags flags;

            List<string> resourceNames = new List<string>();
            StringBuilder szName = StringBuilderPool.Get();
            IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)(m_manifestModule).RawImport;

            try
            {
                while (true)
                {
                    // Get next manifest resource token.
                    assemblyImport.EnumManifestResources(ref hEnum, out resourceToken, 1, out size);
                    if (size == 0)
                        break;

                    // Get resource name based on its token.
                    assemblyImport.GetManifestResourceProps(resourceToken, null, 0, out chName, out implementationToken, out offset, out flags);

                    szName.Length = 0;
                    szName.EnsureCapacity(chName);

                    assemblyImport.GetManifestResourceProps(resourceToken, szName, szName.Capacity, out chName, out implementationToken, out offset, out flags);
                    resourceNames.Add(szName.ToString());
                }
            }
            finally
            {
                hEnum.Close(assemblyImport);
            }

            StringBuilderPool.Release(ref szName);

            return resourceNames.ToArray();
        }

        override public AssemblyName GetName()
        {
            // This should get inlined and brought over to Assembly
            // default is copiedName = false
            return m_name;
        }

        override public AssemblyName GetName(bool copiedName)
        {
            // true to set CodeBase to shadow copy; 
            throw new NotImplementedException();
        }

        override public string FullName
        {
            get { return m_name.FullName; }
        }

        override public string Location
        {
            get { return m_manifestFile; }
        }

        override public Type[] GetExportedTypes()
        {
            // Return all visible types.
            Type[] allTypes = this.GetTypes();

            // This could be nicely represented as a linq expression:
            //   return allTypes.Where(t => t.IsVisible).ToArray();
            // once we can compile against Orcas.
            List<Type> list = new List<Type>();
            foreach (var t in allTypes)
            {
                if (t.IsVisible)
                    list.Add(t);
            }
            return list.ToArray();

        }

        override public Type GetType(string name)
        {
            return this.GetType(name, false, false);
        }

        override public Type GetType(string name, bool throwOnError)
        {
            return this.GetType(name, throwOnError, false);
        }

        public override Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            Type match = null;

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            // Check all modules. We don't want to throw on
            // no match found since we have other modules to inspect.
            for (int i = 0; i < m_modules.Length; i++)
            {
                match = m_modules[i].GetType(name, false, ignoreCase);
                if (match != null)
                {
                    return match;
                }
            }

            // If no match found, check for policy about type forwarding.            
            Type t = m_manifestModule.Policy.TryTypeForwardResolution(this, name, ignoreCase);
            if (t != null)
            {
                return t;
            }

            // No match found.
            if (throwOnError)
            {
                throw new TypeLoadException(String.Format(
                    CultureInfo.InvariantCulture, Resources.CannotFindTypeInModule, name, m_modules[0].ScopeName));
            }

            return null;
        }

        /// <summary>
        /// Gets all types on the assembly. 
        /// </summary>
        override public Type[] GetTypes()
        {
            List<Type> types = new List<Type>();
            foreach (Module module in m_modules)
            {
                types.AddRange(module.GetTypes());
            }

            return types.ToArray();
        }

        /// <summary>
        /// Gets module with the specified name. Returns null if there is no such module.
        /// </summary>
        public override Module GetModule(string name)
        {
            foreach (Module module in m_modules)
            {
                // TODO: check if Reflection uses Culture-aware comparison.
                if (module.ScopeName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return module;
                }
            }

            // No matches found.
            return null;
        }

        public override Module[] GetModules(bool getResourceModules)
        {
            return m_modules;
        }

        public override Module[] GetLoadedModules(bool getResourceModules)
        {
            return m_modules;
        }

        public override Module ManifestModule
        {
            get
            {
                return m_modules[0];
            }
        }

        internal MetadataOnlyModule ManifestModuleInternal
        {
            get
            {
                return m_manifestModule;
            }
        }

        public override string CodeBase 
        {
            get 
            {
                return GetCodeBaseFromManifestModule(m_manifestModule);
            }
        }

        /// <summary>
        /// The method returns a string representing the CodeBase property
        /// of an Assembly or AssemblyName from the manifest module.
        /// </summary>
        internal static string GetCodeBaseFromManifestModule(MetadataOnlyModule manifestModule) 
        {
            string modulePath = manifestModule.FullyQualifiedName;
            
            if (!Utility.IsValidPath(modulePath)) 
            {
                // The modulePath could be empty if the assembly is loaded from binary data
                // Reflection returns the caller's assembly's CodeBase in that case. 
                // Refer to DevDiv bug #627801. 
                return String.Empty;
            } 
            else 
            {
                // Need to format the module path to match the result in the Reflection.
                try
                {
                    return new Uri(modulePath).ToString();
                }
                catch (Exception e)
                {
                    Debug.Assert(false, "Unexpected exception thrown by Uri code: " + e.Message);
                    throw;
                }
            }
        }

        override public MethodInfo EntryPoint
        {
            get
            {
                // Entry point is stored in the IMAGE_COR20_HEADER outside of the metadata.
                var m = this.m_manifestModule.RawMetadata;
                                
                Token t = m.ReadEntryPointToken();
                if (t.IsNil)
                {
                    // No entry point token. Common for dlls.
                    return null;
                }
                // See Ecma II 24.3.3.2 for details. This can be a MethodDEf or a File token. 
                switch(t.TokenType)
                {
                    case TokenType.FieldDef:
                        // Haven't implemented the file case.
                        throw new NotImplementedException();
                    case TokenType.MethodDef:
                        {
                            // Token type should be a MethodDef to a MethodInfo (not a ctor).
                            Debug.Assert(t.IsType(TokenType.MethodDef));

                            MethodBase method = this.ManifestModule.ResolveMethod(t.Value);
                            Debug.Assert(method != null);

                            return (MethodInfo)method;
                        }
                    default:
                        throw new InvalidOperationException(Resources.InvalidMetadata);            
                }                
            }
        }

        public override string ImageRuntimeVersion
        {
            get
            {
                return m_manifestModule.GetRuntimeVersion();
            }
        }

        #endregion // region Assembly Members


        /// <summary>
        /// Gets the assembly token for a module. If module contains manifest, it returns
        /// a valid assembly token; otherwise returns Token.Nil.
        /// </summary>
        internal static Token GetAssemblyToken(MetadataOnlyModule module)
        {
            // This only works for the manifest module. 
            // This will often be 0x20000001.
            int token;
            int hResult = ((IMetadataAssemblyImport)module.RawImport).GetAssemblyFromScope(out token);

            if (hResult == 0)
            {
                return new Token(token);
            }
            else
            {
                return Token.Nil;
            }
        }

        
        

        public override FileStream[] GetFiles(bool getResourceModules)
        {
            List<string> filenames = new List<string>();
            //Return all the module files in the assembly.
            foreach (Module m in m_modules)
            {
                filenames.Add(m.FullyQualifiedName);
            }
            if (getResourceModules)
            {
                //get all the resource files.
                string directory = Path.GetDirectoryName(m_manifestFile);

                foreach (string filename in MetadataOnlyAssembly.GetFileNamesFromFilesTable(m_manifestModule, true))
                {
                    //The filename in the metadata doesn't contain the fullpath.
                    //Assume that the files in the assembly are in the same directory as the manifest file.
                    filenames.Add(Path.Combine(directory, filename));
                }
            }
            return ConvertFileNamesToStreams(filenames.ToArray());
        }

        public override FileStream GetFile(string name)
        {
            Module m = GetModule(name);
            if (m == null)
                return null;

            return new FileStream(m.FullyQualifiedName,
                                  FileMode.Open,
                                  FileAccess.Read, FileShare.Read);
        }

        static private FileStream[] ConvertFileNamesToStreams(string[] filenames)
        {
            return Array.ConvertAll<string, FileStream>(filenames, n => new FileStream(n, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return m_manifestModule.GetCustomAttributeData(MetadataOnlyAssembly.GetAssemblyToken(m_manifestModule));
        }

        public override AssemblyName[] GetReferencedAssemblies()
        {
            // Get the list of assemblies that this assembly references.
            // The references are just stored in the metadata. The TypeUniverse (fusion) is responsible 
            // for binding the assembly names to actual assemblies. 
            // See IIB.21.5 for more information about the Assembly Reference table.
            // Traversing the list of references should absolutely not cause any references to be loaded.

            IMetadataAssemblyImport import = (IMetadataAssemblyImport)this.m_manifestModule.RawImport;
            List<AssemblyName> list = new List<AssemblyName>();
            HCORENUM hEnum = new HCORENUM();

            try
            {
                while (true)
                {
                    Token token;
                    int count;
                    int res = import.EnumAssemblyRefs(ref hEnum, out token, 1, out count);
                    Marshal.ThrowExceptionForHR(res);

                    if (count == 0)
                    {
                        Debug.Assert(res == 0x1); // S_FALSE
                        break;
                    }

                    Debug.Assert(res == 0); // S_OK

                    var name = AssemblyNameHelper.GetAssemblyNameFromRef(token, this.m_manifestModule, import);
                    list.Add(name);
                }
            }
            finally
            {
                hEnum.Close(import);                
            }

            return list.ToArray();
        }

        #region IAssembly2 Members

        public ITypeUniverse TypeUniverse
        {
            get
            {
                return m_manifestModule.AssemblyResolver;
            }
        }

        #endregion
    }
}
