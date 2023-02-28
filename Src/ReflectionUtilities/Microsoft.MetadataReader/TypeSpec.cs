
using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Globalization;
using System.Reflection.Adds;
using System.Diagnostics;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using BindingFlags = System.Reflection.BindingFlags;
using Type = System.Reflection.Mock.Type;
using Binder = System.Reflection.Binder;
using CallingConventions = System.Reflection.CallingConventions;
using ParameterModifier = System.Reflection.ParameterModifier;
using TypeAttributes = System.Reflection.TypeAttributes;
using FieldAttributes = System.Reflection.FieldAttributes;
using MemberTypes = System.Reflection.MemberTypes;
using PropertyAttributes = System.Reflection.PropertyAttributes;
#endif

namespace Microsoft.MetadataReader
{   
        
    /// <summary>
    /// Represent a type object around a TypeSpec token.  
    /// A TypeSpec could become any other type, including a TypeDef, TypeRef, Generic instantation, array, modifier, etc.
    /// TypeSpecs become very common when dealing with generics.
    /// </summary>
    /// Explicitly have a DebuggerDisplay here to display the typespec token and not inherit the debugger display from 
    /// TypeProxy. ToString(), Name and FullName cannot be used in the debugger display since they may cause exceptions when
    /// the type resolution fails.
    [DebuggerDisplay("{m_typeSpecToken}")]
    internal class TypeSpec : TypeProxy, ITypeSpec
    {
        // The type-spec token. This can be used to retrieve the signature blob, which can then be
        // parsed to get a TypeSpec tree.
        // This is valid in the this.m_resolver scope.
        readonly Token m_typeSpecToken;

        // Provides generic type and method args, which can be referred to be the signature.
        readonly GenericContext m_context;

        /// <summary>
        /// Represent a type spec
        /// </summary>
        /// <param name="module">module scope that the token is valid in. </param>
        /// <param name="typeSpecToken">a typespec token in that scope</param>
        /// <param name="typeArgs">the generic type args for resolving vars</param>
        /// <param name="methodArgs">the generic method args for resolving mvars.</param>
        public TypeSpec(MetadataOnlyModule module, Token typeSpecToken, Type[] typeArgs, Type[] methodArgs)
            : base(module)
        {
            Debug.Assert(typeSpecToken.IsType(TokenType.TypeSpec));
            Debug.Assert(module.IsValidToken(typeSpecToken));

            m_typeSpecToken = typeSpecToken;
            m_context = new GenericContext(typeArgs, methodArgs);
        }

        #region ITypeSpec Members

        public Token TypeSpecToken
        {
            get { return m_typeSpecToken;  }
        }

        public byte[] Blob
        {
            get {
                // Get the raw bytes for the signature.
                // Metadata expects valid token; caller must enforce that.

                EmbeddedBlobPointer pSig;
                int cbSig;
                int ret = this.m_resolver.RawImport.GetTypeSpecFromToken(m_typeSpecToken, out pSig, out cbSig);
                Debug.Assert(ret == 0);

                return this.m_resolver.ReadEmbeddedBlob(pSig, cbSig);                
            }
        }
        
        public Module DeclaringScope
        {
            get { return this.Resolver;  }
        }
        #endregion

        protected override Type GetResolvedTypeWorker()
        {
            var sig = this.Blob;
            int index = 0;
            var type = SignatureUtil.ExtractType(sig, ref index, this.Resolver, this.m_context);
            Debug.Assert(index == sig.Length); // read the entire signature.            
            return type;
        }
    } // end class TypeSpec
}