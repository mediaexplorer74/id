// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RegValueType
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public enum RegValueType
  {
    [XmlEnum(Name = "REG_SZ")] String,
    [XmlEnum(Name = "REG_EXPAND_SZ")] ExpandString,
    [XmlEnum(Name = "REG_BINARY")] Binary,
    [XmlEnum(Name = "REG_DWORD")] DWord,
    [XmlEnum(Name = "REG_MULTI_SZ")] MultiString,
    [XmlEnum(Name = "REG_QWORD")] QWord,
    [XmlEnum(Name = "REG_HEX")] Hex,
  }
}
