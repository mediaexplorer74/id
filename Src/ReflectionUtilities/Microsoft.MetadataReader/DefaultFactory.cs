// Default factory
// 

using System;
using Microsoft.MetadataReader;
using System.Reflection.Adds;
using Debug=Microsoft.MetadataReader.Internal.Debug;
using BindingFlags = System.Reflection.BindingFlags;

#if USE_CLR_V4
using System.Reflection;  
using Type = System.Type;
#else
using System.Reflection.Mock;
using Type = System.Reflection.Mock.Type;
#endif

namespace Microsoft.MetadataReader
{
    // Default factory for creating LMR types. 
    internal class DefaultFactory : IReflectionFactory
    {
        #region IReflectionFactory Members

        public virtual MetadataOnlyCommonType CreateSimpleType(MetadataOnlyModule scope, Token tokenTypeDef)
        {
            return new MetadataOnlyTypeDef(scope, tokenTypeDef);
        }

        public virtual MetadataOnlyCommonType CreateGenericType(MetadataOnlyModule scope, Token tokenTypeDef, Type[] typeArgs)
        {
            return new MetadataOnlyTypeDef(scope, tokenTypeDef, typeArgs);
        }

        public virtual MetadataOnlyCommonType CreateArrayType(MetadataOnlyCommonType elementType, int rank)
        {
            return new MetadataOnlyArrayType(elementType, rank);
        }

        public virtual MetadataOnlyCommonType CreateVectorType(MetadataOnlyCommonType elementType) 
        {
            return new MetadataOnlyVectorType(elementType);
        }

        public virtual MetadataOnlyCommonType CreateByRefType(MetadataOnlyCommonType type)
        {
            return new MetadataOnlyModifiedType(type, "&");
        }

        public virtual MetadataOnlyCommonType CreatePointerType(MetadataOnlyCommonType type)
        {
            return new MetadataOnlyModifiedType(type, "*");
        }

        public virtual MetadataOnlyTypeVariable CreateTypeVariable(MetadataOnlyModule resolver, Token typeVariableToken)
        {
            return new MetadataOnlyTypeVariable(resolver, typeVariableToken);
        }

        public virtual MetadataOnlyFieldInfo CreateField(MetadataOnlyModule resolver, Token fieldDefToken, Type[] typeArgs, Type[] methodArgs)
        {
            return new MetadataOnlyFieldInfo(resolver, fieldDefToken, typeArgs, methodArgs);
        }

        public virtual MetadataOnlyPropertyInfo CreatePropertyInfo(MetadataOnlyModule resolver, Token propToken, Type[] typeArgs, Type[] methodArgs)
        {
            return new MetadataOnlyPropertyInfo(resolver, propToken, typeArgs, methodArgs);
        }

        public virtual MetadataOnlyEventInfo CreateEventInfo(MetadataOnlyModule resolver, Token eventToken, Type[] typeArgs, Type[] methodArgs)
        {
            return new MetadataOnlyEventInfo(resolver, eventToken, typeArgs, methodArgs);
        }


        #region Method Creation
        /// <summary>
        /// Create a constructor info around the given method
        /// </summary>
        /// <param name="method">method for the constructor</param>
        /// <returns>a constructor info for the given method</returns>
        public virtual MetadataOnlyConstructorInfo CreateConstructorInfo(MethodBase method)
        {
            return new MetadataOnlyConstructorInfo(method);
        }

        /// <summary>
        /// Create a MethodInfo for the given method. 
        /// </summary>
        /// <param name="method">method to create</param>
        /// <returns>can return method directly, or create a new wrapper arodun it.</returns>
        /// <remarks> ConstructorInfo and MethodInfo both correspond to a MethodDef token. LMR needs to
        /// call into the metadata to determine which factory method to call for the token. 
        /// The factory would also need to call into the metadata again to create the instantiated
        /// ConstructorInfo/MethodInfo object. 
        /// To avoid double calls into the metadata, LMR provides the results via an instantiated
        /// MethodInfo. A default factory could just return that method. Or it could create its own derived
        /// method and instantiate it with that data.
        /// </remarks>
        public virtual MetadataOnlyMethodInfo CreateMethodInfo(MetadataOnlyMethodInfo method)
        {
            // We could pass through Method directly, but go through this ctor to excercise the same paths
            // that other public factories will use.
            return new MetadataOnlyMethodInfo(method);
        }

        // Default implementation to create a method or constructor.
        // This will chain to more specific CreateMethod/CreateConstructor callbacks.
        public virtual MethodBase CreateMethodOrConstructor(MetadataOnlyModule resolver, Token methodDef, Type[] typeArgs, Type[] methodArgs)
        {
            // If this is a constructor, we need to instantiate a ConstructorInfo instead to be consistent with m.IsConstructor.
            MetadataOnlyMethodInfo m = new MetadataOnlyMethodInfo(resolver, methodDef, typeArgs, methodArgs);

            if (IsRawConstructor(m))
            {
                MetadataOnlyConstructorInfo ci = this.CreateConstructorInfo(m);
                Debug.Assert(ci is ConstructorInfo);
                return ci;
            }
            else
            {
                MetadataOnlyMethodInfo mi = this.CreateMethodInfo(m);
                Debug.Assert(mi is MethodInfo);
                return mi;
            }
        }
        // Return true iff the method info is for a constructor.
        // LMR must wrap this MethodInfo in a ConstructorMethodInfo before returning it to the user.
        static private bool IsRawConstructor(MethodInfo m)
        {
            // Constructors have a special name
            if ((m.Attributes & System.Reflection.MethodAttributes.RTSpecialName) == 0)
            {
                return false;
            }

            // Check name for ctor or static ctor match.
            string name = m.Name;
            if (name.Equals(System.Reflection.ConstructorInfo.ConstructorName, StringComparison.Ordinal))
            {
                return true;
            }

            if (name.Equals(System.Reflection.ConstructorInfo.TypeConstructorName, StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }
        #endregion // Method Creation


        public virtual bool TryCreateMethodBody(MetadataOnlyMethodInfo method, ref MethodBody body)
        {
            // Specify that we're not override methodbody creation.
            return false;
        }


        public virtual Type CreateTypeRef(MetadataOnlyModule scope, Token tokenTypeRef)
        {   
            return new MetadataOnlyTypeReference(scope, tokenTypeRef);            
        }

        public virtual Type CreateSignatureTypeRef(MetadataOnlyModule scope, Token tokenTypeRef, CorElementType elemType)
        {
            return new MetadataOnlySignatureTypeReference(scope, tokenTypeRef, elemType);
        }

        public virtual Type CreateTypeSpec(MetadataOnlyModule scope, Token tokenTypeSpec, Type[] typeArgs, Type[] methodArgs)
        {
            return new TypeSpec(scope, tokenTypeSpec, typeArgs, methodArgs);
        }

        #endregion
    } // end class DefaultFactory
}
