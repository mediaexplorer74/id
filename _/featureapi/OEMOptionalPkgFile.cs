﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.OEMOptionalPkgFile
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

namespace Microsoft.WindowsPhone.FeatureAPI
{
  public class OEMOptionalPkgFile : OptionalPkgFile
  {
    public OEMOptionalPkgFile()
      : base(FeatureManifest.PackageGroups.OEMFEATURE)
    {
    }

    public OEMOptionalPkgFile(OptionalPkgFile srcPkg)
      : base(srcPkg)
    {
      this.FMGroup = FeatureManifest.PackageGroups.OEMFEATURE;
    }
  }
}
