// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.SupportedLangs
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  public class SupportedLangs
  {
    [XmlArrayItem(ElementName = "Language", IsNullable = false, Type = typeof (string))]
    [DefaultValue(null)]
    [XmlArray]
    public List<string> UserInterface;
    [DefaultValue(null)]
    [XmlArrayItem(ElementName = "Language", IsNullable = true, Type = typeof (string))]
    [XmlArray]
    public List<string> Keyboard;
    [DefaultValue(null)]
    [XmlArrayItem(ElementName = "Language", IsNullable = true, Type = typeof (string))]
    [XmlArray]
    public List<string> Speech;
  }
}
