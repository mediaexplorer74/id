using System;
using System.Collections.Generic;
using Debug=Microsoft.MetadataReader.Internal.Debug;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using Type = System.Reflection.Mock.Type;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// The class is used to represent custom modifiers, defined using modreq ("required modifier") 
    /// and modopt ("optional modifier"). See Standard II.7.1.1 
    /// </summary>
    internal class CustomModifiers
    {
        readonly private List<Type> m_optional;
        readonly private List<Type> m_required;

        public CustomModifiers(List<Type> optModifiers, List<Type> reqModifiers)
        {
            this.m_optional = optModifiers;
            this.m_required = reqModifiers;
        }

        public Type[] OptionalCustomModifiers
        {
            get
            {
                if (m_optional != null)
                {
                    return m_optional.ToArray();
                }
                else
                {
                    return Type.EmptyTypes;
                }
            }
        }

        public Type[] RequiredCustomModifiers
        {
            get
            {
                if (m_required != null)
                {
                    return m_required.ToArray();
                }
                else
                {
                    return Type.EmptyTypes;
                }
            }
        }

    }
}
