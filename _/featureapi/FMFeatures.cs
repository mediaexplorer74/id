// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.FMFeatures
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  public class FMFeatures
  {
    public const string MSFeaturePrefix = "MS_";
    public const string OEMFeaturePrefix = "OEM_";
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (MSOptionalPkgFile))]
    public List<MSOptionalPkgFile> Microsoft;
    [XmlArrayItem(ElementName = "FeatureGroup", IsNullable = false, Type = typeof (FMFeatureGrouping))]
    [XmlArray]
    public List<FMFeatureGrouping> MSFeatureGroups;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (OEMOptionalPkgFile))]
    public List<OEMOptionalPkgFile> OEM;
    [XmlArray]
    [XmlArrayItem(ElementName = "FeatureGroup", IsNullable = false, Type = typeof (FMFeatureGrouping))]
    public List<FMFeatureGrouping> OEMFeatureGroups;

    public void ValidateConstraints(List<string> MSFeatures, List<string> OEMFeatures)
    {
      bool flag = true;
      StringBuilder stringBuilder = new StringBuilder();
      if (this.MSFeatureGroups != null)
      {
        foreach (FMFeatureGrouping msFeatureGroup in this.MSFeatureGroups)
        {
          string errorMessage;
          if (!msFeatureGroup.ValidateConstraints((IEnumerable<string>) MSFeatures, out errorMessage))
          {
            flag = false;
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Errors in Microsoft Features:");
            stringBuilder.AppendLine(errorMessage);
          }
        }
      }
      if (this.OEMFeatureGroups != null)
      {
        foreach (FMFeatureGrouping oemFeatureGroup in this.OEMFeatureGroups)
        {
          string errorMessage;
          if (!oemFeatureGroup.ValidateConstraints((IEnumerable<string>) OEMFeatures, out errorMessage))
          {
            flag = false;
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Errors in OEM Features:");
            stringBuilder.AppendLine(errorMessage);
          }
        }
      }
      if (!flag)
        throw new FeatureAPIException("FeatureAPI!ValidateConstraints: OEMInput file contains invalid Feature combinations:" + stringBuilder.ToString());
    }

    public void Merge(FMFeatures srcFeatures)
    {
      if (srcFeatures.MSFeatureGroups != null)
      {
        if (this.MSFeatureGroups == null)
          this.MSFeatureGroups = srcFeatures.MSFeatureGroups;
        else
          this.MSFeatureGroups.AddRange((IEnumerable<FMFeatureGrouping>) srcFeatures.MSFeatureGroups);
      }
      if (srcFeatures.Microsoft != null)
      {
        if (this.Microsoft == null)
          this.Microsoft = srcFeatures.Microsoft;
        else
          this.Microsoft.AddRange((IEnumerable<MSOptionalPkgFile>) srcFeatures.Microsoft);
      }
      if (srcFeatures.OEMFeatureGroups != null)
      {
        if (this.OEMFeatureGroups == null)
          this.OEMFeatureGroups = srcFeatures.OEMFeatureGroups;
        else
          this.OEMFeatureGroups.AddRange((IEnumerable<FMFeatureGrouping>) srcFeatures.OEMFeatureGroups);
      }
      if (srcFeatures.OEM == null)
        return;
      if (this.OEM == null)
        this.OEM = srcFeatures.OEM;
      else
        this.OEM.AddRange((IEnumerable<OEMOptionalPkgFile>) srcFeatures.OEM);
    }
  }
}
