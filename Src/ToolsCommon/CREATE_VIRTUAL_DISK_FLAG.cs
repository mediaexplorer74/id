// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.CREATE_VIRTUAL_DISK_FLAG
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [Flags]
  public enum CREATE_VIRTUAL_DISK_FLAG
  {
    CREATE_VIRTUAL_DISK_FLAG_NONE = 0,
    CREATE_VIRTUAL_DISK_FLAG_FULL_PHYSICAL_ALLOCATION = 1,
  }
}
