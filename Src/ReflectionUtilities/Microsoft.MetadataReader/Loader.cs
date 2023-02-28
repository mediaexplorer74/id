using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection.Adds;
using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.IO;

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
    /// Helpers for loading LMR assemblies into a universe.
    /// This providers various friendly Load() overloads.
    /// </summary>
    /// <remarks>Whereas the AssemblyFactory class instantiates LMR objects from low-level IMetadataImports, 
    /// this Loader creates the the IMDIs from things like filenames or byte arrays. 
    /// The Loader also updates the containing universe to provide a consistent model.
    /// The Loader can also share state across creation calls, such as a shared dispenser or configuration options
    /// (such as a LMR factory).
    /// The specific signatures on the loader are somewhat arbitrary with the main goal of providing useful convenience
    /// methods for loading LMR assemblies into a mutable universe.
    /// </remarks>
    public class Loader
    {
        // The universe that this loader is applying to.
        readonly private IMutableTypeUniverse m_universe;

        // Share a dispenser across all loads. 
        readonly MetadataDispenser m_dispenser = new MetadataDispenser();

        // Factory that modules are created with. Uses DefaultFactory instead of null.
        private IReflectionFactory m_factory;

        /// <summary>
        /// Creates a metadata loader object and associate it with a universe.
        /// </summary>
        /// <param name="universe">The universe that all loaded assemblies will be loaded into.</param>
        public Loader(IMutableTypeUniverse universe)
        {
            m_universe = universe;
        }

        public CorOpenFlags OpenFlags
        {
            get { return m_dispenser.OpenFlags; }
            set { m_dispenser.OpenFlags = value; }
        }

        /// <summary>
        /// Gets or sets the LMR Factory object associated with new modules.
        /// This will use a default factory rather than return null. 
        /// A factory can be shared across multiple module instances. 
        /// </summary>
        public IReflectionFactory Factory
        {
            get
            {
                if (m_factory == null)
                {
                    m_factory = new DefaultFactory(); 
                }
                return m_factory; 
            }
            set
            {                
                this.m_factory = value;                
            }
        }


        private MetadataFile OpenMetadataFile(string filename)
        {
            return m_dispenser.OpenFileAsFileMapping(filename);
        }

        #region Various Load overloads

        //
        // Various load functions
        // Clients can always inject an assembly directly themselves. 
        //

        /// <summary>
        /// Load an assembly at the given filename
        /// </summary>
        /// <param name="file">filename of assembly to load into the universe</param>
        /// <returns>an assembly for the given file</returns>
        public Assembly LoadAssemblyFromFile(string file)
        {
            var md = OpenMetadataFile(file);

            // The file argument sometimes only contains the file name without path information,
            // need to pass the full path of the file to create the assembly.
            Assembly a = AssemblyFactory.CreateAssembly(m_universe, md, this.Factory, md.FilePath);

            m_universe.AddAssembly(a);
            return a;
        }
                
        /// <summary>
        /// Load a multi-module assembly, explicitly specifying all modules.
        /// </summary>
        /// <param name="manifestFile">the filename to the manifest module</param>
        /// <param name="netModuleFiles">the filenames for the rest of the modules</param>
        /// <returns>An assembly containing all the modules.</returns>
        public Assembly LoadAssemblyFromFile(string manifestFile, string[] netModuleFiles)
        {
            // load the raw metadata for the filenames.
            MetadataFile manifestModuleImport = OpenMetadataFile(manifestFile);

            MetadataFile[] netModuleImports = null;
            if ((netModuleFiles != null) && (netModuleFiles.Length > 0))
            {
                netModuleImports = new MetadataFile[netModuleFiles.Length];
                for (int i = 0; i < netModuleFiles.Length; i++)
                {
                    netModuleImports[i] = OpenMetadataFile(netModuleFiles[i]);
                }
            }

            // Create a LMR object around the raw metadata.
            // The manifestFile argument sometimes only contains the file name without path information,
            // need to pass the full path of the file to create the assembly.
            Assembly assembly = AssemblyFactory.CreateAssembly(this.m_universe, manifestModuleImport, netModuleImports, this.Factory, manifestModuleImport.FilePath, netModuleFiles);
            m_universe.AddAssembly(assembly);
            return assembly;
        }

        /// <summary>
        /// Open assembly from a byte-array containing the same contents as the file.
        /// This is similar to Assembly.Load(byte[]). 
        /// </summary>
        /// <param name="data">raw byte array. This starts at the 'mz' signature not the 'bsjb' signature. </param>
        /// <returns>assembly instantiated around the byte array.</returns>
        public Assembly LoadAssemblyFromByteArray(byte[] data)
        {
            Debug.Assert(data != null);

            var m = m_dispenser.OpenFromByteArray(data);

            Assembly assembly = AssemblyFactory.CreateAssembly(this.m_universe, m, this.Factory, String.Empty);
            this.m_universe.AddAssembly(assembly);
            return assembly;
        }


        /// <summary>
        /// Load just the module (without the assembly) given the filename.
        /// An isolated module may not include the manifest and so certain operations may not be valid.
        /// For general use, use LoadAssembly() instead of LoadModule().
        /// </summary>
        /// <param name="moduleFileName">filename to the module</param>
        /// <returns>A module which may not have a manifest. Some operations may be inavlid.</returns>
        public MetadataOnlyModule LoadModuleFromFile(string moduleFileName)
        {
            // Create a scope around a given file.
            MetadataFile moduleFile = m_dispenser.OpenFileAsFileMapping(moduleFileName);

            return new MetadataOnlyModule(this.m_universe, moduleFile, this.Factory, moduleFileName);
        }

        /// <summary>
        /// Load a module with the given name as part of a mulit-module assembly
        /// </summary>
        /// <param name="containingAssembly">the assembly contaning the module</param>
        /// <param name="moduleName">the modules name from the metadata as recorded in the containing assembly's module table.</param>
        /// <returns>a module in the given assembly</returns>
        /// <remarks>This uses a default policy of looking for the module on disk next to the containing assembly.</remarks>
        public Module ResolveModule(Assembly containingAssembly, string moduleName)
        {
            if ((containingAssembly == null) || string.IsNullOrEmpty(containingAssembly.Location))
            {
                throw new ArgumentException("manifestModule needs to be associated with an assembly with valid location");
            }

            string assemblyFolder = Path.GetDirectoryName(containingAssembly.Location);
            string moduleLocation = Path.Combine(assemblyFolder, moduleName);

            // Create a scope around a given file.
            MetadataFile moduleFile = m_dispenser.OpenFileAsFileMapping(moduleLocation);

            var module = new MetadataOnlyModule(this.m_universe, moduleFile, this.Factory, moduleLocation);

            module.SetContainingAssembly(containingAssembly);

            return module;
        }

        #endregion // Various Load overloads
    } // end class Loader

}
