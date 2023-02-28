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
    /// Implement a PropertyInfo based off an IMetadataImport. 
    /// </summary>
    public class MetadataOnlyPropertyInfo : PropertyInfo
    {
        // Public constructor for factory pattern.
        public MetadataOnlyPropertyInfo(MetadataOnlyModule resolver, Token propToken, Type[] typeArgs, Type[] methodArgs)
        {           
            m_resolver = resolver;
            Debug.Assert(propToken.IsType(TokenType.Property));
            m_PropertyToken = propToken;
            m_context = new GenericContext(typeArgs, methodArgs);
                    
            IMetadataImport import = m_resolver.RawImport;
            EmbeddedBlobPointer pvSigBlob;
            int cbSigBlob;
            int cplusTypeFlag;
            UnusedIntPtr ppValue;
            int chValue;
            PropertyAttributes attrib;
            Token pmdSetter;
            Token pmdGetter;
            Token rmdOtherMethod;
            uint pcOtherMethod;

            import.GetPropertyProps(
                 m_PropertyToken,
                 out m_declaringClassToken,
                 null,
                 0,
                 out m_nameLength,
                 out attrib,
                 out pvSigBlob,
                 out cbSigBlob,
                 out cplusTypeFlag,
                 out ppValue,
                 out chValue,
                 out pmdSetter,
                 out pmdGetter,
                 out rmdOtherMethod,
                 1,
                 out pcOtherMethod);

            m_attrib = attrib;

            byte[] sig = this.m_resolver.ReadEmbeddedBlob(pvSigBlob, cbSigBlob);
            int index = 0;

            CorCallingConvention callConv = SignatureUtil.ExtractCallingConvention(sig, ref index);
            Debug.Assert(callConv != CorCallingConvention.Generic);
            Debug.Assert(callConv != CorCallingConvention.GenericInst);
            Debug.Assert(callConv != CorCallingConvention.LocalSig);
            Debug.Assert(callConv != CorCallingConvention.Field);

            //Skip the number of indexer parameters
            SignatureUtil.ExtractInt(sig, ref index);

            m_propertyType = SignatureUtil.ExtractType(sig, ref index, m_resolver, m_context);
            Debug.Assert(m_propertyType != null);

            m_setterToken = pmdSetter;
            m_getterToken = pmdGetter;
        }

        private void InitializeName()
        {
            if (string.IsNullOrEmpty(m_name))
            {
                IMetadataImport import = m_resolver.RawImport;
                EmbeddedBlobPointer pvSigBlob;
                int cbSigBlob;
                int cplusTypeFlag;
                UnusedIntPtr ppValue;
                int chValue;
                PropertyAttributes attrib;
                Token pmdSetter;
                Token pmdGetter;
                Token rmdOtherMethod;
                uint pcOtherMethod;
                Token declaringClassToken;

                StringBuilder sb = StringBuilderPool.Get(m_nameLength);

                import.GetPropertyProps(
                     m_PropertyToken,
                     out declaringClassToken,
                     sb,
                     sb.Capacity,
                     out m_nameLength,
                     out attrib,
                     out pvSigBlob,
                     out cbSigBlob,
                     out cplusTypeFlag,
                     out ppValue,
                     out chValue,
                     out pmdSetter,
                     out pmdGetter,
                     out rmdOtherMethod,
                     1,
                     out pcOtherMethod);

                m_name = sb.ToString();
                StringBuilderPool.Release(ref sb);
            }
        }

        public override string ToString()
        {
            return DeclaringType.ToString() + "." + Name;
        }

        #region PropertyInfo Members

        public override System.Reflection.PropertyAttributes Attributes
        {
            get { return m_attrib; }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Property;
            }
        }

        public override string Name
        {
            get { InitializeName(); return m_name; }
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

        public override Type PropertyType
        {
            get
            {
                return m_propertyType;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                Type declaringType = m_resolver.GetGenericType(m_declaringClassToken, m_context);
                Debug.Assert(declaringType != null);
                return declaringType;
            }
        }

        public override Object GetConstantValue()
        {
            //TODO: Not a feature of C# or VB.
            //Will implement if we can find a way to test.
            throw new NotImplementedException();
        }

        public override int MetadataToken 
        { 
            get 
            { 
                return m_PropertyToken; 
            } 
        }

        public override bool CanRead
        {
            get
            {
                return !m_getterToken.IsNil;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return !m_setterToken.IsNil;
            }
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            //TODO: In C# and VB, the only accessors for a property are getter and setter.
            //We will add other accessors in the result if we can find a way to test it
            //either in another language or in a created metadata.
            List<MethodInfo> l = new List<MethodInfo>();
            MethodInfo getter = GetGetMethod(nonPublic);
            if (getter != null)
            {
                l.Add(getter);
            }
            MethodInfo setter = GetSetMethod(nonPublic);
            if (setter != null)
            {
                l.Add(setter);
            }
            return l.ToArray();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (m_getterToken.IsNil)
            {
                return null;
            }
            MethodInfo getter = m_resolver.GetGenericMethodInfo(m_getterToken, this.m_context);
            if (nonPublic || getter.IsPublic)
            {
                return getter;
            }
            return null;
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (m_setterToken.IsNil)
            {
                return null;
            }
            MethodInfo setter = m_resolver.GetGenericMethodInfo(m_setterToken, this.m_context);
            if (nonPublic || setter.IsPublic)
            {
                return setter;
            }
            return null;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            // TODO: We should probably be reading this from the property's signature (we throw it away in the ctor)
            // The CLI spec requires that these signatures match, but in WinMD they don't (getter includes HRESULT return
            // value, property signature doesn't).
            MethodInfo getter = GetGetMethod(true);
            if (getter != null)
            {
                return getter.GetParameters();
            }

            return new ParameterInfo[0];
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        #endregion

        public override Module Module
        {
            get { return m_resolver; }
        }

        public override bool Equals(object obj)
        {
            MetadataOnlyPropertyInfo prop = obj as MetadataOnlyPropertyInfo;
            if (prop != null)
            {
                //Need to check the declaring type explicitly, even if the tokens match, because the generic type arguments might
                //no match.
                return prop.m_resolver.Equals(m_resolver) && (prop.m_PropertyToken.Equals(m_PropertyToken)) &&
                    (DeclaringType.Equals(prop.DeclaringType));
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return m_resolver.GetHashCode() * 32767 + m_PropertyToken.GetHashCode();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return m_resolver.GetCustomAttributeData(this.MetadataToken);
        }
        
        readonly private MetadataOnlyModule m_resolver;
        readonly private Token m_PropertyToken;
        readonly private PropertyAttributes m_attrib;
        readonly private Token m_declaringClassToken;
        readonly private Type m_propertyType;
        readonly private GenericContext m_context;
        private string m_name;
        private int m_nameLength;
        readonly private Token m_setterToken;
        readonly private Token m_getterToken;
    }
}
