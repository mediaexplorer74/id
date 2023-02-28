
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
    /// TypeReference (TypeRef token).
    /// This implements the TypeReference interface to allow getting TypeRef specific data.
    /// It also implements the regular Type interface, and then forwards on that to the resolved type.     
    /// The base implementations will call these derived properties which will resolve the proxy.
    /// See code:Ilrun.ITypeReference for more details on the contract.
    /// </summary>
    /// <remarks>
    /// MetadataOnlyTypeReference does not inherit DebuggerDisplayAttribute from TypeProxy. Instead,
    /// it contains Name and FullName, the same as MetadataOnlyCommonType. This is safe since Name and FullName  
    /// are always available for MetadataOnlyTypeReference. Also, the token of the typeref is included.
    /// </remarks>
    [DebuggerDisplay(@"\{Name = {Name} FullName = {FullName} {m_typeRef}\}")]
    internal class MetadataOnlyTypeReference : TypeProxy, ITypeReference
    {
        // A TypeRef token. This is the primary key.
        // This is valid in the this.m_resolver scope.
        Token m_typeRef;

        /// <summary>
        /// Encapsulate a type reference from a metadata scope. 
        /// </summary>
        /// <param name="resolver">metadata scope that the typeref is in (this is likely not the 
        /// scope that the type will actually resolve to.</param>
        /// <param name="typeRef">typeref token for the typeref.</param>
        public MetadataOnlyTypeReference(MetadataOnlyModule resolver, Token typeRef)
            : base(resolver)
        {
            m_typeRef = typeRef;
            Debug.Assert(typeRef.IsType(TokenType.TypeRef));
        }

       

        #region ITypeReference Members

        // Resolves the type, invoking callbacks into the Assembly resolver if needed.
        // This can fail / throw, but should not return null.
        protected override Type GetResolvedTypeWorker()
        {
            // TypeRef tokens aren't generic. If this had instantiated generic, 
            // args, it would have been a TypeSpec instead of a TypeRef.

            return m_resolver.ResolveTypeRef(this);
        }

        public Module DeclaringScope
        {
            get { return m_resolver; }
        }

        public Token TypeRefToken
        {
            get { return m_typeRef; }
        }

        public Token ResolutionScope
        {
            get
            {
                int resScope;
                int size;
                m_resolver.RawImport.GetTypeRefProps(m_typeRef.Value, out resScope, null, 0, out size);
                Token tokResScope = new Token(resScope);
                return tokResScope;
            }

        }

        // Return just the name (which includes namespace), does not return enclosing types.
        public virtual string RawName
        {
            get
            {
                // Avoid doing resolution here, we have the name in the metadata tables.
                int token = this.m_typeRef.Value;
                int resScope, size;
                m_resolver.RawImport.GetTypeRefProps(token, out resScope, null, 0, out size);
                
                StringBuilder sb = StringBuilderPool.Get(size);
                m_resolver.RawImport.GetTypeRefProps(token, out resScope, sb, sb.Capacity, out size);
                
                string result = sb.ToString();
                StringBuilderPool.Release(ref sb);
                return result;
            }
        }

        // We can get names without resolving.
        public override string Namespace
        {
            get
            {
                if (DeclaringType != null)
                {
                    return DeclaringType.Namespace;
                }
                return Utility.GetNamespaceHelper(FullName);
            }
        }
        public override string Name
        {
            get
            {
                return Utility.GetTypeNameFromFullNameHelper(FullName, IsNested);
            }
        }

        // Returns the full name. Note that the TypeRef property includes the fullname, so we can get that
        // without having to resolve. 
        // This also implements the Type.FullName. Normally Type implementations require resolving.
        public override string FullName
        {
            get
            {
                // Avoid callling code:GetResolvedType here, since the name must be available even when we
                // haven't resolved the type.
                int token = this.m_typeRef.Value;

                string name = String.Empty;
                string suffix = String.Empty;

                while (true)
                {
                    int resScope, size;
                    m_resolver.RawImport.GetTypeRefProps(token, out resScope, null, 0, out size);

                    StringBuilder sb = StringBuilderPool.Get(size);
                    Token tkScope = new Token(resScope);

                    m_resolver.RawImport.GetTypeRefProps(token, out resScope, sb, sb.Capacity, out size);

                    // Nested TypeRefs classes can only be nested in other TypeRefs.
                    if (tkScope.IsType(TokenType.TypeRef))
                    {
                        suffix = "+" + sb.ToString() + suffix;
                        // Nested class, loop around
                        token = tkScope.Value;
                        continue;
                    }
                    sb.Append(suffix);

                    name = sb.ToString();
                    StringBuilderPool.Release(ref sb);

                    break;
                }

                return name;
            }
        }

       

        #endregion


        /// <summary>
        /// Get the asssembly name that this TypeRef requests to be resolved to. This is a higher level view
        /// over the ResolutionScope token. 
        /// A sane universe should resolve an assembly to the requested name or fail. However, if the universe
        /// does resolve the assembly to something else, then the actual assembly name may not match the requested 
        /// assembly name.
        /// </summary>
        private AssemblyName RequestedAssemblyName
        {
            get
            {
                // This switch is similar to the one in MOModule.ResolveTypeRef()
                var token = this.ResolutionScope;
                switch (token.TokenType)
                {
                    case TokenType.TypeRef:
                        // TypeRef is nested in another TypeRef. Both are in the same assembly, so 
                        // get the assembly name for the outer type ref.
                        var tr = new MetadataOnlyTypeReference(this.m_resolver, token);
                        return tr.RequestedAssemblyName;


                    case TokenType.AssemblyRef:
                        // TypeRef resolves to another assembly. Get the assembly name from the metadata.
                        AssemblyName an = this.m_resolver.GetAssemblyNameFromAssemblyRef(token);

                        return an;

                    // TypeRef resolves to the same assembly that it's declared in.
                    case TokenType.ModuleRef:
                    case TokenType.Module:
                        return this.m_resolver.Assembly.GetName();


                    default:
                        // The Ecma spec states that a typeRef scope token must be one
                        // of the types listed above. If it's something else, then this likely means corrupted metadata. 
                        Debug.Assert(false, "Unexpected tokResScope");
                        throw new InvalidOperationException(Resources.InvalidMetadata);

                } // end switch
            }
        }


        // Override to get a proxy object without resolution.
        public override Assembly Assembly
        {
            get
            {
                AssemblyName name = this.RequestedAssemblyName;
                Assembly a = new AssemblyRef(name, this.TypeUniverse);
                return a;
            }
        }


        // Override to get the AQN without doing resolution. See the caveat in code:RequestedAssemblyName.
        public override string AssemblyQualifiedName
        {
            get
            {
                string aqn = this.RequestedAssemblyName.ToString();
                string t = this.FullName;
                return System.Reflection.Assembly.CreateQualifiedName(aqn, t);
            }
        }


        // We know that a TypeRef is not a composite type (liek a ByRef, Array, Pointer, etc)
        // We could have a TypeRef to a generic type (eg, IEnumerable`1)
        protected override bool IsByRefImpl()
        {
            return false;
        }
        protected override bool IsArrayImpl()
        {
            return false;
        }
        public override bool IsGenericParameter
        {
            get
            {
                return false;
            }
        }
        protected override bool IsPointerImpl()
        {
            return false;
        }

        protected override bool IsPrimitiveImpl()
        {
            // Primtive types can only occur in the system assembly - avoid resolution by checking
            // for that case first.
            if (!TypeUniverse.WouldResolveToAssembly(RequestedAssemblyName, TypeUniverse.GetSystemAssembly()))
                return false;

            // This is in the system assembly so must not be a problem to resolve
            return GetResolvedType().IsPrimitive;
        }

        // Override to avoid resolution.
        public override Type DeclaringType
        {
            get
            {
                // Avoid doing resolution here, we have the name in the metadata tables.
                // If the type is nested, then the resScope token will be a TypeRef.
                int token = this.m_typeRef.Value;
                int resScope, size;
                m_resolver.RawImport.GetTypeRefProps(token, out resScope, null, 0, out size);
                Token scopeToken = new Token(resScope);
                if (scopeToken.IsType(TokenType.TypeRef))
                {
                    return this.m_resolver.Factory.CreateTypeRef(m_resolver, scopeToken);
                }
                return null;
            }
        }

    } // end class TypeRef


    /// <summary>
    /// SignatureTypeReference (TypeRef token that occurs in a signature).
    /// This is a special case of TypeReference in which we know the CorElementType under which it
    /// was used.
    /// </summary>
    /// <remarks>
    /// This is useful to satisfy some requests without resolving the type
    /// </remarks>
    [DebuggerDisplay(@"\{Name = {Name} FullName = {FullName} ElementType = {m_elemType} {m_typeRef}\}")]
    internal class MetadataOnlySignatureTypeReference : MetadataOnlyTypeReference
    {
        /// <summary>
        /// Encapsulate a type reference from a metadata scope. 
        /// </summary>
        /// <param name="resolver">metadata scope that the typeref is in (this is likely not the 
        /// scope that the type will actually resolve to.</param>
        /// <param name="typeRef">typeref token for the typeref.</param>
        /// <param name="elemType">element type in which TypeRef occured under</param>
        public MetadataOnlySignatureTypeReference(MetadataOnlyModule resolver, Token typeRef, CorElementType elemType)
            : base(resolver, typeRef)
        {
            // For now we just expect these two element types, could more could be possible
            Debug.Assert(elemType == CorElementType.Class || elemType == CorElementType.ValueType);
            m_elemType = elemType;
        }

        protected override bool IsValueTypeImpl()
        {
            // Override default behavior which does a resolve            
            return m_elemType == CorElementType.ValueType;
        }

        private CorElementType m_elemType;
    }


    /// <summary>
    /// Represents raw data from the metadata, prior to being converted into the reflection object model. 
    /// This class is useful for representing typerefs that are encoded as strings names, such as how TypeRefs 
    /// are stored in custom attributes.
    /// 
    /// This is semantically equivalent to a TypeRef, but this form gaurantees we don't do any eager resolution.
    /// 
    /// Once LMR is fully fixed to avoid eager resolution, then this class can safely be replaced with usages 
    /// of Type.    
    /// This is converted to a System.Type by parsing it.
    /// </summary>
    [DebuggerDisplay("{m_TypeName},{m_AssemblyName}")]
    internal class UnresolvedTypeName
    {
        // Full type name (excluding assembly) which can be passed to a type parser.
        readonly private string m_TypeName;

        // Assembly name that the type name resides in. 
        readonly private AssemblyName m_AssemblyName;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="typeName">The full type name, which will eventually be parsed.</param>
        /// <param name="assemblyName">the assembly that the type will be resolved to.</param>
        public UnresolvedTypeName(string typeName, AssemblyName assemblyName)
        {
            Debug.Assert(!string.IsNullOrEmpty(typeName));
            Debug.Assert(assemblyName != null);

            m_TypeName = typeName;
            m_AssemblyName = assemblyName;
        }


        /// <summary>
        /// Parse the name to convert to a reflection System.Type.
        /// </summary>
        /// <param name="universe">type universe that resulting type will be valid in.</param>
        /// <param name="moduleContext">module that the typeName was obtained from. This is passed to the parser
        /// and may be required to disambiguate the type name. See type name parser for details.</param>
        /// <returns>a System.Type for the given type.</returns>
        public Type ConvertToType(ITypeUniverse universe, Module moduleContext)
        {
            // TODO: we could add new overload to type name parser that takes AssemblyName + type name.
            string fullTypeName = string.Format(CultureInfo.InvariantCulture, "{0},{1}", m_TypeName, m_AssemblyName.FullName);

            // This requires resolution.
            // TODO: create typeRef instead by providing TypeRef constructor that accepts AssemblyRef & type name.
            Type type = TypeNameParser.ParseTypeName(universe, moduleContext, fullTypeName);

            return type;
        }

        /// <summary>
        /// Get the full type name, excluding assembly
        /// </summary>
        public string TypeName
        {
            get { return m_TypeName; }
        }
    }
}
