// LocalVariableInfo
// 

using System;
using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.Collections.Generic;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using BindingFlags = System.Reflection.BindingFlags;
using CallingConventions = System.Reflection.CallingConventions;
using MethodAttributes = System.Reflection.MethodAttributes;
using Type = System.Reflection.Mock.Type;
using MemberTypes = System.Reflection.MemberTypes;
#endif

namespace Microsoft.MetadataReader
{
    internal class MetadataOnlyLocalVariableInfo : LocalVariableInfo
    {
        // Type of the local variable.
        readonly private Type m_type;

        // IL index of the local variable.
        readonly private int m_index;

        //Whether this is pinned
        readonly private bool m_fPinned;

        public MetadataOnlyLocalVariableInfo(int index, Type type, bool fPinned)
        {
            m_type = type;
            m_index = index;
            m_fPinned = fPinned;
        }

        public override bool IsPinned
        {
            get { return m_fPinned; }
        }

        public override int LocalIndex
        {
            get { return m_index; }
        }

        public override Type LocalType
        {
            get { return m_type; }
        }
    }
}