// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.UpdateOSOutputPackage
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  public class UpdateOSOutputPackage
  {
    public string Description;
    public string UpdateState;
    public string PackageFile;
    public UpdateOSOutputIdentity Identity;
    public ReleaseType ReleaseType;
    public OwnerType OwnerType;
    public BuildType BuildType;
    public CpuId CpuType;
    public string Culture;
    public string Resolution;
    public string Partition;
    [DefaultValue(false)]
    public bool IsRemoval;
    public int Result;

    public override string ToString() => this.Name + ":" + this.Partition;

    [XmlIgnore]
    public string Name => PackageTools.BuildPackageName(this.Identity.Owner, this.Identity.Component, this.Identity.SubComponent, this.Culture, this.Resolution);
  }
}
