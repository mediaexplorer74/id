// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.FMFeatureGrouping
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  public class FMFeatureGrouping
  {
    private StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;
    [XmlAttribute("Name")]
    public string Name;
    [DefaultValue(false)]
    [XmlAttribute("PublishingFeatureGroup")]
    public bool PublishingFeatureGroup;
    private string _fmID;
    [DefaultValue(FMFeatureGrouping.FeatureConstraints.None)]
    [XmlAttribute("Constraint")]
    public FMFeatureGrouping.FeatureConstraints Constraint;
    [XmlArrayItem(ElementName = "FeatureID", IsNullable = false, Type = typeof (string))]
    [XmlArray]
    public List<string> FeatureIDs;
    [XmlArray]
    [XmlArrayItem(ElementName = "FeatureGroup", IsNullable = false, Type = typeof (FMFeatureGrouping))]
    public List<FMFeatureGrouping> SubGroups;

    [XmlAttribute]
    [DefaultValue(null)]
    public string FMID
    {
      get => this._fmID;
      set
      {
        this._fmID = value;
        if (this.SubGroups == null)
          return;
        foreach (FMFeatureGrouping subGroup in this.SubGroups)
          subGroup.FMID = value;
      }
    }

    [XmlIgnore]
    public List<string> FeatureIDWithFMIDs
    {
      get
      {
        List<string> featureIdWithFmiDs = new List<string>();
        foreach (string featureId in this.FeatureIDs)
          featureIdWithFmiDs.Add(FeatureManifest.GetFeatureIDWithFMID(featureId, this.FMID));
        return featureIdWithFmiDs;
      }
    }

    [XmlIgnore]
    public List<string> AllFeatureIDWithFMIDs
    {
      get
      {
        List<string> featureIdWithFmiDs = new List<string>();
        if (this.FeatureIDs != null)
          featureIdWithFmiDs.AddRange((IEnumerable<string>) this.FeatureIDWithFMIDs);
        if (this.SubGroups != null)
        {
          foreach (FMFeatureGrouping subGroup in this.SubGroups)
            featureIdWithFmiDs.AddRange((IEnumerable<string>) subGroup.AllFeatureIDWithFMIDs);
        }
        return featureIdWithFmiDs;
      }
    }

    [XmlIgnore]
    public List<string> AllFeatureIDs
    {
      get
      {
        List<string> allFeatureIds = new List<string>();
        if (this.FeatureIDs != null)
          allFeatureIds.AddRange((IEnumerable<string>) this.FeatureIDs);
        if (this.SubGroups != null)
        {
          foreach (FMFeatureGrouping subGroup in this.SubGroups)
            allFeatureIds.AddRange((IEnumerable<string>) subGroup.AllFeatureIDs);
        }
        return allFeatureIds;
      }
    }

    public bool ValidateConstraints(IEnumerable<string> FeatureIDs, out string errorMessage)
    {
      bool flag = true;
      StringBuilder stringBuilder = new StringBuilder();
      List<string> list = FeatureIDs.Intersect<string>((IEnumerable<string>) this.AllFeatureIDs, (IEqualityComparer<string>) this.IgnoreCase).ToList<string>();
      int num = list.Count<string>();
      switch (this.Constraint)
      {
        case FMFeatureGrouping.FeatureConstraints.OneOrMore:
          if (num == 0)
          {
            flag = false;
            stringBuilder.AppendLine("One or more of the following features must be selected:");
            using (List<string>.Enumerator enumerator = this.AllFeatureIDs.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                string current = enumerator.Current;
                stringBuilder.AppendFormat("\t{0}\n", (object) current);
              }
              break;
            }
          }
          else
            break;
        case FMFeatureGrouping.FeatureConstraints.ZeroOrOne:
          if (num > 1)
          {
            flag = false;
            stringBuilder.AppendLine("Only one (or none) of the following features may be selected:");
            using (List<string>.Enumerator enumerator = list.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                string current = enumerator.Current;
                stringBuilder.AppendFormat("\t{0}\n", (object) current);
              }
              break;
            }
          }
          else
            break;
        case FMFeatureGrouping.FeatureConstraints.OneAndOnlyOne:
          if (num != 1)
          {
            flag = false;
            if (num == 0)
            {
              stringBuilder.AppendLine("One of the following features must be selected:");
              using (List<string>.Enumerator enumerator = this.AllFeatureIDs.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  string current = enumerator.Current;
                  stringBuilder.AppendFormat("\t{0}\n", (object) current);
                }
                break;
              }
            }
            else
            {
              stringBuilder.AppendLine("Only one of the following features may be selected:");
              using (List<string>.Enumerator enumerator = list.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  string current = enumerator.Current;
                  stringBuilder.AppendFormat("\t{0}\n", (object) current);
                }
                break;
              }
            }
          }
          else
            break;
      }
      errorMessage = stringBuilder.ToString();
      return flag;
    }

    public override string ToString()
    {
      string str = "";
      if (!string.IsNullOrEmpty(this.Name))
        str = this.Name + " ";
      return str + "(Constraint= " + (object) this.Constraint + ")";
    }

    public enum FeatureConstraints
    {
      None,
      OneOrMore,
      ZeroOrOne,
      OneAndOnlyOne,
    }
  }
}
