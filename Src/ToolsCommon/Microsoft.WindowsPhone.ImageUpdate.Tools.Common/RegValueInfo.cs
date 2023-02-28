// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RegValueInfo
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public sealed class RegValueInfo
  {
    public RegValueType Type;
    public string KeyName;
    public string ValueName;
    public string Value;
    public string Partition;

    public RegValueInfo()
    {
    }

    public RegValueInfo(RegValueInfo regValueInfo)
    {
      this.Type = regValueInfo.Type;
      this.KeyName = regValueInfo.KeyName;
      this.ValueName = regValueInfo.ValueName;
      this.Value = regValueInfo.Value;
      this.Partition = regValueInfo.Partition;
    }

    public void SetRegValueType(string strType) => this.Type = RegUtil.RegValueTypeForString(strType);
  }
}
