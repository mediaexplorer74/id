using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Adds;
using System.Text;
using Debug=Microsoft.MetadataReader.Internal.Debug;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using Type = System.Reflection.Mock.Type;
using AssemblyName = System.Reflection.AssemblyName;
using TypeAttributes = System.Reflection.TypeAttributes;
using System.Diagnostics;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// Utility class that parses metadata for information about pseudo-custom attributes.
    /// </summary>
    internal static class PseudoCustomAttributes
    {
        public const string TypeForwardedToAttributeName = "System.Runtime.CompilerServices.TypeForwardedToAttribute";
        public const string SerializableAttributeName = "System.SerializableAttribute";

                
        /// <summary>
        /// Get the TypeForwardedToAttributes on an assembly.
        /// </summary>
        /// <param name="assembly">assembly to look for attributes on.</param>
        /// <returns>List of CustomAttributeData instances describing TypeForwardedToAttributes if present.
        /// Empty list if TypeForwardedToAttribute is not present.</returns>
        /// <remarks>
        /// TypeForwardedTo attributes only occur on the assembly. So this overload is useful when we're explicitly operating with the assembly and
        /// looking for these attributes.
        /// </remarks>
        public static IEnumerable<CustomAttributeData> GetTypeForwardedToAttributes(MetadataOnlyAssembly assembly)
        {
            return GetTypeForwardedToAttributes(assembly.ManifestModuleInternal);
        }

        /// <summary>
        /// Finds info about TypeForwardedToAttributes if present.
        /// </summary>
        /// <param name="module">Module in which a given token is valid.</param>
        /// <param name="token">Token representing object that's target of attributes. Must be a mtAssembly</param>
        /// <returns>List of CustomAttributeData instances describing TypeForwardedToAttributes if present.
        /// Empty list if TypeForwardedToAttribute is not present.</returns>
        /// <remarks>This calls the Type Name Parser which will force assembly resolution because it will force loading the forwarded type to store it as a System.Type arg 
        /// in the custom attribute data</remarks>
        public static IEnumerable<CustomAttributeData> GetTypeForwardedToAttributes(MetadataOnlyModule manifestModule, Token token)
        {
            // TypeForwardedToAttribute's only target is Assembly object.
            if (token.TokenType != TokenType.Assembly)
            {
                return new CustomAttributeData[0]; // empty
            }
            return GetTypeForwardedToAttributes(manifestModule);
        }

        public static IEnumerable<CustomAttributeData> GetTypeForwardedToAttributes(MetadataOnlyModule manifestModule)
        {
            ITypeUniverse itu = manifestModule.AssemblyResolver;

            // Get reflection objects for TypeForwardToAttribute. Cache this outside the loop.
            // Since these refer to types defined in mscorlib, these will be typerefs for any other module.
            // The only constructor argument is of type System.Type. 
            Type argumentType = itu.GetBuiltInType(CorElementType.Type);

            // Get constructor for TypeForwardedToAttribute.
            Assembly systemAssembly = itu.GetSystemAssembly();
            Type attributeType = systemAssembly.GetType(TypeForwardedToAttributeName, false, false);

            // If system assembly doesn't have TypeForwardedTo defined than there is no support
            // for type forwarding.
            // TODO: maybe we should check if we get any RawTypeForwardedToAttributes and raise
            // exception since this could be sign of larger problem?
            if (attributeType == null) yield break;

            // Get the raw TypeForwardedTo data from the metadata.
            IEnumerable<UnresolvedTypeName> raw = GetRawTypeForwardedToAttributes(manifestModule);

            foreach (UnresolvedTypeName utn in raw)
            {
                ConstructorInfo[] constructors = attributeType.GetConstructors();
                Debug.Assert(constructors.Length == 1, "TypeForwardedToAttribute should have only one constructor.");


                Type argumentValue = utn.ConvertToType(itu, manifestModule);

                // package the raw TypeForwardTo data as a custom attribute to follow the reflection API protocols.
                CustomAttributeTypedArgument forwardedTypeInfo = new CustomAttributeTypedArgument(argumentType, argumentValue);

                List<CustomAttributeTypedArgument> typedArguments = new List<CustomAttributeTypedArgument>(1);
                typedArguments.Add(forwardedTypeInfo);

                List<CustomAttributeNamedArgument> namedArguments = new List<CustomAttributeNamedArgument>(0);

                MetadataOnlyCustomAttributeData attribute = new MetadataOnlyCustomAttributeData(
                    constructors[0],
                    typedArguments,
                    namedArguments);

                yield return attribute;
            }
        }

        /// <summary>
        /// Convenience overload for assemblies.
        /// </summary>        
        internal static IEnumerable<UnresolvedTypeName> GetRawTypeForwardedToAttributes(MetadataOnlyAssembly assembly)
        {
            return GetRawTypeForwardedToAttributes(assembly.ManifestModuleInternal);
        }

        internal static bool GetNextExportedType(
            ref HCORENUM hEnum, 
            IMetadataAssemblyImport assemblyImport,
            StringBuilder typeName,
            out Token implementationToken)
        {
            uint size;
            int exportedTypeTokenValue;

            implementationToken = Token.Nil;

            // TypeForwardedToAttribute is stored as a field in the ExportedType metadata table.
            // Implementation column of this table that contains AssemblyRef token means we have
            // type forwarder. This is not part of the CLI spec (yet).

            assemblyImport.EnumExportedTypes(ref hEnum, out exportedTypeTokenValue, 1, out size);
            Debug.Assert((size == 0) || (size == 1), "Unexpected size");
            if (size == 0)
            {
                return false; // done; no more exported types
            }

            int nameLength;
            int implementationTokenValue;
            int typeDefTokenValue;
            CorTypeAttr flags;

            assemblyImport.GetExportedTypeProps(
                exportedTypeTokenValue,
                null,
                0,
                out nameLength,
                out implementationTokenValue,
                out typeDefTokenValue,
                out flags);

            implementationToken = new Token(implementationTokenValue);

            // We are only interested in exported types that are type forwarders. We don't
            // care about names of other exported types.
            if (implementationToken.TokenType == TokenType.AssemblyRef)
            {
                Debug.Assert(nameLength > 0, "Unexpected type name length");
                Debug.Assert(flags == CorTypeAttr.tdForwarder, "Unexpected flag for type forwarder.");

                // Important: Length needs to be set before Capacity is changed. This is new "feature" in CLR v4. If done
                // other way around, setting Length could change Capacity, which would affect all interop calls that use
                // this buffer (since Capacity is usually passed as one of the arguments). Dev10 #814198 opened to track this.
                typeName.Length = 0;
                typeName.EnsureCapacity(nameLength);

                assemblyImport.GetExportedTypeProps(
                    exportedTypeTokenValue,
                    typeName,
                    typeName.Capacity,
                    out nameLength,
                    out implementationTokenValue,
                    out typeDefTokenValue,
                    out flags);
            }

            return true;
        }

        /// <summary>
        /// Enumerate type forwarders. This provides the raw metadata and explicitly avoids doing any resolution.
        /// Returns UnresolvedTypeNames instead of Type to avoid doing an eager resolution.
        /// </summary>
        /// <param name="manifestModule">the manifest module to search. TypeForward data is only on a manifest module</param>
        /// <returns>enumerate of TypeForward data in the module.</returns>
        internal static IEnumerable<UnresolvedTypeName> GetRawTypeForwardedToAttributes(MetadataOnlyModule manifestModule)
        {
            HCORENUM hEnum = new HCORENUM();
            IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)manifestModule.RawImport;

            try
            {
                StringBuilder typeName = StringBuilderPool.Get();
                Token implementationToken;
                while (GetNextExportedType(ref hEnum, assemblyImport, typeName, out implementationToken))
                {
                    if (implementationToken.TokenType == TokenType.AssemblyRef)
                    {
                        AssemblyName assemblyName = AssemblyNameHelper.GetAssemblyNameFromRef(implementationToken, manifestModule, assemblyImport);
                        yield return new UnresolvedTypeName(typeName.ToString(), assemblyName);
                    }
                }

                StringBuilderPool.Release(ref typeName);
            }
            finally
            {
                hEnum.Close(assemblyImport);
            }
        }


        /// <summary>
        /// Convenience overload for assemblies.
        /// </summary>        
        internal static UnresolvedTypeName GetRawTypeForwardedToAttribute(MetadataOnlyAssembly assembly, string fullname, bool ignoreCase)
        {
            return GetRawTypeForwardedToAttribute(assembly.ManifestModuleInternal, fullname, ignoreCase);
        }

        /// <summary>
        /// Enumerate type forwarders and finds one that matches given full name. 
        /// Returns UnresolvedTypeNames instead of Type to avoid doing an eager resolution.
        /// </summary>
        /// <param name="manifestModule">The module to search in.</param>
        /// <param name="fullname">Type's full name.</param>
        /// <param name="ignoreCase">If true, the fullname comparison will be not be case sensitive.</param>
        /// <returns>An unresolved type retrieved from type forwarded attributes or null if type with given name 
        /// cannot be found.</returns>
        /// <remarks>
        /// Having this version of API saves us from converting StringBuilder content to string when we are only looking
        /// for one specific forwarder.
        /// </remarks>
        internal static UnresolvedTypeName GetRawTypeForwardedToAttribute(MetadataOnlyModule manifestModule, string fullname, bool ignoreCase)
        {
            HCORENUM hEnum = new HCORENUM();
            IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)manifestModule.RawImport;
            if (string.IsNullOrEmpty(fullname))
            {
                return null;
            }

            UnresolvedTypeName result = null;
            try
            {
                StringBuilder typeName = StringBuilderPool.Get();
                Token implementationToken;

                while (GetNextExportedType(ref hEnum, assemblyImport, typeName, out implementationToken))
                {
                    if (implementationToken.TokenType == TokenType.AssemblyRef)
                    {
                        // First check the typename to see if we even need to return this.
                        if (fullname.Length != typeName.Length)
                        {
                            continue;
                        }

                        // There is a chance they are same, we need to check.
                        string typeNameString = typeName.ToString();
                        if (!Utility.Compare(typeNameString, fullname, ignoreCase))
                        {
                            continue;
                        }

                        AssemblyName assemblyName = AssemblyNameHelper.GetAssemblyNameFromRef(implementationToken, manifestModule, assemblyImport);
                        result = new UnresolvedTypeName(typeNameString, assemblyName);
                        break;
                   }
                }

                StringBuilderPool.Release(ref typeName);
            }
            finally
            {
                hEnum.Close(assemblyImport);
            }

            return result;
        }


        /// <summary>
        /// Given the representation of a TypeForwarededToAttribute, get the Type parameter from it.
        /// </summary>
        /// <param name="data">a custom attribute representation for a TypeForwardedAttribute</param>
        /// <returns>the System.Type parameter stored in the attribute. This encapsulates where the type is forwarded to.</returns>
        /// <remarks>
        /// This is the inverse on GetTypeForwardedToAttributes.
        /// Given an attribute of: [assembly: TypeForwardedTo(typeof(Widget))]
        /// This returns Typeof(Widget).
        /// </remarks>
        public static Type GetTypeFromTypeForwardToAttribute(CustomAttributeData data)
        {
            Debug.Assert(data.Constructor.DeclaringType.FullName.Equals(TypeForwardedToAttributeName, StringComparison.Ordinal));
            CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
            Type result = (Type)argument.Value;
            return result; 
        }


        /// <summary>
        /// Finds info about SerializableAttribute if present on a type.
        /// </summary>
        /// <param name="module">Module in which a given token is valid.</param>
        /// <param name="token">Token representing object that's target of attribute.</param>
        /// <returns>CustomAttributeData instance describing SerializableAttribute if present.
        /// Null otherwise.</returns>
        public static CustomAttributeData GetSerializableAttribute(MetadataOnlyModule module, Token token)
        {
            // SerializableAttribute's only target is TypeDef object.
            if (token.TokenType != TokenType.TypeDef)
            {
                return null;
            }

            int size;
            TypeAttributes flags;
            int extends;

            // We just need flags. 
            module.RawImport.GetTypeDefProps(token.Value, null, 0, out size, out flags, out extends);

            // If flags do not contain Serializable, the attribute is not present.
            if ((flags & TypeAttributes.Serializable) == 0)
            {
                return null;
            }

            // Type is serializable, construct attribute data. 
            return GetSerializableAttribute(module, /*isRequired*/ true);
        }

        /// <summary>
        /// Creates attribute data for SerializableAttribute.
        /// </summary>
        /// <param name="module">Module to be used to get appropriate type universe and system assembly.</param>
        /// <param name="isRequired">If true, SerializableAttribute must be present in the system assembly (mscorlib).
        /// If false, returns null if SerializableAttribute cannot be found. E.g. Silverlight mscorlib does not
        /// contain this attribute.</param>
        internal static CustomAttributeData GetSerializableAttribute(MetadataOnlyModule module, bool isRequired)
        {
            // Get constructor for SerializableAttribute(). No args.
            Assembly systemAssembly = module.AssemblyResolver.GetSystemAssembly();
            Type attributeType = systemAssembly.GetType(SerializableAttributeName, isRequired, false);
            if (attributeType == null)
            {
                return null;
            }

            ConstructorInfo[] constructors = attributeType.GetConstructors();
            Debug.Assert(constructors.Length == 1, "SerializableAttribute should have only one constructor.");

            var typedArguments = new List<CustomAttributeTypedArgument>(0);
            var namedArguments = new List<CustomAttributeNamedArgument>(0);

            MetadataOnlyCustomAttributeData result = new MetadataOnlyCustomAttributeData(
                constructors[0], typedArguments, namedArguments);

            return result;
        }
    }
}
