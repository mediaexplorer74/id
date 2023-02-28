// Type impl based on IMetadataImport metadata reader.

using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Globalization;
using System.Reflection.Adds;

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
using MemberTypes = System.Reflection.MemberTypes;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// Represents an uninstantiated type variable.  For now, most method are unimplemented.
    /// The defining property of this is that IsGenericParameter returns true; whereas most other Type
    /// implementations return false.
    /// </summary>
    public class MetadataOnlyTypeVariable : MetadataOnlyCommonType
    {
        //One of m_ownerTypeToken and m_ownerMethodToken must be 0.
        readonly private int m_ownerMethodToken;
        readonly private int m_ownerTypeToken;

        readonly private string m_name;
        readonly private uint m_position;
        readonly private MetadataOnlyModule m_resolver;
        readonly private int m_Token;
        readonly private System.Reflection.GenericParameterAttributes m_gpAttributes;

        internal MetadataOnlyTypeVariable(MetadataOnlyModule resolver, Token token)
        {
            m_Token = token.Value;
            m_resolver = resolver;

            Debug.Assert(token != 0);
            m_resolver.GetGenericParameterProps(m_Token, out m_ownerTypeToken, out m_ownerMethodToken, out m_name, out m_gpAttributes, out m_position);
        }

        #region Type Members

        public override string FullName
        {
            get { return null; }
        }

        internal override MetadataOnlyModule Resolver
        {
            get { return m_resolver; }
        }

        public override Type BaseType
        {
            get 
            {
                Type[] typeConstraints = GetGenericParameterConstraints();
                foreach (Type t in typeConstraints)
                {
                    //There can at most be one class in the constraints.
                    if (t.IsClass)
                    {
                        return t;
                    }
                }
                //If no type constraints are specified for this type parameter,
                //return System.Object.
                return m_resolver.AssemblyResolver.GetBuiltInType(CorElementType.Object);
            }
        }

        public override bool Equals(Type txOther)
        {
            if (txOther is MetadataOnlyTypeVariableRef)
            {
                return (m_ownerMethodToken != 0) && (m_position == txOther.GenericParameterPosition);
            }

            //if txOther is not a type parameter, return false.
            MetadataOnlyTypeVariable other = txOther as MetadataOnlyTypeVariable;
            if (other == null)
            {
                return false;
            }

            //two type variables are considered the same if both their names and declaring types/methods are the same.
            if (this.Name != other.Name)
            {
                return false;
            }
            //The type parameters can be equal only if the owners are in the same module.
            //Therefore we can simply compare the metadata token of the owners.
            //This can save us by not creating the owner type/method object.
            return this.m_ownerTypeToken == other.m_ownerTypeToken &&
                   this.m_ownerMethodToken == other.m_ownerMethodToken &&
                   this.Module.Equals(other.Module);
        }


        public override bool IsAssignableFrom(Type c)
        {
            return MetadataOnlyTypeDef.IsAssignableFromHelper(this, c);
        }

        public override Type UnderlyingSystemType
        {
            get { return this; }
        }


        public override Type GetElementType()
        {
            return null;
        }

        public override int MetadataToken
        {
            get { return m_Token; }
        }

        public override MethodInfo[] GetMethods(BindingFlags flags)
        {
            return new MethodInfo[0];
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return new ConstructorInfo[0];
        }

        public override FieldInfo[] GetFields(BindingFlags flags)
        {
            return new FieldInfo[0];
        }

        public override FieldInfo GetField(string name, BindingFlags flags)
        {
            return null;
        }

        public override PropertyInfo[] GetProperties(BindingFlags flags)
        {
            return new PropertyInfo[0];
        }

        public override EventInfo[] GetEvents(System.Reflection.BindingFlags flags)
        {
            return new EventInfo[0];
        }

        public override EventInfo GetEvent(string name, System.Reflection.BindingFlags flags)
        {
            return null;
        }

        public override Type MakeGenericType(Type[] argTypes)
        {
            // You can't make a generic type around a Type variable 
            throw new InvalidOperationException();
        }


        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return null;
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return Type.EmptyTypes;
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            // In MSDN, the following is mentioned for the Type.TypeAttributes property.
            // If the current Type represents a generic type parameter — that is, if the 
            // IsGenericParameter property returns true — the TypeAttributes value returned 
            // by this property is unspecified. 
            // LMR returns a value that is the same as what we get from the reflection here. 
            // But since it is unspecified, people should not depend on it.
            return TypeAttributes.Public;
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return null;
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return null;
        }

        // This is the most distinguishing property of this derived type.
        public override bool IsGenericParameter
        {
            get { return true; }
        }

        public override Type[] GetGenericArguments()
        {
            return Type.EmptyTypes;
        }

        public override Type[] GetGenericParameterConstraints()
        {
            List<Type> l = new List<Type>(m_resolver.GetConstraintTypes(m_Token));
            return l.ToArray();
        }

        public override Type GetGenericTypeDefinition()
        {
            throw new InvalidOperationException();
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return null;
        }

        public override Type[] GetInterfaces()
        {
            return MetadataOnlyTypeDef.GetAllInterfacesHelper(this);
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            return MetadataOnlyModule.GetInterfaceHelper(GetInterfaces(), name, ignoreCase);
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return MetadataOnlyTypeDef.GetMembersHelper(this, bindingAttr);
        }

        public override Guid GUID
        {
            get { return Guid.Empty; }
        }

        protected override bool HasElementTypeImpl()
        {
            return false;
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException();
        }


        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return this.Resolver.GetCustomAttributeData(this.MetadataToken);
        }

        public override System.Reflection.GenericParameterAttributes GenericParameterAttributes
        {
            get 
            {
                return m_gpAttributes;
            }
        }

        public override int GenericParameterPosition
        {
            get
            {
                return (int)m_position;
            }
        }
        #endregion


        #region MemberInfo Members

        public override MemberTypes MemberType
        {
            get { return MemberTypes.TypeInfo; }
        }

        public override Type DeclaringType
        {
            get 
            {
                if (m_ownerTypeToken != 0)
                {
                    return m_resolver.ResolveType(m_ownerTypeToken);
                }
                else if (DeclaringMethod != null)
                {
                    return DeclaringMethod.DeclaringType;
                }

                return null; 
            }
        }

        public override MethodBase DeclaringMethod
        {
            get
            {
                if (m_ownerMethodToken != 0)
                {
                    return m_resolver.ResolveMethod(m_ownerMethodToken);
                }
                return null;
            }
        }

        public override string Name
        {
            get { return m_name; }
        }

        public override string Namespace
        {
            get 
            {
                if (this.DeclaringType != null)
                {
                    return this.DeclaringType.Namespace;
                }
                if (this.DeclaringMethod != null)
                {
                    return this.DeclaringMethod.DeclaringType.Namespace;
                }
                return null;
            }
        }

        public override Assembly Assembly
        {
            get 
            {
                return m_resolver.Assembly;
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

        public override Type ReflectedType
        {
            get { throw new NotSupportedException(); }
        }

        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        protected override TypeCode GetTypeCodeImpl()
        {
            // Unresolved generic type parameters are just considered objects.
            return TypeCode.Object;
        }
    }

} // namespace
