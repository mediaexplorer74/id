// LMR implementation of a method body

using System;
using Debug=Microsoft.MetadataReader.Internal.Debug;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Adds;
using System.Runtime.InteropServices;

#if USE_CLR_V4
using System.Reflection;  
#else
using System.Reflection.Mock;
using BindingFlags = System.Reflection.BindingFlags;
using CallingConventions = System.Reflection.CallingConventions;
using MethodAttributes = System.Reflection.MethodAttributes;
using Type = System.Reflection.Mock.Type;
using MemberTypes = System.Reflection.MemberTypes;
using ExceptionHandlingClauseOptions = System.Reflection.ExceptionHandlingClauseOptions;
#endif

namespace Microsoft.MetadataReader
{
    /// <summary>
    /// Base implementation for a MethodBody. This provides stub implementations of the methods. It also
    /// provides a default implementation of the LocalVariables property built on the LocalSignatureMetadataToken property.
    /// </summary>
    internal class MetadataOnlyMethodBody : MethodBody
    {
        // The method that this body is for. This also provides a backpointer to the metadata importer and
        // ability to fetch the RVA.
        readonly MetadataOnlyMethodInfo m_method;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="method">method that this body corresponds to.</param>
        protected MetadataOnlyMethodBody(MetadataOnlyMethodInfo method)
        {
            Debug.Assert(method != null);
            m_method = method;
        }

        /// <summary>
        /// Helper to create a method body and invoke the reflection factory .
        /// </summary>
        /// <param name="method">method to request the body for</param>
        /// <returns>null if the method does not have an IL body, else the instantiated body.</returns>
        internal static MethodBody TryCreate(MetadataOnlyMethodInfo method)
        {
            // First give the factory an opportunity to create.
            MetadataOnlyModule scope = method.Resolver;

            MethodBody b = null;
            if (scope.Factory.TryCreateMethodBody(method, ref b))            
            {
                // Factory claimed ownership, return the results. 
                return b;
            }

            // Factory didn't hook, now try LMR's creation path.
            return MetadataOnlyMethodBodyWorker.Create(method);
        }

        /// <summary>
        /// Method that this body belongs to.
        /// </summary>
        protected MetadataOnlyMethodInfo Method
        {
            get { return m_method; }
        }

