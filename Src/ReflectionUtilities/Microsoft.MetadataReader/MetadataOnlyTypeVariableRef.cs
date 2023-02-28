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
    /// Represents a type variable extracted from memberRef signature. 
    /// It is only used for signature matching.
    /// </summary>
    /// <remarks>
    /// This class is used internally only and it should not be exposed
    /// publicly even when all LMR APIs become public. That's why most
    /// methods throw exceptions. 
    /// </remarks>
    internal class MetadataOnlyTypeVariableRef : MetadataOnlyCommonType
    {
        readonly private MetadataOnlyModule m_resolver;
        readonly private Token m_ownerToken;
        readonly private int m_position;

        internal MetadataOnlyTypeVariableRef(MetadataOnlyModule resolver, Token ownerToken, int position)
        {
            Debug.Assert(resolver != null, "resolver can't be null");
            Debug.Assert(!ownerToken.IsNil, "owner token can't be Nil");
            Debug.Assert(position >= 0, "position must be zero or positive");

            m_resolver = resolver;
            m_ownerToken = ownerToken;
            m_position = position;
        }

        private bool IsMethodVar
        {
            get
            {
                return m_ownerToken.IsType(TokenType.MemberRef) || m_ownerToken.IsType(TokenType.MethodDef);
            }
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
                throw new InvalidOperationException();
            }
        }

        public override bool Equals(Type other)
        {
            // Use fast comparison for our own type.
            MetadataOnlyTypeVariableRef otherRef = other as MetadataOnlyTypeVariableRef;
            if (otherRef != null)
            {
                return (this.Resolver.Equals(otherRef.Resolver) &&
                    (m_ownerToken.Value == otherRef.m_ownerToken.Value) &&
                    (m_position == otherRef.m_position));
            }

            if (other.IsGenericParameter)
            {
                // Check if both variables are MethodVars or TypeVars. 
                bool isSameKind = (this.IsMethodVar == (other.DeclaringMethod != null));
                return (m_position == other.GenericParameterPosition) && isSameKind;
            }

            return false;
        }

        public override bool IsAssignableFrom(Type c)
        {
            throw new InvalidOperationException();
        }

        public override Type UnderlyingSystemType
        {
            get { throw new InvalidOperationException(); }
        }

        public override Type GetElementType()
        {
            throw new InvalidOperationException();
        }

        public override int MetadataToken
        {
            get { throw new InvalidOperationException(); }
        }

        public override MethodInfo[] GetMethods(BindingFlags flags)
        {
            throw new InvalidOperationException();
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            throw new InvalidOperationException();
        }

        public override FieldInfo[] GetFields(BindingFlags flags)
        {
            throw new InvalidOperationException();
        }

        public override FieldInfo GetField(string name, BindingFlags flags)
        {
            throw new InvalidOperationException();
        }

        public override PropertyInfo[] GetProperties(BindingFlags flags)
        {
            throw new InvalidOperationException();
        }

        public override EventInfo[] GetEvents(System.Reflection.BindingFlags flags)
        {
            throw new InvalidOperationException();
        }

        public override EventInfo GetEvent(string name, System.Reflection.BindingFlags flags)
        {
            throw new InvalidOperationException();
        }

        public override Type MakeGenericType(Type[] argTypes)
        {
            throw new InvalidOperationException();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new InvalidOperationException();
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            throw new InvalidOperationException();
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            throw new InvalidOperationException();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new InvalidOperationException();
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new InvalidOperationException();
        }

        // This is the most distinguishing property of this derived type.
        public override bool IsGenericParameter
        {
            get { return true; }
        }

        public override Type[] GetGenericArguments()
        {
            throw new InvalidOperationException();
        }

        public override Type[] GetGenericParameterConstraints()
        {
            throw new InvalidOperationException();
        }

        public override Type GetGenericTypeDefinition()
        {
            throw new InvalidOperationException();
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new InvalidOperationException();
        }

        public override Type[] GetInterfaces()
        {
            throw new InvalidOperationException();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new InvalidOperationException();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            throw new InvalidOperationException();
        }

        public override Guid GUID
        {
            get { throw new InvalidOperationException(); }
        }

        protected override bool HasElementTypeImpl()
        {
            throw new InvalidOperationException();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException();
        }


        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            throw new InvalidOperationException();
        }

        public override System.Reflection.GenericParameterAttributes GenericParameterAttributes
        {
            get
            {
                // TODO: check if this can ever be important part in signature matching
                throw new InvalidOperationException();
            }
        }

        public override int GenericParameterPosition
        {
            get
            {
                return m_position;
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
                if (!this.IsMethodVar)
                {
                    if (m_ownerToken.IsType(TokenType.TypeDef))
                    {
                        return m_resolver.Factory.CreateSimpleType(m_resolver, m_ownerToken);
                    }
                    else
                    {
                        return m_resolver.Factory.CreateTypeRef(m_resolver, m_ownerToken);
                    }
                }
                return null;
            }
        }

        public override MethodBase DeclaringMethod
        {
            get
            {
                if (this.IsMethodVar)
                {
                    return m_resolver.Factory.CreateMethodOrConstructor(m_resolver, m_ownerToken, null, null);
                }
                return null;
            }
        }

        public override string Name
        {
            get { return null; }
        }

        public override string Namespace
        {
            get
            {
                return null;
            }
        }

        public override Assembly Assembly
        {
            get
            {
                // important to implement so we can create arrays of TypeVarRefs 
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
            if (this.IsMethodVar)
            {
                return "MVar!!" + this.GenericParameterPosition.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                return "Var!" + this.GenericParameterPosition.ToString(CultureInfo.InvariantCulture);
            }
        }

        #endregion

        protected override TypeCode GetTypeCodeImpl()
        {
            throw new InvalidOperationException();
        }
    }
}
