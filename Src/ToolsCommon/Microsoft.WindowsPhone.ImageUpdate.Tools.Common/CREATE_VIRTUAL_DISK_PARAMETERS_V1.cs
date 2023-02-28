// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.CREATE_VIRTUAL_DISK_PARAMETERS_V1
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [CLSCompliant(false)]
  public struct CREATE_VIRTUAL_DISK_PARAMETERS_V1
  {
    public Guid UniqueId;
    public ulong MaximumSize;
    public uint BlockSizeInBytes;
    public uint SectorSizeInBytes;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string ParentPath;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string SourcePath;
  }
}