        /// <summary>
        /// empty implementation from base class
        /// </summary>
        public override IList<ExceptionHandlingClause> ExceptionHandlingClauses
        {
            get
            {
                // We can get the EH clauses from the method Header. The clauses are after the IL. 
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// empty implementation from base class
        /// </summary>
        public override bool InitLocals
        {
            get
            {
                //Need to be overriden in the sub type.
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// empty implementation from base class. 
        /// </summary>
        public override int LocalSignatureMetadataToken
        {
            get
            {
                //Need to be overriden in the sub type.
                throw new InvalidOperationException(); 
            }
        }

        /// <summary>
        /// Default implementation built on top of code:LocalSignatureMetadataToken
        /// </summary>
        public override IList<LocalVariableInfo> LocalVariables
        {
            get
            {
                Token sigToken = new Token(this.LocalSignatureMetadataToken);
                EmbeddedBlobPointer pSig = new EmbeddedBlobPointer();
                int cbSig = 0;

                // Be flexible for either 0 or 0x11000000. 
                if (!sigToken.IsNil)
                {
                    m_method.Resolver.RawImport.GetSigFromToken(sigToken, out pSig, out cbSig);
                }
                if (cbSig == 0)
                {
                    //We get this if there are no local variables
                    return new MetadataOnlyLocalVariableInfo[0];
                }
                else
                {
                    GenericContext context = new GenericContext(m_method);

                    byte[] localSig = m_method.Resolver.ReadEmbeddedBlob(pSig, cbSig);
                    int index = 0;

                    //Calling convention
                    CorCallingConvention callConv = SignatureUtil.ExtractCallingConvention(localSig, ref index);
                    Debug.Assert(callConv == CorCallingConvention.LocalSig);

                    //Number of variables
                    int varCount = SignatureUtil.ExtractInt(localSig, ref index);

                    //Local variables
                    var locals = new MetadataOnlyLocalVariableInfo[varCount];
                    for (int i = 0; i < varCount; i++)
                    {
                        TypeSignatureDescriptor descriptor = SignatureUtil.ExtractType(localSig, ref index, m_method.Resolver, context, true);
                        locals[i] = new MetadataOnlyLocalVariableInfo(i, descriptor.Type, descriptor.IsPinned);
                    }

                    return locals;
                }
            }
        }

        /// <summary>
        /// empty implementation from base class. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override int MaxStackSize
        {
            get
            {
                //Need to be overriden in the sub type.
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// empty implementation from base class. 
        /// </summary>
        public override byte[] GetILAsByteArray()
        {
            //Need to be overriden in the sub type.
            throw new InvalidOperationException();
        }
    }


    /// <summary>
    /// MethodBody implementation that reads data from static metadata.
    /// This may not work in cases where the metadata is not valid (like dynamic modules or edit-and-continue).
    /// </summary>
    internal class MetadataOnlyMethodBodyWorker : MetadataOnlyMethodBody
    {
        private static readonly byte[] s_EmptyByteArray = new byte[0];
        /// <summary>
        /// unmanaged header from metadata. This may be a tiny or fat header.
        /// </summary>
        readonly private IMethodHeader m_header;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">method that the body is for</param>
        /// <param name="header">raw metadata header containing information for body</param>
        public MetadataOnlyMethodBodyWorker(MetadataOnlyMethodInfo method, IMethodHeader header)
            : base(method)
        {
            m_header = header;

            Debug.Assert(method != null);
            Debug.Assert(header != null);
        }

        /// <summary>
        /// Helper function to create. 
        /// </summary>
        /// <param name="method">method to create body for</param>
        /// <returns>null if no method body, else the new method body.</returns>
        internal static MethodBody Create(MetadataOnlyMethodInfo method)
        {
            MetadataOnlyModule scope = method.Resolver;

            // For static metadata, if there's no RVA, there's no method body.
            // For non-static metadata (such as in debugger cases), the debugger client should use a factory
            // to hook MethodBody creation to handle debugger cases.
            uint rva = scope.GetMethodRva(method.MetadataToken);
            if (rva == 0)
                return null;

            IMethodHeader header = GetMethodHeader(rva, scope);

            MetadataOnlyMethodBody m = new MetadataOnlyMethodBodyWorker(method, header);
            return m;
        }

        /// <summary>
        /// Get the method header from the metadata at the given RVA.
        /// </summary>
        /// <param name="rva">rva of method header, from MethodDefProps. </param>
        /// <param name="scope">scope that the RVA is valid in.</param>
        /// <returns>a method header</returns>
        public static IMethodHeader GetMethodHeader(uint rva, MetadataOnlyModule scope)
        {
            Debug.Assert(rva != 0);

            // Read first byte to determine if the header is Tiny or fat.
            byte[] b1 = scope.RawMetadata.ReadRva(rva, 1);
            MethodHeaderFlags flags = (MethodHeaderFlags)(b1[0] & 3);

            // Read the appropriate header
            IMethodHeader header;
            if (flags == MethodHeaderFlags.FatFormat)
            {
                header = scope.RawMetadata.ReadRvaStruct<FatHeader>(rva);
                Debug.Assert(Marshal.SizeOf(typeof(FatHeader)) == header.HeaderSizeBytes);
                Debug.Assert((header.Flags & MethodHeaderFlags.FatFormat) != 0);                
            }
            else if (flags == MethodHeaderFlags.TinyFormat)
            {
                header = new TinyHeader(b1[0]);
                Debug.Assert((header.Flags & MethodHeaderFlags.TinyFormat) != 0);                
                return header;
            }
            else
            {
                throw new InvalidOperationException(Resources.InvalidMetadata);
            }
            Debug.Assert(header.LocalVarSigTok == 0 || header.LocalVarSigTok.IsType(TokenType.Signature));
            return header;
        }

        public override IList<ExceptionHandlingClause> ExceptionHandlingClauses
        {
            get
            {
                // No exception clauses in this method.
                if ((this.m_header.Flags & MethodHeaderFlags.MoreSects) == 0)
                {
                    return new ExceptionHandlingClause[0];
                }

                // See section 24.4.5 for layout of Exception handling structures. The layout is:
                //   Method Header
                //   raw IL Code bytes
                //   Extra metadata sections (aligned at 4 byte boundary)
                //   
                // The extra metadata sections describe the exception handling caluses. Their format is:
                //   - 4 bytes for an exception header. Header provides number of clauses.
                //   - array of clauses.
                MetadataOnlyModule scope = this.Method.Resolver;

                uint rva = scope.GetMethodRva(this.Method.MetadataToken);
                var s = rva + this.m_header.HeaderSizeBytes + m_header.CodeSize;
                s = ((s - 1) & ~0x3) + 4; // align up to next 4 byte boundary
                

                //
                // Get the flags from the header
                //
                Byte f = scope.RawMetadata.ReadRvaStruct<Byte>(s);
                CorILMethod_Sect f2 = (CorILMethod_Sect)f;

                // Only valid flags are for exception handling. 
                if ((f2 & ~(CorILMethod_Sect.EHTable | CorILMethod_Sect.FatFormat)) != 0)
                {
                    Debug.Assert(false); 
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture, Resources.UnsupportedExceptionFlags, f2.ToString()));
                }
                
                ExceptionHandlingClause[] clauses;

                // 
                //  Read exception header. This is always 4 bytes. 
                // 
                int dataSize; // total size of subsequent clauses in bytes
                int numClauses; // number of clauses 
                int sizeClause; // size of each clause in bytes.
                
                // EH info has both a fat / small header, which use fat clauses or small clauses respectively.
                // Compilers can prefer a small header, and then spill over to a fat header if the small clause
                // can't describe the exception regions.
                bool fat = (f2 & CorILMethod_Sect.FatFormat) != 0;
                
                if (fat)
                {
                    // Fat header:
                    // Byte 0 = flags 
                    // Next 3 bytes (s+1,s+2,s+3) describe data size.
                    var  x = scope.RawMetadata.ReadRva(s +1, 3);
                dataSize = x[0] + x[1] * 256 + x[2] * (256*256);

                    sizeClause = 24; // sizeof the fat clauses
                   
                }
                else
                {
                    // Tiny Header. 
                    // Byte 0 = flags
                    // Byte 1 = data isze
                    // Byte 2,3 = reserved (must be 0)
                    dataSize = scope.RawMetadata.ReadRvaStruct<Byte>(s + 1);
                                    
                    sizeClause = 12; // sizeof small clauses.
                }

                // DataSize should be of form (n*sizeof(Clause)+4), n== number of clauses, 4= sizeof(header)
                numClauses = (dataSize - 4) / sizeClause;
                Debug.Assert(numClauses * sizeClause + 4 == dataSize); // should be even multiple

                clauses = new ExceptionHandlingClauseWorker[numClauses];
                
                //
                // Now read the actual exception clauses.
                //
                
                // Clauses are laid out sequentially, starting at s+4.
                var s2 = s + 4;
                for (int i = 0; i < numClauses; i++)
                {
                    IEHClause data = (fat) ? 
                        (IEHClause) scope.RawMetadata.ReadRvaStruct<EHFat>(s2) : 
                        (IEHClause) scope.RawMetadata.ReadRvaStruct<EHSmall>(s2);

                    s2 += sizeClause;
                    clauses[i] = new ExceptionHandlingClauseWorker(this.Method, data);
                }

                return Array.AsReadOnly<ExceptionHandlingClause>(clauses);
            }
        }

        // Expose exception handling data through reflection object.
        class ExceptionHandlingClauseWorker : ExceptionHandlingClause
        {
            private readonly MethodInfo m_method;
            private readonly IEHClause m_data;

            public ExceptionHandlingClauseWorker(MethodInfo method, IEHClause data)
            {
                m_method = method;
                m_data = data;
            }

            public override Type CatchType
            {
                get
                {
                    // Catch handlers can have generic types, so be sure to pass the proper generic context.
                    var token = m_data.ClassToken;
                    
                    var module = m_method.Module;
                    var t = module.ResolveType(token, m_method.DeclaringType.GetGenericArguments(), m_method.GetGenericArguments());
                    return t;
                }
            }

            public override int FilterOffset
            {
                get
                {
                    return m_data.FilterOffset;
                }
            }

            public override System.Reflection.ExceptionHandlingClauseOptions Flags
            {
                get
                {
                    return (System.Reflection.ExceptionHandlingClauseOptions) m_data.Flags;
                }
            }

            public override int HandlerLength
            {
                get
                {
                    return m_data.HandlerLength;
                }
            }

            public override int HandlerOffset
            {
                get
                {
                    return m_data.HandlerOffset;
                }
            }

            public override int TryLength
            {
                get
                {
                    return m_data.TryLength;
                }
            }

            public override int TryOffset
            {
                get
                {
                    return m_data.TryOffset;
                }
            }   
        }

        /// <summary>
        /// override from base class
        /// </summary>
        public override int MaxStackSize
        {
            get
            {
                return this.m_header.MaxStack;
            }
        }

        /// <summary>
        /// override from base class
        /// </summary>
        public override byte[] GetILAsByteArray()
        {
            // If we are reading from an assembly with no IL (like a reference assembly)
            // we can just skip all the other steps. 
            if (m_header.CodeSize == 0)
            {
                return s_EmptyByteArray;
            }

            // The IL byte array is stored directly after the method header.
            MetadataOnlyModule scope = this.Method.Resolver;

            uint rva = scope.GetMethodRva(this.Method.MetadataToken);
            var bytesIl = scope.RawMetadata.ReadRva(rva + this.m_header.HeaderSizeBytes, m_header.CodeSize);
            Debug.Assert(bytesIl != null);
            
            return bytesIl;
        }

        /// <summary>
        /// override from base class
        /// </summary>
        public override bool InitLocals
        {
            get
            {
                return (this.m_header.Flags & MethodHeaderFlags.InitLocals) != 0; 
            }
        }

        /// <summary>
        /// override from base class
        /// </summary>
        public override int LocalSignatureMetadataToken
        {
            get
            {
                // 0 if there are no locals (as opposed to 0x11000000).
                var token = this.m_header.LocalVarSigTok.Value;

                Debug.Assert((token == 0) || this.Method.Resolver.IsValidToken(token));
                return token;
            }
        }

        #region native structures        
        /// <summary>
        /// Base interface to provide uniform access to tiny and fat method headers.
        /// Since the headers are LayoutKind.Sequential, they can't inherit from a base class, so this must
        /// be an interface.
        /// 
        /// See IIb.24.4 for details on MethodBody layout in the metadata.
        /// This somewhat corresponds to the IMAGE_COR_ILMETHOD in CorHdr.h.
        /// 
        /// A method header is located at the RVA specified in the GetMethodProps(). The method header is
        /// followed by the IL bytes stream. That's followed by 'extra' sections', which may describe
        /// exception handling regions.
        /// </summary>
        internal interface IMethodHeader
        {
            /// <summary>
            /// maximum il stack depth that this function may use.
            /// </summary>
            int MaxStack { get; }

            /// <summary>
            /// Size of the IL code in bytes
            /// </summary>
            int CodeSize { get; }

            /// <summary>
            /// Local signature token. 0 if no locals.
            /// </summary>
            Token LocalVarSigTok { get; }

            /// <summary>
            /// Flags for this method
            /// </summary>
            MethodHeaderFlags Flags { get; }

            /// <summary>
            /// Size of the method header in bytes. 
            /// </summary>
            int HeaderSizeBytes { get; }
        }

        /// <summary>
        /// Flags for method headers. See spec IIb.24.4.4
        /// </summary>
        [Flags]
        internal enum MethodHeaderFlags
        {
            /// <summary>
            /// Method header is fat. Use code:FatHeader
            /// </summary>
            FatFormat = 0x3,
            
            /// <summary>
            /// Method header is tiny. Use code:TinyHeader
            /// </summary>
            TinyFormat = 0x2,

            /// <summary>
            /// More sections follow after this header. See IIb.24.4.5.
            /// </summary>
            MoreSects = 0x8,

            /// <summary>
            /// Call default constructor on all local variables.
            /// </summary>
            InitLocals = 0x10,
        }

        /// <summary>
        /// Tiny header layout. spec IIb.24.4.2. This corresponds to IMAGE_COR_ILMETHOD_TINY in CorHdr.h
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class TinyHeader : IMethodHeader
        {
            // Fields for unmanaged definition.
            readonly byte FlagsAndSize;

            // ctor used for marshallers.
            public TinyHeader()
            {
            }
            public TinyHeader(byte data)
            {
                this.FlagsAndSize = data;
            }

            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public MethodHeaderFlags Flags
            {
                get { return (MethodHeaderFlags)(FlagsAndSize & 3); }
            }

            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public int CodeSize
            {
                get { return (FlagsAndSize >> 2) & 0x3F; }
            }

            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public int MaxStack
            {
                get
                {
                    // Tiny Header has hardcoded max-stack size of 8.
                    return 8;
                }
            }

            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public Token LocalVarSigTok
            {
                // Tiny headers don't have any locals
                get { return Token.Nil; }
            }

            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public int HeaderSizeBytes
            {
                // A tiny header is 1 byte.
                get { return 1; }
            }
        }

        /// <summary>
        /// Fat header layout. spec IIb.24.4.3. This corresponds to IMAGE_COR_ILMETHOD_FAT in CorHdr.h
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class FatHeader : IMethodHeader
        {
            // These fields are the raw layout of the unmanaged struct.
            private readonly short m_FlagsAndSize; // Flags and Size combined
            private readonly short m_MaxStack;
            private readonly uint m_CodeSize;
            private readonly uint m_LocalVarSigTok;


            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public MethodHeaderFlags Flags
            {
                get { return (MethodHeaderFlags)(m_FlagsAndSize & 0xFFF); }
            }

            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public int MaxStack
            {
                get { return m_MaxStack; }
            }

            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public int CodeSize
            {
                get { return (int)m_CodeSize; }
            }

            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public Token LocalVarSigTok
            {
                get { return new Token(m_LocalVarSigTok); }
            }

            /// <summary>
            /// Implement IMethodHeader
            /// </summary>
            public int HeaderSizeBytes
            {
                get
                {
                    int size = (m_FlagsAndSize >> 12) & 0xF;
                    return size * 4;
                }
            }
        }

        //
        // Exception structures
        // 

        // Describe the flags in the EH header. See 24.4.5 of IIB.
        [Flags]
        enum CorILMethod_Sect
        {
            EHTable = 0x1,    // exception handling data
            OptILTable = 0x2, // reserved, shall be 0
            FatFormat = 0x40, // if true, use fat header and fat clauses.
            MoreSects = 0x80, // another data section occurs after this current section
        }

        // Interface to provide uniform access to tiny or fat header.
        // See IIB.24.4.6. 
        interface IEHClause
        {
            /// <summary>
            /// Flags describing exception clause type (catch-block, filter, finally, or fault)
            /// </summary>
            ExceptionHandlingClauseOptions Flags { get; }
            
            /// <summary>
            /// IL offset in bytes from start of header
            /// </summary>
            int TryOffset { get; }
            
            /// <summary>
            /// Length in bytes of the try block
            /// </summary>
            int TryLength { get; }

            /// <summary>
            /// IL offset in bytes of the handler for the try block
            /// </summary>
            int HandlerOffset { get; }

            /// <summary>
            /// length in bytes of the IL code for the handler
            /// </summary>
            int HandlerLength { get; }

            /// <summary>
            /// metadata Token for a type-based exception handler. This is valid in the scope of the method
            /// containing the IL code.
            /// </summary>
            Token ClassToken { get; }

            /// <summary>
            /// Offset in method body ofr filter-based exception handler.
            /// </summary>
            int FilterOffset { get; }
        }

        /// <summary>
        /// Raw layout of a small version of an EH Clause.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class EHSmall : IEHClause
        {
            private readonly UInt16 m_Flags;
            private readonly UInt16 m_TryOffset;
            private readonly Byte m_TryLength;
            
            // private readonly UInt16 m_HandlerOffset;
            private readonly Byte m_HandlerOffset1;
            private readonly Byte m_HandlerOffset2;

            private readonly Byte m_HandlerLength;
            private readonly UInt32 m_ClassToken;
            private readonly Int32 m_FilterOffset;

            #region IEHClause Members

            ExceptionHandlingClauseOptions IEHClause.Flags
            {
                get { return (ExceptionHandlingClauseOptions)this.m_Flags; }
            }

            int IEHClause.TryOffset
            {
                get { return this.m_TryOffset; }
            }

            int IEHClause.TryLength
            {
                get { return this.m_TryLength; }
            }

            int IEHClause.HandlerOffset
            {
                get { return this.m_HandlerOffset2 * 256 + this.m_HandlerOffset1; }
            }

            int IEHClause.HandlerLength
            {
                get { return this.m_HandlerLength;  }
            }

            Token IEHClause.ClassToken
            {
                get { return new Token(this.m_ClassToken);  }
            }

            int IEHClause.FilterOffset
            {
                get { return this.m_FilterOffset; }
            }

            #endregion
        }

        /// <summary>
        /// Raw layout of a fat version of an EH clause.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class EHFat : IEHClause
        {
            private readonly UInt32 m_Flags;
            private readonly Int32 m_TryOffset;
            private readonly Int32 m_TryLength;
            private readonly Int32 m_HandlerOffset;
            private readonly Int32 m_HandlerLength;
            private readonly UInt32 m_ClassToken;
            private readonly Int32 m_FilterOffset;

            #region IEHClause Members

            ExceptionHandlingClauseOptions IEHClause.Flags
            {
                get { return (ExceptionHandlingClauseOptions)this.m_Flags; }
            }

            int IEHClause.TryOffset
            {
                get { return this.m_TryOffset; }
            }

            int IEHClause.TryLength
            {
                get { return this.m_TryLength; }
            }

            int IEHClause.HandlerOffset
            {
                get { return this.m_HandlerOffset; }
            }

            int IEHClause.HandlerLength
            {
                get { return this.m_HandlerLength; }
            }

            Token IEHClause.ClassToken
            {
                get { return new Token(this.m_ClassToken); }
            }

            int IEHClause.FilterOffset
            {
                get { return this.m_FilterOffset; }
            }

            #endregion
        }



        #endregion
    }

} // namespace Microsoft.MetadataReader