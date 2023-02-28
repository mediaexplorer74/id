using System;
using System.Collections.Generic;
using System.Text;
using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.Reflection.Adds;
using System.Runtime.InteropServices;

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
using EventAttributes = System.Reflection.EventAttributes;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// Implement a FieldInfo based off an IMetadataImport. 
    /// </summary>
    public class MetadataOnlyEventInfo : EventInfo
    {
        public MetadataOnlyEventInfo(MetadataOnlyModule resolver, Token eventToken, Type[] typeArgs, Type[] methodArgs)
        {
            m_resolver = resolver;
            Debug.Assert(eventToken.IsType(TokenType.Event));
            m_eventToken = eventToken;
            m_context = new GenericContext(typeArgs, methodArgs);

            IMetadataImport import = m_resolver.RawImport;
            int attrib;
            int pmdAddMethod;
            int pmdRaiseMethod;
            int pmdRemoveMethod;
            int rmdOtherMethod;
            uint pcOtherMethod;

            import.GetEventProps(
                 m_eventToken,
                 out m_declaringClassToken,
                 null,
                 0,
                 out m_nameLength,
                 out attrib,
                 out m_eventHandlerTypeToken,
                 out pmdAddMethod,
                 out pmdRemoveMethod,
                 out pmdRaiseMethod,
                 out rmdOtherMethod,
                 1,
                 out pcOtherMethod);

            m_attrib = (EventAttributes)attrib;
            m_addMethodToken = new Token(pmdAddMethod);
            m_removeMethodToken = new Token(pmdRemoveMethod);
            m_raiseMethodToken = new Token(pmdRaiseMethod);
        }

        public override string ToString()
        {
            return DeclaringType.ToString() + "." + Name;
        }

        /// <summary>
        /// Lookup event name only when really needed and cache it in this instance.
        /// </summary>
        private void InitializeName()
        {
            if (string.IsNullOrEmpty(m_name))
            {
                IMetadataImport import = m_resolver.RawImport;
                int chEvent;
                int attrib;
                int pmdAddMethod;
                int pmdRaiseMethod;
                int pmdRemoveMethod;
                int rmdOtherMethod;
                uint pcOtherMethod;

                StringBuilder sb = StringBuilderPool.Get(m_nameLength);

                import.GetEventProps(
                     m_eventToken,
                     out m_declaringClassToken,
                     sb,
                     sb.Capacity,
                     out chEvent,
                     out attrib,
                     out m_eventHandlerTypeToken,
                     out pmdAddMethod,
                     out pmdRemoveMethod,
                     out pmdRaiseMethod,
                     out rmdOtherMethod,
                     1,
                     out pcOtherMethod);

                m_name = sb.ToString();
                StringBuilderPool.Release(ref sb);
            }
        }

        #region EventInfo Members

        public override System.Reflection.EventAttributes Attributes
        {
            get { return m_attrib; }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Event;
            }
        }

        public override string Name
        {
            get { InitializeName();  return m_name; }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override Type ReflectedType
        {
            get { throw new NotSupportedException(); }
        }

        public override Type EventHandlerType
        {
            get
            {
                Type eventHandlerType = m_resolver.GetGenericType(new Token(m_eventHandlerTypeToken), m_context);
                Debug.Assert(eventHandlerType != null);

                return eventHandlerType;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                Type declaringType = m_resolver.GetGenericType(new Token(m_declaringClassToken), m_context);
                Debug.Assert(declaringType != null);
                return declaringType;
            }
        }

        public override int MetadataToken 
        { 
            get 
            { 
                return m_eventToken; 
            } 
        }

        public override MethodInfo GetAddMethod(bool nonPublic)
        {
            if (m_addMethodToken.IsNil)
            {
                return null;
            }
            MethodInfo addMethod = m_resolver.GetGenericMethodInfo(m_addMethodToken, this.m_context);
            if (nonPublic || addMethod.IsPublic)
            {
                return addMethod;
            }
            return null;
        }

        public override MethodInfo GetRemoveMethod(bool nonPublic)
        {
            if (m_removeMethodToken.IsNil)
            {
                return null;
            }
            MethodInfo removeMethod = m_resolver.GetGenericMethodInfo(m_removeMethodToken, this.m_context);
            if (nonPublic || removeMethod.IsPublic)
            {
                return removeMethod;
            }
            return null;
        }

        public override MethodInfo GetRaiseMethod(bool nonPublic)
        {
            if (m_raiseMethodToken.IsNil)
            {
                return null;
            }
            MethodInfo raiseMethod = m_resolver.GetGenericMethodInfo(m_raiseMethodToken, this.m_context);
            if (nonPublic || raiseMethod.IsPublic)
            {
                return raiseMethod;
            }
            return null;
        }

        #endregion

        public override Module Module
        {
            get { return m_resolver; }
        }

        public override bool Equals(object obj)
        {
            MetadataOnlyEventInfo ev = obj as MetadataOnlyEventInfo;
            if (ev != null)
            {
                //Need to check the declaring type explicitly, even if the tokens match, because the generic type arguments might
                //no match.
                return ev.m_resolver.Equals(m_resolver) && (ev.m_eventToken.Equals(m_eventToken)) &&
                    (DeclaringType.Equals(ev.DeclaringType));
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return m_resolver.GetHashCode() * 32767 + m_eventToken.GetHashCode();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return m_resolver.GetCustomAttributeData(this.MetadataToken);
        }

        private MetadataOnlyModule m_resolver;
        private int m_eventToken;
        private EventAttributes m_attrib;
        private int m_declaringClassToken;
        private int m_eventHandlerTypeToken;
        private GenericContext m_context;
        private string m_name;
        private int m_nameLength;
        private Token m_addMethodToken;
        private Token m_removeMethodToken;
        private Token m_raiseMethodToken;
    }
}
