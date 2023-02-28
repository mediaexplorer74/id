// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.PkgVersion
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using System;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  [CLSCompliant(false)]
  public class PkgVersion
  {
    [XmlAttribute]
    public ushort Major;
    [XmlAttribute]
    public ushort Minor;
    [XmlAttribute]
    public ushort QFE;
    [XmlAttribute]
    public ushort Build;
  }
}
