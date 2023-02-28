// Factory for instantiating LMR objects


using Microsoft.MetadataReader;
using System.Reflection.Adds;
using Debug=Microsoft.MetadataReader.Internal.Debug;

#if USE_CLR_V4
using System.Reflection;  
using Type = System.Type;
#else
using System.Reflection.Mock;
using Type = System.Reflection.Mock.Type;
#endif


namespace Microsoft.MetadataReader
{    
    /// <summary>
    /// Factory object supplied to LMR, allow creation of custom derived objects.
    /// See code:DefaultFactory
    /// </summary>
    public interface IReflectionFactory
    {
        // Create for a TypeDef token
        MetadataOnlyCommonType CreateSimpleType(MetadataOnlyModule scope, Token tokenTypeDef);

        // Create for a TypeDef token with type arguments
        MetadataOnlyCommonType CreateGenericType(MetadataOnlyModule scope, Token tokenTypeDef, Type[] typeArgs);

        // Create a multidimensional array type.
        // This gets invoked from Type.MakeArrayType(int rank)
        // The CLR makes a distinction between vectors (that is, one-dimensional 
        // arrays that are always zero-based) and multidimensional arrays. 
        // A vector, which always has only one dimension, is not the same
        // as a multidimensional array that happens to have only one dimension.
        // You cannot use this method overload to create a vector type; if rank
        // is 1, this method overload returns a multidimensional array type that
        // happens to have one dimension.
        MetadataOnlyCommonType CreateArrayType(MetadataOnlyCommonType elementType, int rank);

        // Create a vector type.
        // This gets invoked from Type.MakeArrayType()        
        MetadataOnlyCommonType CreateVectorType(MetadataOnlyCommonType elementType);

        // Create a modifier. T-->T&
        // This gets invoked from Type.MakeByRefType() 
        MetadataOnlyCommonType CreateByRefType(MetadataOnlyCommonType type);

        // Create a modifier T --> T*
        // This gets invoked from Type.MakePointerType()        
        MetadataOnlyCommonType CreatePointerType(MetadataOnlyCommonType type);

        /// <summary>
        /// Creates a type variable.
        /// </summary>
        MetadataOnlyTypeVariable CreateTypeVariable(MetadataOnlyModule resolver, Token typeVariableToken);

        // Create a field. 
        MetadataOnlyFieldInfo CreateField(MetadataOnlyModule resolver, Token fieldDefToken, Type[] typeArgs, Type[] methodArgs);

        // Create a propertyInfo. 
        // propToken is the PropertyDef token in the scope of the resolver module. 
        MetadataOnlyPropertyInfo CreatePropertyInfo(MetadataOnlyModule resolver, Token propToken, Type[] typeArgs, Type[] methodArgs);

        MetadataOnlyEventInfo CreateEventInfo(MetadataOnlyModule resolver, Token eventToken, Type[] typeArgs, Type[] methodArgs);


        /// <summary>
        /// Hook creating a MethodInfo or ConstructorInfo based on methodDef token. This does not work for ref tokens.
        /// </summary>
        /// <param name="resolver">module that the token is scoped to</param>
        /// <param name="methodToken">a methodDef token for a Constructor or methodInfo.</param>
        /// <param name="typeArgs">type arguments for a generic method. May be null or 0-length. </param>
        /// <param name="methodArgs">method arguments for a generic method. May be null or 0-length. </param>
        /// <returns>a MethodBase</returns>
        /// <remarks>Normally, you need to access the metadata to determine whether a token represents a Method or a Constructor.
        /// The metadata access can be expensive, and using a token allows a factory to do efficient caching on results without having 
        /// to access the metadata.</remarks>
        MethodBase CreateMethodOrConstructor(MetadataOnlyModule resolver, Token methodToken, Type[] typeArgs, Type[] methodArgs);


        /// <summary>
        /// Allow creating an IL method body for the given method. A method may not have a method body (such
        /// as a pinvoke). So this has 3 states:
        /// 1. If the factory does not hook, return false. Ignore body parameter. 
        /// 2. If the factory does hook, return true
        ///     2a. and there is no method body, set body=null.
        ///     2b. if there is a method body, set body= created instance of the body.
        ///  
        /// The factory can use the code:LMRMethodBody to help implement the method body.
        /// </summary>
        /// <param name="method">method to create the body for. </param>
        /// <param name="body">null or newly created method body.</param>
        /// <returns>true if the body is valid, else false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        bool TryCreateMethodBody(MetadataOnlyMethodInfo method, ref MethodBody body);        

        
                
        /// <summary>
        /// Create for a TypeRef token.
        /// - could implementing caching for TypeRefs
        /// - could eagerly resolve Types (so return type may not implement ITypeReference)
        /// - could return an ITypeReference with an arbitrary resolution policy)
        /// 
        /// Since a TypeRef can resolve to a non-LMR type, the return type here must be
        /// System.Type instead of a LMR-specific type.
        /// </summary>
        /// <param name="scope">the module that the token is valid in </param>
        /// <param name="tokenTypeRef">a typeRef token within the module</param>
        /// <returns>a Type object corresponding to the typeref token. The factory may eagerly resolve the token, 
        /// or return a proxy object that does deferred resolution. </returns>
        Type CreateTypeRef(MetadataOnlyModule scope, Token tokenTypeRef);

        /// <summary>
        /// Create for a TypeRef token that occured in a signature.
        /// - could implementing caching for TypeRefs
        /// - could eagerly resolve Types (so return type may not implement ITypeReference)
        /// - could return an ITypeReference with an arbitrary resolution policy)
        /// 
        /// Since a TypeRef can resolve to a non-LMR type, the return type here must be
        /// System.Type instead of a LMR-specific type.
        /// </summary>
        /// <param name="scope">the module that the token is valid in </param>
        /// <param name="tokenTypeRef">a typeRef token within the module</param>
        /// <param name="elementType">the element type used for this token</param>
        /// <returns>a Type object corresponding to the typeref token. The factory may eagerly resolve the token, 
        /// or return a proxy object that does deferred resolution. </returns>
        Type CreateSignatureTypeRef(MetadataOnlyModule scope, Token tokenTypeRef, CorElementType elementType);

        /// <summary>
        /// Create for a TypeSpec token. This is similar to a TypeRef that it can create a proxy type.
        /// </summary>
        /// <param name="module">module scope that the token is valid in. </param>
        /// <param name="typeSpecToken">a typespec token in that scope</param>
        /// <param name="typeArgs">the generic type args for resolving vars</param>
        /// <param name="methodArgs">the generic method args for resolving mvars.</param>
        Type CreateTypeSpec(MetadataOnlyModule scope, Token tokenTypeRef, Type[] typeArgs, Type[] methodArgs);
    }
}
