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
    /// A Type Universe that explicitly provides convenience methods for easily loading metadata 
    /// files with LMR. This is just a trivial wrapper around a LMR Loader object, and then any interesting 
    /// logic should be in the Loader object so that other universe implementations can easily pick it up.
    /// Non-LMR modules can still be loaded into the universe. 
    /// </summary> // add XML comments....
    public class DefaultUniverse : SimpleUniverse
    {
        // The loader, which handles shared state (factory, dispenser), and provides convenience
        // operators for loading modules.
        Loader m_loader;

        public DefaultUniverse()
        {
            this.m_loader = new Loader(this);
        }

        // Implements ITypeUniverse
        // Provide a default implementation of module resolution which just looks at the file.
        public override Module ResolveModule(Assembly containingAssembly, string moduleName)
        {
            return this.Loader.ResolveModule(containingAssembly, moduleName);
        }

        /// <summary>
        /// The underlying Loader object. 
        /// </summary>
        public Loader Loader
        {
            get { return this.m_loader; }
        }


        #region Loader Wrappers
        // These should all just be forwarders to the loader.
        // See the loader object for moree detailed comments.
       
        
        public Assembly LoadAssemblyFromFile(string manifestFileName, string[] netModuleFileNames)
        {
            return this.Loader.LoadAssemblyFromFile(manifestFileName, netModuleFileNames);
        }

        public Assembly LoadAssemblyFromFile(string manifestFileName)
        {
            return this.Loader.LoadAssemblyFromFile(manifestFileName);
        }

        public MetadataOnlyModule LoadModuleFromFile(string netModulePath)
        {
            return this.Loader.LoadModuleFromFile(netModulePath);
        }
                
        internal Assembly LoadAssemblyFromByteArray(byte[] data)
        {
            return this.Loader.LoadAssemblyFromByteArray(data);
        }
        #endregion // Loader Wrappers
    }



}