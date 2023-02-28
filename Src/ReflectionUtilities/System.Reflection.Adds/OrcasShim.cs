// LMR should only depend on mscorlib.
// - This prepares LMR for eventually being included in Mscorlib if we replace Reflection.Only APIs with LMR.
// - A key Proteus multi-targetting scenario involves running a build of LMR inside a .NET 2.0 process. 

// So provide implementation of APIs outside mscorlib. If we move LMR onto Orcas, then we can switch off
// the placeholders in this file and move to the 'real' definitions in Orcas.

namespace Microsoft.MetadataReader.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    // Very basic implementation of an assert dialog so that we don't need to link against System.dll.
    internal class Debug
    {
        // Dialog Box Command IDs, From WinUser.h
        enum MessageBoxResult
        {
            IDABORT = 3,
            IDRETRY = 4,
            IDIGNORE = 5,
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [System.Runtime.InteropServices.DllImport("user32.dll", BestFitMapping = false)]
        static extern int MessageBoxA(int h, string m, string c, int type);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.MetadataReader.Internal.Debug.MessageBoxA(System.Int32,System.String,System.String,System.Int32)")]
        static MessageBoxResult MessageBox(string message)
        {
            const int MB_ICONEXCLAMATION = 0x30;
            const int MB_ABORTRETRYIGNORE = 0x00000002;
            int res = MessageBoxA(0, message, "LMR Assert failed", MB_ICONEXCLAMATION | MB_ABORTRETRYIGNORE);
            return (MessageBoxResult)res;            
        }
               

        [Conditional("DEBUG")]
        public static void Assert(bool f)
        {
            Assert(f, "Assert failed");
        }

        [Conditional("DEBUG")]
        public static void Assert(bool f, string message)
        {
            if (!f)
            {
                // If you stop here, the assert failed.
                Debugger.Log(0, "assert", message);

                // Show a message box UI before we break into the debugger.
                string stack = System.Environment.StackTrace;
                var result = MessageBox(message + "\r\n" + stack + 
@"
Abort - terminate the process
Retry - break into the debugger
Ignore - ignore the assert and continue running"
);
                if (result == MessageBoxResult.IDABORT)
                {
                    Environment.Exit(1);
                }
                if (result == MessageBoxResult.IDRETRY)
                {
                    Debugger.Break();
                }
            }
        }

        [Conditional("DEBUG")]
        public static void Fail(string message)
        {
            Assert(false, message);
        }
    }
   

#if !USE_CLR_V4
    // Definition of a System.Collections.Generic.HashSet.
    // For V4 build, we're part of System.Design.dll, which links against Sytem.core and includes a definition of HashSet.    
    class HashSet<T>
    {
        Dictionary<T, object> m_contents = new Dictionary<T, object>();
        public bool Contains(T element)
        {
            return m_contents.ContainsKey(element);
        }
        public void Add(T element)
        {
            m_contents[element] = null;
        }
        public int Count
        {
            get { return m_contents.Count; }
        }
        public void CopyTo(T[] array)
        {
            int i = 0;
            foreach (var kv in m_contents)
            {
                array[i] = kv.Key;
                i++;
            }
        }
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var t in other)
                this.Add(t);
        }
    }
#endif
}