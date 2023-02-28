using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace System.Reflection.Adds
{
#if USE_CLR_V4
    using System.Reflection;
#else
    using System.Reflection.Mock;
#endif

    /// <summary>
    /// Represents a type that can resolve to another type. 
    /// This can commonly handle TypeRef and TypeSpecs in the metadata.
    /// </summary>
    internal interface ITypeProxy
    {
        // Get the Type that this resolves to. 
        // For a TypeRef, this may invoke the resolver callbacks, which could imply
        // arbitrary policy from resolving, to loading new assemblies into the TypeUniverse, to requesting
        // information from the User. This can also return a "dummy" type. 
        //
        // This should throw instead of returning null to indicate an error. 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        Type GetResolvedType();


        /// <summary>
        /// Get the type universe that this type is in. A type and its resolved type are in the same universe.
        /// </summary>
        ITypeUniverse TypeUniverse { get; }
    }

}