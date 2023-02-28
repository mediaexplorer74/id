using System;
using System.Collections.Generic;
using System.Text;
using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.Runtime.InteropServices;
using System.Reflection.Adds;
using System.Threading;

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
    /// Implement a FieldInfo based off an IMetadataImport. 
    /// </summary>
    public class MetadataOnlyFieldInfo : FieldInfo, IFieldInfo2
    {
        public MetadataOnlyFieldInfo(MetadataOnlyModule resolver, Token fieldDefToken, Type[] typeArgs, Type[] methodArgs)
        {
            m_resolver = resolver;
            Debug.Assert(fieldDefToken.IsType(TokenType.FieldDef));
            m_fieldDefToken = fieldDefToken;

            if ((typeArgs != null) || (methodArgs != null))
            {
                m_context = new GenericContext(typeArgs, methodArgs);
            }

            IMetadataImport import = m_resolver.RawImport;

            // Caller should have verified that we have a valid token.
            Debug.Assert(m_resolver.IsValidToken(m_fieldDefToken));
            EmbeddedBlobPointer pvSigBlob;
            int cbSigBlob;
            int cplusTypeFlag;
            IntPtr ppValue;
            int chValue;
            FieldAttributes attrib;

            // Just get some of information about properties. Get rest later only if needed.
            import.GetFieldProps(m_fieldDefToken, out m_declaringClassToken, null, 0, out m_nameLength, out attrib,
                out pvSigBlob, out cbSigBlob, out cplusTypeFlag, out ppValue, out chValue);

            m_attrib = attrib;
        }
        
        /// <summary>
        /// Gets just field name. If this is never needed we avoid allocating string for it.
        /// </summary>
        private void InitializeName()
        {
            if (string.IsNullOrEmpty(m_name))
            {
                IMetadataImport import = m_resolver.RawImport;

                // Caller should have verified that we have a valid token.
                Debug.Assert(m_resolver.IsValidToken(m_fieldDefToken));

                int chField;
                EmbeddedBlobPointer pvSigBlob;
                int cbSigBlob;
                int cplusTypeFlag;
                IntPtr ppValue;
                int chValue;
                FieldAttributes attrib;
                int declaringClass;

                StringBuilder sb = StringBuilderPool.Get(m_nameLength);
                import.GetFieldProps(m_fieldDefToken, out declaringClass, sb, sb.Capacity, out chField, out attrib,
                    out pvSigBlob, out cbSigBlob, out cplusTypeFlag, out ppValue, out chValue);

                m_name = sb.ToString();
                StringBuilderPool.Release(ref sb);
            }
        }

        private void Initialize()
        {
            if (m_initialized) return;

            IMetadataImport import = m_resolver.RawImport;

            // Caller should have verified that we have a valid token.
            Debug.Assert(m_resolver.IsValidToken(m_fieldDefToken));

            int chField;
            EmbeddedBlobPointer pvSigBlob;
            int cbSigBlob;
            int cplusTypeFlag;
            IntPtr ppValue;
            int chValue;
            FieldAttributes attrib;
            int declaringClass;

            import.GetFieldProps(m_fieldDefToken, out declaringClass, null, 0, out chField, out attrib,
                out pvSigBlob, out cbSigBlob, out cplusTypeFlag, out ppValue, out chValue);

            byte[] sig = this.m_resolver.ReadEmbeddedBlob(pvSigBlob, cbSigBlob);

            int idx = 0;
            CorCallingConvention callConv = SignatureUtil.ExtractCallingConvention(sig, ref idx);
            Debug.Assert(callConv == CorCallingConvention.Field);

            // Possibly required or optional modifiers.
            m_customModifiers = SignatureUtil.ExtractCustomModifiers(sig, ref idx, m_resolver, m_context);

            if (m_resolver.RawImport.IsValidToken((uint)m_declaringClassToken))
            {
                Type ownerType = m_resolver.ResolveType(m_declaringClassToken);
                if (ownerType.IsGenericType && (m_context == null || m_context.TypeArgs == null || m_context.TypeArgs.Length == 0))
                {
                    // Update the generic context with the owner type's generic arguments.
                    if (m_context == null)
                    {
                        m_context = new GenericContext(ownerType.GetGenericArguments(), null);
                    }
                    else
                    {
                        m_context = new GenericContext(ownerType.GetGenericArguments(), m_context.MethodArgs);
                    }
                }
            }
            m_fieldType = SignatureUtil.ExtractType(sig, ref idx, m_resolver, m_context);
            Debug.Assert(idx == sig.Length, "Some of our signature bytes went unprocessed.  This is probably caused by a bug in the signature parser");
            
            m_initialized = true;
        }
                
        // This definition matches the CLR's 
        public override string ToString()
        {
            return (MetadataOnlyCommonType.TypeSigToString(this.FieldType) + " " + this.Name);
        } 


        //The method ParseDefaultValue() returns the value of a field stored in the metadata.
        //The method is called to get the value of a literal field.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private object ParseDefaultValue()
        {
            Initialize();

            IMetadataImport import = m_resolver.RawImport;
            int chField;
            EmbeddedBlobPointer pvSigBlob;
            int cbSigBlob;
            int cplusTypeFlag;
            IntPtr ppValue;
            int chValue;
            FieldAttributes attrib;
            int declaringClassToken;
            import.GetFieldProps(m_fieldDefToken, out declaringClassToken, null, 0, out chField, out attrib,
                out pvSigBlob, out cbSigBlob, out cplusTypeFlag, out ppValue, out chValue);

            if (ppValue == IntPtr.Zero)
            {
                //Metadata does not contain the value of the field.
                throw new InvalidOperationException();
            }
                        
            // Some static data, such as the raw constants stored in literal arrays  var x = int [] { 1,2,3,4,5},
            // are stored as RVA based static fields.
            // We can retrieve the RVA via imdi.GetRVA(fieldDef), but then we still need to resolve the RVA and
            // hav a way to return the data.            

            byte[] sig = m_resolver.ReadEmbeddedBlob(pvSigBlob, cbSigBlob);

            int index = 0;

            CorCallingConvention callConv = SignatureUtil.ExtractCallingConvention(sig, ref index);            
            Debug.Assert(callConv == CorCallingConvention.Field);

            CorElementType elementType = SignatureUtil.ExtractElementType(sig, ref index);
            if (elementType == CorElementType.ValueType)
            {
                //skip the type token, the underlying type can be obtained by using the FieldType
                SignatureUtil.ExtractToken(sig, ref index);
                //The field has an Enum type.
                //Need to convert the element type to the underlying type to get the value.
                Debug.Assert(FieldType.IsEnum);
                //In IL Enum fields can have different type than the Enum's underlying type.
                //Therefore we shouldn't cast the value to the Enum's underlying type here.
                //Instead, we need to use the field's type specified in metadata.
                elementType = (CorElementType)cplusTypeFlag;
            }
            else if (elementType == CorElementType.GenericInstantiation)
            {
                //if the field type is an instantiated type, it must be a enum type
                Type fieldType = SignatureUtil.ExtractType(sig, ref index, m_resolver, m_context);
                Debug.Assert(fieldType.IsEnum);
                //In IL Enum fields can have different type than the Enum's underlying type.
                //Therefore we shouldn't cast the value to the Enum's underlying type here.
                //Instead, we need to use the field's type specified in metadata.
                elementType = (CorElementType)cplusTypeFlag;
            }

            
            switch (elementType)
            {
                case CorElementType.Bool:
                    byte v = Marshal.ReadByte(ppValue);
                    if (v == 0) return false;
                    else return true;
                case CorElementType.Char:
                    //Char is unicode character and is 2 bytes
                    return (char)Marshal.ReadInt16(ppValue);
                case CorElementType.SByte:
                    return (sbyte)Marshal.ReadByte(ppValue);
                case CorElementType.Byte:
                    return Marshal.ReadByte(ppValue);
                case CorElementType.Short:
                    return Marshal.ReadInt16(ppValue);
                case CorElementType.UShort:
                    return (ushort)Marshal.ReadInt16(ppValue);
                case CorElementType.Int:
                    return Marshal.ReadInt32(ppValue);
                case CorElementType.UInt:
                    return (uint)Marshal.ReadInt32(ppValue);
                case CorElementType.Long:
                    return Marshal.ReadInt64(ppValue);
                case CorElementType.ULong:
                    return (ulong)Marshal.ReadInt64(ppValue);
                case CorElementType.IntPtr:
                    return Marshal.ReadIntPtr(ppValue);
                case CorElementType.String:
                    return Marshal.PtrToStringAuto(ppValue, chValue);
                case CorElementType.Class:
                    //The value of a literal field that has a class type can only be null.
                    return null;
                case CorElementType.Float:
                    Single[] singleArray = new Single[1];
                    Marshal.Copy(ppValue, singleArray, 0, 1);
                    return singleArray[0];
                case CorElementType.Double:
                    Double[] doubleArray = new Double[1];
                    Marshal.Copy(ppValue, doubleArray, 0, 1);
                    return doubleArray[0];
                case CorElementType.UIntPtr:
                // Technically U and the floating-point ones are options in the CLI, but not in the CLS or C#, so these are NYI
                default:
                    throw new InvalidOperationException(Resources.IncorrectElementTypeValue);
            }
        }

        #region FieldInfo Members

        public override FieldAttributes Attributes
        {
            get { return m_attrib; }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Field;
            }
        }

        public override string Name
        {
            get
            {
                InitializeName();
                return m_name;
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

        public override Type[] GetOptionalCustomModifiers()
        {
            Initialize();
            if (m_customModifiers == null)
            {
                return Type.EmptyTypes;
            }
            return m_customModifiers.OptionalCustomModifiers;
        }

        public override Type[] GetRequiredCustomModifiers()
        {
            Initialize();
            if (m_customModifiers == null)
            {
                return Type.EmptyTypes;
            }
            return m_customModifiers.RequiredCustomModifiers;
        }

        public override Type FieldType
        {
            get
            {
                Initialize();
                return m_fieldType;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                Initialize();
                Type declaringType = m_resolver.GetGenericType(new Token(m_declaringClassToken), m_context);
                Debug.Assert(declaringType != null);
                return declaringType;
            }
        }

        public override Object GetValue(Object obj)
        {
            // This gets the 'live' value on the given instance objectInstance.
            // since LMR is a static metadata reader, there are no live objects and this must fail.
            //Metadata only contains value info for literal fields.
            throw new NotSupportedException();
        }

        /// <summary>
        /// implementation of IField2
        /// </summary>
        public virtual byte[] GetRvaField()
        {
            // Check attrs for HasFieldRVA, which may not be literal.            
            if ((this.Attributes & FieldAttributes.HasFieldRVA) == 0)
            {
                throw new InvalidOperationException(Resources.OperationValidOnRVAFieldsOnly);
            }
            
            var s = this.FieldType.StructLayoutAttribute;
            if (s.Value == LayoutKind.Auto)
            {
                // This case is suspicious: why would we need the raw bytes of an auto-layout struct?
                // if it's auto layout, how could the caller use the bytes? So throw for now until we
                // have a good case.
                throw new InvalidOperationException(Resources.OperationInvalidOnAutoLayoutFields);
            }


            uint rva;
            uint flags;
            this.m_resolver.RawImport.GetRVA(this.MetadataToken, out rva, out flags);
            int count = s.Size; // read the full size of the field type.

            // FieldType may be a primitive, in which case there's no StructLayoutAttribute and so size defaults to 0.
            // C# may generate this code for:
            //   new byte[] {1,2,3,4}
            // the raw data for the byte array can be stored in a 32-bit word, so it's encoded as an rva to 
            // an I4, instead of an RVA to an opaque struct.
            if (count == 0)
            {
                switch (Type.GetTypeCode(this.FieldType))
                {
                    case TypeCode.Int32:
                        count = 4;
                        break;
                    case TypeCode.Int64:
                        count = 8;
                        break;
                }
            }

            // Call out to the metadata to resolve the RVA against a base address for module.
            byte[] b = this.m_resolver.RawMetadata.ReadRva((long)rva, count);
            Debug.Assert(b.Length == count);
            return b;            
        }

        public override Object GetRawConstantValue()        
        {
            // See Ecma 15.1.2 for more information about literal fields. 
            // "Literal fields become part of the metadata but cannot be accessed by the code."
            if (!this.IsLiteral)
            {
                throw new InvalidOperationException(Resources.OperationValidOnLiteralFieldsOnly);
            }
            return ParseDefaultValue();
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get { throw new NotSupportedException(); }
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override int MetadataToken { get { return m_fieldDefToken; } }
        #endregion

        public override Module Module
        {
            get { return m_resolver; }
        }
        
        public override bool Equals(object obj)
        {
            MetadataOnlyFieldInfo f = obj as MetadataOnlyFieldInfo;
            if (f != null)
            {
                //Need to check the declaring type explicitly, even if the tokens match, because the generic type arguments might
                //no match.
                return f.m_resolver.Equals(m_resolver) && (f.m_fieldDefToken.Equals(m_fieldDefToken)) &&
                    (DeclaringType.Equals(f.DeclaringType));
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return m_resolver.GetHashCode() * 32767 + m_fieldDefToken.GetHashCode();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return m_resolver.GetCustomAttributeData(this.MetadataToken);
        }

        readonly private MetadataOnlyModule m_resolver;
        readonly private int m_fieldDefToken;
        readonly private FieldAttributes m_attrib;
        readonly private int m_declaringClassToken;
        private Type m_fieldType;
        private GenericContext m_context;
        private string m_name;
        private int m_nameLength;
        CustomModifiers m_customModifiers;
        private bool m_initialized;
    }
}
