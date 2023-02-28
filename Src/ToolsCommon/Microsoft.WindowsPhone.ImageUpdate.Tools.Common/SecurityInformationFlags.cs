// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.SecurityInformationFlags
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [CLSCompliant(false)]
  public enum SecurityInformationFlags : uint
  {
    OWNER_SECURITY_INFORMATION = 1,
    GROUP_SECURITY_INFORMATION = 2,
    DACL_SECURITY_INFORMATION = 4,
    SACL_SECURITY_INFORMATION = 8,
    MANDATORY_ACCESS_LABEL = 16, // 0x00000010
    UNPROTECTED_SACL_SECURITY_INFORMATION = 268435456, // 0x10000000
    UNPROTECTED_DACL_SECURITY_INFORMATION = 536870912, // 0x20000000
    PROTECTED_SACL_SECURITY_INFORMATION = 1073741824, // 0x40000000
    PROTECTED_DACL_SECURITY_INFORMATION = 2147483648, // 0x80000000
  }
}
