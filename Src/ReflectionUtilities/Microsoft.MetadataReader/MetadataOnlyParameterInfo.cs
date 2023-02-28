using System;
using System.Collections.Generic;
using System.Text;
using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.Runtime.InteropServices;
using System.Reflection.Adds;
using System.Globalization;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using BindingFlags = System.Reflection.BindingFlags;
using Type = System.Reflection.Mock.Type;
using ParameterAttributes = System.Reflection.ParameterAttributes;
#endif

namespace Microsoft.MetadataReader
{
    // Implement a ParameterInfo based off an IMetadataImport.
    // Contrast that to code:SimpleParameterInfo, which is implemented purely from the information in the
    // signature without any additional metadata.
    internal class MetadataOnlyParameterInfo : ParameterInfo
    {
        internal MetadataOnlyParameterInfo(MetadataOnlyModule resolver, Token parameterToken, Type paramType, CustomModifiers customModifiers)
        {
            m_resolver = resolver;
            Debug.Assert(parameterToken.IsType(TokenType.ParamDef));
            m_parameterToken = parameterToken;

            // Parameter type information is not in the metadata for the parameter, but in the
            // metadata for the method. We pass in the parameter type computed at the parent method
            // for efficiency reason so that we do not access the metada of the parent method multiple
            // times.
            m_paramType = paramType;
            m_customModifiers = customModifiers;
                        
            IMetadataImport importer = m_resolver.RawImport;
            uint pulSequence, pdwAttr, pdwCPlusTypeFlag, pcchValue;

            UnusedIntPtr ppValue;
            importer.GetParamProps(
                m_parameterToken,
                out m_parentMemberToken,
                out pulSequence,
                null,
                0,
                out m_nameLength,
                out pdwAttr,
                out pdwCPlusTypeFlag,
                out ppValue,
                out pcchValue);
            m_position = (int)pulSequence - 1;
            m_attrib = (ParameterAttributes)pdwAttr;
        }

        void InitializeName()
        {
            if (string.IsNullOrEmpty(m_name))
            {
                IMetadataImport importer = m_resolver.RawImport;
                uint pulSequence, pdwAttr, pdwCPlusTypeFlag, pcchValue, size;
                int parentMemberToken;
                UnusedIntPtr ppValue;

                StringBuilder name = StringBuilderPool.Get((int)m_nameLength);
                importer.GetParamProps(
                    m_parameterToken,
                    out parentMemberToken,
                    out pulSequence,
                    name,
                    (uint)name.Capacity,
                    out size,
                    out pdwAttr,
                    out pdwCPlusTypeFlag,
                    out ppValue,
                    out pcchValue);

                m_name = name.ToString();
                StringBuilderPool.Release(ref name);
            }
        }

        #region ParameterInfo Members

        override public System.Reflection.ParameterAttributes Attributes
        {
            get { return m_attrib; }
        }

        public override Type[] GetOptionalCustomModifiers()
        {
            if (m_customModifiers == null)
            {
                return Type.EmptyTypes;
            }
            return m_customModifiers.OptionalCustomModifiers;
        }

        public override Type[] GetRequiredCustomModifiers()
        {
            if (m_customModifiers == null)
            {
                return Type.EmptyTypes;
            }
            return m_customModifiers.RequiredCustomModifiers;
        }

        override public string Name
        {
            get { InitializeName();  return m_name; }
        }

        public override MemberInfo Member
        {
            get 
            {
                return m_resolver.ResolveMethod(m_parentMemberToken);
            }
        }

        override public int Position
        {
            get { return m_position; }
        }

        override public Type ParameterType
        {
            get { return m_paramType; }
        }

        public override int MetadataToken
        {
            get
            {
                return m_parameterToken;
            }
        }

        override public Object DefaultValue
        {
            //Should use RawDefaultValue instead.
            get { throw new InvalidOperationException(); }
        }

        override public Object RawDefaultValue
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public override bool Equals(object obj)
        {
            MetadataOnlyParameterInfo f = obj as MetadataOnlyParameterInfo;
            if (f != null)
            {
                return f.m_resolver.Equals(m_resolver) && (f.m_parameterToken.Equals(m_parameterToken));
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return m_resolver.GetHashCode() * 32767 + m_parameterToken.GetHashCode();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return m_resolver.GetCustomAttributeData(this.MetadataToken);
        }

        public override string ToString() 
        {
            return string.Format(
                CultureInfo.InvariantCulture, 
                "{0} {1}", 
                MetadataOnlyCommonType.TypeSigToString(ParameterType),
                Name
            );
        }

        /// <summary>
        /// New API to get at any marshaling information on this parameter
        /// </summary>
        /// <remarks>
        /// Note that LMR doesn't expose most pseudo-custom attributes as attributes like reflection does.
        /// We could do that instead but it would be slower (need to synthesize a ConstructorInfo, etc.),
        /// and this is easier to use anyway.
        /// </remarks>
        public MarshalAsAttribute GetMarshalInfo()
        {
            EmbeddedBlobPointer pNativeType;
            int cbNativeType;
            m_resolver.RawImport.GetFieldMarshal(m_parameterToken, out pNativeType, out cbNativeType);

            byte[] blob = m_resolver.RawMetadata.ReadEmbeddedBlob(pNativeType, cbNativeType);
            int idx = 0;
            UnmanagedType u = SignatureUtil.ExtractUnmanagedType(blob, ref idx);
            var attr = new MarshalAsAttribute(u);
            if (u == UnmanagedType.LPArray)
            {
                UnmanagedType et = SignatureUtil.ExtractUnmanagedType(blob, ref idx);
                attr.ArraySubType = et;
                if (idx < blob.Length)
                {
                    int paramNum = SignatureUtil.ExtractInt(blob, ref idx);
                    attr.SizeParamIndex = checked((short)paramNum);
                    if (idx < blob.Length)
                    {
                        int numElem = SignatureUtil.ExtractInt(blob, ref idx);
                        attr.SizeConst = numElem;
                    }
                }
                else
                    // TODO: How does reflection fill this in when not specified?
                    attr.SizeParamIndex = -1;
            }
            // Note that we don't currently fill in the extra details of other special UnmanagedTypes

            return attr;
        }
        
        readonly private MetadataOnlyModule m_resolver;
        readonly private int m_parameterToken;
        readonly private ParameterAttributes m_attrib;
        readonly private Type m_paramType;
        readonly private CustomModifiers m_customModifiers;

        private string m_name;
        private uint m_nameLength;

        //position is starting from zero.
        readonly private int m_position;

        // Token of the member that the field is in.
        readonly private int m_parentMemberToken;
    }
}
