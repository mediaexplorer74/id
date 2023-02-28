using System;
using System.Collections.Generic;
using System.Text;
using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.Runtime.InteropServices;
using System.Reflection.Adds;

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
    /// <summary>
    /// A ParameterInfo constructed purely from information in the method signature without any other
    /// backing metadata. Contrast to code:MetadataOnlyParameterInfo, which is backed by real metadata.
    /// The CLR just needs the method signature and doesn't actually need parameter metadata. Compilers may
    /// emit parameter metadata at their discretion. One case where the C# compiler omits the metadata is
    /// for anonymous delegates whose body does not use any parameters.
    /// 
    /// Properties not passed to the constructor return values to match reflection's existing
    /// implementation. Reflection's choice here is arguably an arbitrary policy decision, which is why
    /// these ParameterInfos are routed through code:IMetadataExtensionsPolicy. 
    /// </summary>
    internal class SimpleParameterInfo : ParameterInfo
    {
        readonly MemberInfo m_member;
        readonly Type m_paramType;
        readonly int m_position;

        // Create via code:IMetadataExtensionsPolicy.GetFakeParameterInfo
        internal SimpleParameterInfo(MemberInfo member, Type paramType, int position)
        {
            Debug.Assert(member != null);
            Debug.Assert(paramType != null);

            m_member = member;
            m_paramType = paramType;
            m_position = position;

        }

        // This matches the CLR's impl
        public override string ToString()
        {
            StringBuilder sb = StringBuilderPool.Get();
            MetadataOnlyCommonType.TypeSigToString(this.ParameterType, sb);
            sb.Append(' '); // Reflection has the extra space
            
            string result = sb.ToString();
            StringBuilderPool.Release(ref sb);
            return result;
        }

        public override int Position
        {
            get { return m_position; }
        }

        public override Type ParameterType
        {
            get { return m_paramType; }
        }

        public override MemberInfo Member
        {
            get { return m_member; }
        }

        #region Default properties
        // These are properties determined from reflection.

        public override int MetadataToken
        {
            get
            {
                // Reflection explicit has rid==0x08, instead of a pure 0 token.
                return 0x08000000;
            }
        }

        public override ParameterAttributes Attributes
        {
            get { return ParameterAttributes.None; }
        }

        public override string Name
        {
            // The name would be stored in the metadata.
            // In CLR 3.5, Reflection explicitly returns null here instead of String.Empty.
            // CLR 4.0 has a breaking change to return String.Empty.
            // LMR matches CLR V4's behavior.
            get { return String.Empty; }
        }

        public override object DefaultValue
        {
            get { return null; }
        }

        public override object RawDefaultValue
        {
            get { return null; }
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return new CustomAttributeData[0];
        }

        public override Type[] GetOptionalCustomModifiers()
        {
            return Type.EmptyTypes;
        }

        public override Type[] GetRequiredCustomModifiers()
        {
            return Type.EmptyTypes;
        }
        #endregion // Default properties
    }

}
