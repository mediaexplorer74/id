// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.ProcessPrivilege
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class ProcessPrivilege
  {
    public static void Adjust(TokenPrivilege privilege, bool enablePrivilege)
    {
      if (NativeSecurityMethods.IU_AdjustProcessPrivilege(privilege.Value, enablePrivilege) != 0)
        throw new Exception(string.Format("Failed to adjust privilege with name {0} and value {1}", (object) privilege.Value, (object) enablePrivilege));
    }
  }
}
