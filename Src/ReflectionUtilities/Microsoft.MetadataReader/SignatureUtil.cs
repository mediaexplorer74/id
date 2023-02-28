using System;
using System.Collections.Generic;
using System.Globalization;
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
using CallingConventions = System.Reflection.CallingConventions;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// Custom attribute named argument identificators.
    /// </summary>
    internal enum NamedArgumentType
    {
        Field,
        Property
    }

    /// <summary>
    /// Encapsulates information about types as encoded in signature blobs.
    /// </summary>
    internal class TypeSignatureDescriptor
    {
        /// <summary>
        /// Final type encoded in sig blob. For generic types, this could be type after instantiation.
        /// E.g T is instantiated with int.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Custom modifiers for this parameter, if any are present.
        /// </summary>
        public CustomModifiers CustomModifiers { get; set; }

        /// <summary>
        /// Determines if parameter was pinned or not.
        /// </summary>
        public bool IsPinned { get; set; }
    }

    /// <summary>
    /// Encapsulates information encoded in method signature blobs.
    /// </summary>
    internal class MethodSignatureDescriptor
    {
        /// <summary>
        /// Method's calling convention.
        /// </summary>
        public CorCallingConvention CallingConvention { get; set; }

        /// <summary>
        /// Number of generic method arguments if this is a generic method.
        /// </summary>
        public int GenericParameterCount { get; set; }

        /// <summary>
        /// Descriptor of return parameter.
        /// </summary>
        public TypeSignatureDescriptor ReturnParameter { get; set; }

        /// <summary>
        /// Descriptors of all parameters.
        /// </summary>
        public TypeSignatureDescriptor[] Parameters { get; set; }
    }

    internal static class SignatureUtil
    {
        internal static CorElementType ExtractElementType(byte[] sig, ref int index)
        {
            int data = ExtractInt(sig, ref index);
            return (CorElementType)data;
        }

        internal static UnmanagedType ExtractUnmanagedType(byte[] sig, ref int index)
        {
            int data = ExtractInt(sig, ref index);
            return (UnmanagedType)data;
        }

        internal static CorCallingConvention ExtractCallingConvention(byte[] sig, ref int index)
        {
            int data = ExtractInt(sig, ref index);
            return (CorCallingConvention)data;
        }

        /// <summary>
        /// Extract optional and required custom modifiers from the signature blob.
        /// An optional modifier is a type following CorElementType.CModOpt.
        /// An required modifier is a type following CorElementType.CModReqd.
        /// </summary>
        /// <returns>
        /// return null if there is no custom modifiers.
        /// </returns>
        internal static CustomModifiers ExtractCustomModifiers(
            byte[] sig,
            ref int index,
            MetadataOnlyModule resolver,
            GenericContext context)
        {
            //extract required or optional modifiers from the sig blob
            CorElementType corType;
            int oldIdx = index;
            corType = SignatureUtil.ExtractElementType(sig, ref index);

            List<Type> optionalModifiers = null;
            List<Type> requiredModifiers = null;
            if (corType == CorElementType.CModOpt || corType == CorElementType.CModReqd)
            {
                optionalModifiers = new List<Type>();
                requiredModifiers = new List<Type>();
            }
            else
            {
                //revert the index to the last token
                index = oldIdx;
                return null;
            }

            while (corType == CorElementType.CModOpt || corType == CorElementType.CModReqd)
            {
                Token token = ExtractToken(sig, ref index);
                Type modifierType = resolver.ResolveTypeTokenInternal(token, context);

                if (corType == CorElementType.CModOpt)
                {
                    optionalModifiers.Add(modifierType);
                }
                else
                {
                    requiredModifiers.Add(modifierType);
                }
                oldIdx = index;
                corType = SignatureUtil.ExtractElementType(sig, ref index);
            }

            //revert the index to the last token
            index = oldIdx;
            return new CustomModifiers(optionalModifiers, requiredModifiers);
        }

        
        internal static Type ExtractType(
            byte[] sig,
            ref int index,
            MetadataOnlyModule resolver,
            GenericContext context)
        {
            TypeSignatureDescriptor descriptor = ExtractType(sig, ref index, resolver, context, false);
            return descriptor.Type;
        }

        /// <summary>
        /// Extracts type from method signature + any additional information encoded.
        /// </summary>
        /// <param name="sig">Signature blob.</param>
        /// <param name="index">Current index into signature blob; updated as type is extracted</param>
        /// <param name="resolver">Module in which this signature is valid.</param>
        /// <param name="context">Generic context that should be used when type variables need to be instantiated.</param>
        /// <param name="fAllowPinned">Determines if pinned types are allowed.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static TypeSignatureDescriptor ExtractType(
            byte[] sig,
            ref int index,
            MetadataOnlyModule resolver,
            GenericContext context,
            bool fAllowPinned)
        {
            TypeSignatureDescriptor result = new TypeSignatureDescriptor();
            result.IsPinned = false;

            CorElementType corType = ExtractElementType(sig, ref index);

            switch (corType)
            {
                case CorElementType.IntPtr:
                case CorElementType.SByte:
                case CorElementType.Short:
                case CorElementType.Int:
                case CorElementType.Long:
                case CorElementType.Float:
                case CorElementType.Double:
                case CorElementType.UIntPtr:
                case CorElementType.Byte:
                case CorElementType.UShort:
                case CorElementType.UInt:
                case CorElementType.ULong:
                case CorElementType.Void:
                case CorElementType.Bool:
                case CorElementType.Char:
                case CorElementType.String:
                case CorElementType.Object:
                    result.Type = resolver.AssemblyResolver.GetBuiltInType(corType);
                    break;

                case CorElementType.Array:
                    Type elementType = ExtractType(sig, ref index, resolver, context);
                    int rank = ExtractInt(sig, ref index);
                    //skip the remaining sizes of the array type since they are not needed.
                    int sizeNum = ExtractInt(sig, ref index);
                    Debug.Assert(sizeNum <= rank);
                    for (int i = 0; i < sizeNum; i++)
                    {
                        ExtractInt(sig, ref index);
                    }
                    //skip the lower bounds information.
                    int lowerBoundNum = ExtractInt(sig, ref index);
                    Debug.Assert(lowerBoundNum <= rank);
                    for (int i = 0; i < lowerBoundNum; i++)
                    {
                        ExtractInt(sig, ref index);
                    }
                    result.Type = elementType.MakeArrayType(rank);
                    break;

                case CorElementType.Byref:
                    result.Type = ExtractType(sig, ref index, resolver, context).MakeByRefType();
                    break;

                case CorElementType.Pointer:
                    result.Type = ExtractType(sig, ref index, resolver, context).MakePointerType();
                    break;

                case CorElementType.SzArray:
                    result.Type = ExtractType(sig, ref index, resolver, context).MakeArrayType();
                    break;

                case CorElementType.Class:
                case CorElementType.ValueType:
                    Token token = ExtractToken(sig, ref index);
                    result.Type =  resolver.ResolveTypeTokenInternal(token, corType, context);
                    Debug.Assert((corType == CorElementType.Class && (result.Type.IsInterface || result.Type.IsClass)) || (corType == CorElementType.ValueType && result.Type.IsValueType));
                    break;

                case CorElementType.CModOpt:
                    //In C++, types can have custom modifiers. The modifiers are not used in
                    //the LMR or reflection (there are no corresponding methods or properties to
                    //access them), just omit the custom modifiers for types here.
                    Token token1 = ExtractToken(sig, ref index);
                    resolver.ResolveTypeTokenInternal(token1, context);
                    result.Type = ExtractType(sig, ref index, resolver, context);
                    break;

                case CorElementType.CModReqd:
                    //In C++, types can have custom modifiers. The modifiers are not used in
                    //the LMR or reflection (there are no corresponding methods or properties to
                    //access them), just omit the custom modifiers for types here.
                    Token token2 = ExtractToken(sig, ref index);
                    resolver.ResolveTypeTokenInternal(token2, context);
                    result.Type = ExtractType(sig, ref index, resolver, context);
                    break;

                case CorElementType.GenericInstantiation:
                    int oldIdx = index;    
                    
                    // #1: Get raw type (open type). 
                    // Note: The raw type is always non-instantiated, so pass in a null context.
                    Type rawType = ExtractType(sig, ref index, resolver, null);
                    Debug.Assert(oldIdx != index);

                    // #2: Get number of generic arguments.
                    int typeArgCount = ExtractInt(sig, ref index);
                    Type[] typeArgs = new Type[typeArgCount];

                    // #3: Get all generic arguments.
                    for (int i = 0; i < typeArgs.Length; i++)
                    {
                        // The type args can be or contain generic parameters, so propagate our context through.
                        typeArgs[i] = ExtractType(sig, ref index, resolver, context);
                    }

                    // #4: Construct generic instantiation.
                    result.Type = rawType.MakeGenericType(typeArgs);
                    break;

                case CorElementType.MethodVar:
                    int mvarIndex = ExtractInt(sig, ref index);
                    if (GenericContext.IsNullOrEmptyMethodArgs(context))
                    {
                        throw new ArgumentException(Resources.TypeArgumentCannotBeResolved);
                    }
                    Debug.Assert(mvarIndex < context.MethodArgs.Length);

                    result.Type = context.MethodArgs[mvarIndex];
                    break;

                case CorElementType.TypeVar:
                    int varIndex = ExtractInt(sig, ref index);
                    if (GenericContext.IsNullOrEmptyTypeArgs(context))
                    {
                        throw new ArgumentException(Resources.TypeArgumentCannotBeResolved);
                    }
                    Debug.Assert(varIndex < context.TypeArgs.Length);

                    result.Type = context.TypeArgs[varIndex];
                    break;

                case CorElementType.FnPtr:
                    SignatureUtil.ExtractCallingConvention(sig, ref index);
                    int fnPtrParamCount = SignatureUtil.ExtractInt(sig, ref index);
                    //skip the function pointer return type
                    SignatureUtil.ExtractType(sig, ref index, resolver, context);
                    //skip the function pointer parameter types
                    for (int j = 0; j < fnPtrParamCount; j++)
                    {
                        SignatureUtil.ExtractType(sig, ref index, resolver, context);
                    }
                    //Reflection returns IntPtr for function pointer types
                    result.Type = resolver.AssemblyResolver.GetBuiltInType(CorElementType.IntPtr);
                    break;

                case CorElementType.TypedByRef:
                    result.Type = resolver.AssemblyResolver.GetTypeXFromName("System.TypedReference");
                    break;

                case CorElementType.Pinned:
                    Debug.Assert(fAllowPinned, "Unexpected pinned type");
                    result.IsPinned = true;
                    result.Type = ExtractType(sig, ref index, resolver, context);
                    break;

                case CorElementType.End:
                case CorElementType.Internal:
                case CorElementType.Max:
                case CorElementType.Modifier:
                case CorElementType.Sentinel:
                default:
                    Debug.Assert(false, "Bad element type");
                    throw new ArgumentException(Resources.IncorrectElementTypeValue);
            }

            return result;
        }

        /// <summary>
        /// Constructs type of a custom attribute argument (typed or named) based on its
        /// type ID extracted from the custom attribute blob. 
        /// </summary>
        internal static void ExtractCustomAttributeArgumentType(
            ITypeUniverse universe,
            Module module,
            byte[] customAttributeBlob,
            ref int index,
            out CorElementType argumentTypeId,
            out Type argumentType)
        {
            argumentTypeId = SignatureUtil.ExtractElementType(customAttributeBlob, ref index);
            SignatureUtil.VerifyElementType(argumentTypeId);

            // Construct actual primitive type to return. For non-arrays, just get type based on its
            // type ID. For arrays, determine if they are strongly typed or not (i.e. do we have something
            // like int32[] or object[].

            if (argumentTypeId == CorElementType.SzArray)
            {
                // Array element's type ID is stored immediately after argument type ID.
                CorElementType arrayElementTypeId = SignatureUtil.ExtractElementType(customAttributeBlob, ref index);
                SignatureUtil.VerifyElementType(arrayElementTypeId);

                if (arrayElementTypeId == BoxedValue)
                {
                    // Construct object[].
                    argumentType = universe.GetBuiltInType(CorElementType.Object).MakeArrayType();
                }
                else if (arrayElementTypeId == CorElementType.Enum)
                {
                    // construct enum[]
                    argumentType = SignatureUtil.ExtractTypeValue(universe, module, customAttributeBlob, ref index);
                    argumentType = argumentType.MakeArrayType();
                }
                else
                {
                    // Construct strongly typed array like int[].
                    argumentType = universe.GetBuiltInType(arrayElementTypeId).MakeArrayType();
                }
            }
            else if (argumentTypeId == CorElementType.Enum)
            {
                argumentType = SignatureUtil.ExtractTypeValue(universe, module, customAttributeBlob, ref index);
                if (argumentType == null)
                {
                    throw new ArgumentException(Resources.InvalidCustomAttributeFormatForEnum);
                }
            }
            else
            {
                if (argumentTypeId == BoxedValue)
                {
                    // We can't determine actual type for a "boxed" value since real type ID (FieldOrPropType)
                    // doesn't follow BoxedValue marker (0x51) for named arguments. 
                    argumentType = null;
                }
                else
                {
                    // For non-arrays, just construct a primitive type instance based on its ID. 
                    argumentType = universe.GetBuiltInType(argumentTypeId);
                }
            }
        }

        internal static bool IsVarArg(CorCallingConvention conv)
        {
            CorCallingConvention c = (conv & CorCallingConvention.Mask);
            return (c == CorCallingConvention.VarArg);
        }

        /// <summary>
        /// Decodes integer on given index position in the signature blob
        /// and advances to the next position. 
        /// <param name="sig">Metadata signature blob.</param>
        /// <param name="index">Start index in the blob.</param>
        /// <returns>Decoded integer value.</returns>
        internal static int ExtractInt(byte[] sig, ref int index)
        {
            //See CorSigUncompress data, cor.h, line 2284, for reference.

            int result;

            //Smallest
            if ((sig[index] & 0x80) == 0x00)
            {
                result = sig[index];
                index += 1;
            }
            //Medium
            else if ((sig[index] & 0xC0) == 0x80)
            {
                result = ((sig[index] & 0x3F) << 8) | sig[index + 1];
                index += 2;
            }
            else if ((sig[index] & 0xE0) == 0xC0)
            {
                result = ((sig[index] & 0x1F) << 24) | (sig[index + 1] << 16) | (sig[index + 2] << 8) | (sig[index + 3]);
                index += 4;
            }
            else
            {
                //We don't recognize this encoding
                throw new ArgumentException(Resources.InvalidMetadataSignature);
            }

            return result;
        }


        internal static Token ExtractToken(byte[] sig, ref int index)
        {
            //See CorSigUncompressToken, cor.h, line 2381, for reference.
            uint data = (uint)ExtractInt(sig, ref index);
            uint tkType = s_tkCorEncodeToken[data & 0x03];
            uint tk = TokenFromRid(data >> 2, tkType);
            return new Token(tk);
        }


        /// <summary>
        /// Maps Type to its CorElementType value for Types valid in custom attribute signatures.
        /// This throws if the type is not a primitive type definde in the system assembly, an enum, or array.
        /// </summary>
        internal static CorElementType GetTypeId(Type type)
        {
            if (type.IsEnum)
            {
                // Enums may be defined in user modules; but their underlying type must be 
                // one of the builtin in integral types.
                return GetTypeId(MetadataOnlyModule.GetUnderlyingType(type));
            }
            else if (type.IsArray)
            {
                // We can't just pass array type's full name into type map since it could 
                // be something like System.Int32[]. For any of array sub-types we can 
                // just return CorElementType.ELEMENT_TYPE_SZARRAY directly since that's
                // the only information we need.
                return CorElementType.SzArray;
            }

            CorElementType result;           

            bool found = TypeMapForAttributes.LookupPrimitive(type, out result);
            
            if (found)
            {
                return result;
            }
            else
            {
                // Contrast this behavior to Type.GetTypeCode() which treats all other objects as a 
                // System.Object. 
                throw new ArgumentException(Resources.UnsupportedTypeInAttributeSignature);
            }
        }

        /// <summary>
        /// Extracts a string value from the blob array.
        /// </summary>
        /// <remarks>This is just a convenience method to make dealing with strings easier.</remarks>
        internal static string ExtractStringValue(byte[] blob, ref int index)
        {
            return (string)ExtractValue(CorElementType.String, blob, ref index);
        }


        /// <summary>
        /// Extracts an unsigned int value from the blob array.
        /// </summary>
        /// <remarks>This is just a convenience method to make dealing with uints easier.</remarks>
        internal static uint ExtractUIntValue(byte[] blob, ref int index)
        {
            return (uint)ExtractValue(CorElementType.UInt, blob, ref index);
        }

        /// <summary>
        /// Extracts full name of a type from custom attribute blob and uses that name to 
        /// construct appropriate type object instance.
        /// </summary>
        /// <param name="universe">Universe that type resolution should happen in.</param>
        /// <param name="module">Module that contains this custom attribute blob.</param>
        /// <param name="blob">Custom attribute blob that's being parsed.</param>
        /// <param name="index">Current index inside the blob. Updated as type name is extracted.</param>
        /// <returns>Appropriate type instance or null if type name is null.</returns>
        internal static Type ExtractTypeValue(ITypeUniverse universe, Module module, byte[] blob, ref int index)
        {
            Type result = null;
            string assemblyQualifiedName = SignatureUtil.ExtractStringValue(blob, ref index);

            // In case of custom attribute blobs, type arguments can be encoded without full assembly name in 
            // two cases:
            // 1) type is defined in the same assembly as custom attribute using it as argument
            // 2) type is from system assembly i.e. mscorlib
            // See Serge Lidin's book, page 312.
            if(!String.IsNullOrEmpty(assemblyQualifiedName))
            {
                // TODO: this will call resolver if module is not loaded; we should just return TypeRef 
                // instead of resolving in those cases.
                bool throwOnError = true;
                bool useSystemAssemblyToResolveTypes = true;
                result = TypeNameParser.ParseTypeName(
                    universe,
                    module,
                    assemblyQualifiedName,
                    useSystemAssemblyToResolveTypes,
                    MetadataOnlyModule.IsWindowsRuntime(module),
                    throwOnError);
            }

            return result;
        }

        /// <summary>
        /// Extracts value at a given index from blob using passed type ID.
        /// </summary>
        /// <remarks>Implementation depends on BitConverter class. This is important from
        /// little/big-endian perspective since custom attribute signatures are encoded in
        /// little-endian format. This is ok on Windows but might not be ok on Mac for example.</remarks>
        /// <param name="typeId">Type ID of value that needs to be extracted.</param>
        /// <param name="blob">Byte array containing value.</param>
        /// <param name="index">Offset from which to start extracting value.</param>
        /// <returns>Value extracted from byte array.</returns>
        internal static object ExtractValue(CorElementType typeId, byte[] blob, ref int index)
        {
            object result;
            switch (typeId)
            {
                case CorElementType.Bool:
                    result = BitConverter.ToBoolean(blob, index);
                    index += 1;
                    return result;

                case CorElementType.Byte:
                    result = blob[index];
                    index += 1;
                    return result;

                case CorElementType.SByte:
                    result = (sbyte)blob[index];
                    index += 1;
                    return result;

                case CorElementType.Short:
                    result = BitConverter.ToInt16(blob, index);
                    index += 2;
                    return result;

                case CorElementType.UShort:
                    result = BitConverter.ToUInt16(blob, index);
                    index += 2;
                    return result;

                case CorElementType.Int:
                    result = BitConverter.ToInt32(blob, index);
                    index += 4;
                    return result;

                case CorElementType.UInt:
                    result = BitConverter.ToUInt32(blob, index);
                    index += 4;
                    return result;

                case CorElementType.Long:
                    result = BitConverter.ToInt64(blob, index);
                    index += 8;
                    return result;

                case CorElementType.ULong:
                    result = BitConverter.ToUInt64(blob, index);
                    index += 8;
                    return result;

                case CorElementType.Char:
                    result = BitConverter.ToChar(blob, index);
                    index += 2;
                    return result;

                case CorElementType.Float:
                    result = BitConverter.ToSingle(blob, index);
                    index += 4;
                    return result;

                case CorElementType.Double:
                    result = BitConverter.ToDouble(blob, index);
                    index += 8;
                    return result;

                case CorElementType.String:
                    // Check if encoded string is null first. Single byte with value 0xFF 
                    // means that string is null.
                    if (blob[index] == 0xFF)
                    {
                        index += 1;
                        result = null;
                    }
                    else
                    {
                        // Note that string length is encoded and in big-endian form.
                        int size = ExtractInt(blob, ref index);

                        // Strings are encoded in UTF8 format.
                        result = Encoding.UTF8.GetString(blob, index, size);
                        index += size;
                    }

                    return result;

                default:
                    Debug.Assert(false, "invalid value type");
                    throw new InvalidOperationException(Resources.IncorrectElementTypeValue);

            }
        }

        /// <summary>
        /// Creates a ready-only list of typed arguments populated by values extracted from byte array.
        /// </summary>
        /// <remarks>Reflection wraps each array element into CustomAttributeTypedArgument even when 
        /// custom attribute's constructor parameter is stronly typed array like int[]. That's why we don't 
        /// create int[] but list of wrapped integers.</remarks>
        /// <param name="elementType">Type of list elements.</param>
        /// <param name="universe">Universe in which type resolution should happen.</param>
        /// <param name="module">Module that contains custom attribute blob.</param>
        /// <param name="size">Size of list - previously extracted from the same custom attribute blob.</param>
        /// <param name="blob">Custom attribute blob.</param>
        /// <param name="index">Current index into custom attribute blob. Will be updated to new position
        /// as list elements are parsed.</param>
        /// <returns>Read only list of CustomAttributeTypedArguments that describe each element in the list.
        /// This matches what Reflection constructs and returns.</returns>
        internal static IList<CustomAttributeTypedArgument> ExtractListOfValues(
            Type elementType,
            ITypeUniverse universe,
            Module module,
            uint size,
            byte[] blob,
            ref int index)
        {
            CorElementType typeId = SignatureUtil.GetTypeId(elementType);
            List<CustomAttributeTypedArgument> result = new List<CustomAttributeTypedArgument>((int)size);

            // Check type of elements in an array. If they have static type of object then they
            // are "boxed" i.e. each element's true type is encoded before its value is encoded.
            // If array's elements are not object type (e.g. int[]), they have the same primitive 
            // type and only their values are encoded in the blob.

            if (typeId == CorElementType.Object)
            {
                for (int i = 0; i < size; i++)
                {
                    CorElementType arrayElementTypeId = SignatureUtil.ExtractElementType(blob, ref index);
                    SignatureUtil.VerifyElementType(arrayElementTypeId);

                    Type arrayElementType = null;
                    object value = null;
                    if (arrayElementTypeId == CorElementType.SzArray)
                    {
                        // TODO: implement arrays as elements of other arrays - this should be very rare
                        throw new NotImplementedException(Resources.ArrayInsideArrayInAttributeNotSupported);
                    }
                    else if (arrayElementTypeId == CorElementType.Enum)
                    {
                        arrayElementType = SignatureUtil.ExtractTypeValue(universe, module, blob, ref index);
                        if (arrayElementType != null)
                        {
                            Type enumUnderlyingType = MetadataOnlyModule.GetUnderlyingType(arrayElementType);
                            CorElementType enumUnderlyingTypeId = SignatureUtil.GetTypeId(enumUnderlyingType);
                            value = ExtractValue(enumUnderlyingTypeId, blob, ref index);
                        }
                        else
                        {
                            throw new ArgumentException(Resources.InvalidCustomAttributeFormatForEnum);
                        }
                    }
                    else
                    {
                        arrayElementType = universe.GetBuiltInType(arrayElementTypeId);
                        value = ExtractValue(arrayElementTypeId, blob, ref index);
                    }

                    result.Add(new CustomAttributeTypedArgument(arrayElementType, value));
                }
            }
            else if (typeId == CorElementType.Type)
            {
                for (int i = 0; i < size; i++)
                {
                    object value = SignatureUtil.ExtractTypeValue(universe, module, blob, ref index);
                    result.Add(new CustomAttributeTypedArgument(elementType, value));
                }
            }
            else if (typeId == CorElementType.SzArray)
            {
                // There seem to be no way to use jagged arrays as CA parameters in C#. 
                // E.g. C# does not allow this initialization: [Arrays(new int[][]{ new int[] {1,2}, new int[]{3}})] 
                // The ECMA spec is not clear on this. 
                // TODO: check if this is is still allowed in other languages.
                throw new ArgumentException(Resources.JaggedArrayInAttributeNotSupported);
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    object value = ExtractValue(typeId, blob, ref index);
                    result.Add(new CustomAttributeTypedArgument(elementType, value));
                }
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// Detemines if a given element type can be a custom attribute argument.
        /// </summary>
        internal static bool IsValidCustomAttributeElementType(CorElementType elementType)
        {
            return TypeMapForAttributes.IsValidCustomAttributeElementType(elementType);
        }

        /// <summary>
        /// Verifies that element type read from a custom attribute blob is
        /// one of allowed types. 
        /// </summary>
        /// <remarks>These values come straight from metadata so we have to 
        /// make sure they make sense before interpreting them.</remarks>
        internal static void VerifyElementType(CorElementType elementType)
        {
            if ((elementType == CorElementType.Bool) ||
                (elementType == CorElementType.Char) ||
                (elementType == CorElementType.SByte) ||
                (elementType == CorElementType.Byte) ||
                (elementType == CorElementType.Short) ||
                (elementType == CorElementType.UShort) ||
                (elementType == CorElementType.Int) ||
                (elementType == CorElementType.UInt) ||
                (elementType == CorElementType.Long) ||
                (elementType == CorElementType.ULong) ||
                (elementType == CorElementType.Float) ||
                (elementType == CorElementType.Double) ||
                (elementType == CorElementType.String) ||
                (elementType == CorElementType.Type) ||
                (elementType == CorElementType.SzArray) ||
                (elementType == CorElementType.Enum) ||
                (elementType == BoxedValue))  // This is special value for boxed elements.
            {
                return;
            }

            throw new ArgumentException(Resources.InvalidElementTypeInAttribute);
        }

        /// <summary>
        /// Determines if named argument represents field or property.
        /// </summary>
        internal static NamedArgumentType ExtractNamedArgumentType(byte[] customAttributeBlob, ref int index)
        {
            // Name argument type is stored as a single byte. 
            byte value = (byte)SignatureUtil.ExtractValue(CorElementType.Byte, customAttributeBlob, ref index);

            if (value == PropertyId)
            {
                return NamedArgumentType.Property;
            }
            else if (value == FieldId)
            {
                return NamedArgumentType.Field;
            }
            else
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "{0} {1}", Resources.InvalidCustomAttributeFormat, Resources.ExpectedPropertyOrFieldId));
            }
        }

        /// <summary>
        /// Extracts information about a method from its signature blob.
        /// </summary>
        /// <param name="methodSignatureBlob">Encoded method signature.</param>
        /// <param name="resolver">Module in which signature blob is valid.</param>
        /// <param name="context">Generic context that should be used when type variables need to be instantiated.</param>
        /// <returns>Method signature description.</returns>
        internal static MethodSignatureDescriptor ExtractMethodSignature(
            SignatureBlob methodSignatureBlob,
            MetadataOnlyModule resolver,
            GenericContext context)
        {
            Debug.Assert(context != null, "Generic context can't be null.");

            byte[] signatureBlob = methodSignatureBlob.GetSignatureAsByteArray();
            int index = 0;

            MethodSignatureDescriptor result = new MethodSignatureDescriptor();
            result.ReturnParameter = new TypeSignatureDescriptor();
            result.GenericParameterCount = 0;

            // First, get calling convention.
            result.CallingConvention = SignatureUtil.ExtractCallingConvention(signatureBlob, ref index);
            Debug.Assert(result.CallingConvention != CorCallingConvention.LocalSig);
            Debug.Assert(result.CallingConvention != CorCallingConvention.Field);

            bool fExplicitThis = (result.CallingConvention & CorCallingConvention.ExplicitThis) != 0;
            bool fGeneric = (result.CallingConvention & CorCallingConvention.Generic) != 0;

            // Generic method signatures (like void Foo<T>(T t, ...)) contain number of generic arguments. 
            // We have to have same number of them in the context. 
            if (fGeneric)
            {
                int numberOfGenericParameters = SignatureUtil.ExtractInt(signatureBlob, ref index);

                if (numberOfGenericParameters <= 0)
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture, "{0} {1}", Resources.InvalidMetadataSignature,
                        Resources.ExpectedPositiveNumberOfGenericParameters));
                }

                context = context.VerifyAndUpdateMethodArguments(numberOfGenericParameters);
                result.GenericParameterCount = numberOfGenericParameters;
            }

            // Get number of parameters in the signature
            int paramCount = SignatureUtil.ExtractInt(signatureBlob, ref index);

            bool allowPinned = false; 

            // Extract return type.
            // The return type might have custom modifiers.
            // Save the custom modifiers to create MethodInfo.ReturnParameter.

            // TODO: can we incorporate CM extraction into type extraction?
            CustomModifiers modifiers = SignatureUtil.ExtractCustomModifiers(signatureBlob, ref index, resolver, context);
            result.ReturnParameter = SignatureUtil.ExtractType(signatureBlob, ref index, resolver, context, allowPinned);
            result.ReturnParameter.CustomModifiers = modifiers;
            Debug.Assert(result.ReturnParameter.Type != null);

            // If we have an explicit 'this', get past the 'this' paramter.
            if (fExplicitThis)
            {
                SignatureUtil.ExtractType(signatureBlob, ref index, resolver, context);
                paramCount--;
            }

            // Get method's parameters and custom modifiers (if any), exluding 'this' parameter.
            result.Parameters = new TypeSignatureDescriptor[paramCount];

            for (int i = 0; i < paramCount; i++)
            {
                modifiers = SignatureUtil.ExtractCustomModifiers(signatureBlob, ref index, resolver, context);
                result.Parameters[i] = SignatureUtil.ExtractType(signatureBlob, ref index, resolver, context, allowPinned);
                result.Parameters[i].CustomModifiers = modifiers; 
            }

            if (index != signatureBlob.Length)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, 
                    "{0} {1}", Resources.InvalidMetadataSignature, Resources.ExtraInformationAfterLastParameter));
            }

            return result;
        }

        /// <summary>
        /// Gets Reflection calling convention coresponding to passed CorCallingConvention.
        /// </summary>
        internal static CallingConventions GetReflectionCallingConvention(CorCallingConvention callConvention)
        {
            CallingConventions result = (CallingConventions)0;
            if ((callConvention & CorCallingConvention.Mask) == CorCallingConvention.HasThis)
            {
                result = result | CallingConventions.HasThis;
            }
            else if ((callConvention & CorCallingConvention.Mask) == CorCallingConvention.ExplicitThis)
            {
                result = result | CallingConventions.ExplicitThis;
            }

            if (SignatureUtil.IsVarArg(callConvention))
            {
                result = result | CallingConventions.VarArgs;
            }
            else
            {
                result = result | CallingConventions.Standard;
            }

            return result;
        }

        /// <summary>
        /// Determines if method's calling convention matches passed calling convention. 
        /// </summary>
        internal static bool IsCallingConventionMatch(MethodBase method, CallingConventions callConvention)
        {
            if ((callConvention & CallingConventions.Any) == 0)
            {
                if ((callConvention & CallingConventions.VarArgs) != 0 &&
                    (method.CallingConvention & CallingConventions.VarArgs) == 0)
                    return false;

                if ((callConvention & CallingConventions.Standard) != 0 &&
                    (method.CallingConvention & CallingConventions.Standard) == 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if method has expected number of generic parameters.
        /// </summary>
        internal static bool IsGenericParametersCountMatch(MethodInfo method, int expectedGenericParameterCount)
        {
            int genericParameterCount = 0;
            if (method.IsGenericMethod)
            {
                genericParameterCount = method.GetGenericArguments().Length; 
            }

            return (genericParameterCount == expectedGenericParameterCount);
        }

        /// <summary>
        /// Determines if method's parameter types match the passed type array.
        /// The types need to be exactly the same.
        /// </summary>
        internal static bool IsParametersTypeMatch(MethodBase method, Type[] parameterTypes)
        {
            if (parameterTypes == null)
            {
                return true;
            }

            ParameterInfo[] methodParameters = method.GetParameters();
            if (methodParameters.Length != parameterTypes.Length)
            {
                return false;
            }

            int numParams = methodParameters.Length;
            for (int i = 0; i < numParams; i++)
            {
                if (!methodParameters[i].ParameterType.Equals(parameterTypes[i]))
                    return false;
            }

            return true;
        }

        private static uint TokenFromRid(uint rid, uint tkType)
        {
            //See corhdr.h, line 1410, for reference.
            return rid | tkType;
        }


        /// <summary>
        /// Determines which string comparison should be used based on
        /// binding flags passed in. It could be either case sensitive or
        /// case insensitive.
        /// </summary>
        internal static StringComparison GetStringComparison(BindingFlags flags)
        {
            StringComparison comparison;
            if ((flags & BindingFlags.IgnoreCase) != 0)
            {
                comparison = StringComparison.OrdinalIgnoreCase;
            }
            else
            {
                comparison = StringComparison.Ordinal;
            }

            return comparison;
        }

        //See cor.h, line 2350, for reference.
        private static readonly uint[] s_tkCorEncodeToken = { 
                                                      (uint)TokenType.TypeDef, 
                                                      (uint)TokenType.TypeRef, 
                                                      (uint)TokenType.TypeSpec, 
                                                      (uint)TokenType.BaseType };

        // hide the map in a nested class so that we can do validation on it.
        public static class TypeMapForAttributes
        {
            // Types allowed in custom attribute blobs.
            // This is not a complete mapping of all CorElementTypes since some of those aren't allowed
            //  in attribute blobs.
            // Using type names (instead of metadata tokens) lets this be static data shared across all universe implementations.
            // Caller is responsible to ensure that tht type is in the system module. Our wrapper methods validate this.
            private static readonly Dictionary<string, CorElementType> s_typeNameMapForAttributes = new Dictionary<string, CorElementType>() { 
                { "System.Boolean", CorElementType.Bool },
                { "System.Char",    CorElementType.Char },
                { "System.SByte",   CorElementType.SByte },
                { "System.Byte",    CorElementType.Byte },
                { "System.Int16",   CorElementType.Short },
                { "System.UInt16",  CorElementType.UShort },
                { "System.Int32",   CorElementType.Int },
                { "System.UInt32",  CorElementType.UInt },
                { "System.Int64",   CorElementType.Long },
                { "System.UInt64",  CorElementType.ULong },
                { "System.Single",  CorElementType.Float },
                { "System.Double",  CorElementType.Double },
                { "System.IntPtr",  CorElementType.IntPtr  },
                { "System.UIntPtr", CorElementType.UIntPtr  },
                { "System.Array",   CorElementType.SzArray },
                { "System.String",  CorElementType.String },
                { "System.Type",    CorElementType.Type },
                { "System.Object",  CorElementType.Object }
            };

            /// <summary>
            /// Detemines if a given element type can be a custom attribute argument.
            /// </summary>
            public static bool IsValidCustomAttributeElementType(CorElementType elementType)
            {
                return s_typeNameMapForAttributes.ContainsValue(elementType);
            }

            /// <summary>
            /// lookup the given primitive type
            /// </summary>
            /// <param name="type">type to lookup. Must be a primitive</param>
            /// <param name="result">the corelement type for the name</param>
            /// <returns>true if result is valid</returns>
            public static bool LookupPrimitive(Type type, out CorElementType result)
            {
                result = CorElementType.End;
                // Must validate that the type is from mscorlib before doing a name-based lookup.
                // You can define your own "System.Int32" class in your own module. 
                // Given that this returns CorElementTypes; callers already expect the type to be in the 
                // the system assembly. We need the universe to validate that.            
                ITypeUniverse universe = Helpers.Universe(type);
                if (universe != null)
                {
                    bool fIsSystem = universe.GetSystemAssembly().Equals(type.Assembly);
                    if (!fIsSystem)
                    {
                        return false;
                    }
                }

                // TODO: we might need to check if type is typeof(System.Type) or derives from Type once 
                // we integrate with CLR instead of comparing its full name with "System.Type". 
                return s_typeNameMapForAttributes.TryGetValue(type.FullName, out result);                
            }
        } // end class

        // The values below are defined in the CLI spec, 
        // Partition II, section 23.3, page 190.

        // Custom attribute named argument identifiers.
        private const byte FieldId = 0x53;
        private const byte PropertyId = 0x54;

        // Value indicating that custom attribute blob contains "boxed" value.
        private const CorElementType BoxedValue = (CorElementType)0x51;
    }

    /// <summary>
    /// Represent a raw signature blob from the Metadata.
    /// Run this through a signature parser to get a friendly form.
    /// </summary>
    internal class SignatureBlob
    {
        /// <summary>
        /// create a signature blob around a set of bytes.
        /// </summary>
        /// <param name="data">raw bytes in the signature</param>
        /// <remarks>This just provides the semantics that the given byte[] is a metadata signature.</remarks>
        SignatureBlob(byte[] data)
        {
            m_signature = data;
        }

        /// <summary>
        /// Read a signature blob from metadata storage.
        /// </summary>
        /// <param name="storage">the raw metadata storage that provides the backing unmanaged memory</param>
        /// <param name="pointer">a pointer into the raw metadata storage</param>
        /// <param name="countBytes">number of bytes to read</param>
        /// <returns>signature copied from the storage</returns>
        /// <remarks>This is conceptually just a ReadMemory() at the pointer's address. We pass in the underlying 
        /// storage object so that we can validate that the unmanaged memory address is actually valid.</remarks>
        public static SignatureBlob ReadSignature(MetadataFile storage, EmbeddedBlobPointer pointer, int countBytes)
        {
            return new SignatureBlob(storage.ReadEmbeddedBlob(pointer, countBytes));
        }
        

        /// <summary>
        /// Get the signature as a byte-array. Caller should not mutate the bytes; although we don't enforce
        /// this for efficiency.
        /// </summary>
        /// <returns>A byte array of the signature.</returns>
        public byte[] GetSignatureAsByteArray()
        {
            return m_signature;
        }

        // The bytes in the signature blob could be mutated.
        private readonly byte[] m_signature;
    }
}
