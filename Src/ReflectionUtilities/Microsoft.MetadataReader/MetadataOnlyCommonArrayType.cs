using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using Microsoft.MetadataReader.Internal;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using Type = System.Reflection.Mock.Type;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// This class defines a common base type for vector types and multi-dimensional array types.
    /// </summary>
    internal class MetadataOnlyCommonArrayType : MetadataOnlyCommonType
    {
        readonly private MetadataOnlyCommonType m_elementType;

        // Base type for vector types is System.Array
        readonly private Type m_baseType;

        public MetadataOnlyCommonArrayType(MetadataOnlyCommonType elementType)
        {
            // TODO: Creating vectors of ByRef types should not be allowed when we 
            // need to be compatible with RefOnly. But we have to make it possible for
            // other scenarios like VS interpreter or language IDE. 
            // This should be plugged in through our policy object.
            // Utility.VerifyNotByRef(elementType);

            ITypeUniverse universe = Helpers.Universe(elementType);
            Debug.Assert(universe != null);
            m_baseType = universe.GetTypeXFromName("System.Array");
            m_elementType = elementType;
        }

        public override string Namespace
        {
            get { return m_elementType.Namespace; }
        }

        internal override MetadataOnlyModule Resolver
        {
            get { return m_elementType.Resolver; }
        }

        public override Type BaseType
        {
            get { return m_baseType; }
        }

        protected override bool IsArrayImpl()
        {
            return true;
        }

        public override Type UnderlyingSystemType
        {
            get { return this; }
        }
     

        public override Type GetElementType()
        {
            return m_elementType;
        }

        public override int GetHashCode()
        {
            // Don't use metadata token for array hash code since it's not unique.
            return m_elementType.GetHashCode();
        }

        
        //TypeDef for this type.
        public override int MetadataToken
        {
            get
            {
                // Empirically, reflection gives a typeDef token with rid=0.
                return (int)TokenType.TypeDef;
            }
        }


        public override Type[] GetGenericArguments()
        {
            return m_elementType.GetGenericArguments(); 
        }

        public override Type GetGenericTypeDefinition()
        {
            throw new InvalidOperationException();
        }

        // Internal only helper for GetMethods().
        internal override IEnumerable<MethodBase> GetDeclaredMethods()
        {
            return this.Resolver.Policy.GetExtraArrayMethods(this);
        }

        // Internal only helper for GetMethods().
        internal override IEnumerable<MethodBase> GetDeclaredConstructors()
        {
            return this.Resolver.Policy.GetExtraArrayConstructors(this);
        }

        public override FieldInfo[] GetFields(System.Reflection.BindingFlags flags)
        {
            //No direct fields
            return new FieldInfo[0];
        }

        public override FieldInfo GetField(string name, System.Reflection.BindingFlags bindingAttr)
        {
            //No direct fields
            return null;
        }

 
        public override EventInfo[] GetEvents(System.Reflection.BindingFlags flags)
        {
            //No direct events
            return new EventInfo[0];
        }

        public override EventInfo GetEvent(string name, System.Reflection.BindingFlags flags)
        {
            //No direct events
            return null;
        }

        protected override System.Reflection.TypeAttributes GetAttributeFlagsImpl()
        {
            // These are the flags that the CLR's implementation appears to use.
            System.Reflection.TypeAttributes result = 
                   System.Reflection.TypeAttributes.Class |
                   System.Reflection.TypeAttributes.AutoLayout |
                   System.Reflection.TypeAttributes.Sealed |
                   System.Reflection.TypeAttributes.Public |
                   System.Reflection.TypeAttributes.AnsiClass;


            // Arrays are considered serializable, even if they're element type is not.
            result |= System.Reflection.TypeAttributes.Serializable;
            return result;
        }

        public override Type GetNestedType(string name, System.Reflection.BindingFlags bindingAttr)
        {
            return null;
        }

        public override Type[] GetNestedTypes(System.Reflection.BindingFlags bindingAttr)
        {
            return new Type[0];
        }

              

        public override Type[] GetInterfaces()
        {
            //return all the interfaces that System.Array implements
            List<Type> l = new List<Type>(m_baseType.GetInterfaces());

            // Loader may add additional interfaces, so hook policy object to get them.
            l.AddRange(this.Resolver.Policy.GetExtraArrayInterfaces(m_elementType));

            return l.ToArray();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            return MetadataOnlyModule.GetInterfaceHelper(GetInterfaces(), name, ignoreCase);
        }

        public override Type MakeGenericType(Type[] argTypes)
        {
            // Given a T[], can't directly instantiate it. We must decompose, instantiate the individual
            // open types, and then recompose.

            Debug.Assert(!this.IsGenericTypeDefinition);
            throw new InvalidOperationException();            
        }

        public override System.Reflection.MemberTypes MemberType
        {
            get { return System.Reflection.MemberTypes.TypeInfo; }
        }

        public override Type DeclaringType
        {
            get 
            {
                //No declaring type
                return null;
            }
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override Assembly Assembly
        {
            get 
            {
                return m_elementType.Assembly;
            }
        }


        public override Guid GUID
        {
            get { return Guid.Empty; }
        }

        protected override bool HasElementTypeImpl()
        {
            return true;
        }

        public override object InvokeMember(string name, System.Reflection.BindingFlags invokeAttr, Binder binder, object target, object[] args, System.Reflection.ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            // Array types don't have TypeDef rows in the TypeDef table so we can't query metadata for their attributes. 
            // CLR decided that they should be serializable always so we just construct it here (assuming it exists in mscorlib). 

            CustomAttributeData attribute = PseudoCustomAttributes.GetSerializableAttribute(this.Resolver, /*isRequired*/ false);
            if (attribute != null)
            {
                return new CustomAttributeData[] { attribute };
            }
            else
            {
                return new CustomAttributeData[0];
            }
        }

        public override System.Reflection.GenericParameterAttributes GenericParameterAttributes
        {
            get { throw new InvalidOperationException(Resources.ValidOnGenericParameterTypeOnly); }
        }

        protected override TypeCode GetTypeCodeImpl()
        {
            // All arrays types are considered objects.
            return TypeCode.Object;
        }

        #region Members that have different implementations on vectors and multi-dimensional array types that should throw exception here
        public override string FullName 
        {
            get { throw new InvalidOperationException(); }
        }

        public override string Name 
        {
            get { throw new InvalidOperationException(); }
        }

        public override bool IsAssignableFrom(Type c) 
        {
            throw new InvalidOperationException();
        }

        public override bool Equals(Type o) 
        {
            throw new InvalidOperationException();
        }
        #endregion
    }
}
