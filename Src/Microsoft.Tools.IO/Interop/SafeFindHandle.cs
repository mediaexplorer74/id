// Decompiled with JetBrains decompiler
// Type: Microsoft.Tools.IO.Interop.SafeFindHandle
// Assembly: Microsoft.Tools.IO, Version=1.1.11.0, Culture=neutral, PublicKeyToken=1a5b963c6f0fbeab
// MVID: 222C54A4-0FE1-469A-8627-EF94B226BBFA
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Tools.IO.dll

using Microsoft.Win32.SafeHandles;

namespace Microsoft.Tools.IO.Interop
{
  internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
  {
    internal SafeFindHandle()
      : base(true)
    {
    }

    protected override bool ReleaseHandle() => NativeMethods.FindClose(this.handle);
  }
}
