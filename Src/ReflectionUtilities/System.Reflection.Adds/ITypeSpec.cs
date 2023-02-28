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
    /// Represents a TypeSpec in the metadata.
    /// Represents a metadata signature.  
    /// </summary>
    /// <remarks>
    /// Some metadata items return a TypeSpec (such as a Type's base type token)
    /// Others return the signature blob directly (such as a Field's Type).
    /// </remarks>
    internal interface ITypeSpec : ITypeSignatureBlob
    {
        // A TypeSpec token. This is valid in the ITypeSignature.DeclaringScope importer.
        Token TypeSpecToken
        {
            get;
        }
    }


}