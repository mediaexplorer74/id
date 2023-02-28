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
    /// Represents a type signature blob.  
    /// This derives from ITypeProxy to provide a Type object model over the raw byte array.
    /// </summary>
    internal interface ITypeSignatureBlob : ITypeProxy
    {
        /// <summary>
        /// Get the raw bytes for the blob. 
        /// Anything that has a blob also means a signature parser and TypeAlgebra tree. 
        /// </summary>
        byte[] Blob { get; }


        /// <summary>
        /// The scope that the tokens in the blob are valid in.
        /// </summary>
        Module DeclaringScope
        {
            get;
        }
    }

}