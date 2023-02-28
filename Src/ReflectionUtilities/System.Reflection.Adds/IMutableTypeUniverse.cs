

namespace System.Reflection.Adds
{
#if USE_CLR_V4
    using System.Reflection;
    using Type = System.Type;
#else
    using System.Reflection.Mock;
#endif

    /// <summary>
    /// Mutable universe extends a ITypeUniverse with operations to actually load assemblies into the universe.
    /// </summary>
    public interface IMutableTypeUniverse : ITypeUniverse
    {
        /// <summary>
        /// Register the assembly as being loaded into this universe
        /// </summary>
        /// <param name="assembly">An assembly that was created to be in this universe. It should implement IAssembly2
        /// and have its TypeUniverse set to this instance.</param>
        void AddAssembly(Assembly assembly);

        // Could add unload, delta count stuff here...
    }

}
