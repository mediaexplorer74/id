// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.SOCPkgFile
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  public class SOCPkgFile : PkgFile
  {
    [XmlAttribute("SOC")]
    public string SOC;

    public SOCPkgFile(FeatureManifest.PackageGroups fmGroup)
      : base(fmGroup)
    {
    }

    public SOCPkgFile()
      : base(FeatureManifest.PackageGroups.SOC)
    {
      if (this.CPUType != null)
        return;
      this.CPUType = FeatureManifest.CPUType_ARM;
    }

    [XmlIgnore]
    public override string GroupValue
    {
      get => this.SOC;
      set => this.SOC = value;
    }
  }
}
