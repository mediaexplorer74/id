// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.PkgFile
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  public class PkgFile
  {
    public static readonly string DefaultLanguagePattern = "_Lang_";
    public static readonly string DefaultResolutionPattern = "_Res_";
    private string _ID;
    [XmlAttribute("Path")]
    public string Directory;
    [XmlAttribute("Name")]
    public string Name;
    [XmlAttribute("NoBasePackage")]
    [DefaultValue(false)]
    public bool NoBasePackage;
    [XmlAttribute("FeatureIdentifierPackage")]
    [DefaultValue(false)]
    public bool FeatureIdentifierPackage;
    [XmlAttribute("Resolution")]
    public string Resolution;
    [XmlAttribute("Language")]
    public string Language;
    [DefaultValue(null)]
    [XmlAttribute("CPUType")]
    public string CPUType;
    [XmlAttribute("Partition")]
    [DefaultValue(null)]
    public string Partition;
    private VersionInfo? _version = new VersionInfo?();
    [XmlIgnore]
    public FeatureManifest.PackageGroups FMGroup;
    [XmlIgnore]
    public OEMInput OemInput;

    public PkgFile()
    {
    }

    public PkgFile(FeatureManifest.PackageGroups fmGroup) => this.FMGroup = fmGroup;

    public PkgFile(PkgFile srcPkg) => this.CopyPkgFile(srcPkg);

    [XmlAttribute("ID")]
    public string ID
    {
      get => string.IsNullOrEmpty(this._ID) ? Path.GetFileNameWithoutExtension(this.Name) : this._ID;
      set => this._ID = value;
    }

    [XmlAttribute("Version")]
    [DefaultValue(null)]
    public string Version
    {
      get => !this._version.HasValue || string.IsNullOrEmpty(this._version.ToString()) ? (string) null : this._version.ToString();
      set
      {
        if (string.IsNullOrEmpty(value))
        {
          this._version = new VersionInfo?();
        }
        else
        {
          string[] source = value.Split('.');
          ushort[] numArray = new ushort[4];
          for (int index = 0; index < Math.Min(((IEnumerable<string>) source).Count<string>(), 4); ++index)
            numArray[index] = !string.IsNullOrEmpty(source[index]) ? ushort.Parse(source[index]) : (ushort) 0;
          if (((IEnumerable<string>) source).Count<string>() != 4)
            this._version = new VersionInfo?();
          else
            this._version = new VersionInfo?(new VersionInfo(numArray[0], numArray[1], numArray[2], numArray[3]));
        }
      }
    }

    [XmlIgnore]
    public virtual string GroupValue
    {
      get => (string) null;
      set
      {
      }
    }

    [XmlIgnore]
    public virtual bool IncludeInImage => this.OemInput != null;

    [XmlIgnore]
    public string PackagePath
    {
      get
      {
        string name = this.RawPackagePath;
        if (!string.IsNullOrEmpty(this.CPUType))
          name = name.Replace("$(cputype)", this.CPUType);
        if (this.OemInput != null)
          name = this.OemInput.ProcessOEMInputVariables(name);
        return Environment.ExpandEnvironmentVariables(name);
      }
    }

    [XmlIgnore]
    public string RawPackagePath => Path.Combine(this.Directory, this.Name);

    public string GetLanguagePackagePath(string language)
    {
      string name = this.RawLanguagePackagePath.Replace("$(langid)", language);
      if (this.OemInput != null)
        name = this.OemInput.ProcessOEMInputVariables(name);
      return Environment.ExpandEnvironmentVariables(name);
    }

    [XmlIgnore]
    public string RawLanguagePackagePath
    {
      get
      {
        string extension = Path.GetExtension(this.Name);
        return Path.Combine(this.Directory, this.Name.Replace(extension, PkgFile.DefaultLanguagePattern + "$(langid)" + extension));
      }
    }

    public string GetResolutionPackagePath(string resolution)
    {
      string name = this.RawResolutionPackagePath.Replace("$(resid)", resolution);
      if (this.OemInput != null)
        name = this.OemInput.ProcessOEMInputVariables(name);
      return Environment.ExpandEnvironmentVariables(name);
    }

    [XmlIgnore]
    public string RawResolutionPackagePath
    {
      get
      {
        string extension = Path.GetExtension(this.Name);
        return Path.Combine(this.Directory, this.Name.Replace(extension, PkgFile.DefaultResolutionPattern + "$(resid)" + extension));
      }
    }

    public void ProcessVariables()
    {
      if (this.OemInput == null)
        return;
      this.Directory = this.OemInput.ProcessOEMInputVariables(this.Directory);
    }

    public static List<string> GetSupportedList(string list)
    {
      char[] chArray = new char[1]{ ';' };
      List<string> supportedList = new List<string>();
      list = list.Trim();
      list = list.Replace("(", "");
      list = list.Replace(")", "");
      list = list.Replace("!", "");
      supportedList.AddRange((IEnumerable<string>) list.Split(chArray));
      return supportedList;
    }

    public static string GetSupportedListString(List<string> list, List<string> supportedList)
    {
      string supportedListString = (string) null;
      if (list.Count<string>() > 0)
        supportedListString = list.Except<string>((IEnumerable<string>) supportedList, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase).Count<string>() != 0 || supportedList.Except<string>((IEnumerable<string>) list, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase).Count<string>() != 0 ? "(" + string.Join(");(", (IEnumerable<string>) list) + ")" : "*";
      return supportedListString;
    }

    public static List<string> GetSupportedList(string list, List<string> supportedValues)
    {
      List<string> supportedList = new List<string>();
      list = list.Trim();
      if (list.CompareTo("*") == 0)
        return supportedValues;
      List<string> stringList = !list.Contains("*") ? PkgFile.GetSupportedList(list) : throw new FeatureAPIException("FeatureAPI!GetSupportedList: Supported values list '" + list + "' cannot include '*' unless it is alone (\"*\" to specify all supported values)");
      if (list.Contains("!"))
      {
        int num = list.IndexOf("!");
        if (list.LastIndexOf("!") != num || num != 0)
          throw new FeatureAPIException("FeatureAPI!GetSupportedList: Supported values list '" + list + "' cannot contain both include and exclude values.  Exclude lists must contain a '!' at the beginning.");
        foreach (string supportedValue in supportedValues)
        {
          bool flag = false;
          foreach (string b in stringList)
          {
            if (string.Equals(supportedValue, b, StringComparison.OrdinalIgnoreCase))
            {
              flag = true;
              break;
            }
          }
          if (!flag)
            supportedList.Add(supportedValue);
        }
      }
      else
      {
        foreach (string b in stringList)
        {
          foreach (string supportedValue in supportedValues)
          {
            if (string.Equals(supportedValue, b, StringComparison.OrdinalIgnoreCase))
            {
              supportedList.Add(supportedValue);
              break;
            }
          }
        }
      }
      return supportedList;
    }

    public void CopyPkgFile(PkgFile srcPkgFile)
    {
      this.Directory = srcPkgFile.Directory;
      this.Language = srcPkgFile.Language;
      this.Name = srcPkgFile.Name;
      this.NoBasePackage = srcPkgFile.NoBasePackage;
      this.OemInput = srcPkgFile.OemInput;
      this.Resolution = srcPkgFile.Resolution;
      this.FeatureIdentifierPackage = srcPkgFile.FeatureIdentifierPackage;
      this.CPUType = srcPkgFile.CPUType;
    }

    public virtual void InitializeWithMergeResult(
      MergeResult result,
      FeatureManifest.PackageGroups fmGroup,
      string groupValue,
      List<string> supportedLanguages,
      List<string> supportedResolutions)
    {
      this.Directory = Path.GetDirectoryName(result.FilePath);
      this.Name = Path.GetFileName(result.FilePath);
      this._ID = result.PkgInfo.Name;
      this.FeatureIdentifierPackage = result.FeatureIdentifierPackage;
      this.Partition = result.PkgInfo.Partition;
      this._version = new VersionInfo?(result.PkgInfo.Version);
      this.FMGroup = fmGroup;
      if (result.Languages != null)
        this.Language = PkgFile.GetSupportedListString(((IEnumerable<string>) result.Languages).ToList<string>(), supportedLanguages);
      if (result.Resolutions != null)
        this.Resolution = PkgFile.GetSupportedListString(((IEnumerable<string>) result.Resolutions).ToList<string>(), supportedResolutions);
      this.GroupValue = groupValue;
    }
  }
}
