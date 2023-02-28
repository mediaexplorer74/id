//-----------------------------------------------------------------------------
// General wrappers for metadata tokens.

using System;
using System.Diagnostics;
using System.Globalization;

namespace System.Reflection.Adds
{
    /// <summary>
    /// Types for metadata tokens. These are from the unmanaged metadata interfaces in Cor.h
    /// </summary>
#if USE_CLR_V4
    public enum TokenType
#else
    public enum TokenType
#endif
    {
        Module = 0x00000000,
        TypeRef = 0x01000000,
        TypeDef = 0x02000000,
        FieldDef = 0x04000000,
        MethodDef = 0x06000000,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param")]
        ParamDef = 0x08000000,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Impl")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        InterfaceImpl = 0x09000000,
        MemberRef = 0x0a000000,
        CustomAttribute = 0x0c000000,
        Permission = 0x0e000000,
        Signature = 0x11000000,
        Event = 0x14000000,
        Property = 0x17000000,
        ModuleRef = 0x1a000000,
        TypeSpec = 0x1b000000,
        Assembly = 0x20000000,
        AssemblyRef = 0x23000000,
        File = 0x26000000,
        ExportedType = 0x27000000,
        ManifestResource = 0x28000000,
        GenericPar = 0x2a000000,
        MethodSpec = 0x2b000000,
        String = 0x70000000,
        Name = 0x71000000,
        BaseType = 0x72000000,
        Invalid = 0x7FFFFFFF,
    }

    /// <summary>
    /// Metadata table indexes as defined by the CLI standard.
    /// </summary>
#if USE_CLR_V4
    public enum MetadataTable
#else
    public enum MetadataTable
#endif
    {
        Module = 0x00,
        TypeRef = 0x01,
        TypeDef = 0x02,
        FieldDef = 0x04,
        MethodDef = 0x06,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param")]
        ParamDef = 0x08,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Impl")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        InterfaceImpl = 0x09,
        MemberRef = 0x0a,
        CustomAttribute = 0x0c,
        Permission = 0x0e,
        Signature = 0x11,
        Event = 0x14,
        Property = 0x17,
        ModuleRef = 0x1a,
        TypeSpec = 0x1b,
        Assembly = 0x20,
        AssemblyRef = 0x23,
        File = 0x26,
        ExportedType = 0x27,
        ManifestResource = 0x28,
        GenericPar = 0x2a,
        MethodSpec = 0x2b,
    }

    /// <summary>
    /// Wrapper around an arbitrary metadata token. This should be sizeof(int).
    /// </summary>
#if USE_CLR_V4
    public struct Token
#else
    public struct Token
#endif    
    {
        [DebuggerStepThrough]
        public Token(int value)
        {
            this.value = value;
        }

        public Token(TokenType type, int rid)
        {
            // type is already shifted left 24 bits, so we can just add
            this.value = ((int) type) + rid;
        }

        //Convienence method - don't care if value is signed or unsigned
        //[CLSCompliant(false)]
        [DebuggerStepThrough]
        public Token(uint value)
        {
            this.value = (int)value;
        }

        public static readonly Token Nil = new Token(0);

        // the actual raw token.
        private int value;

        /// <summary>
        /// Get the token as an integer
        /// </summary>
        public int Value
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// Get the type of token (MethodDef, MethodRef, TypeDef, etc)
        /// </summary>
        public TokenType TokenType
        {
            get
            {
                return (TokenType)(value & 0xFF000000);
            }
        }

        /// <summary>
        /// Get the row index (RID) of the token. This generally starts at 1,
        /// but 0 can be used for special cases (like invalid token)
        /// </summary>
        public int Index
        {
            get
            {
                return value & 0x00FFFFFF;
            }
        }

        public bool IsNil
        {
            get
            {
                return Index == 0;
            }
        }

        public static implicit operator int(Token token) { return token.value; }
        public static bool operator ==(Token token1, Token token2) { return (token1.value == token2.value); }
        public static bool operator !=(Token token1, Token token2) { return !(token1 == token2); }
        public static bool operator ==(Token token1, int token2) { return (token1.value == token2); }
        public static bool operator !=(Token token1, int token2) { return !(token1 == token2); }
        public static bool operator ==(int token1, Token token2) { return (token1 == token2.value); }
        public static bool operator !=(int token1, Token token2) { return !(token1 == token2); }

        public static bool IsType(int token, params TokenType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if ((int)(token & 0xFF000000) == (int)types[i])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if this token is of the given type.
        /// </summary>
        public bool IsType(TokenType type)
        {
            return this.TokenType == type;
        }

        public override bool Equals(object obj)
        {
            if (obj is Token)
            {
                Token oToken = (Token)obj;
                return (value == oToken.value);
            }
            if (obj is int)
            {
                return (value == (int)obj);
            }
            return false;
        }

        public override int GetHashCode() { return value.GetHashCode(); }


        /// <summary>
        /// Get a user-friendly string representation of the token.
        /// </summary>
        public override string ToString()
        {
            string s = String.Format(CultureInfo.InvariantCulture, "{0}(0x{1:x})", this.TokenType, this.Index);
            return s;
        }
    } // Token class

} // namespace