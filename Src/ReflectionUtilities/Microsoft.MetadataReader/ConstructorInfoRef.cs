
using Debug = Microsoft.MetadataReader.Internal.Debug;
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
using AssemblyName =  System.Reflection.AssemblyName;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// Simple proxy to just chain back to the declaring type. 
    /// This is used in custom attributes so facilitate the design pattern of:
    ///    Ca.Constructor.DeclaringType.FullName
    /// so that we can get the custom attribute name (and argument types) without resolving.
    /// </summary>
    internal class ConstructorInfoRef : ConstructorInfoProxy
    {
        // The type that this constructor is for.
        readonly Type m_declaringType;

        // The token for this constructor which can be resolved on m_scope to get the real constructorInfo object.
        readonly Token m_token;

        // The scope that m_token is valid in.
        readonly MetadataOnlyModule m_scope;

        public ConstructorInfoRef(Type declaringType, MetadataOnlyModule scope, Token token)
        {
            m_declaringType = declaringType;
            m_token = token;
            m_scope = scope;
        }

        protected override ConstructorInfo GetResolvedWorker()
        {
            MethodBase method = m_scope.ResolveMethod(m_token);
            return (ConstructorInfo)method;
        }

        public override Type DeclaringType
        {
            get { return this.m_declaringType; }
        }

        /// <summary>
        /// Get the parameter information available in the MethodRef without resolving to a def
        /// </summary>
        /// <returns>An array of ParameterInfo objects describing the types (and modifiers) of parameters (but not their
        /// names and other attributes present only in the definition)</returns>
        /// <remarks>
        /// Note that we don't override GetParameters() because it could be a breaking change - omitting information
        /// (tokens, names, in/out, etc.) that the caller may care about.  For custom attribute processing we explicitly
        /// opt-in to using this API.
        /// Note also that this isn't technically the same thing as a method definition signature.  For example, the reference
        /// signature can include precise types where the definition may be a varargs signature.  Also, I'm not sure if 
        /// optional modifiers are required to match exactly.
        /// </remarks>
        public ParameterInfo[] GetSignatureParameters()
        {
            // Note that we avoid caching the signature and get it again as needed, consistent with LMR policy
            // In some cases this may be a little wasteful (we already called GetMemberRefData to create this object)
            Token declDummy;
            string nameDummy;
            SignatureBlob signatureBlob;
            m_scope.GetMemberRefData(m_token, out declDummy, out nameDummy, out signatureBlob);

            // Can't be a generic instantiation
            var tempContext = new GenericContext(null, null);

            var descr = SignatureUtil.ExtractMethodSignature(signatureBlob, m_scope, tempContext);
            Debug.Assert(descr.CallingConvention != CorCallingConvention.Generic);

            ParameterInfo[] parameters = new SimpleParameterInfo[descr.Parameters.Length];
            for(int i = 0; i < descr.Parameters.Length; i++)
            {
                parameters[i] = new SignatureParameterInfo(this, descr.Parameters[i].Type, i, descr.Parameters[i].CustomModifiers);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Parameters created from a methodRef signature
    /// </summary>
    // Re-use SimpleParameterInfo for convenience, but add custom modifiers since we have them.
    // Perhaps it would be better to override other methods and throw rather than return the dummy values?
    internal class SignatureParameterInfo : SimpleParameterInfo
    {
        public SignatureParameterInfo(MemberInfo member, Type paramType, int position, CustomModifiers modifiers)
            : base(member, paramType, position)
        {
            m_modifiers = modifiers;
        }

        public override Type[] GetOptionalCustomModifiers()
        {
            return m_modifiers.OptionalCustomModifiers;
        }

        public override Type[] GetRequiredCustomModifiers()
        {
            return m_modifiers.RequiredCustomModifiers;
        }

        private CustomModifiers m_modifiers;
    }
}