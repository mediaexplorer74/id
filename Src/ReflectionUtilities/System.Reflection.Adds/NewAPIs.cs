// Prototype new APIs that could be added to Reflection.
// 



namespace System.Reflection.Adds
{
    using Microsoft.MetadataReader.Internal;

#if USE_CLR_V4
    using System.Reflection;
    using Type = System.Type;
#else
    using System.Reflection.Mock;

#endif

    /// <summary>
    /// New APIs on System.Reflection.Assembly
    /// </summary>
    internal interface IAssembly2
    {
        // Get the type universe that this assembly is in.
        // All Assemblies (and Types) and their transitive closure are contained in a single universe.
        // This should never return null. 
        ITypeUniverse TypeUniverse { get; }
    }

    /// <summary>
    /// New APIs on System.Reflection.FieldInfo
    /// </summary>
    internal interface IFieldInfo2
    {
        /// <summary>
        /// Read the raw bytes for an Rva based field
        /// </summary>
        /// <returns>byte array of contents</returns>
        /// <remarks>See Ecma IIa.15.3.2 for more details. 
        /// A common case for RVA fields is array literal initialization. </remarks>
        byte[] GetRvaField();
    }

    /// <summary>
    /// New APIs on System.Reflection.Module
    /// </summary>
    internal interface IModule2
    {
        /// <summary>
        /// Gets number of rows in a metadata table.
        /// </summary>
        /// <param name="metadataTableIndex">Metadata table index.</param>
        /// <returns>Number of rows in the specified metadata table.</returns>
        int RowCount(MetadataTable metadataTableIndex);


        /// <summary>
        /// Get the assembly name for the given assembly token in the metadata scope.
        /// </summary>
        /// <param name="token">assembly ref token valid in this module's scope.</param>
        /// <returns>Assembly name containing information from the metadata scope for the given assembly token.</returns>
        /// <remarks>An AssemblyName object represents the metadata stored in an assembly ref token. 
        /// This is similar to Assembly.GetReferencedAssemblies(), except that this provides information at the module
        /// granularity and it can map a specific assembly ref token.</remarks>
        AssemblyName GetAssemblyNameFromAssemblyRef(Token token);
    }


    // These should become new APIs (or even extension methods).
    internal static class Helpers
    {
        /// <summary>
        /// Get the type universe from a type.
        /// </summary>
        /// <param name="type">type to get universe.</param>
        /// <returns>Returns null if type is not in a universe (such as with refleciton types) 
        /// For ITypeProxy, get universe without resolving. </returns>
        public static ITypeUniverse Universe(Type type)
        {
            // If it's a type proxy (including type refs), get the universe via the type proxy interface
            // so that we don't accidentally resolve.
            ITypeProxy proxy = type as ITypeProxy;
            if (proxy != null)
            {
                return proxy.TypeUniverse;
            }
            
            // Not a proxy, we can safely get the assembly and resolve.
            Assembly a = type.Assembly;
            
            var ia2 = a as IAssembly2;
            if (ia2 == null)
            {
                // Eventually, all types should have a universe. For CLR runtime types, we can infer the 
                // universe based off the appdomain.
                return null;
            }

            return ia2.TypeUniverse;
        }


        /// <summary>
        /// Return a resolved version of the type, if applicable.
        /// </summary>
        /// <param name="type">type to ensure resolved</param>
        /// <returns>a resolved version of the type</returns>
        /// <remarks>LMR's deferred resolution is directly at odds with Reflection's eager validation.
        /// This can be used to resolve a type and force validation to occur to get reflection error semantics.
        /// </remarks>
        public static Type EnsureResolve(Type type)
        {
            while (true)
            {
                var proxy = type as ITypeProxy;
                if (proxy == null)
                    break;

                type = proxy.GetResolvedType();
            }
            return type;
        }
    }
}
