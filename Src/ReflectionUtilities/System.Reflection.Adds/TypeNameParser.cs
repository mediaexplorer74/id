// Type Name parser.
// This is like a Type.GetType(string) that takes in an ITypeUniverse to work across Universes.
// This should share a Parser with the BCL.
// 
// See Type.GetType(string) for the grammer.
//    http://msdn.microsoft.com/en-us/library/w3f99sx1.aspx
// 
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace System.Reflection.Adds
{
    // Usings need to be declared within the namespace so that they take precedence over binding based off
    // the containing namespace name. 
    #if USE_CLR_V4
    using System.Reflection;  
    #else
    using System.Reflection.Mock;
    using BindingFlags = System.Reflection.BindingFlags;
    using AssemblyName = System.Reflection.AssemblyName;
    using Type = System.Reflection.Mock.Type;
    #endif

    internal static class TypeNameParser
    {
        private enum TokenType
        {
            Id,
            LeftBracket, // generics or arrays
            RightBracket,
            Comma, // used in assembly name, arrays
            Plus, // nested types
            Equals, // used in assembly name
            Reference, // & - used for ByRef
            Pointer, // * - used for pointer types
            EndOfInput
        }

        private class Token
        {
            private Token(TokenType tokenType, string value)
            {
                TokenType = tokenType;
                Value = value;
            }

            internal TokenType TokenType { get; private set; }
            internal string Value { get; private set; }

            internal static readonly Token Plus = new Token(TokenType.Plus, null);
            internal static readonly Token LeftBracket = new Token(TokenType.LeftBracket, null);
            internal static readonly Token RightBracket = new Token(TokenType.RightBracket, null);
            internal static readonly Token Comma = new Token(TokenType.Comma, null);
            internal static new readonly Token Equals = new Token(TokenType.Equals, null);

            internal static readonly Token Reference = new Token(TokenType.Reference, null);
            internal static readonly Token Pointer = new Token(TokenType.Pointer, null);

            internal static Token MakeIdToken(string value)
            {
                return new Token(TokenType.Id, value);
            }
        }

        /// <summary>
        /// Parses type name and returns type instance represented by type name.  
        /// </summary>
        /// <param name="universe">If non-null, the univese that the returned type is valid in. This 
        /// can be used to search for names.</param>
        /// <param name="module">If non-null, this is the module that input is valid in. 
        /// If null, then universe must be non-null and the type is search throughout the entire universe.</param>
        /// <param name="input">String containing type name.</param>
        /// <returns>Type that corresponds to given type name.</returns>
        /// <exception cref="TypeLoadException">Thrown when type can't be found in a given module/universe.</exception>
        /// <exception cref="ArgumentException">Thrown when input contains more characters than expected.</exception>
        public static Type ParseTypeName(ITypeUniverse universe, Module module, string input)
        {
            bool throwOnError = true;
            return ParseTypeName(universe, module, input, throwOnError);
        }

#if !USE_CLR_V4

        /// <summary>
        /// Parses type name and returns type instance represented by type name. Uses LMR's own 
        /// parsing implementation. Used on Orcas only. 
        /// </summary>
        /// <param name="universe">If non-null, the univese that the returned type is valid in. This 
        /// can be used to search for names.</param>
        /// <param name="module">If non-null, this is the module that input is valid in. 
        /// If null, then universe must be non-null and the type is search throughout the entire universe.</param>
        /// <param name="input">String containing type name.</param>
        /// <param name="throwOnError">Controls behavior for cases when type with a given name cannot be found.</param>
        /// <returns>Type that corresponds to given type name. Can return null if type cannot be found and 
        /// throwOnError flag is false.</returns>
        /// <exception cref="TypeLoadException">Thrown when type can't be found in a given module/universe and
        /// throwOnError flag is true.</exception>
        /// <exception cref="ArgumentException">Thrown when input contains more characters than expected.</exception>
        public static Type ParseTypeName(ITypeUniverse universe, Module module, string input, bool throwOnError)
        {
            Debug.Assert((universe != null) || (module != null));

            int idx = 0;

            Type result = ParseTypeName(universe, module, input, ref idx, throwOnError, false, false);

            // If we are requested to throw, check input for extra characters. 
            if (throwOnError && (idx != input.Length))
            {
                throw new ArgumentException(Resources.ExtraCharactersAfterTypeName);
            }

            return result;
        }

#else

        /// <summary>
        /// Parses type name and returns type instance represented by type name. Uses CLR's type name
        /// parser that's new in CLR v4.
        /// </summary>
        /// <param name="universe">If non-null, the univese that the returned type is valid in. This 
        /// can be used to search for names.</param>
        /// <param name="module">If non-null, this is the module that input is valid in. 
        /// If null, then universe must be non-null and the type is search throughout the entire universe.</param>
        /// <param name="input">String containing type name.</param>
        /// <param name="throwOnError">Controls behavior for cases when type with a given name cannot be found.</param>
        /// <returns>Type that corresponds to given type name. Can return null if type cannot be found and 
        /// throwOnError flag is false.</returns>
        /// <exception cref="TypeLoadException">Thrown when type can't be found in a given module/universe and
        /// throwOnError flag is true.</exception>
        /// <exception cref="ArgumentException">Thrown when input contains more characters than expected.</exception>
        public static Type ParseTypeName(ITypeUniverse universe, Module module, string input, bool throwOnError)
        {
            return ParseTypeName(universe, module, input, false, false, throwOnError);
        }

        /// <summary>
        /// Parses type name and returns type instance represented by type name. Uses CLR's type name
        /// parser that's new in CLR v4.
        /// </summary>
        /// <param name="universe">If non-null, the univese that the returned type is valid in. This 
        /// can be used to search for names.</param>
        /// <param name="module">If non-null, this is the module that input is valid in. 
        /// If null, then universe must be non-null and the type is search throughout the entire universe.</param>
        /// <param name="input">String containing type name.</param>
        /// <param name="useSystemAssemblyToResolveTypes">True if the system assembly(mscorlib) should be used to 
        /// resolve types when the type cannot be found in module</param>
        /// <param name="useWindowsRuntimeResolution">True if Windows Runtime type resolution should be used</param>
        /// <param name="throwOnError">Controls behavior for cases when type with a given name cannot be found.</param>
        /// <returns>Type that corresponds to given type name. Can return null if type cannot be found and 
        /// throwOnError flag is false.</returns>
        /// <exception cref="TypeLoadException">Thrown when type can't be found in a given module/universe and
        /// throwOnError flag is true.</exception>
        /// <exception cref="ArgumentException">Thrown when input contains more characters than expected.</exception>
        /// <returns></returns>
        public static Type ParseTypeName(
            ITypeUniverse universe,
            Module module,
            string input,
            bool useSystemAssemblyToResolveTypes,
            bool useWindowsRuntimeResolution,
            bool throwOnError)
        {
            Module systemModule = universe.GetSystemAssembly().ManifestModule;

            Debug.Assert((universe != null) || (module != null));

            Func<AssemblyName, Assembly> assemblyResolver = delegate(AssemblyName assemblyName)
            {
                Debug.Assert(assemblyName != null);
                return DetermineAssembly(assemblyName, module, universe);
            };

            Func<Assembly, string, bool, Type> typeResolver = delegate(Assembly assembly, string simpleTypeName, bool ignoreCase)
            {
                Debug.Assert(!TypeNameParser.IsCompoundType(simpleTypeName));

                bool throwOnErrorInCallback = false;
                Type result = null;

                if(assembly != null)
                {
                    // Try to get the type from the assembly returned from the assembly resolver
                    result = assembly.GetType(simpleTypeName, throwOnErrorInCallback, ignoreCase);
                }
                else
                {
                    // If type name doesn't contain assembly name
                    if(null == result) 
                    {
                        // Try to get the type from the module containing the typename
                        Debug.Assert(module != null);
                        result = module.GetType(simpleTypeName, throwOnErrorInCallback, ignoreCase);
                    }

                    if(null == result && useSystemAssemblyToResolveTypes) 
                    {
                        // Try to get the type from the system module(mscorlib)
                        Debug.Assert(systemModule != null);
                        result = systemModule.GetType(simpleTypeName, throwOnErrorInCallback, ignoreCase);
                    }

                    if(null == result && useWindowsRuntimeResolution) 
                    {
                        // Try to get the type using Windows Runtime resolution. 
                        result = universe.ResolveWindowsRuntimeType(simpleTypeName, throwOnErrorInCallback, ignoreCase);
                    }
                }

                return result;
            };

            return Type.GetType(input, assemblyResolver, typeResolver, throwOnError);
        }
#endif

        private static Type ParseTypeName(
            ITypeUniverse universe, 
            Module defaultTokenResolver, 
            string input, 
            ref int idx, 
            bool throwOnError,
            bool isGenericArgument,
            bool expectAssemblyName)
        {
            List<string> path = new List<string>();
            List<Type> genericTypeArgs = new List<Type>();            
            AssemblyName assemblyName = null;

            //Raw type name, plus outer types if nested.
            for (; ; )
            {
                // The parsed type name does not contain leading spaces.
                // This is CLR's behavior.
                path.Add(ReadIdWithoutLeadingSpaces(input, ref idx));
                if (PeekNextToken(input, idx) != TokenType.Plus)
                {
                    break;
                }
                else
                {
                    //Consume the plus and continue
                    ReadSpecialToken(input, TokenType.Plus, ref idx);
                    continue;
                }
            }

            // Check if we have any of these cases:
            // [T] [T1, T2] [[T1, AssemblyName], T2]  
            if (IsGenericType(input, idx))
            {
                //Eat the left brackets
                ReadSpecialToken(input, TokenType.LeftBracket, ref idx);

                for (; ; )
                {
                    // Check if we have case like: [[T1, AssemblyName], ...]
                    bool isTypeWithAssemblyName = false;
                    if (PeekNextToken(input, idx) == TokenType.LeftBracket)
                    {
                        isTypeWithAssemblyName = true;
                        ReadSpecialToken(input, TokenType.LeftBracket, ref idx);
                    }

                    //Get the generic type argument
                    Type genericArgument = ParseTypeName(universe, defaultTokenResolver, input, ref idx, throwOnError, true, isTypeWithAssemblyName);
                    if (genericArgument == null)
                    {
                        Debug.Assert(!throwOnError, "ParseTypeName should return null only when throwOnError is false.");
                        return null;
                    }
                    genericTypeArgs.Add(genericArgument);

                    // If type had assembly name it should have a right bracket at the end. 
                    if (isTypeWithAssemblyName)
                    {
                        ReadSpecialToken(input, TokenType.RightBracket, ref idx);
                    }

                    //Check if we have more generic arguments
                    if (PeekNextToken(input, idx) == TokenType.Comma)
                    {
                        ReadSpecialToken(input, TokenType.Comma, ref idx);
                    }
                    else
                    {
                        break;
                    }
                }

                //Eat the final right bracket to close the generic clause
                ReadSpecialToken(input, TokenType.RightBracket, ref idx);
            }

            // Format is:
            //  <Type/Nesting/Generics> <Modifiers> ',' <AssemblyName>
            // Look for assembly name

            int idxModifiers = idx;

            // We need to read past modifiers to get to the assembly name.
            ReadModifiers(null, input, ref idx);

            int idxCheck = idx;

            // Assembly specifier or generic argument separator
            if (!isGenericArgument || expectAssemblyName)
            {
                if (PeekNextToken(input, idx) == TokenType.Comma)
                {
                    ReadSpecialToken(input, TokenType.Comma, ref idx);
                    assemblyName = ParseAssemblyInfo(input, ref idx);
                }
            }

            // TODO: we really should be propagating the throwOnError flag into here so that if assembly resolution
            // fails, we don't necessarily throw.
            Assembly assembly = DetermineAssembly(assemblyName, defaultTokenResolver, universe);
            Type t = Resolve(path, genericTypeArgs, assembly);

            // If we can't resolve type in a given module and caller specified
            // throwOnError=false just return null so caller can try with different
            // module if needed.
            if (t == null)
            {
                if (throwOnError)
                {
                    throw new TypeLoadException(string.Format(
                        CultureInfo.InvariantCulture, Resources.CannotFindTypeInModule, input, assembly.ToString()));
                }
                else
                {
                    return null;
                }
            }

            t = ReadModifiers(t, input, ref idxModifiers);
            Debug.Assert(idxModifiers == idxCheck);

            return t;
        }

        /// <summary>
        /// Determines if input contains generic type specifier.
        /// </summary>
        /// <remarks>
        /// A left-bracket could either be a generic arg list or an array
        ///  []  [,]  [*]  are arrays
        ///  [T] [T1, T2] [[T1, AssemblyName], T2]  are generic arg lists            
        ///    Elements in the generic arg list are enclosed in a set of [] if it has an assembly
        ///    qualified name. (AQNs include commas; so this determine whether the comma is separating
        ///    generic args or separating the type from the AssemblyName)
        /// </remarks>
        /// <param name="input">String containing type name that's being parsed.</param>
        /// <param name="idx">Current index in input string.</param>
        /// <returns>True if type is generic type; otherwise false.</returns>
        private static bool IsGenericType(string input, int idx)
        {
            if ((PeekNextToken(input, idx) == TokenType.LeftBracket) && 
                !((PeekSecondToken(input, idx) == TokenType.RightBracket) ||
                  (PeekSecondToken(input, idx) == TokenType.Comma) ||
                  (PeekSecondToken(input, idx) == TokenType.Pointer)))
            {
                return true;
            }

            return false;
        }

        // Read modifiers (like &, [], *).
        // Update idx to be after modifiers
        // If type != null, apply them and return the new type.
        static Type ReadModifiers(Type type, string input, ref int idx)
        {
            // Loop through to catch multiple modifiers. 
            for (; ; )
            {
                // Array, single or multi dimensional
                if (PeekNextToken(input, idx) == TokenType.LeftBracket)
                {
                    if (PeekSecondToken(input, idx) == TokenType.RightBracket)
                    {
                        // Single dimension array specifier
                        ReadSpecialToken(input, TokenType.LeftBracket, ref idx);
                        ReadSpecialToken(input, TokenType.RightBracket, ref idx);

                        if (type != null)
                        {
                            type = type.MakeArrayType();
                        }
                        continue;
                    }
                    else if (PeekSecondToken(input, idx) == TokenType.Comma)
                    {
                        // Multi-dimension array specifier

                        ReadSpecialToken(input, TokenType.LeftBracket, ref idx);

                        int rank = 1;
                        while (PeekNextToken(input, idx) == TokenType.Comma)
                        {
                            ReadSpecialToken(input, TokenType.Comma, ref idx);
                            rank++;
                        }

                        if (PeekNextToken(input, idx) == TokenType.RightBracket)
                        {
                            ReadSpecialToken(input, TokenType.RightBracket, ref idx);
                            if (type != null)
                            {
                                type = type.MakeArrayType(rank);
                            }
                            continue;
                        }
                        else
                        {
                            throw new ArgumentException(Resources.UnexpectedCharacterFound);
                        }
                    }
                }

                // Reference
                if (PeekNextToken(input, idx) == TokenType.Reference)
                {
                    ReadSpecialToken(input, TokenType.Reference, ref idx);

                    if (type != null)
                    {
                        type = type.MakeByRefType();
                    }
                    continue;
                }

                // Pointer
                if (PeekNextToken(input, idx) == TokenType.Pointer)
                {
                    ReadSpecialToken(input, TokenType.Pointer, ref idx);

                    if (type != null)
                    {
                        type = type.MakePointerType();
                    }
                    continue;
                }


                break;
            }
            return type;
        }

        private static AssemblyName ParseAssemblyInfo(string input, ref int idx)
        {
            // The Assembly construct takes a string parser. We may be able to use that as the parser instead.
            AssemblyName result = new AssemblyName();
            // The assembly name does not contain leading or ending spaces.
            // It is CLR's behavior.
            result.Name = ReadIdWithoutLeadingAndEndingSpaces(input, ref idx);

            for(;;)
            {
                switch(PeekNextToken(input, idx))
                {
                    case TokenType.Comma:
                        break;
                    default:
                        return result;
                }

                ReadSpecialToken(input, TokenType.Comma, ref idx);

                // category should not contain leading or ending spaces.
                string category = ReadIdWithoutLeadingAndEndingSpaces(input, ref idx);
                ReadSpecialToken(input, TokenType.Equals, ref idx);
                // value should not contain leading or ending spaces.
                string value = ReadIdWithoutLeadingAndEndingSpaces(input, ref idx);
                switch(category)
                {
                    case "Version":
                        if (result.Version != null)
                        {
                            throw new ArgumentException(Resources.VersionAlreadyDefined);
                        }
                        else
                        {
                            result.Version = new Version(value);
                        }
                        break;
                    case "Culture":
                        if (!(value.Equals("neutral")))
                        {
                            result.CultureInfo = CultureInfo.GetCultureInfo(value);
                        }
                        else
                        {
                            result.CultureInfo = CultureInfo.InvariantCulture;
                        }
                        break;
                    case "PublicKeyToken":
                        if (!(value.Equals("null")))
                        {
                            if ((value.Length & 1) != 0)
                            {
                                throw new ArgumentException(Resources.InvalidPublicKeyTokenLength);
                            }
                            byte[] publicKeyToken = new byte[value.Length / 2];
                            for (int i = 0; i < publicKeyToken.Length; i++)
                            {
                                // NumberStyles.HexNumber is not culture specific (only 0-9, A-F, and spaces allowed)
                                // so we can use CultureInfo.InvariantCulture for parsing.
                                publicKeyToken[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                            }
                            result.SetPublicKeyToken(publicKeyToken);
                        }
                        else
                        {
                            result.SetPublicKeyToken(new byte[0]);
                        }
                        break;
                    default:
                        throw new ArgumentException(string.Format(
                            CultureInfo.InvariantCulture, Resources.UnrecognizedAssemblyAttribute, category));
                }
            }
        }


        // Determine which assembly to do a lookup in.
        // a universe + assemblyName are explicitly supplied, use that.
        // Else use the default token resolver supplied. 
        // This will throw if not enough information is given to determine the module.
        // Lokoup in an assembly is responsible for locating the right module within that assembly that contains the type.
        static Assembly DetermineAssembly(AssemblyName assemblyName,
            Module defaultTokenResolver,
            ITypeUniverse universe)
        {
            //Find out which token resolver to use
            Module tr = defaultTokenResolver;
            if (assemblyName != null)
            {
                if (universe == null)
                {
                    throw new ArgumentException(Resources.HostSpecifierMissing);
                }

                Assembly a = universe.ResolveAssembly(assemblyName);

                if (a == null)
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture, Resources.UniverseCannotResolveAssembly, assemblyName));
                }
                return a;
            }
            else if (defaultTokenResolver == null)
            {
                throw new ArgumentException(Resources.DefaultTokenResolverRequired);
            }

            Debug.Assert(tr != null);
            return tr.Assembly;
        }

        /// <summary>
        /// Given the parsed string components of a type, create an actual type object.
        /// Caller can then apply modifiers to this.
        /// </summary>
        /// <param name="path">Type name parts.</param>
        /// <param name="genericTypeArgs">Type arguments for generic types.</param>
        /// <param name="assembly">Assembly in which type should be looked up.</param>
        /// <returns>Type that corresponds to given type name or null if type cannot be found.</returns>
        private static Type Resolve(
            List<string> path,
            List<Type> genericTypeArgs,
            Assembly assembly)
        {
            Debug.Assert(assembly != null);

            //Get the outermost raw type
            Debug.Assert(path.Count > 0, "Path should always contain at least one string");
            Type type = assembly.GetType(path[0], false);
            
            // If type was not found in current module, return null so caller can try
            // another module. 
            if (type == null)
            {
                return null;
            }

            //Process nested types
            for (int i = 1; i < path.Count; i++)
            {
                Type nestedType = type.GetNestedType(path[i], BindingFlags.Public | BindingFlags.NonPublic);
                // If nested type was not found in current module, return null so caller can try
                // another module. 
                if (nestedType == null)
                {
                    return null;
                }
                type = nestedType;
            }

            //Process generic type arguments
            if (genericTypeArgs.Count > 0)
            {
                type = type.MakeGenericType(genericTypeArgs.ToArray());
            }

            //Done
            return type;
        }

        private static void ReadSpecialToken(string input, TokenType expected, ref int idx)
        {
            Token tok = ReadToken(input, ref idx);
            if ((tok == null) || (tok.TokenType != expected))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.ExpectedTokenType, expected));
            }
        }

        /// <summary>
        /// The string returned for the IdToken may contain whitespaces,
        /// including leading and ending whitespaces, e.g. " Foo ".
        /// </summary>
        private static string ReadIdToken(string input, ref int idx)
        {
            Token tok = ReadToken(input, ref idx);
            if ((tok == null) || (tok.TokenType != TokenType.Id))
            {
                throw new ArgumentException(Resources.IdTokenTypeExpected);
            }
            else
            {
                return tok.Value;
            }
        }

        /// <summary>
        /// Return a string without leading spaces, e.g. used for type name.
        /// The result can have ending spaces, e.g. "Type Name ".
        /// </summary>
        private static string ReadIdWithoutLeadingSpaces(string input, ref int idx) {
            return TrimLeadingSpaces(ReadIdToken(input, ref idx));
        }

        /// <summary>
        /// Return a string without leading end ending spaces, e.g. for assmbly name.
        /// The result may contain spaces in the middle, e.g. "Assembly Name".
        /// </summary>
        private static string ReadIdWithoutLeadingAndEndingSpaces(string input, ref int idx) {
            return ReadIdToken(input, ref idx).Trim();
        }

        private static TokenType PeekNextToken(string input, int idx)
        {
            Token tok = ReadToken(input, ref idx);
            if (tok == null)
                return TokenType.EndOfInput;
            else
                return tok.TokenType;
        }

        private static TokenType PeekSecondToken(string input, int idx)
        {
            Token tok = ReadToken(input, ref idx);
            if (tok == null)
                throw new ArgumentException(Resources.UnexpectedEndOfInput);
            else
            {
                return PeekNextToken(input, idx);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static Token ReadToken(string input, ref int idx)
        {
            StringBuilder sb = new StringBuilder();
            bool fLiteral = false;
            int i;
            for (i = idx; i < input.Length; i++)
            {
                char c = input[i];
                if (fLiteral)
                {
                    sb.Append(c);
                    fLiteral = false;
                }
                else
                {
                    // whitespaces are not used as separators, 
                    // and they are included in the result.
                    switch (c)
                    {
                        case '\'':
                            fLiteral = true;
                            break;
                        case '+':
                        case '[':
                        case ']':
                        case ',':
                        case '=':
                        case '&':
                        case '*':
                            if (sb.Length > 0)
                            {
                                //Return the id string we accumulated
                                idx = i;
                                return Token.MakeIdToken(sb.ToString());
                            }
                            else
                            {
                                idx = i + 1;
                                switch (c)
                                {
                                    case '+':
                                        return Token.Plus;
                                    case '[':
                                        return Token.LeftBracket;
                                    case ']':
                                        return Token.RightBracket;
                                    case ',':
                                        return Token.Comma;
                                    case '=':
                                        return Token.Equals;
                                    case '&':
                                        return Token.Reference;
                                    case '*':
                                        return Token.Pointer;
                                    default:
                                        throw new InvalidOperationException(Resources.UnexpectedCharacterFound);
                                }
                            }
                        default:
                            sb.Append(c);
                            break;

                    }
                }
            }
            if (fLiteral)
            {
                throw new ArgumentException(Resources.EscapeSequenceMissingCharacter);
            }

            idx = i;
            //If we have an id string, return it
            if (sb.Length > 0)
            {
                return Token.MakeIdToken(sb.ToString());
            }
            else
            {
                //No more input
                return null;
            }
        }

        /// <summary>
        /// The method rips off the leading whitespaces in the input.
        /// The result is String.Empty if the input only contains whitespaces.
        /// </summary>
        private static string TrimLeadingSpaces(string str) {
            Debug.Assert(str != null);
            int start = 0;
            while (start < str.Length && Char.IsWhiteSpace(str, start)) {
                start++;
            }
            // The result is String.Empty if start is the same as str.Length.
            return str.Substring(start);
        }

        /// <summary>

        /// Detemines if given type name represents a compound type (e.g. generic, array, nested,
        /// pointer, or reference).
        /// </summary>
        /// <returns>True if the typename requires any parsing and is not just found in the TypeDef name table.</returns>
        public static bool IsCompoundType(string name)
        {
            return name.IndexOfAny(compoundTypeNameCharacters) > 0;
        }

        static readonly char[] compoundTypeNameCharacters = new char[] { '+', ',', '[', '*', '&' };
    }
}
