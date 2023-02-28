// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.OptionalPkgFile
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  public class OptionalPkgFile : PkgFile
  {
    [XmlArray]
    [XmlArrayItem(ElementName = "FeatureID", IsNullable = false, Type = typeof (string))]
    public List<string> FeatureIDs;

    public OptionalPkgFile(FeatureManifest.PackageGroups fmGroup)
      : base(fmGroup)
    {
    }

    public OptionalPkgFile(OptionalPkgFile srcPkg)
      : base((PkgFile) srcPkg)
    {
      this.FMGroup = srcPkg.FMGroup;
      this.FeatureIDs = srcPkg.FeatureIDs;
    }

    [XmlIgnore]
    public override string GroupValue
    {
      get => string.Join(";", this.FeatureIDs.ToArray());
      set => this.FeatureIDs = ((IEnumerable<string>) value.Split(new char[1]
      {
        ';'
      }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>();
    }
  }
}
