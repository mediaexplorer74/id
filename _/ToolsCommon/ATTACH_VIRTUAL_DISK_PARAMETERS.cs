// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.ATTACH_VIRTUAL_DISK_PARAMETERS
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct ATTACH_VIRTUAL_DISK_PARAMETERS
  {
    public ATTACH_VIRTUAL_DISK_VERSION Version;
    public int Reserved;
  }
}
