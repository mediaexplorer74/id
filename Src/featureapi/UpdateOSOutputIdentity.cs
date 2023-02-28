// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.UpdateOSOutputIdentity
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using System;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  public class UpdateOSOutputIdentity
  {
    public string Owner;
    public string Component;
    public string SubComponent;
    [CLSCompliant(false)]
    public PkgVersion Version;

    public override string ToString()
    {
      string str = this.Owner + "." + this.Component;
      if (!string.IsNullOrEmpty(this.SubComponent))
        str = str + "." + this.SubComponent;
      return str;
    }
  }
}
