using System.Diagnostics;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Text;
using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.Reflection.Adds;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using BindingFlags = System.Reflection.BindingFlags;
using Type = System.Reflection.Mock.Type;
#endif


namespace Microsoft.MetadataReader
{
    /// <summary>
    /// Represents generic context used for signature blob parsing when we have MemberRef. 
    /// Lets us create method's template that we can use for comparison with all the methods
    /// on a target class.
    /// </summary>
    internal class OpenGenericContext : GenericContext
    {
        private readonly MetadataOnlyModule m_resolver;
        private readonly Token m_ownerMethod;

        /// <summary>
        /// Initializes both type arguments and method arguments. 
        /// Used when we already know number of method arguments i.e.
        /// after verification. 
        /// </summary>
        public OpenGenericContext(Type[] typeArgs, Type[] methodArgs)
            :base(typeArgs, methodArgs)
        {
            Debug.Assert(typeArgs != null, "typeArgs can't be null");
            Debug.Assert(methodArgs != null, "methodArgs can't be null");
        }

        /// <summary>
        /// Only initializes type arguments (if there are any). Method arguments are initialized
        /// later, once we have information for signature blob.
        /// </summary>
        public OpenGenericContext(MetadataOnlyModule resolver, Type ownerType, Token ownerMethod)
            : base(null, null)
        {
            Debug.Assert(resolver != null, "resolver can't be null");
            Debug.Assert(ownerType != null, "ownerType can't be null");
            Debug.Assert(ownerMethod.IsType(TokenType.MemberRef), "OpenGenericContext should be used for MemberRefs only.");

            m_resolver = resolver;
            m_ownerMethod = ownerMethod;

            int numberOfTypeArguments = ownerType.GetGenericArguments().Length;

            Type[] typeArgs = new Type[numberOfTypeArguments];
            Token ownerTypeToken = new Token(ownerType.MetadataToken);

            for (int i = 0; i < numberOfTypeArguments; i++)
            {
                typeArgs[i] = new MetadataOnlyTypeVariableRef(resolver, ownerTypeToken, i);
            }

            this.TypeArgs = typeArgs;
        }

        /// <summary>
        /// Called once we know generic method's arity to initialize metod arguments.
        /// </summary>
        public override GenericContext VerifyAndUpdateMethodArguments(int expectedNumberOfMethodArgs)
        {
            if (expectedNumberOfMethodArgs != this.MethodArgs.Length)
            {
                Type[] methodArgs = new Type[expectedNumberOfMethodArgs];
                for (int i = 0; i < expectedNumberOfMethodArgs; i++)
                {
                    methodArgs[i] = new MetadataOnlyTypeVariableRef(m_resolver, m_ownerMethod, i);
                }

                return new OpenGenericContext(this.TypeArgs, methodArgs);
            }

            return this;
        }
    }
}