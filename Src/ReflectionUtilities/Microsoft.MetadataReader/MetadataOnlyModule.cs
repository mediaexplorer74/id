// Provide implementation of System.Reflection.Module around a raw IMetadataImport.

using System;
using Debug = Microsoft.MetadataReader.Internal.Debug;
using System.Text;

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Reflection.Adds;
using System.Threading;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using BindingFlags = System.Reflection.BindingFlags;
using Type = System.Reflection.Mock.Type;
using CallingConventions = System.Reflection.CallingConventions;
using ParameterModifier = System.Reflection.ParameterModifier;
using TypeAttributes = System.Reflection.TypeAttributes;
using FieldAttributes = System.Reflection.FieldAttributes;
using MethodAttributes = System.Reflection.MethodAttributes;
using MemberTypes = System.Reflection.MemberTypes;
using AssemblyName = System.Reflection.AssemblyName;
using AssemblyNameFlags = System.Reflection.AssemblyNameFlags;
using AmbiguousMatchException = System.Reflection.AmbiguousMatchException;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// TokenResolver represents a module. Derived classes may:
    /// - override caching and creation policy. (although this could be deferred to a policy object)
    /// - associate auxillary data (although they could do this via a Hash; or via a 'object Tag' property)
    /// 
    /// Token resolver for scopes within an MDbgModule
    /// This will create Type, MethodInfo, etc for metadata tokens within the module. 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class MetadataOnlyModule : Module, IModule2, IDisposable
    {
        readonly private IMetadataExtensionsPolicy m_policy;
        readonly private IReflectionFactory m_factory;
        readonly private string m_modulePath;

        // Wrapper around a IMetadataImport interface
        // This holds raw native resources and should be released during the dispose.
        // The module owns this metadata object and will dispose it. 
        readonly private MetadataFile m_metadata;

        // CLR v2.0/v3.5: Since the IMetadataImport implementation from mscorwks does not implement the free-threaded
        // marshaller, we need to have a per-thread interface.
        // To track reference counts, this should be explicitly released (and not just set to null).
        // Use Marshal.GetUniqueObjectForIUnknown() and Marshal.ReleaseComObject();
        //
        // CLR v4.0: IMetadataImport implementation from mscorwks does implement the free-threaded marshaller,
        // and we don't need to have per-thread interface. We only initialize it once and use aferwards 
        // regardless of the current thread.

#if !USE_CLR_V4
        // CLR v2.0/v3.5: thread that the m_cachedThreadAffinityImporter is valid for. 
        // CLR v4.0: not needed
        private readonly Dictionary<Thread, IMetadataImport> m_cachedThreadAffinityImporter;
        private readonly object m_lock = new object();
#else
        private IMetadataImport m_cachedThreadAffinityImporter;
#endif
        // Cache of ScopeName property.
        // Performance run on Fib(20) showed ScopeName taking 20% of the time. This is probably because it
        // was being called from TokenResolver.Equals.
        private string m_scopeName;

        // readonly lazy init by code:CreateTypeCodeMapping
        // The index into the mapping is the TypeCode, which is a 0-based dense enum. 
        // These tokens are all scoped in the System assembly. Clients can do a linear search for token to
        // reverse lookup the token's type code. If the token is not in the map, its type code is TypeCode.Object.
        private Token[] m_typeCodeMapping;


        public MetadataOnlyModule(ITypeUniverse universe, MetadataFile import, string modulePath)
            : this(universe, import, new DefaultFactory(), modulePath)
        {
        }


        // universe - type universe that the module is valid in.
        // import - raw IMetadataImport object. 
        // factory - used to create reflection objects (Type, MethodInfo, etc)
        public MetadataOnlyModule(ITypeUniverse universe, MetadataFile import, IReflectionFactory factory, string modulePath)
        {
            Debug.Assert(universe != null);
            Debug.Assert(import != null);
            Debug.Assert(factory != null);


            m_assemblyResolver = universe;
            m_metadata = import;
            m_factory = factory;

            m_policy = new MetadataExtensionsPolicy20(universe);

            m_modulePath = modulePath;

#if USE_CLR_V4

            object p = Marshal.GetUniqueObjectForIUnknown(m_metadata.RawPtr);
            m_cachedThreadAffinityImporter = (IMetadataImport)p;
#else
            m_cachedThreadAffinityImporter = new Dictionary<Thread, IMetadataImport>();
#endif
        }

        public override string FullyQualifiedName
        {
            get
            {
                return m_modulePath;
            }
        }

        /// <summary>
        /// Get policy object that specifies reflection behavior not directly corresponding to metadata.
        /// </summary>
        internal IMetadataExtensionsPolicy Policy
        {
            get
            {
                return m_policy;
            }
        }

        internal IReflectionFactory Factory
        {
            get { return m_factory; }
        }

        public override string ToString()
        {
            if (m_metadata == null)
                return "uninitialized";

            return this.ScopeName;
        }

        public override bool Equals(object obj)
        {
            if (obj == (object)this)
                return true;

            MetadataOnlyModule resolver = obj as MetadataOnlyModule;
            if (resolver != null)
            {
                // Modules must be in the same type universe.
                if (!m_assemblyResolver.Equals(resolver.AssemblyResolver))
                {
                    return false;
                }



                return ScopeName == resolver.ScopeName;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            // TODO: This is out of sync with the Equals definition.
            return m_metadata.RawPtr.GetHashCode() + m_assemblyResolver.GetHashCode();
        }

        readonly private ITypeUniverse m_assemblyResolver;

        public ITypeUniverse AssemblyResolver { get { return m_assemblyResolver; } }


        internal bool IsValidToken(int token)
        {
            return this.RawImport.IsValidToken((uint)token);
        }
        internal bool IsValidToken(Token token)
        {
            return IsValidToken(token.Value);
        }

        /// <summary>
        /// Helper to read bytes embedded in metadata blob.
        /// </summary>
        public byte[] ReadEmbeddedBlob(EmbeddedBlobPointer pointer, int countBytes)
        {
            return this.m_metadata.ReadEmbeddedBlob(pointer, countBytes);
        }

        /// <summary>
        /// Get the raw low level metadata file.
        /// </summary>
        internal MetadataFile RawMetadata
        {
            get { return m_metadata; }
        }

        internal IMetadataImport RawImport { get { return GetThreadSafeImporter(); } }

#if !USE_CLR_V4
        // We have two versions of GetThreadSafeImporter depending on which CLR we run.
        // On CLR v4 we can take advantage of fact that COM object that implements 
        // IMetadataImport also agreggates FTM so we don't need to perform any explicit
        // thread management, which makes our code simpler. But since we have to run on
        // Orcas too, we can't completely remove our thread management code. 

        /// <summary>
        /// CLR v2.0/3.5: Get a raw Importer that's usable by this thread. This can't be used on another thread. 
        /// </summary>
        /// <returns>a raw Importer interface that can be used only on this thread.</returns>
        private IMetadataImport GetThreadSafeImporter()
        {
            // GetUniqueObjectForIUnknown() is very slow, and most calls will be on the same thread.
            // So cache the last importer + thread, and return that if can to avoid calling GetUniqueObjectForIUnknown.
            // We use this policy because our scenarios have calls coming in waves on the same thread; but
            // on different threads over time. It's simpler to track just a single RCW (the most recent),
            // than it is to have a mapping of multiple threads. 
            // This is about a 5x speed increase in the tests.

            IMetadataImport result;
            lock (m_lock)
            {
                if (!m_cachedThreadAffinityImporter.TryGetValue(Thread.CurrentThread, out result))
                {
                    // Get a per-thread importer. Since the RCW has thread-affinity, we need to create a new RCW for
                    // this thread (hence the call to Get*Unique*ObjectForIUnknown instead of just GetObject).
                    // See code:MetadataDispenser.OpenFile for details.
                    object p = Marshal.GetUniqueObjectForIUnknown(m_metadata.RawPtr);
                    result = (IMetadataImport)p;

                    m_cachedThreadAffinityImporter.Add(Thread.CurrentThread, result);
                }
            }

            return result;
        }

#else

        /// <summary>
        /// CLR v4.0: Gets a raw Importer that's usable by all threads since in v4.0 object that
        /// implements this interface implements FTM too. 
        /// </summary>
        private IMetadataImport GetThreadSafeImporter()
        {
            return m_cachedThreadAffinityImporter;
        }
#endif

        // Returns the name of the scope stored in the metadata. This usually corresponds to the filename,
        // but it can be different. For example, "mscorlib.dll" has a scope name of "CommonLanguageRuntimeLibrary"
        public override string ScopeName
        {
            get
            {
                if (m_scopeName == null)
                {
                    var imdi = GetThreadSafeImporter();
                    int size;

                    Guid mvid;
                    imdi.GetScopeProps(null, 0, out size, out mvid);
                    StringBuilder sb = StringBuilderPool.Get(size);
                    imdi.GetScopeProps(sb, sb.Capacity, out size, out mvid);
                    sb.Length = size - 1; // chop off trailing null               

                    m_scopeName = sb.ToString();
                    StringBuilderPool.Release(ref sb);
                }
                return m_scopeName;
            }
        }

        // Get the GUID for this module.
        public override Guid ModuleVersionId
        {
            get
            {
                int size;
                Guid mvid;
                this.RawImport.GetScopeProps(null, 0, out size, out mvid);
                return mvid;
            }
        }

        public override string Name
        {
            get
            {
                return System.IO.Path.GetFileName(m_modulePath);
            }
        }

        #region Module Members

        // Resolve a TypeDef token within this module. 
        // Since the typedef is within this module, we know that the implementation will return a LMR type. 
        internal MetadataOnlyCommonType ResolveTypeDefToken(Token token)
        {
            Debug.Assert(token.IsType(TokenType.TypeDef));

            MetadataOnlyCommonType type = m_factory.CreateSimpleType(this, token);
            return type;
        }


        // Ensure the token is valid and throw ArgumentException if it's not.        
        private void EnsureValidToken(Token token)
        {
            // Metadata APIs expect valid tokens. We can get here from public surface, so validate it now. 
            if (!this.IsValidToken(token))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidMetadataToken, token.ToString()));
            }
        }

        /// <summary>
        /// Internal API to resolve tokens to Type objects.
        /// This handles TypeDefs, TypeRefs and TypeSpecs.        
        /// </summary>
        /// <remarks>
        /// The generic context is only used for TypeSpecs. So if you pass a TypeDef for List<T>, the generic
        /// context is ignored and you get a Type for List<T>. If you want List<int>, then you need to
        /// MakeGenericType(typeof(int)) on it.
        /// </remarks>
        internal Type ResolveTypeTokenInternal(Token token, GenericContext context)
        {
            EnsureValidToken(token);

            if (token.IsType(TokenType.TypeDef))
            {
                // Ignore generic context. If this is a generic type, then give back the open type (eg, List<T>).
                return ResolveTypeDefToken(token);
            }
            else if (token.IsType(TokenType.TypeRef))
            {
                // Return a code:ITypeReference that describes the TypeRef properties, without actually
                // resolving the TypeRef. 
                // This will also implement the Type interface and forward all requests on that to the
                // resolved type.
                return this.Factory.CreateTypeRef(this, token);
            }
            else if (token.IsType(TokenType.TypeSpec))
            {
                // A TypeSpec. This can describe an arbitrary type, including with generic parameters.
                Type[] typeArgs = null;
                Type[] methodArgs = null;
                if (context != null)
                {
                    typeArgs = context.TypeArgs;
                    methodArgs = context.MethodArgs;
                }
                return this.Factory.CreateTypeSpec(this, token, typeArgs, methodArgs);
            }
            else
            {
                // Wrong token type.
                throw new ArgumentException(Resources.TypeTokenExpected);
            }
        }

        /// <summary>
        /// Internal API to resolve tokens and their element type to Type objects.
        /// This handles TypeDefs, TypeRefs and TypeSpecs.        
        /// </summary>
        internal Type ResolveTypeTokenInternal(Token token, CorElementType elementType, GenericContext context)
        {
            if (token.IsType(TokenType.TypeRef))
            {
                return this.Factory.CreateSignatureTypeRef(this, token, elementType);
            }
            else
            {
                return ResolveTypeTokenInternal(token, context);
            }
        }

        /// <summary>
        /// Helper method to create a generic type. 
        /// For example: List<T> + { T='int'} --> List<int>.
        /// </summary>
        /// <remarks>
        /// This is called from MetadataOnlyXXXInfo classes to get DeclaringType, 
        /// event handler, or similar objects. 
        /// Difference between GetGenericType and ResolveTypeTokenInternal is that 
        /// ResolveTypeTokenInternal ignores generic context when token is TypeDef, while
        /// this method propagates it.
        /// </remarks>
        internal Type GetGenericType(Token token, GenericContext context)
        {
            Type[] typeArgs = null;
            Type[] methodArgs = null;
            if (context != null)
            {
                typeArgs = context.TypeArgs;
                methodArgs = context.MethodArgs;
            }

            if (token.IsType(TokenType.TypeDef))
            {
                if ((typeArgs != null) && (typeArgs.Length > 0))
                {
                    return m_factory.CreateGenericType(this, token, typeArgs);
                }
                else
                {
                    // Go through factory for simple types so type is taken from 
                    // the cache if there is one. 
                    return m_factory.CreateSimpleType(this, token);
                }
            }
            else if (token.IsType(TokenType.TypeRef))
            {
                var t = m_factory.CreateTypeRef(this, token);
                if ((typeArgs != null) && (typeArgs.Length > 0))
                {
                    t = t.MakeGenericType(typeArgs); // can't call with empty args even on a non-generic type.
                }
                return t;
            }
            else if (token.IsType(TokenType.TypeSpec))
            {
                // Even for TypeSpecs context could be null. E.g. if TypeSpec represents non-generic
                // TypeDef that wasn't optimized.
                return m_factory.CreateTypeSpec(this, token, typeArgs, methodArgs);
            }
            else
            {
                // Wrong token type.
                throw new ArgumentException(Resources.TypeTokenExpected);
            }
        }

        private MethodBase ResolveMethodTokenInternal(Token methodToken, GenericContext context)
        {
            // Metadata APIs expect valid tokens. We can get here from public surface, so validate it now. 
            EnsureValidToken(methodToken);

            if (methodToken.IsType(TokenType.MethodDef))
            {
                // MethodDef aren't generic, so don't need to pass context along
                return ResolveMethodDef(methodToken);
            }
            else if (methodToken.IsType(TokenType.MemberRef))
            {
                // A MemberRef has no generic arguments by definition, else it would be a MethodSpec.
                // So pass in null for generic method arguments.
                return ResolveMethodRef(methodToken, context, null);
            }
            else if (methodToken.IsType(TokenType.MethodSpec))
            {
                return ResolveMethodSpec(methodToken, context);
            }



            // Wrong token type. We could get here with corrupted metadata.
            throw new ArgumentException(Resources.MethodTokenExpected);

        }

        /// <summary>
        /// Resolves a reference to an instantiated generic method.
        /// </summary>
        /// <param name="methodToken">MethodSpec token.</param>
        /// <param name="context">Generic context containing types used for intantiation.</param>
        /// <returns>MethodInfo instance containing info about an instantiated generic method.</returns>
        private MethodInfo ResolveMethodSpec(Token methodToken, GenericContext context)
        {
            Debug.Assert(methodToken.IsType(TokenType.MethodSpec));

            // First, get the properties of the method: 
            // 1) generic method this is an instantiation of (could be MethodDef or MethodRef) and
            // 2) signature blob for this instantiation
            //
            Token parentMethodTokenValue;
            EmbeddedBlobPointer signatureBlobPointer;
            int signatureBlobSize;
            ((IMetadataImport2)this.RawImport).GetMethodSpecProps(methodToken, out parentMethodTokenValue, out signatureBlobPointer, out signatureBlobSize);

            // Convert signature blob in a byte array so we can analyze it.
            //
            byte[] signature = this.ReadEmbeddedBlob(signatureBlobPointer, signatureBlobSize);
            int index = 0;

            CorCallingConvention callConv = SignatureUtil.ExtractCallingConvention(signature, ref index);
            Debug.Assert(callConv == CorCallingConvention.GenericInst);

            // These are the generic method arguments **not** the parameter types.
            int parameterCount = SignatureUtil.ExtractInt(signature, ref index);
            Type[] parameters = new Type[parameterCount];
            for (int i = 0; i < parameterCount; i++)
            {
                parameters[i] = SignatureUtil.ExtractType(signature, ref index, this, context);
            }

            Token parentMethodToken = new Token(parentMethodTokenValue);
            MethodInfo result;
            switch (parentMethodToken.TokenType)
            {
                case TokenType.MethodDef:
                    // If there were a type context, we wouldn't have had a methodDef - would have had a memberRef instead.
                    result = GetGenericMethodInfo(parentMethodToken, new GenericContext(null, parameters));
                    break;

                case TokenType.MemberRef:
                    // We get here for methods in another assembly or when there are type parameters.

                    // Resolve reference to the generic method this is an instantiation of.
                    result = (MethodInfo)ResolveMethodRef(parentMethodToken, context, parameters);

                    break;

                default:
                    Debug.Assert(false, "invalid token type");
                    throw new InvalidOperationException();
            }

            return result;
        }

        // Returns a MethdoX for a given MethodDef token.
        private MethodBase ResolveMethodDef(Token methodToken)
        {
            Debug.Assert(methodToken.IsType(TokenType.MethodDef));
            //if the method contains generic parameters, add them to the context
            List<Type> args = GetTypeParameters(methodToken.Value);
            GenericContext context = null;
            if (args.Count > 0)
            {
                context = new GenericContext(null, args.ToArray());
            }

            var m = MetadataOnlyMethodInfo.Create(this, methodToken, context);

            return m;
        }

        // Wraper to get a MethodInfo, when the method is known that it's not a constructor.
        internal MethodInfo GetGenericMethodInfo(Token methodToken, GenericContext genericContext)
        {
            return (MethodInfo)GetGenericMethodBase(methodToken, genericContext);
        }

        // Get a method which may or may not be a constructor.
        // methodToken - method def to get a method for. 
        // genericContext - generic arguments.
        internal MethodBase GetGenericMethodBase(Token methodToken, GenericContext genericContext)
        {
            if (genericContext != null)
            {
                if (((genericContext.TypeArgs == null) || (genericContext.TypeArgs.Length == 0))
                    && ((genericContext.MethodArgs == null) || (genericContext.MethodArgs.Length == 0))
                )
                {
                    //No type args or method args; just use null for the context.
                    genericContext = null;
                }
            }
            Debug.Assert(methodToken.IsType(TokenType.MethodDef));
            var m = MetadataOnlyMethodInfo.Create(this, methodToken, genericContext);
            return m;
        }

        /// <summary>
        /// Resolves a MethodRef token to a method/constructor.
        /// </summary>
        /// <remarks>
        /// Since ResolveTypeTokenInternal could return a non-LMR type we can't rely on having
        /// access to its metadata through IMetaDataImport interface. Instead, we have to go through 
        /// public Reflection APIs only. Future optimization: If this approach happens to be too slow, 
        /// we could check its type and go through a fast path is it's a LMR type (or TypeRef around
        /// LMR type).
        /// 
        /// This can be used to get the constructor for a custom-attribute. 
        /// </remarks>
        /// <param name="memberRef">member ref token to resolve</param>
        /// <param name="context">generic context of caller. </param>
        /// <param name="genericMethodParameters">generic parameters to method, or null</param>        
        internal MethodBase ResolveMethodRef(Token memberRef, GenericContext context, Type[] genericMethodParameters)
        {
            // Say we're calling a MethodSpec:: 
            // class Class1 {
            //  static IEnumerable<!!TResult> Foo<TSource,TResult> (...) {
            //    ...
            //    call Class2<!!TSource>::Select<!!TResult>(...)   <-- example, resolve this call
            //    ...
            //  }
            //  class Class2<TSource> 
            //  {
            //     IEnumerable<!!TResult> Select<TResult> (!TSource, !!TResult) {
            //     }
            //  }
            // }
            // Say we're inside Foo method and resolving the call to Select2.
            // There are 2 generic contexts involved here:
            // 1) Context of caller (in Foo).   (TypeArgs=none, MethodArgs=TSource,TResult)
            //      This is the passed in context.
            // 2) Context inside Select method.   (TypeArgs=TSource, MethodArgs=TResult)
            //      This is computed from the type arguments of the resolved function's declaring type and
            //      the genericMethodParameters.
            // 
            // The parameters types for Select are resolved against Select's context, not Foo's context. 
            // context is the context for Foo. Parameter types are part of the signature of the method that
            // we return. Parameter types are also essential for matching overloads, which is done inside the
            // caller's context. 
            // 

            // Get member ref info
            string methodName;
            SignatureBlob signatureBlob;
            Token declaringTypeToken;
            GetMemberRefData(memberRef, out declaringTypeToken, out methodName, out signatureBlob);

            // MemberRefs are used to describe calls to varargs. In this case, the declaring token 
            // is the method that the varargs binds to, not the type.
            // Don't support varargs yet, so check and throw.
            {
                var sig = signatureBlob.GetSignatureAsByteArray();
                int index = 0;

                // First, get calling convention.
                var c = SignatureUtil.ExtractCallingConvention(sig, ref index);
                if (c == CorCallingConvention.VarArg)
                {
                    throw new NotImplementedException(Resources.VarargSignaturesNotImplemented);
                }
            }

            // Resolve the type that contains the method
            Type declaringType = ResolveTypeTokenInternal(declaringTypeToken, context);

            MethodSignatureDescriptor descriptor;

            // If declaring type is an array then all its methods are "fake" i.e. they are not actually
            // stored in the metadata. In this case, there is no reason to create open version of method's
            // signature since such thing does not exist. We create a closed version only and find 
            // straigt argument match, without matching signature "shapes".
            //
            // E.g. declaring array like this:
            //
            //      List<Dictionary<T, U>>[,] matrix 
            //
            // where T, U are coming from enclosing class, will create Set method like this:
            //
            // void Set(int, int, List<Dictionary<Var!1, Var!2>>
            //
            // This method doesn't ever exist in its truly open form and there is no potential for
            // ambiguity so we can safely match based on closed version.

            if (declaringType.IsArray)
            {
                descriptor = SignatureUtil.ExtractMethodSignature(signatureBlob, this, context);
            }
            else
            {
                // Open context contains just proxy type variables. We use this context to extract method's signature and 
                // not lose any information about method's shape (e.g. which parameters are generic).
                GenericContext openMethodContext = new OpenGenericContext(this, declaringType, memberRef);
                descriptor = SignatureUtil.ExtractMethodSignature(signatureBlob, this, openMethodContext);
            }

            // Closed context has generic parameters instantiated (if there are any). We use this context when querying 
            // declaringType for all of its methods. One of them will be result that we are returning.
            GenericContext closedMethodContext = new GenericContext(declaringType.GetGenericArguments(), genericMethodParameters);

            MethodBase matchingMethod = SignatureComparer.FindMatchingMethod(
                methodName,
                declaringType,
                descriptor,
                closedMethodContext);

            if (matchingMethod == null)
            {
                throw new MissingMethodException(declaringType.Name, methodName);
            }

            return matchingMethod;
        }

        // Given a field reference token, get the field.
        internal FieldInfo ResolveFieldRef(Token memberRef, GenericContext context)
        {
            //Get member ref info
            string name;
            SignatureBlob blob;
            Token declaringTypeToken;
            GetMemberRefData(memberRef, out declaringTypeToken, out name, out blob);

            //Resolve the type
            Type declaringType = ResolveTypeTokenInternal(declaringTypeToken, context);

            FieldInfo f = declaringType.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            Debug.Assert(f != null);
            return f;
        }

        // Given a field token (def or ref), and the generic context, resolve it to a FieldInfo object.
        // Resolving refs may lead to a non-LMR implementation. 
        internal FieldInfo ResolveFieldTokenInternal(Token fieldToken, GenericContext context)
        {
            if (fieldToken.IsType(TokenType.FieldDef))
            {
                //If we get here, the field is in a non-generic class.  If the field's type
                //were generic, we would have gotten a MemberRef instead.
                FieldInfo result = this.Factory.CreateField(this, fieldToken, null, null);
                return result;
            }
            else if (fieldToken.IsType(TokenType.MemberRef))
            {
                FieldInfo result = ResolveFieldRef(fieldToken, context);
                return result;
            }
            else
            {
                Debug.Assert(false);
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, fieldToken.ToString()));
            }
        }

        public override string ResolveString(int metadataToken)
        {
            Token tokenString = new Token(metadataToken);
            Debug.Assert(tokenString.IsType(TokenType.String));

            int size;
            var import = this.RawImport;
            import.GetUserString(tokenString, null, 0, out size);
            char[] buffer = new char[size];
            import.GetUserString(tokenString, buffer, buffer.Length, out size);
            Debug.Assert(size == buffer.Length, "Successive calls to IMetadataImport::GetUserString() on the same token returned different sizes");
            string result = new string(buffer);
            return result;
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return GetCustomAttributeData(this.MetadataToken);
        }

        #endregion


        #region Resolution
        // Resolution for TokenRefs. Requires calling back to a resolver object.
        // ICorDebug API can inspect the target to see how Refs were actually resolved. 

        // Resolve a TypeReference token to a System.Type object. 
        // The resulting Type is not necessarily implemented as a LMR type.
        // TypeRefs don't have generic parameters. A generic reference would be encoded in a TypeSpec which would have a
        // TypeRef in it. 
        internal Type ResolveTypeRef(ITypeReference typeReference)
        {
            Token tokResScope = typeReference.ResolutionScope;
            string name = typeReference.RawName;
            Type result;

            switch (tokResScope.TokenType)
            {
                case TokenType.TypeRef:
                    //This happens if tokenTypeRef is a nested class.  We need to resolve 
                    //the enclosing class and then go back and get the class we are interested in.

                    Type tOuter = Factory.CreateTypeRef(this, tokResScope);


                    // Lookup by name. We already resolved for nested classes, so don't include nested classes in
                    // the name lookup here (so use Name instead of FullName)
                    // This lookup will trigger a Type resolution on tOuter.

                    result = tOuter.GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic);
                    return result;

                case TokenType.AssemblyRef:
                    // Callback to TypeUniverse to do resolution.
                    Assembly assembly = m_assemblyResolver.ResolveAssembly(this, tokResScope);

                    // Validate results.
                    if (assembly == null)
                    {
                        Debug.Assert(false);
                        throw new InvalidOperationException(Resources.ResolverMustResolveToValidAssembly);
                    }
                    {
                        // Validate that the assembly is still in the same universe.
                        IAssembly2 ia2 = (IAssembly2)assembly;

                        if (ia2.TypeUniverse != m_assemblyResolver)
                        {
                            Debug.Assert(false);
                            throw new InvalidOperationException(Resources.ResolvedAssemblyMustBeWithinSameUniverse);
                        }
                    }

                    result = assembly.GetType(name, true);
                    return result;

                case TokenType.ModuleRef:
                    // This works for netmodules that are loaded as part of an assembly i.e. modules that have
                    // back pointer to their assembly.
                    // TODO: do we want to ask the host to resolve ModuleRefs as part of their resolution policy?

                    Module module = ResolveModuleRef(tokResScope);
                    result = module.GetType(typeReference.FullName);
                    return result;

                case TokenType.Module:
                    // The token specifies the current module. This can happen when compiler does not optimize
                    // metadata it emits. This is allowed.
                    // The spec in TypeRef table definition (II.22.38 “TypeRef : 0x01”) mentions ResolutionScope value:
                    //      d. a Module token, if the target type is defined in the current module - 
                    //         this should not occur in a CLI (“compressed metadata”) module [WARNING]
                    return this.GetType(typeReference.FullName);
            }

            // The Ecma spec states that a typeRef scope token must be one
            // of the types listed above. If it's something else, then this likely means corrupted metadata. 
            Debug.Assert(false, "Unexpected tokResScope");
            throw new InvalidOperationException(Resources.InvalidMetadata);
        }


        internal Module ResolveModuleRef(Token moduleRefToken)
        {
            Debug.Assert(moduleRefToken.TokenType == TokenType.ModuleRef, "Token type must be ModuleRef");

            if (this.Assembly == null)
            {
                throw new InvalidOperationException(Resources.CannotResolveModuleRefOnNetModule);
            }

            StringBuilder szName = StringBuilderPool.Get();
            int chName;

            var import = this.RawImport;
            import.GetModuleRefProps(moduleRefToken.Value, null, 0, out chName);
            szName.EnsureCapacity(chName);
            import.GetModuleRefProps(moduleRefToken.Value, szName, szName.Capacity, out chName);

            string text = szName.ToString();
            StringBuilderPool.Release(ref szName);
            return this.Assembly.GetModule(text);
        }

        /// <summary>
        /// Lookup a TypeDef token for a given top-level type name. Does not handle nested classes.
        /// This is useful for looking up system types (Enum, Int32, etc)</summary>
        /// <param name="className">top-level type name to lookup</param>
        /// <returns>typedef token of type. Throws it type not found.</returns>
        internal Token LookupTypeToken(string className)
        {
            return this.FindTypeDefByName(null, className, true);
        }

        // Find the TypeDef token in the given scope for a given class name.
        // Throw on error.
        internal Token FindTypeDefByName(Type outerType, string className, bool fThrow)
        {
            Token outerTypeToken = new Token(0);
            if (outerType != null)
            {
                //The outer type must always come from the same assembly as its nested types, so the outer type's token
                //resolver must be this.  This assumption is necessary because we need to pass the outer type's raw token
                //down to IMetaDataImport.
                if (outerType.Module != this)
                {
                    Debug.Assert(false, "Outer type has different token resolver");
                    throw new InvalidOperationException(Resources.DifferentTokenResolverForOuterType);
                }

                outerTypeToken = new Token(outerType.MetadataToken);
            }

            return FindTypeDefByName(outerTypeToken, className, fThrow);
        }

        // fThrow - true iff throw when the requested type is missing. 
        internal Token FindTypeDefByName(Token outerTypeDefToken, string className, bool fThrow)
        {
            if (!outerTypeDefToken.IsNil)
            {
                Debug.Assert(outerTypeDefToken.TokenType == TokenType.TypeDef);
                Debug.Assert(IsValidToken(outerTypeDefToken));
            }

            int token;
            int hr = this.RawImport.FindTypeDefByName(className, outerTypeDefToken, out token);
            if (!fThrow && (hr == unchecked((int)0x80131130)))
            {
                // Returns 0x80131130 on missing item.
                return Token.Nil;
            }

            if (hr != 0)
            {
                throw Marshal.GetExceptionForHR(hr);
            }

            Token t = new Token(token);
            Debug.Assert(t.IsType(TokenType.TypeDef));
            return t;
        }

        #endregion // Resolution

        #region IMetadataScope Members



        internal void GetMemberRefData(Token token, out Token declaringTypeToken, out string nameMember, out SignatureBlob sig)
        {
            if (!IsValidToken(token))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, token.ToString()));
            }

            uint size;

            EmbeddedBlobPointer ppvSigBlob;
            uint pbSig;

            var import = this.RawImport;
            import.GetMemberRefProps(token,
                                         out declaringTypeToken,
                                         null,
                                         0,
                                         out size,
                                         out ppvSigBlob,
                                         out pbSig
                                         );

            StringBuilder member = StringBuilderPool.Get((int)size);
            import.GetMemberRefProps(token,
                                         out declaringTypeToken,
                                         member,
                                         member.Capacity,
                                         out size,
                                         out ppvSigBlob,
                                         out pbSig
                                         );

            nameMember = member.ToString();
            StringBuilderPool.Release(ref member);
            sig = SignatureBlob.ReadSignature(this.RawMetadata, ppvSigBlob, (int)pbSig);
        }

        /// <summary>
        /// Get the RVA for the given method token. Return 0 if no RVA. 
        /// In some cases (dynamic modules, edit-and-continue), there may not be a usable RVA.
        /// </summary>
        /// <param name="methodDef">method def token to get the RVA for</param>
        /// <returns>the RVA of the method. </returns>
        internal uint GetMethodRva(int methodDef)
        {
            uint size;
            MethodAttributes pdwAttr;
            EmbeddedBlobPointer ppvSigBlob;
            uint pulCodeRVA, pdwImplFlags;
            uint pcbSigBlob;
            int classToken;

            var import = this.RawImport;
            import.GetMethodProps((uint)methodDef,
                                      out classToken,
                                      null,
                                      0,
                                      out size,
                                      out pdwAttr,
                                      out ppvSigBlob,
                                      out pcbSigBlob,
                                      out pulCodeRVA, // the RVA we want.
                                      out pdwImplFlags);

            return pulCodeRVA;
        }

        internal System.Reflection.MethodImplAttributes GetMethodImplFlags(int methodToken)
        {
            uint rva;
            uint implFlags;
            this.RawImport.GetRVA(methodToken, out rva, out implFlags);
            return (System.Reflection.MethodImplAttributes)implFlags;
        }

        internal void GetMethodAttrs(
            Token methodDef,
            out Token declaringTypeDef,
            out MethodAttributes attrs,
            out uint nameLength)
        {
            Debug.Assert(methodDef.IsType(TokenType.MethodDef));

            if (!IsValidToken(methodDef))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, methodDef.ToString()));
            }

            EmbeddedBlobPointer ppvSigBlob;
            uint pulCodeRVA, pdwImplFlags;
            uint pcbSigBlob;
            int classToken;

            uint methodToken = (uint)methodDef.Value;

            var import = this.RawImport;
            import.GetMethodProps(
                (uint)methodToken,
                 out classToken,
                 null,
                 0,
                 out nameLength,
                 out attrs,
                 out ppvSigBlob,
                 out pcbSigBlob,
                 out pulCodeRVA,
                 out pdwImplFlags);
            
            declaringTypeDef = new Token(classToken);
        }

        internal void GetMethodSig(
           Token methodDef,
           out SignatureBlob signature)
        {
            Debug.Assert(methodDef.IsType(TokenType.MethodDef));

            if (!IsValidToken(methodDef))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, methodDef.ToString()));
            }

            uint size;
            EmbeddedBlobPointer ppvSigBlob;
            uint pulCodeRVA, pdwImplFlags;
            uint pcbSigBlob;
            int classToken;
            MethodAttributes attrs;

            uint methodToken = (uint)methodDef.Value;

            var import = this.RawImport;
            import.GetMethodProps(
                (uint)methodToken,
                 out classToken,
                 null,
                 0,
                 out size,
                 out attrs,
                 out ppvSigBlob,
                 out pcbSigBlob,
                 out pulCodeRVA,
                 out pdwImplFlags);

            signature = SignatureBlob.ReadSignature(this.RawMetadata, ppvSigBlob, (int)pcbSigBlob);
        }

        internal void GetMethodName(
            Token methodDef,
            uint nameLength,
            out string name)
        {
            Debug.Assert(methodDef.IsType(TokenType.MethodDef));

            if (!IsValidToken(methodDef))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, methodDef.ToString()));
            }

            MethodAttributes pdwAttr;
            EmbeddedBlobPointer ppvSigBlob;
            uint pulCodeRVA, pdwImplFlags;
            uint pcbSigBlob;
            int classToken;

            uint methodToken = (uint)methodDef.Value;

            var import = this.RawImport;

            StringBuilder szMethodName = StringBuilderPool.Get((int)nameLength);
            import.GetMethodProps(
                (uint)methodToken,
                 out classToken,
                 szMethodName,
                 szMethodName.Capacity,
                 out nameLength,
                 out pdwAttr,
                 out ppvSigBlob,
                 out pcbSigBlob,
                 out pulCodeRVA,
                 out pdwImplFlags);

            name = szMethodName.ToString();
            StringBuilderPool.Release(ref szMethodName);
            // Normally we could retrieve the IL bytes from the code RVA.
        }

        //Get the underlying type of an Enum type.
        //The input parameter represents a type token of the Enum type.
        internal CorElementType GetEnumUnderlyingType(Token tokenTypeDef)
        {
            var import = this.RawImport;
            HCORENUM hEnum = new HCORENUM();
            try
            {
                int mdFieldDef;
                uint numFieldDefs;
                FieldAttributes fieldAttributes;
                int nameSize;
                int cPlusTypeFlab;
                IntPtr ppValue;
                int pcchValue;
                EmbeddedBlobPointer ppvSig;
                int size;
                int classToken;

                // From standard I.8.5.2a regarding Enums:
                // The underlying type is a built-in CLR integer type.
                // "It shall have exactly one instance filed, and the type of that field defines the underlying type of the enumeration"

                import.EnumFields(ref hEnum, tokenTypeDef.Value, out mdFieldDef, 1, out numFieldDefs);
                while (numFieldDefs != 0)
                {
                    import.GetFieldProps(mdFieldDef, out classToken, null, 0, out nameSize, out fieldAttributes, out ppvSig, out size, out cPlusTypeFlab, out ppValue, out pcchValue);
                    Debug.Assert(tokenTypeDef.Value == classToken);

                    // Enums should have one instance field that indicates the underlying type
                    if ((fieldAttributes & FieldAttributes.Static) == 0)
                    {
                        Debug.Assert(size == 2); // Primitive type field sigs should be two bytes long

                        byte[] sig = this.ReadEmbeddedBlob(ppvSig, size);

                        int index = 0;

                        CorCallingConvention callConv = SignatureUtil.ExtractCallingConvention(sig, ref index);
                        Debug.Assert(callConv == CorCallingConvention.Field);

                        return SignatureUtil.ExtractElementType(sig, ref index);
                    }

                    import.EnumFields(ref hEnum, tokenTypeDef.Value, out mdFieldDef, 1, out numFieldDefs);
                }
                Debug.Fail("Should never get here.");
                throw new ArgumentException(Resources.OperationValidOnEnumOnly);
            }
            finally
            {
                hEnum.Close(import);
            }
        }

        // Raw helper to get the TypeAttributes from the metadata tables. 
        internal void GetTypeAttributes(Token tokenTypeDef, out Token tokenExtends, out TypeAttributes attr, out int nameLength)
        {
            Debug.Assert(tokenTypeDef.IsType(TokenType.TypeDef));
            Debug.Assert(IsValidToken(tokenTypeDef));

            int tkExtends;
            var import = this.RawImport;
            import.GetTypeDefProps(
                tokenTypeDef.Value,
                null,
                0,
                out nameLength,
                out attr,
                out tkExtends);

            tokenExtends = new Token(tkExtends);
        }

        // Raw helper to get the TypeNames from the metadata tables. 
        internal void GetTypeName(Token tokenTypeDef, int nameLength, out string name)
        {
            Debug.Assert(tokenTypeDef.IsType(TokenType.TypeDef));
            Debug.Assert(IsValidToken(tokenTypeDef));

            TypeAttributes attr;
            int tkExtends;
            var import = this.RawImport;
            StringBuilder szTypedef = StringBuilderPool.Get(nameLength);
            import.GetTypeDefProps(
                tokenTypeDef.Value,
                szTypedef,
                szTypedef.Capacity,
                out nameLength,
                out attr,
                out tkExtends);

            //Type names (including the namespace) in the metadata may contain special characters.
            //Need to quote such names to match the reflection behavior.
            name = TypeNameQuoter.GetQuotedTypeName(szTypedef.ToString());
            StringBuilderPool.Release(ref szTypedef);
            Debug.Assert(name.Length >= 1);
        }

        /// <summary>
        /// Get the constructors that match flags on a given type. 
        /// </summary>
        internal static ConstructorInfo[] GetConstructorsOnType(MetadataOnlyCommonType type, System.Reflection.BindingFlags flags)
        {
            CheckBindingFlagsInMethod(flags, "GetConstructorsOnType");

            List<ConstructorInfo> result = new List<ConstructorInfo>();
            const bool isInherited = false;

            var constructors = type.GetDeclaredConstructors();
            
            foreach (ConstructorInfo constructorInfo in constructors)
            {
                if (Utility.IsBindingFlagsMatching(constructorInfo, isInherited, flags))
                {
                    result.Add(constructorInfo);
                }
            }

            return result.ToArray();
        }

        internal static ConstructorInfo GetConstructorOnType(
            MetadataOnlyCommonType type, BindingFlags bindingAttr, Binder binder,
            CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            CheckBinderAndModifiersforLMR(binder, modifiers);

            var methods = GetConstructorsOnType(type, bindingAttr);
            foreach (ConstructorInfo m in methods)
            {
                if (!SignatureUtil.IsCallingConventionMatch(m, callConvention))
                {
                    continue;
                }

                if (!SignatureUtil.IsParametersTypeMatch(m, types))
                {
                    continue;
                }
                return m;
            }

            return null;
        }

        static private void CheckBinderAndModifiersforLMR(Binder binder, ParameterModifier[] modifiers)
        {
            //binder must be null for LMR.
            if (binder != null)
            {
                throw new NotSupportedException();
            }

            //ParameterModifier is not handled by LMR
            if (modifiers != null && modifiers.Length != 0)
            {
                throw new NotSupportedException();
            }
        }

        // Common helper for multiple Type classes to use. GetMethodImpl is really just a filter on
        // GetMethods(). This does the filtering and returns the match. 
        internal static MethodInfo GetMethodImplHelper(
            Type type,
            String name, 
            BindingFlags bindingAttr, 
            Binder binder, 
            CallingConventions callConv,
            Type[] types, 
            ParameterModifier[] modifiers)
        {
            //ParameterModifier is not handled by LMR but binder is.
            if (modifiers != null && modifiers.Length != 0)
            {
                throw new NotSupportedException();
            }

            //methods are sorted in the order of inheritance,
            //the most derived is at the first.

            var methods = type.GetMethods(bindingAttr);

            if (binder == null)
            {
                return FilterMethod(methods, name, bindingAttr, callConv, types);
            }

            // Create array of candidates for custom binder based on method name and 
            // calling convention. 
            List<MethodBase> candidates = new List<MethodBase>();
            StringComparison comparison = SignatureUtil.GetStringComparison(bindingAttr);
            foreach (MethodInfo m in methods)
            {
                if (!m.Name.Equals(name, comparison))
                {
                    continue;
                }

                if (!SignatureUtil.IsCallingConventionMatch(m, callConv))
                {
                    continue;
                }

                candidates.Add(m);
            }

            return binder.SelectMethod(bindingAttr, candidates.ToArray(), types, modifiers) as MethodInfo;
        }

        /// <summary>
        /// Find the method matching all the criteria in the method array.
        /// </summary>
        private static MethodInfo FilterMethod(
            MethodInfo[] methods,
            String name,
            BindingFlags bindingAttr,
            CallingConventions callConv,
            Type[] types)
        {
            bool found = false;
            MethodInfo match = null;

            StringComparison comparison = SignatureUtil.GetStringComparison(bindingAttr);

            foreach (MethodInfo m in methods)
            {
                //if already found in the most derived type, no need to go further
                if (found && match.DeclaringType != null && !match.DeclaringType.Equals(m.DeclaringType))
                {
                    break;
                }

                if (!m.Name.Equals(name, comparison))
                {
                    continue;
                }

                if (!SignatureUtil.IsCallingConventionMatch(m, callConv))
                {
                    continue;
                }

                if (!SignatureUtil.IsParametersTypeMatch(m, types))
                {
                    continue;
                }

                if (!found)
                {
                    match = m;
                    found = true;
                }
                else
                {
                    throw new AmbiguousMatchException();
                }
            }

            return match;
        }

        /// <summary>
        /// Returns the methods (but not constructors) on the given Type and its base types (if requested in the flags).
        /// </summary>
        /// <remarks>
        /// This a helper function shared by multiple MetadataOnlyCommonType derivations to implement: 
        ///     MethodInfo[] Type.GetMethods(...) 
        /// </remarks>
        internal static MethodInfo[] GetMethodsOnType(MetadataOnlyCommonType type, System.Reflection.BindingFlags flags)
        {
            CheckBindingFlagsInMethod(flags, "GetMethodsOnType");

            List<MethodInfo> result = new List<MethodInfo>();
            const bool isInherited = false;

            // Get the methods that match binding flags on the type itself first.
            foreach (MethodInfo methodInfo in type.GetDeclaredMethods())
            {
                if (Utility.IsBindingFlagsMatching(methodInfo, isInherited, flags))
                {
                    result.Add(methodInfo);
                }
            }

            // Get the methods on the base type if there is a base type and if flag requests them.
            // We can't use base type's Resolver since base type might not be a LMR type.
            //
            if (WalkInheritanceChain(flags) && (type.BaseType != null))
            {
                MethodInfo[] inheritedMethods = type.BaseType.GetMethods(flags);
                List<MethodInfo> filteredInheritedMembers = new List<MethodInfo>();

                // Filter out any methods that don't match flags or that are overriden by methods on the type
                // itself. There should be no overrides in inheritedMethods array if base type(s) correctly 
                // implement GetMethods(...) method.
                foreach (MethodInfo methodInfo in inheritedMethods)
                {
                    if (IncludeInheritedMethod(methodInfo, result, flags))
                    {
                        filteredInheritedMembers.Add(methodInfo);
                    }
                }

                result.AddRange(filteredInheritedMembers);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Determines if walk up the inheritance chain is requested based on flags. 
        /// </summary>
        private static bool WalkInheritanceChain(System.Reflection.BindingFlags flags)
        {
            // If DeclaredOnly is specified, inheritance chain doesn't need to be examined.
            if ((flags & BindingFlags.DeclaredOnly) != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Filters inherited properties by eliminating overloads.
        /// </summary>
        private static IList<PropertyInfo> FilterInheritedProperties(
            IList<PropertyInfo> inheritedProperties,
            IList<PropertyInfo> properties,
            System.Reflection.BindingFlags flags)
        {
            if ((properties == null) || (properties.Count == 0))
            {
                // If there is no properties to filter against, just return 
                // original list.
                return inheritedProperties;
            }

            List<PropertyInfo> result = new List<PropertyInfo>();
            List<MethodInfo> getters = new List<MethodInfo>();
            List<MethodInfo> setters = new List<MethodInfo>();

            // Create separate lists of getters and setters for efficient 
            // overload checking.
            foreach (PropertyInfo property in properties)
            {
                MethodInfo getter = property.GetGetMethod();
                if (getter != null)
                {
                    getters.Add(getter);
                }

                MethodInfo setter = property.GetSetMethod();
                if (setter != null)
                {
                    setters.Add(setter);
                }
            }

            // If either setter or getter doesn't match flags or is overloaded,
            // we don't include the whole property. 
            foreach (PropertyInfo property in inheritedProperties)
            {
                MethodInfo getter = property.GetGetMethod();
                if ((getter != null) && !IncludeInheritedAccessor(getter, getters, flags))
                {
                    continue;
                }

                MethodInfo setter = property.GetSetMethod();
                if ((setter != null) && !IncludeInheritedAccessor(setter, setters, flags))
                {
                    continue;
                }

                result.Add(property);
            }

            return result;
        }

        /// <summary>
        /// Filters inherited events by eliminating overloads. Overload in case of events is 
        /// simply any inherited event that has the same name as an event directly on a type. 
        /// </summary>
        private static IList<EventInfo> FilterInheritedEvents(IList<EventInfo> inheritedEvents, IList<EventInfo> events)
        {
            if ((events == null) || (events.Count == 0))
            {
                // If there is no events to filter against, just return 
                // original list.
                return inheritedEvents;
            }

            List<EventInfo> result = new List<EventInfo>();

            // Compare name of each inherited event with events on a type itself and 
            // skip any that already exists on type.
            foreach (EventInfo inheritedEvent in inheritedEvents)
            {
                bool nameMatchFound = false;
                foreach (EventInfo directEvent in events)
                {
                    if (inheritedEvent.Name.Equals(directEvent.Name, StringComparison.Ordinal))
                    {
                        nameMatchFound = true;
                        break;
                    }
                }

                if (!nameMatchFound)
                {
                    result.Add(inheritedEvent);
                }
            }

            return result;
        }

        /// <summary>
        /// Determines if an inherited method should be included when walking up inheritance chain.
        /// </summary>
        private static bool IncludeInheritedMethod(MethodInfo inheritedMethod, IEnumerable<MethodInfo> methods, System.Reflection.BindingFlags flags)
        {
            if (!inheritedMethod.IsStatic)
            {
                // Inherited instance members should always be included unless
                // they are virtual and explicitly overriden.
                if (inheritedMethod.IsVirtual)
                {
                    return !IsOverride(methods, inheritedMethod);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                // Inherited static members should be included only when 
                // FlattenHierarchy is specified. It doesn't matter if there
                // are methods with matching signature on the derived class already. 
                if ((flags & BindingFlags.FlattenHierarchy) != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines if an inherited property getter/setter should be included when walking up inheritance chain.
        /// </summary>
        private static bool IncludeInheritedAccessor(
            MethodInfo inheritedMethod,
            IEnumerable<MethodInfo> methods,
            System.Reflection.BindingFlags flags)
        {
            if (!inheritedMethod.IsStatic)
            {
                // Inherited instance getters/setters are included unless
                // they are explicitly overriden or hidden. For properties, it
                // does not matter if they are virtual or not (as opposed to methods). 
                return !IsOverride(methods, inheritedMethod);
            }
            else
            {
                // Inherited static getters/setters should be included only when 
                // FlattenHierarchy is specified and there are no other properties
                // with matching signature already. 
                if ((flags & BindingFlags.FlattenHierarchy) != 0)
                {
                    return !IsOverride(methods, inheritedMethod);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines if an inherited field should be included when walking up inheritance chain.
        /// </summary>
        private static bool IncludeInheritedField(FieldInfo inheritedField, System.Reflection.BindingFlags flags)
        {
            if (inheritedField.IsPrivate)
            {
                // Private inherited fields should never be included.
                return false;
            }
            if (!inheritedField.IsStatic)
            {
                // Inherited public/protected instance fields should be included. 
                return true;
            }
            else if ((flags & BindingFlags.FlattenHierarchy) != 0)
            {
                // Public/protected static fields should be included only when FlattenHierarchy is requested.
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Filter to use with code:GetMethodsOnDeclaredTypeOnly
        /// </summary>
        internal enum EMethodKind
        {
            Constructor,
            Methods
        }

        /// <summary>
        /// Common helper function for
        ///   MethodInfo[] Type.GetMethods(...) and 
        ///   ConstructorInfo[] Type.GetConstructor().
        /// Gets just the methods and constructors that this type implements, not the ones it inherits. 
        /// </summary>
        /// <remarks>
        /// This is on the TokenResolver so that it can be shared by multiple Type implementations. 
        /// </remarks>
        internal IEnumerable<MethodBase> GetMethodBasesOnDeclaredTypeOnly(Token tokenTypeDef, GenericContext context, EMethodKind kind)
        {
            IMetadataImport import = this.RawImport;
            int methodToken;

            HCORENUM hEnum = new HCORENUM();
            try
            {
                while (true)
                {
                    int size;
                    import.EnumMethods(ref hEnum, tokenTypeDef.Value, out methodToken, 1, out size);
                    if (size == 0)
                        break;

                    List<Type> genericParams = GetTypeParameters(methodToken);
                    GenericContext newContext = new GenericContext(context.TypeArgs, genericParams.ToArray());

                    MethodBase methodBase = this.GetGenericMethodBase(new Token(methodToken), newContext);

                    if ((methodBase is ConstructorInfo) != (kind == EMethodKind.Constructor))
                        continue;

                    yield return methodBase;
                }
            }
            finally
            {
                hEnum.Close(import);
            }
        }

        //Get generic type parameters for a type or method token
        private List<Type> GetTypeParameters(int token)
        {
            List<Type> result = new List<Type>();
            foreach (int gpTokenValue in GetGenericParameterTokens(token))
            {
                Token gpToken = new Token(gpTokenValue);
                if (gpToken.IsType(TokenType.GenericPar))
                {
                    result.Add(this.Factory.CreateTypeVariable(this, gpToken));
                }
            }
            return result;
        }

        // Signature comparer helper GetMethodsOnType() 
        // m1 may have unresolved generic args. 
        // methodCandidate should always be a fully resolved type.
        static bool MatchSignatures(MethodBase m1, MethodBase methodCandidate)
        {
            Debug.Assert(m1 != null);
            Debug.Assert(methodCandidate != null);

            if (m1.Name != methodCandidate.Name)
            {
                // Check for explicit interfaces. Explicit interface impls may contain the interface type in
                // the method name. See Type.GetInterfaceMapping().
                bool fIsExplicitInterface = (m1.Name.Length > methodCandidate.Name.Length && 
                    m1.Name[m1.Name.Length - methodCandidate.Name.Length - 1] == '.' &&  
                    m1.Name.EndsWith(methodCandidate.Name, StringComparison.Ordinal));

                if (!fIsExplicitInterface)
                {
                    return false;
                }
            }

            if (m1.IsStatic != methodCandidate.IsStatic)
                return false;

            ParameterInfo[] p1 = m1.GetParameters();
            ParameterInfo[] pCandidate = methodCandidate.GetParameters();

            if (p1.Length != pCandidate.Length)
                return false;

            // This may need to be ContainsGenericArguments.
            if (m1.IsGenericMethodDefinition)
            {
                Type[] args = methodCandidate.GetGenericArguments();
                m1 = (m1 as MethodInfo).MakeGenericMethod(args);
                p1 = m1.GetParameters();
            }

            // In order to compare parameter types, we need both methods fully resolved. 
            Debug.Assert(!m1.IsGenericMethodDefinition);

            // Match parameter types.
            for (int i = 0; i < p1.Length; i++)
            {
                Type t1 = p1[i].ParameterType;
                Type t2 = pCandidate[i].ParameterType;
                Debug.Assert(t1 != null);
                Debug.Assert(t2 != null);

                if (!t1.Equals(t2))
                    return false;
            }

            //check on return types. 
            MethodInfo mi1 = m1 as MethodInfo;
            MethodInfo mi2 = methodCandidate as MethodInfo;
            if ((mi1 != null && mi2 == null) ||
                (mi1 == null && mi2 != null))
            {
                return false;
            }
            else if (mi1 != null)
            {
                Type retType1 = mi1.ReturnType;
                if (!retType1.Equals(mi2.ReturnType))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///Check if method m overrides any of passed methods.       
        /// </summary>
        static private bool IsOverride(IEnumerable<MethodInfo> methods, MethodInfo m)
        {
            foreach (MethodInfo method in methods)
            {
                if (IsOverride(method, m))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///Check if two methods override each other.       
        /// </summary>
        static private bool IsOverride(MethodInfo m1, MethodInfo m2)
        {
            return MatchSignatures(m1, m2);
        }

        /// <summary>
        /// Returns the fields on the given Type and its base types (if requested in the flags).
        /// </summary>
        /// <remarks>
        /// This a helper function shared by multiple MetadataOnlyCommonType derivations to implement: 
        ///     FieldInfo[] Type.GetFields(...) 
        /// </remarks>
        internal static FieldInfo[] GetFieldsOnType(MetadataOnlyCommonType type, System.Reflection.BindingFlags flags)
        {
            CheckBindingFlagsInMethod(flags, "GetFieldsOnType");

            List<FieldInfo> result = new List<FieldInfo>();
            const bool isInherited = false;

            // Get the fields that match binding flags on the type itself first.
            foreach (FieldInfo fieldInfo in type.Resolver.GetFieldsOnDeclaredTypeOnly(new Token(type.MetadataToken), type.GenericContext))
            {
                if (Utility.IsBindingFlagsMatching(fieldInfo, isInherited, flags))
                {
                    result.Add(fieldInfo);
                }
            }

            // Get the fields on the base type if there is a base type and if flag requests them.
            // We can't use base type's Resolver since base type might not be a LMR type.
            //
            if (WalkInheritanceChain(flags) && (type.BaseType != null))
            {
                FieldInfo[] inheritedFields = type.BaseType.GetFields(flags);
                List<FieldInfo> filteredInheritedFields = new List<FieldInfo>();

                // Filter out any fields that don't match flags.
                foreach (FieldInfo fieldInfo in inheritedFields)
                {
                    if (IncludeInheritedField(fieldInfo, flags))
                    {
                        filteredInheritedFields.Add(fieldInfo);
                    }
                }

                result.AddRange(filteredInheritedFields);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Gets fields on a specified type using IMetadataImport API. Does not get fields 
        /// on base classes.
        /// </summary>
        private IEnumerable<FieldInfo> GetFieldsOnDeclaredTypeOnly(Token typeDefToken, GenericContext context)
        {
            HCORENUM hEnum = new HCORENUM();
            IMetadataImport import = this.RawImport;

            var typeArgs = Type.EmptyTypes;
            var methodArgs = Type.EmptyTypes;
            if (context != null)
            {
                typeArgs = context.TypeArgs;
                methodArgs = context.MethodArgs;
            }

            int fieldToken;
            try
            {
                while (true)
                {
                    uint size;
                    import.EnumFields(ref hEnum, typeDefToken, out fieldToken, 1, out size);
                    if (size == 0)
                        break;

                    // This does not do caching. We eventually need to let the resolver own the caching.
                    FieldInfo fieldInfo = this.Factory.CreateField(this, new Token(fieldToken), typeArgs, methodArgs);
                    yield return fieldInfo;
                }
            }
            finally
            {
                hEnum.Close(import);
            }
        }

        /// <summary>
        /// Returns the properties on the given Type and its base types (if requested in the flags).
        /// </summary>
        /// <remarks>
        /// This a helper function shared by multiple MetadataOnlyCommonType derivations to implement: 
        ///     MethodInfo[] Type.GetProperties(...) 
        /// </remarks>
        internal static PropertyInfo[] GetPropertiesOnType(MetadataOnlyCommonType type, System.Reflection.BindingFlags flags)
        {
            CheckBindingFlagsInMethod(flags, "GetPropertiesOnType");

            List<PropertyInfo> result = new List<PropertyInfo>();
            bool isInherited = false;

            // Get the properties that match binding flags on the type itself first.
            foreach (PropertyInfo propertyInfo in type.GetDeclaredProperties())
            {
                bool isStatic = false;
                bool isPublic = false;
                CheckIsStaticAndIsPublicOnProperty(propertyInfo, ref isStatic, ref isPublic);

                if (Utility.IsBindingFlagsMatching(propertyInfo, isStatic, isPublic, isInherited, flags))
                {
                    result.Add(propertyInfo);
                }
            }

            // Get the properties on the base type if there is a base type and if flag requests them.
            // We can't use base type's Resolver since base type might not be a LMR type.
            //
            if (WalkInheritanceChain(flags) && (type.BaseType != null))
            {
                PropertyInfo[] inheritedProperties = type.BaseType.GetProperties(flags);

                // Filter out any properties that are overriden by properties on the type
                // itself. There should be no overrides in inheritedProperties array if base type(s) correctly 
                // implement GetProperties(...) method.
                IList<PropertyInfo> filteredInheritedProperties = FilterInheritedProperties(inheritedProperties, result, flags);
                result.AddRange(filteredInheritedProperties);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Common helper function for
        ///   PropertyInfo[] Type.GetProperties(...) 
        /// Gets just properties that this type implements, not the ones it inherits. 
        /// </summary>
        /// <remarks>
        /// This is on the TokenResolver so that it can be shared by multiple Type implementations. 
        /// </remarks>
        internal IEnumerable<PropertyInfo> GetPropertiesOnDeclaredTypeOnly(Token tokenTypeDef, GenericContext context)
        {
            HCORENUM hEnum = new HCORENUM();
            var import = this.RawImport;

            int propertyToken;
            try
            {
                while (true)
                {
                    uint size;
                    import.EnumProperties(ref hEnum, tokenTypeDef.Value, out propertyToken, 1, out size);
                    if (size == 0)
                        break;

                    PropertyInfo property = this.Factory.CreatePropertyInfo(this, new Token(propertyToken), context.TypeArgs, context.MethodArgs);
                    yield return property;
                }
            }
            finally
            {
                hEnum.Close(import);
            }
        }

        /// <summary>
        /// Returns the events on the given Type and its base types (if requested in the flags).
        /// </summary>
        /// <remarks>
        /// This a helper function shared by multiple MetadataOnlyCommonType derivations to implement: 
        ///     MethodInfo[] Type.GetEvents(...) 
        /// </remarks>
        static internal EventInfo[] GetEventsOnType(MetadataOnlyCommonType type, System.Reflection.BindingFlags flags)
        {
            CheckBindingFlagsInMethod(flags, "GetEventsOnType");

            List<EventInfo> result = new List<EventInfo>();
            const bool isInherited = false;

            // Get the events that match binding flags on the type itself first.
            foreach (EventInfo eventInfo in type.Resolver.GetEventsOnDeclaredTypeOnly(new Token(type.MetadataToken), type.GenericContext))
            {
                bool isStatic = false;
                bool isPublic = false;
                CheckIsStaticAndIsPublicOnEvent(eventInfo, ref isStatic, ref isPublic);

                if (Utility.IsBindingFlagsMatching(eventInfo, isStatic, isPublic, isInherited, flags))
                {
                    result.Add(eventInfo);
                }
            }

            // Get the properties on the base type if there is a base type and if flag requests them.
            // We can't use base type's Resolver since base type might not be a LMR type.
            //
            if (WalkInheritanceChain(flags) && (type.BaseType != null))
            {
                EventInfo[] inheritedEvents = type.BaseType.GetEvents(flags);

                // Filter out any events that are hidden by events on the type
                // itself. There should be no overrides in inheritedEvents array if base type(s) correctly 
                // implement GetEvents(...) method.
                IList<EventInfo> filteredInheritedEvents = FilterInheritedEvents(inheritedEvents, result);
                result.AddRange(filteredInheritedEvents);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Common helper function for
        ///   PropertyInfo[] Type.GetEvents(...) 
        /// Gets just events that this type implements, not the ones it inherits. 
        /// </summary>
        /// <remarks>
        /// This is on the TokenResolver so that it can be shared by multiple Type implementations. 
        /// </remarks>
        private IEnumerable<EventInfo> GetEventsOnDeclaredTypeOnly(Token tokenTypeDef, GenericContext context)
        {
            HCORENUM hEnum = new HCORENUM();
            var import = this.RawImport;

            int eventToken;
            try
            {
                while (true)
                {
                    uint size;
                    import.EnumEvents(ref hEnum, tokenTypeDef.Value, out eventToken, 1, out size);
                    if (size == 0)
                        break;

                    EventInfo eventInfo = this.Factory.CreateEventInfo(this, new Token(eventToken), context.TypeArgs, context.MethodArgs);
                    yield return eventInfo;
                }
            }
            finally
            {
                hEnum.Close(import);
            }
        }


        internal IEnumerable<Type> GetNestedTypesOnType(MetadataOnlyCommonType type, BindingFlags flags)
        {
            return GetNestedTypesOnType(new Token(type.MetadataToken), flags);
        }

        //Lazily calculate all the nested type information and store in the field.
        //The key of the dictionary is the nesting type token.
        //The value of the dictionary is the nested types in the type of the key token.
        //If a type does not have nested types, its token doesn't appear in the dictionary.        
        //This is readonly data. Wrap the dictionary in its own class to enforce that.
        class NestedTypeCache
        {
            readonly private Dictionary<int, List<int>> m_cache;

            // Initialize the cache for the given module
            public NestedTypeCache(MetadataOnlyModule outer)
            {
                // Operate on local copy so that creation is thread-safe.
                m_cache = new Dictionary<int, List<int>>();

                var typeTokenList = outer.GetTypeTokenList();

                foreach (int token in typeTokenList)
                {
                    int enclosingTypeToken = outer.GetNestedClassProps(new Token(token));
                    if (enclosingTypeToken == 0)
                    {
                        continue;
                    }
                    if (m_cache.ContainsKey(enclosingTypeToken))
                    {
                        //if the type already has an entry in the dictionary, add the nested type
                        Debug.Assert(m_cache[enclosingTypeToken] != null);
                        m_cache[enclosingTypeToken].Add(token);
                    }
                    else
                    {
                        //create an entry for the type
                        List<int> nestedTypes = new List<int>();
                        nestedTypes.Add(token);
                        m_cache.Add(enclosingTypeToken, nestedTypes);
                    }
                }
            }

            // Get the typedef tokens for types nested in tokenTypeDef
            // Return null if there are no nestings.
            public IEnumerable<int> GetNestedTokens(Token tokenTypeDef)
            {
                List<int> list;
                if (this.m_cache.TryGetValue(tokenTypeDef, out list))
                {
                    return list;
                }
                return null;
            }
        }

        // This is conceptually read-only, but it's lazily initialized.
        NestedTypeCache m_nestedTypeInfo;

        // Ensure the dictionary containing the nested type information for all the types in
        // the module is initialized.
        void EnsureNestedTypeCacheExists()
        {
            if (m_nestedTypeInfo == null)
            {
                // In a race, we just double-initialize the data. 
                m_nestedTypeInfo = new NestedTypeCache(this);
            }
            Debug.Assert(m_nestedTypeInfo != null);
        }

        // This is on the TokenResolver so that it can be shared by multiple Type implementations. 
        // Get the nested types in this type definition.
        internal IEnumerable<Type> GetNestedTypesOnType(Token tokenTypeDef, BindingFlags flags)
        {
            CheckBindingFlagsInMethod(flags, "GetNestedTypesOnType");

            EnsureNestedTypeCacheExists();

            var e = m_nestedTypeInfo.GetNestedTokens(tokenTypeDef);
            if (e != null)
            {
                foreach (int typeToken in e)
                {
                    Type type = ResolveType(typeToken);
                    const bool isStatic = false;
                    bool isPublic = type.IsPublic || type.IsNestedPublic;

                    if (Utility.IsBindingFlagsMatching(type, isStatic, isPublic, false, flags))
                    {
                        yield return type;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all custom attributes on a member.
        /// </summary>
        /// <param name="memberTokenValue">Member's metadata token.</param>
        /// <returns>List of CustomAttributeData instances describing all custom attributes.</returns>
        public IList<CustomAttributeData> GetCustomAttributeData(int memberTokenValue)
        {
            List<CustomAttributeData> result = new List<CustomAttributeData>();
            HCORENUM hEnum = new HCORENUM();

            Token customAttributeTokenValue;
            Token customAttributeTargetTokenValue;
            Token customAttributeConstructorTokenValue; // could be methodDef or memberRef
            EmbeddedBlobPointer customAttributeBlob;
            int customAttributeBlobSize;

            var import = this.RawImport;
            try
            {
                while (true)
                {
                    uint size;
                    // Pass zero as tkType to get all custom attribute tokens.
                    import.EnumCustomAttributes(ref hEnum, memberTokenValue, 0, out customAttributeTokenValue, 1, out size);
                    if (size == 0)
                        break;

                    // Get attribute properties using token from enumeration.
                    import.GetCustomAttributeProps(
                        customAttributeTokenValue,
                        out customAttributeTargetTokenValue,
                        out customAttributeConstructorTokenValue,
                        out customAttributeBlob,
                        out customAttributeBlobSize);

                    Debug.Assert(memberTokenValue == customAttributeTargetTokenValue,
                        "Expected custom attribute properties to match passed parameter.");


                    // Resolve constructor. This must be a memberRef or methodDef token.
                    // This just creates a proxy that can gets us the declaring type and fullname.
                    // We'll parse the blob lazily.                    
                    ConstructorInfo ctor = ResolveCustomAttributeConstructor(customAttributeConstructorTokenValue);

                    CustomAttributeData data = new MetadataOnlyCustomAttributeData(this, customAttributeTokenValue, ctor);

                    result.Add(data);
                }
            }
            finally
            {
                hEnum.Close(import);
            }

            // Get PCAs if policy requires them.
            IEnumerable<CustomAttributeData> pseudoCustomAttributes =
                m_policy.GetPseudoCustomAttributes(this, new Token(memberTokenValue));
            result.AddRange(pseudoCustomAttributes);

            return result;
        }




        /// <summary>
        /// Return a ConstructorInfo proxy for the ctor token which can be used in a Custom Attribute information.  
        /// This proxy just supports getting the custom attribute name without resolving (eg, calling ctor.DeclaringType.FullName.)
        /// </summary>
        /// <param name="customAttributeConstructorTokenValue">token from custom attr  representing the contstructor. </param>
        /// <returns></returns>
        /// <remarks>
        /// Reflection API has Custom Attributes expose the constructor Info, but what clients really want
        /// is a fast way to get to the string name without resolution. 
        ///
        /// We want to do lazy resolution here:
        /// - performance: faster filtering of custom attributes. Clients just need the string name and
        ///    not the fully resolved constructor Info (which would require resolving all the type parameters
        ///    too).
        /// - avoid eager assembly resolution. CustomAttr args require assembly resolution, but not 
        ///    if we're just getting the attribute name,
        /// </remarks>
        ConstructorInfo ResolveCustomAttributeConstructor(Token customAttributeConstructorTokenValue)
        {
            // This is like a very specialized case of ResolveMethod(Token) and ResolveMethodImpl() which 
            // takes advantage of knowing that the token value is for a custom attr ctor. 

            Token methodToken = customAttributeConstructorTokenValue;

            // Metadata APIs expect valid tokens. We can get here from public surface, so validate it now. 
            EnsureValidToken(methodToken);

            // The Partition II "22.10 Custom Attributes" explicitly says:
            //   "Type shall index a valid row in the Method or MethodRef table.  
            //    That row shall be a constructor method (for the class of which this information forms an instance)" 
            //   
            // This means that: 
            // 1. It can't be a MethodSpec. 
            // 2. We can't encode generic args into the metadata. This means we don't need a generic context.
            // 2. We can explicitly cast to a ConstructorInfo. 
            if (methodToken.IsType(TokenType.MethodDef))
            {
                // MethodDef aren't generic, so don't need to pass context along
                MethodBase method = ResolveMethodDef(methodToken);
                return (ConstructorInfo)method;
            }
            else if (methodToken.IsType(TokenType.MemberRef))
            {
                // A MemberRef has no generic arguments by definition, else it would be a MethodSpec.
                // So pass in null for generic method arguments.                
                string methodName;
                SignatureBlob signatureBlob;
                Token declaringTypeToken;
                GetMemberRefData(methodToken, out declaringTypeToken, out methodName, out signatureBlob);

                // Resolve the type that contains the method
                Type declaringType = ResolveTypeTokenInternal(declaringTypeToken, null);

                return new ConstructorInfoRef(declaringType, this, methodToken);

            }
            // Custom attribute should not be a method spec.

            // Wrong token type. We could get here with corrupted metadata.
            throw new ArgumentException(Resources.MethodTokenExpected);
        }

        /// <summary>
        /// Parses the custom attribute blob and returns list of constructor arguments and
        /// list of named arguments. This allows lazily parsing the attribute blob.
        /// </summary>
        /// <param name="token">metadata token for the attribute instance. This can be used to lookup the
        /// attribute's parameter blob.</param>
        /// <param name="constructorInfo"> constructor for the attribute. </param>
        /// <param name="constructorArguments"> out parameter set to non-null (potentially 0-length)  list of constructor arguments. </param>
        /// <param name="namedArguments">out parameter set to non-null (potentially 0-length) list of named arguments. </param>
        internal void LazyAttributeParse(
            Token token,
            ConstructorInfo constructorInfo,
            out IList<CustomAttributeTypedArgument> constructorArguments,
            out IList<CustomAttributeNamedArgument> namedArguments)
        {
            var import = this.RawImport;

            Token customAttributeTargetTokenValue;
            Token customAttributeConstructorTokenValue; // could be methodDef or memberRef
            EmbeddedBlobPointer customAttributeBlobPtrStart;
            int customAttributeBlobSize;

            // Get attribute properties using token from enumeration.
            import.GetCustomAttributeProps(
                token,
                out customAttributeTargetTokenValue,
                out customAttributeConstructorTokenValue,
                out customAttributeBlobPtrStart,
                out customAttributeBlobSize);

            byte[] customAttributeBlob = this.RawMetadata.ReadEmbeddedBlob(customAttributeBlobPtrStart, customAttributeBlobSize);

            // customAttributeConstructorTokenValue may be a MemberRef, but it should resolve to the
            // constructorInfo passed in. 
            Debug.Assert(constructorInfo != null, "Constructor field must be initialized.");

            int index = 0;

            // Verify and skip prolog.
            if (BitConverter.ToInt16(customAttributeBlob, index) != 0x0001)
            {
                throw new ArgumentException(Resources.InvalidCustomAttributeFormat);
            }
            index += 2;

            constructorArguments = GetConstructorArguments(constructorInfo, customAttributeBlob, ref index);
            namedArguments = GetNamedArguments(constructorInfo, customAttributeBlob, ref index);
        }

        /// <summary>
        /// Parses the custom attribute blob and returns list of constructor arguments. Assumes that prolog is 
        /// already verified and skipped.
        /// </summary>
        private IList<CustomAttributeTypedArgument> GetConstructorArguments(
            ConstructorInfo constructorInfo,
            byte[] customAttributeBlob,
            ref int index)
        {
            // Get information about constructor parameters (type & count) from construction info, avoiding
            // a resolve (using just the methodRef signature) if possible.
            // Note that it's important that we rely only on parameter type here.  Other information (name,
            // in/out flags, etc.) is not part of the methodRef signature.
            ParameterInfo[] parameters;
            var constructorRef = constructorInfo as ConstructorInfoRef;
            if (constructorRef != null)
                parameters = constructorRef.GetSignatureParameters();
            else
                parameters = constructorInfo.GetParameters();

            // Get actual parameter values from metadata blob.
            IList<CustomAttributeTypedArgument> constructorArguments = new List<CustomAttributeTypedArgument>(parameters.Length);

            // Enumerate all typed arguments and read their values.
            for (int i = 0; i < parameters.Length; i++)
            {
                // TODO: getting parameter types might require resolving (e.g. for enums), we need to deal with it better.
                Type argumentType = parameters[i].ParameterType;
                CorElementType typeId = SignatureUtil.GetTypeId(argumentType);

                object argumentValue = null;
                Type actualArgumentType = null;

                if (typeId != CorElementType.Object)
                {
                    // Extract strongly typed argument value. We already have its type from constructor info.
                    argumentValue = GetCustomAttributeArgumentValue(typeId, argumentType, customAttributeBlob, ref index);
                    actualArgumentType = argumentType;
                }
                else
                {
                    // Construct a type for argument whose static type was System.Object. Its actual type must be 
                    // one of the primitive types.
                    CorElementType actualArgumentTypeId;
                    SignatureUtil.ExtractCustomAttributeArgumentType(
                        this.AssemblyResolver,
                        this,
                        customAttributeBlob,
                        ref index,
                        out actualArgumentTypeId,
                        out actualArgumentType);

                    // Extract actual argument value (single or an array).
                    argumentValue = GetCustomAttributeArgumentValue(actualArgumentTypeId, actualArgumentType, customAttributeBlob, ref index);
                }

                CustomAttributeTypedArgument constructorArgument = new CustomAttributeTypedArgument(actualArgumentType, argumentValue);
                constructorArguments.Add(constructorArgument);
            }

            return constructorArguments;
        }


        /// <summary>
        /// Parses the custom attribute blob and returns list of named arguments. Assumes that typed
        /// arguments are already parsed.
        /// </summary>
        private IList<CustomAttributeNamedArgument> GetNamedArguments(
            ConstructorInfo constructorInfo,
            byte[] customAttributeBlob,
            ref int index)
        {
            // Enumerate all named arguments and read their values.
            // Number of named arguments is stored in an unsigned int16 field - even when there are no named
            // arguments. In that case the field contains zero and ends CA blob.
            ushort numberOfNamedArguments = BitConverter.ToUInt16(customAttributeBlob, index);
            index += 2;
            IList<CustomAttributeNamedArgument> namedArguments = new List<CustomAttributeNamedArgument>(numberOfNamedArguments);

            if ((numberOfNamedArguments == 0) && (index != customAttributeBlob.Length))
            {
                throw new ArgumentException(Resources.InvalidCustomAttributeFormat);
            }

            for (int i = 0; i < numberOfNamedArguments; i++)
            {
                // Determine if argument is a field or property.
                NamedArgumentType argumentType = SignatureUtil.ExtractNamedArgumentType(customAttributeBlob, ref index);

                // Extract argument type.
                CorElementType namedArgumentTypeId;
                Type namedArgumentType;
                SignatureUtil.ExtractCustomAttributeArgumentType(
                    this.AssemblyResolver,
                    this,
                    customAttributeBlob,
                    ref index,
                    out namedArgumentTypeId,
                    out namedArgumentType);

                // Extract argument name.
                string argumentName = SignatureUtil.ExtractStringValue(customAttributeBlob, ref index);

                // Extract real argument type if needed.
                // In cases when argument is "boxed" we don't know its real type until we skip 
                // argument name. 
                if (namedArgumentType == null)
                {
                    SignatureUtil.ExtractCustomAttributeArgumentType(
                        this.AssemblyResolver,
                        this,
                        customAttributeBlob,
                        ref index,
                        out namedArgumentTypeId,
                        out namedArgumentType);
                }

                // Extract argument value.
                object namedArgumentValue = GetCustomAttributeArgumentValue(namedArgumentTypeId, namedArgumentType, customAttributeBlob, ref index);

                // TODO: we need to have LMRFieldInfo and LMRPropInfo constructors that accept just name
                // plus whatever else we have from CA blob once we switch to not resolve everything by default.
                MemberInfo memberInfo;
                if (argumentType == NamedArgumentType.Field)
                {
                    memberInfo = constructorInfo.DeclaringType.GetField(argumentName, BindingFlags.Instance | BindingFlags.Public);
                }
                else
                {
                    memberInfo = constructorInfo.DeclaringType.GetProperty(argumentName);
                }
                Debug.Assert(memberInfo != null, "Expected to find appropriate field/property on an attribute instance.");

                CustomAttributeTypedArgument typedArgument = new CustomAttributeTypedArgument(namedArgumentType, namedArgumentValue);
                CustomAttributeNamedArgument namedArgument = new CustomAttributeNamedArgument(memberInfo, typedArgument);
                namedArguments.Add(namedArgument);
            }

            if (index != customAttributeBlob.Length)
            {
                throw new ArgumentException(Resources.InvalidCustomAttributeFormat);
            }

            return namedArguments;
        }

        /// <summary>
        /// Gets custom attribute argument's value from the blob.
        /// </summary>
        /// <param name="typeId">Value's type ID.</param>
        /// <param name="type">Value's type. Only needed for values that are arrays. Ignored for other values.</param>
        /// <param name="customAttributeBlob">Blob that contains custom attribute encoding.</param>
        /// <param name="index">Current index into the blob.</param>
        /// <returns>Custom attributes argument's value.</returns>
        private object GetCustomAttributeArgumentValue(CorElementType typeId, Type type, byte[] customAttributeBlob, ref int index)
        {
            Debug.Assert(typeId != CorElementType.Object, "Type can't be object.");

            object value = null;
            switch (typeId)
            {
                case CorElementType.Type:
                    value = SignatureUtil.ExtractTypeValue(this.AssemblyResolver, this, customAttributeBlob, ref index);
                    break;

                case CorElementType.SzArray:
                    // Get array size and type of its arguments.
                    uint arraySize = SignatureUtil.ExtractUIntValue(customAttributeBlob, ref index);

                    // Check if array is null and if not, extract all the elements. 
                    if (arraySize != 0xFFFFFFFF)
                    {
                        value = SignatureUtil.ExtractListOfValues(
                            type.GetElementType(),
                            this.AssemblyResolver,
                            this,
                            arraySize,
                            customAttributeBlob,
                            ref index);
                    }

                    break;

                case CorElementType.Enum:
                    Debug.Assert(type.IsEnum, "Expected Enum type.");

                    Type enumUnderlyingType = MetadataOnlyModule.GetUnderlyingType(type);
                    CorElementType enumUnderlyingTypeId = SignatureUtil.GetTypeId(enumUnderlyingType);
                    value = SignatureUtil.ExtractValue(enumUnderlyingTypeId, customAttributeBlob, ref index);

                    break;

                default:
                    // Use Assert instead of exception because typeId comes from our code, which should never pass invalid one here.
                    Debug.Assert(SignatureUtil.IsValidCustomAttributeElementType(typeId), "Invalid element type for custom attribute argument.");

                    value = SignatureUtil.ExtractValue(typeId, customAttributeBlob, ref index);
                    break;
            }

            return value;
        }

        /// <summary>
        /// Finds the underlying type of an enum type.
        /// </summary>
        /// <remarks>We might need to expose this on our own Enum derivation if it turns out that our types
        /// can't be passed to System.Enum.GetUnderlyingType.</remarks>
        internal static Type GetUnderlyingType(Type enumType)
        {
#if false // USE_CLR_V4
            // Latest CLR V4 bits has a bug, this still doesn't work...
            // CLR V4 implementation handles non-CLR types.
            return System.Enum.GetUnderlyingType(enumType);
#else
            // For mock definitions, we need an implementation of System.Enum.GetUnderlyingType that works on non-CLR types.
            Debug.Assert(enumType.IsEnum, "enumType must be an Enum.");

            // We can't rely on enum's field name to always be "value__" since non-MS code generators
            // can use any name. The name is not part of the standard. The CLS just says that there must be
            // one and only one instance field on an enum type. 

            FieldInfo[] valueFields = enumType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(valueFields != null, "Enums must have public instance fields.");
            Debug.Assert(valueFields.Length == 1, "Enums must have exactly one public instance field.");

            return valueFields[0].FieldType;
#endif
        }

        // This is on the TokenResolver so that it can be shared by multiple Type implementations. 
        // Get the nested types in this type definition.
        internal Type GetEnclosingType(Token tokenTypeDef)
        {
            Token enclosingTypeToken = new Token(GetNestedClassProps(tokenTypeDef));

            if (enclosingTypeToken.IsNil)
            {
                return null;
            }
            //TODO: The enclosing type may not be closed after generics is added.
            return this.ResolveTypeTokenInternal(enclosingTypeToken, null);
        }
        #endregion


        /// <summary>
        /// Gets the Assembly Name for the given assembly ref token.
        /// </summary>
        public AssemblyName GetAssemblyNameFromAssemblyRef(Token assemblyRefToken)
        {
            IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)this.RawImport;
            return AssemblyNameHelper.GetAssemblyNameFromRef(assemblyRefToken, this, assemblyImport);
        }


        // Wrapper for IMetadataImport::GetNestedClassProps
        // If this is a nested type, returns the token for the outer type.
        // Else returns a 0 token if the type is not nested. 
        internal Token GetNestedClassProps(Token tokenTypeDef)
        {
            int tok;
            int hr = this.RawImport.GetNestedClassProps(tokenTypeDef, out tok);
            if (hr == 0)
            {
                //Success
                return new Token(tok);
            }
            else if (hr == (unchecked((int)(0x80131130))))
            {
                //This happens if the type is not nested.  The error is benign.
                return new Token(0);
            }
            else
            {
                //Something else went wrong.
                throw Marshal.GetExceptionForHR(hr);
            }
        }

        // Get the number of generic parameters for the given token. 
        internal int CountGenericParams(Token token)
        {
            IMetadataImport2 importer2 = this.RawImport as IMetadataImport2;
            if (importer2 == null)
            {
                // If we can't get an Importer2, then that's a pre-generics release, which means 
                // count of generic params == 0.
                return 0;
            }

            int dummy;
            uint dummy2;
            HCORENUM hEnum = new HCORENUM();
            int count;
            importer2.EnumGenericParams(ref hEnum, token.Value, out dummy, 1, out dummy2);
            try
            {
                importer2.CountEnum(hEnum, out count);
            }
            finally
            {
                hEnum.Close(importer2);
            }
            return count;
        }

        /// <summary>
        /// Get the tokens of the generic parameters in the type or method. 
        /// </summary>
        internal IEnumerable<int> GetGenericParameterTokens(int typeOrMethodToken)
        {
            Token token = new Token(typeOrMethodToken);
            Debug.Assert(token.IsType(TokenType.TypeDef) || token.IsType(TokenType.MethodDef));

            IMetadataImport2 importer2 = this.RawImport as IMetadataImport2;
            if (importer2 == null)
            {
                // If we can't get an Importer2, then that's a pre-generics release, which means 
                yield break;
            }

            int mdGenericParam;
            uint count;
            HCORENUM hEnum = new HCORENUM();
            try
            {
                for (; ; )
                {
                    // This iterates through the tokens 1 at a time. Types usually have 0 or very few
                    // generic arguments. For example, in mscorlib, there are a total of 65 generic args
                    // spread across 2329 types. That's an averaged of .03 generic args on a type.
                    importer2.EnumGenericParams(ref hEnum, typeOrMethodToken, out mdGenericParam, 1, out count);
                    if (count != 1)
                    {
                        break;//No generic params
                    }
                    Debug.Assert(!(new Token(mdGenericParam)).IsNil);
                    yield return mdGenericParam;
                }
            }
            finally
            {
                hEnum.Close(importer2);
            }
        }

        //Get the constraint types of the generic type parameter.
        internal IEnumerable<Type> GetConstraintTypes(int gpToken)
        {
            Token token = new Token(gpToken);
            Debug.Assert(token.IsType(TokenType.GenericPar));

            IMetadataImport2 importer2 = this.RawImport as IMetadataImport2;
            if (importer2 == null)
            {
                // If we can't get an Importer2, then that's a pre-generics release, which means 
                yield break;
            }

            int mdGenericConstraint;
            uint count;
            HCORENUM hEnum = new HCORENUM();
            try
            {
                for (; ; )
                {
                    importer2.EnumGenericParamConstraints(ref hEnum, gpToken, out mdGenericConstraint, 1, out count);
                    if (count != 1)
                        break;//No generic constraints
                    Debug.Assert(!(new Token(mdGenericConstraint)).IsNil);

                    int ownerParam;
                    int constraintTypeToken;

                    importer2.GetGenericParamConstraintProps(mdGenericConstraint, out ownerParam, out constraintTypeToken);
                    Debug.Assert(ownerParam == gpToken);
                    yield return ResolveTypeTokenInternal(new Token(constraintTypeToken), null);
                }
            }
            finally
            {
                hEnum.Close(importer2);
            }
        }

        //Get the owner type or method and the name of the generic paramter.
        //One of the ownerType and ownerMethod must be 0.
        internal void GetGenericParameterProps(
            int mdGenericParam,
            out int ownerTypeToken,
            out int ownerMethodToken,
            out string name,
            out System.Reflection.GenericParameterAttributes attributes,
            out uint genIndex)
        {
            IMetadataImport2 importer2 = this.RawImport as IMetadataImport2;
            Debug.Assert(importer2 != null);

            HCORENUM hEnum = new HCORENUM();
            try
            {
                int genFlags, ptkOwner, ptkKind;
                uint genArgNameSize;

                importer2.GetGenericParamProps(mdGenericParam,
                                               out genIndex,
                                               out genFlags,
                                               out ptkOwner,
                                               out ptkKind,
                                               null,
                                               0,
                                               out genArgNameSize);
                attributes = (System.Reflection.GenericParameterAttributes)genFlags;
                StringBuilder genArgName = StringBuilderPool.Get((int)genArgNameSize);
                importer2.GetGenericParamProps(mdGenericParam,
                                               out genIndex,
                                               out genFlags,
                                               out ptkOwner,
                                               out ptkKind,
                                               genArgName,
                                               (uint)genArgName.Capacity,
                                               out genArgNameSize);
                name = genArgName.ToString();
                StringBuilderPool.Release(ref genArgName);
                Token ownerToken = new Token(ptkOwner);
                if (ownerToken.IsType(TokenType.MethodDef))
                {
                    ownerMethodToken = ptkOwner;
                    ownerTypeToken = 0;
                }
                else
                {
                    ownerTypeToken = ptkOwner;
                    ownerMethodToken = 0;
                }
            }
            finally
            {
                hEnum.Close(importer2);
            }
        }

        //Get the interfaces the type implements directly.
        internal IEnumerable<Type> GetInterfacesOnType(Type type)
        {
            Debug.Assert(type.Module == this, "GetInterfacesOnType() called on wrong token resolver");

            //If type is a type variable, get the interfaces it implements from its type constraints.
            //e.g 
            //class Foo<T> where T:IFoo
            //IFoo will be in the result when getting interfaces for the type variable T.
            if (type.IsGenericParameter)
            {
                foreach (Type c in GetConstraintTypes(type.MetadataToken))
                {
                    if (c.IsInterface)
                    {
                        yield return c;
                    }
                }
            }
            else
            {
                var import = this.RawImport;
                foreach (var tImpl in EnumerateInterfaceImplsOnType(type))
                {
                    yield return GetInterfaceTypeFromInterfaceImpl(type, tImpl);
                }
            }
        }

        internal IEnumerable<Token> EnumerateInterfaceImplsOnType(Type type)
        {
            var import = this.RawImport;

            HCORENUM hEnum = new HCORENUM();
            int rImpls;
            int cImpls = 1;
            for (; ; )
            {
                import.EnumInterfaceImpls(ref hEnum, type.MetadataToken, out rImpls, 1, ref cImpls);
                if (cImpls != 1)
                    break;

                Token tImpl = new Token(rImpls);
                Debug.Assert(tImpl.IsType(TokenType.InterfaceImpl));

                yield return tImpl;
            }
            hEnum.Close(import);
        }

        internal Type GetInterfaceTypeFromInterfaceImpl(Type type, Token tImpl)
        {
            int cls;
            int iface;
            this.RawImport.GetInterfaceImplProps(tImpl.Value, out cls, out iface);
            Token tkClass = new Token(cls);
            Token tkInterface = new Token(iface);
            Debug.Assert(tkClass.Equals(type.MetadataToken));
            Type result = ResolveTypeTokenInternal(tkInterface, new GenericContext(type.GetGenericArguments(), null));
            return result;
        }

        static public Type GetInterfaceHelper(Type[] interfaces, string name, bool ignoreCase)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            bool match = false;
            Type result = null;

            foreach (Type i in interfaces)
            {
                match = Utility.Compare(name, i.Name, ignoreCase);
                

                if (match)
                {
                    if (result != null)
                    {
                        throw new AmbiguousMatchException();
                    }
                    else
                    {
                        result = i;
                    }
                }
            }

            return result;
        }

        #region Scope-wide Enumeration
        // Get all the TypeDefinitions in this scope
        // This does not give back instantiated generics (TypeSpecs).
        // Signature is consistent with reflection.  See code:GetTypes for reflection version that returns
        // an array.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<Type> GetTypeList()
        {
            // Fastest enumeration would probably be to get the table size and then zip through that
            // creating tokens.
            foreach (int typeToken in GetTypeTokenList())
            {
                Type result = this.ResolveTypeTokenInternal(new Token(typeToken), null);
                yield return result;
            }
        }

        // Get all the TypeDefinition tokens in this scope
        private IEnumerable<int> GetTypeTokenList()
        {
            var import = this.RawImport;

            HCORENUM hEnum = new HCORENUM();

            try
            {
                uint count = 1;
                int rTypeDefs;
                for (; ; )
                {
                    import.EnumTypeDefs(ref hEnum, out rTypeDefs, 1, out count);
                    if (count != 1)
                        break;//No more types

                    yield return rTypeDefs;
                }
            }
            finally
            {
                hEnum.Close(import);
            }
        }
        #endregion

        //CheckBindingFlagsInMethod is used to check if the input Binding flags contains any
        //flag that is not supported by LMR. This method is used by GetXXXOnType().
        private static void CheckBindingFlagsInMethod(BindingFlags flags, string methodName)
        {
            // FlattenHierarchy requires base-class resolution.
            const BindingFlags ok = BindingFlags.DeclaredOnly |
                                    BindingFlags.Instance |
                                    BindingFlags.Static |
                                    BindingFlags.Public |
                                    BindingFlags.NonPublic |
                                    BindingFlags.FlattenHierarchy |
                                    BindingFlags.IgnoreCase |
                // flags required by TargetFrameworkProvider which provides invocation ability
                                    BindingFlags.CreateInstance |
                                    BindingFlags.GetField |
                                    BindingFlags.GetProperty |
                                    BindingFlags.InvokeMethod |
                                    BindingFlags.SetField |
                                    BindingFlags.SetProperty |
                                    BindingFlags.NonPublic |
                                    BindingFlags.ExactBinding;
            if ((flags | ok) != ok)
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
                    Resources.MethodIsUsingUnsupportedBindingFlags, methodName, flags));
        }


        /// <summary>
        /// Checks if property is static and/or public.
        /// Property is static if one of its set/get accessors is static.
        /// Property is public if one of its set/get accessors is public.
        /// </summary>
        private static void CheckIsStaticAndIsPublicOnProperty(PropertyInfo propertyInfo, ref bool isStatic, ref bool isPublic)
        {
            bool nonPublic = true;
            MethodInfo getter = propertyInfo.GetGetMethod(nonPublic);
            CheckIsStaticAndIsPublic(getter, ref isStatic, ref isPublic);

            MethodInfo setter = propertyInfo.GetSetMethod(nonPublic);
            CheckIsStaticAndIsPublic(setter, ref isStatic, ref isPublic);
        }

        /// <summary>
        /// Checks if event is static and/or public.
        /// Event is static if one of its add/remove/raise accessors is static.
        /// Event is public if one of its add/remove/raise accessors is public.
        /// </summary>
        private static void CheckIsStaticAndIsPublicOnEvent(EventInfo eventInfo, ref bool isStatic, ref bool isPublic)
        {
            bool nonPublic = true;
            MethodInfo addMethod = eventInfo.GetAddMethod(nonPublic);
            CheckIsStaticAndIsPublic(addMethod, ref isStatic, ref isPublic);

            MethodInfo removeMethod = eventInfo.GetRemoveMethod(nonPublic);
            CheckIsStaticAndIsPublic(removeMethod, ref isStatic, ref isPublic);

            MethodInfo raiseMethod = eventInfo.GetRaiseMethod(nonPublic);
            CheckIsStaticAndIsPublic(raiseMethod, ref isStatic, ref isPublic);
        }

        /// <summary>
        /// Check if a Method is static or public. Used for properties and events. They are considered static/public
        /// if any of accessor methods are static/public.
        /// </summary>
        private static void CheckIsStaticAndIsPublic(MethodInfo methodInfo, ref bool isStatic, ref bool isPublic)
        {
            // If there is no particular accessor method, state is unchanged.  
            if (methodInfo == null)
            {
                return;
            }

            if (methodInfo.IsStatic)
            {
                isStatic = true;
            }

            if (methodInfo.IsPublic)
            {
                isPublic = true;
            }
        }


        // Assembly creation will set the assembly backpointer on contained modules.
        Assembly m_assembly;
        internal void SetContainingAssembly(Assembly assembly)
        {
            Debug.Assert(m_assembly == null); // only set once.
            m_assembly = assembly;
        }

        #region Module Members

        // Backpointer to the assembly that this module is contained in.
        // If we open just the .NetModule, then we won't have a containing assembly. ILDasm can do this.
        public override Assembly Assembly
        {
            get
            {
                return m_assembly;
            }
        }

#if !USE_CLR_V4

        // All the Module.GetType(string) and Assembly.GetType(string) overloads chain down to this point.
        override public Type GetType(string className, bool throwOnError, bool ignoreCase)
        {
            if (ignoreCase)
            {
                // Metadata FindTypeDefByName() doesn't support case-insensitive lookup, so we'll need to
                // roll this ourselves.
                throw new NotImplementedException(Resources.CaseInsensitiveTypeLookupNotImplemented);
            }

            if (className == null)
            {
                throw new ArgumentNullException("className");
            }

            // Needs to handle parsing of compound types. 
            // Since ParseTypeName will call back into Module.GetType() for parsing atoms, we need a compound check to avoid 
            // infinite recursion.
            if (TypeNameParser.IsCompoundType(className))
            {
                return TypeNameParser.ParseTypeName(this.m_assemblyResolver, this, className, throwOnError);
            }

            // need to plumb through a no-throw option.
            Token tk = FindTypeDefByName(null, className, false);
            if (tk.IsNil)
            {
                if (throwOnError)
                {
                    throw new TypeLoadException(string.Format(
                        CultureInfo.InvariantCulture, Resources.CannotFindTypeInModule, className, this.ToString()));
                }
                return null;
            }

            Type t = this.ResolveType(tk.Value);
            return t;
        }

#else
        override public Type GetType(string className, bool throwOnError, bool ignoreCase)
        {
            if (ignoreCase)
            {
                // Metadata FindTypeDefByName() doesn't support case-insensitive lookup, so we'll need to
                // roll this ourselves.
                throw new NotImplementedException(Resources.CaseInsensitiveTypeLookupNotImplemented);
            }

            Func<AssemblyName, Assembly> assemblyResolverCallback = delegate(AssemblyName assemblyName)
            {
                Debug.Assert(assemblyName != null);
                return AssemblyResolver.ResolveAssembly(assemblyName);
            };

            Func<Assembly, string, bool, Type> typeResolver = delegate(Assembly assembly, string simpleTypeName, bool ignoreCaseInCallback)
            {
                bool throwOnErrorInCallback = false;
                if (assembly != null)
                {
                    Type t = assembly.GetType(simpleTypeName, throwOnErrorInCallback, ignoreCaseInCallback);
                    return t;
                }
                else
                {
                    // need to plumb through a no-throw option.
                    Token tk = FindTypeDefByName(null, simpleTypeName, false);

                    if (tk.IsNil)
                    {
                        return null;
                    }

                    Type t = this.ResolveType(tk.Value);
                    return t;
                }
            };

            return Type.GetType(className, assemblyResolverCallback, typeResolver, throwOnError);
        }

#endif

        // Reflection version which returns an array. See code:GetTypeList() for enum version.
        override public Type[] GetTypes()
        {
            List<Type> l = new List<Type>(GetTypeList());
            return l.ToArray();
        }

        override public Type[] FindTypes(TypeFilter filter, object filterCriteria)
        {
            List<Type> l = new List<Type>();
            foreach (Type t in GetTypeList())
            {
                if (filter(t, filterCriteria))
                {
                    l.Add(t);
                }
            }
            return l.ToArray();
        }

        //TODO: 
        //GetField() and GetFields() return global fields in a module, which
        //is not supported in C#;
        private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            if (name == null) throw new ArgumentNullException("name");
            FieldInfo[] fs = GetFields(bindingAttr);
            foreach (FieldInfo f in fs)
            {
                if (f.Name.Equals(name))
                {
                    return f;
                }
            }
            return null;
        }
        public override FieldInfo[] GetFields(BindingFlags bindingFlags)
        {
            CheckBindingFlagsInMethod(bindingFlags, "GetFields");

            var import = this.RawImport;

            HCORENUM hEnum = new HCORENUM();
            List<FieldInfo> result = new List<FieldInfo>();
            try
            {
                uint count = 1;
                int globalFields;
                for (; ; )
                {
                    import.EnumFields(ref hEnum, this.MetadataToken, out globalFields, 1, out count);
                    if (count != 1)
                        break;//No more types

                    FieldInfo fieldInfo = ResolveField(globalFields);
                    if (Utility.IsBindingFlagsMatching(fieldInfo, false, bindingFlags))
                    {
                        result.Add(fieldInfo);
                    }
                }
            }
            finally
            {
                hEnum.Close(import);
            }
            return result.ToArray();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            CheckBinderAndModifiersforLMR(binder, modifiers);
            MethodInfo[] methods = GetMethods(bindingAttr);
            return FilterMethod(methods, name, bindingAttr, callConvention, types);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingFlags)
        {
            CheckBindingFlagsInMethod(bindingFlags, "GetMethods");

            var import = this.RawImport;

            HCORENUM hEnum = new HCORENUM();
            List<MethodInfo> result = new List<MethodInfo>();

            try
            {
                int count = 1;
                int globalMethods;
                for (; ; )
                {
                    import.EnumMethods(ref hEnum, this.MetadataToken, out globalMethods, 1, out count);
                    if (count != 1)
                        break;//No more types

                    MethodBase method = ResolveMethodTokenInternal(new Token(globalMethods), null);
                    if (Utility.IsBindingFlagsMatching(method, false, bindingFlags))
                    {
                        MethodInfo methodInfo = method as MethodInfo;
                        if (methodInfo != null)
                        {
                            //not add constructors.
                            result.Add(methodInfo);
                        }
                    }
                }
            }
            finally
            {
                hEnum.Close(import);
            }
            return result.ToArray();
        }

        // Get the metadata token for this module. This is probably only interesting in multi-module assemblies.
        public override int MetadataToken
        {
            get
            {
                int token;
                this.RawImport.GetModuleFromScope(out token);
                return token;
            }
        }

        public override bool IsResource()
        {
            return false;
        }

        #endregion

        public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            // This handles TypeDef,Ref,Specs.
            // Note that the generic context is only used for a TypeSpec. So if we pass in a TypeDef for List<T>, we
            // get back the open generic type.
            var t = ResolveTypeTokenInternal(new Token(metadataToken), new GenericContext(genericTypeArguments, genericMethodArguments));

            // Reflection's behavior will validate the type that it hands back. 
            // Just resolve it to force validation to mimic reflection semantics.
            // But still return the proxy so that clients can get at it.
            Helpers.EnsureResolve(t);

            return t;
        }

        public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            // This handles FieldDef, MemberRef, 
            // Just as with code:ResolveType, the generic args are used to resolve specs, but not used for
            // FieldDefs. Callers must resolve the type and then call GetField() on the resolved generic type.
            return this.ResolveFieldTokenInternal(new Token(metadataToken), new GenericContext(genericTypeArguments, genericMethodArguments));
        }

        public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            // Don't use GetGenericMethod(), that has the wrong semantics around generics. It will apply generic args to a MethodDef.
            // Whereas ResolveMethod() should only apply the generic args to a ref / spec.
            return this.ResolveMethodTokenInternal(new Token(metadataToken), new GenericContext(genericTypeArguments, genericMethodArguments));
        }

        public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new NotImplementedException();
        }

        public override byte[] ResolveSignature(int metadataToken)
        {
            throw new NotImplementedException();
        }

        private const BindingFlags MembersDeclaredOnTypeOnly = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Determine if this module is the symbol module, as decided by the TypeUniverse.
        /// </summary>
        /// <returns>Return true iff this is the system module (mscorlib). </returns>
        /// <remarks>This is needed if the caller wants to compare cached token or name values for
        /// builtin types.</remarks>
        internal bool IsSystemModule()
        {
            var u = this.AssemblyResolver;
            return u.GetSystemAssembly().Equals(this.Assembly);

        }

        /// <summary>
        /// Determine if the module is a windows runtime metadata module
        /// </summary>
        /// <param name="module">The module to determine if it is a windows runtime metadata module</param>
        /// <returns>True if the module is a windows runtime metadata module</returns>
        static internal bool IsWindowsRuntime(Module module)
        {
            return module.Assembly.GetName().ContentType == AssemblyContentType.WindowsRuntime;
        }        

        // Hang this on module so that we have access to a shared readonly mapping.
        internal TypeCode GetTypeCode(Type type)
        {
            // TypeCode of an enum is that of its underlying type.
            // Check this before checking for system assembly since user-defined Enums shouldn't be
            // returning Object.
            if (type.IsEnum)
            {
                type = MetadataOnlyModule.GetUnderlyingType(type);
                return Type.GetTypeCode(type);
            }

            // If we're not in the system assembly, then typecode is just Object.
            if (!IsSystemModule())
            {
                return TypeCode.Object;
            }


            Token token = new Token(type.MetadataToken);
            // Derived class should have taken care of this first.
            Debug.Assert(!token.IsNil);


            if (m_typeCodeMapping == null)
            {
                m_typeCodeMapping = CreateTypeCodeMapping();
            }

            // Lookup well known types
            for (int i = 0; i < m_typeCodeMapping.Length; i++)
            {
                if (token == m_typeCodeMapping[i])
                    return (TypeCode)i;
            }

            // If if it's not in the well-known list, just assume object.
            return TypeCode.Object;
        }


        /// <summary>
        /// Return a mapping for code:m_typeCodeMapping. See that field for exact semantics of this array.
        /// This must be called from the assembly's module.
        /// </summary>
        private Token[] CreateTypeCodeMapping()
        {
            // This must be called from the system assembly. All type name lookups are system types in the
            // system assembly.
            Debug.Assert(this.IsSystemModule());

            return new Token[] {
                new Token(),  // TypeCode.Empty
                LookupTypeToken("System.Object"), // 1
                LookupTypeToken("System.DBNull"),
                LookupTypeToken("System.Boolean"),
                LookupTypeToken("System.Char"),
                LookupTypeToken("System.SByte"),
                LookupTypeToken("System.Byte"),
                LookupTypeToken("System.Int16"),
                LookupTypeToken("System.UInt16"),
                LookupTypeToken("System.Int32"),
                LookupTypeToken("System.UInt32"),
                LookupTypeToken("System.Int64"),
                LookupTypeToken("System.UInt64"),
                LookupTypeToken("System.Single"),
                LookupTypeToken("System.Double"),
                LookupTypeToken("System.Decimal"),
                LookupTypeToken("System.DateTime"), // 17 == TypeCode.DateTime
                new Token(), // skipped
                LookupTypeToken("System.String"), // 18 == TypeCode.String
            };
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose this module. This should be called in the context of disposing the parent assembly.
        /// This will release the unmanaged metadata pointers this module owns.
        /// Caller is responsible for thread safey here and to not dispose while another thread is using.
        /// Caller should not use after this has been diposed.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //
                // free managed resources. 
                //

#if !USE_CLR_V4
                //Release the thread-affinity imports for all the threads.
                foreach (IMetadataImport import in m_cachedThreadAffinityImporter.Values)
                {
                    int refCount = Marshal.ReleaseComObject(import);
                    // After the module is disposed, there should be 
                    // no reference to m_cachedThreadAffinityImporter.
                    // 
                    Debug.Assert(refCount == 0);
                }
                m_cachedThreadAffinityImporter.Clear();
#else
                if (m_cachedThreadAffinityImporter != null)
                {
                    int refCount = Marshal.ReleaseComObject(m_cachedThreadAffinityImporter);
                    // After the module is disposed, there should be 
                    // no reference to m_cachedThreadAffinityImporter.
                    // 
                    Debug.Assert(refCount == 0);
                    m_cachedThreadAffinityImporter = null;
                }
#endif

                // Free the managed wrappers for the native metadata objects.
                // The metadata needs to be released after m_cachedThreadAffinityImporter
                // is released to avoid having an IMetadataImport using a disposed metadata.
                //
                if (m_metadata != null) {
                    m_metadata.Dispose();
                }

                // Many other managed fields are readonly (initialized in constructor), 
                // so we can't null them out here. We don't need to either, since they don't hold native resources.
            }

            // No native resources to free
        }


        #endregion


        #region IModule2 members

        /// <summary>
        /// Gets number of rows in a metadata table.
        /// </summary>
        public int RowCount(MetadataTable metadataTableIndex)
        {
            IMetadataTables metadataTables = (IMetadataTables)this.RawImport;

            int rowCount;
            int notNeededCount;
            UnusedIntPtr notNeededPtr;

            metadataTables.GetTableInfo(
                metadataTableIndex,
                out notNeededCount,
                out rowCount,
                out notNeededCount,
                out notNeededCount,
                out notNeededPtr);

            return rowCount;
        }


        #endregion // IModule2 members


        public override void GetPEKind(out System.Reflection.PortableExecutableKinds peKind, out System.Reflection.ImageFileMachine machine)
        {
            // This information is stored in the metadata (as opposed to the PEIMage headers)
            var imdi = (IMetadataImport2)this.RawImport;
            imdi.GetPEKind(out peKind, out machine);
        }

        public override int MDStreamVersion
        {
            get
            {
                // The Metadata version is from the '#~ stream' structure in the metadata blob. See II 24.2.6 for details.
                // It should be (minor | (major << 16)).
                // This is 0 for resources, but a MOModule doesn't represent resources.
                throw new NotImplementedException();
            }
        }

        public string GetRuntimeVersion()
        {
            var imp2 = (IMetadataImport2)this.RawImport;

            int versionStringSize;
            imp2.GetVersionString(null, 0, out versionStringSize);

            StringBuilder versionString = StringBuilderPool.Get(versionStringSize);
            imp2.GetVersionString(versionString, versionString.Capacity, out versionStringSize);
            string result = versionString.ToString();
            StringBuilderPool.Release(ref versionString);

            return result;
        }

    } // end class MetadataOnlyModule
}
