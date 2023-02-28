// FeatureManifest.cs
// Type: Microsoft.WindowsPhone.FeatureAPI.FeatureManifest
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  [XmlRoot(ElementName = "FeatureManifest", IsNullable = false, Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  [XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  public class FeatureManifest
  {
    public const string BuildType_FRE = "fre";
    public const string BuildType_CHK = "chk";
    public const string Prerelease_Protected = "protected";
    public const string Prerelease_Protected_Replacement = "replacement";
    public static readonly string CPUType_ARM = ((object) (CpuId) 2).ToString();
    public static readonly string CPUType_X86 = ((object) (CpuId) 1).ToString();
    private static StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (PkgFile))]
    public List<PkgFile> BasePackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (PkgFile))]
    public List<PkgFile> CPUPackages;
    public BootUIPkgFile BootUILanguagePackageFile;
    public BootLocalePkgFile BootLocalePackageFile;
    public FMFeatures Features;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (ReleasePkgFile))]
    [XmlArray]
    public List<ReleasePkgFile> ReleasePackages;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (PrereleasePkgFile))]
    [XmlArray]
    public List<PrereleasePkgFile> PrereleasePackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (OEMDevicePkgFile))]
    public List<OEMDevicePkgFile> OEMDevicePlatformPackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (DeviceLayoutPkgFile))]
    public List<DeviceLayoutPkgFile> DeviceLayoutPackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (SOCPkgFile))]
    public List<SOCPkgFile> SOCPackages;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (SVPkgFile))]
    [XmlArray]
    public List<SVPkgFile> SVPackages;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (DevicePkgFile))]
    [XmlArray]
    public List<DevicePkgFile> DeviceSpecificPackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (SpeechPkgFile))]
    public List<SpeechPkgFile> SpeechPackages;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (KeyboardPkgFile))]
    [XmlArray]
    public List<KeyboardPkgFile> KeyboardPackages;
    private OEMInput _oemInput;
    [XmlIgnore]
    public string SourceFile;
    [XmlIgnore]
    public bool IsFromOldFormat;

    public FeatureManifest()
    {
    }

    public FeatureManifest(FeatureManifest srcFM)
    {
      if (srcFM == null)
        return;
      if (srcFM.BasePackages != null)
        this.BasePackages = new List<PkgFile>((IEnumerable<PkgFile>) srcFM.BasePackages);
      if (srcFM.BootLocalePackageFile != null)
        this.BootLocalePackageFile = srcFM.BootLocalePackageFile;
      if (srcFM.BootUILanguagePackageFile != null)
        this.BootUILanguagePackageFile = srcFM.BootUILanguagePackageFile;
      if (srcFM.CPUPackages != null)
        this.CPUPackages = new List<PkgFile>((IEnumerable<PkgFile>) srcFM.CPUPackages);
      if (srcFM.DeviceLayoutPackages != null)
        this.DeviceLayoutPackages = new List<DeviceLayoutPkgFile>((IEnumerable<DeviceLayoutPkgFile>) srcFM.DeviceLayoutPackages);
      if (srcFM.DeviceSpecificPackages != null)
        this.DeviceSpecificPackages = new List<DevicePkgFile>((IEnumerable<DevicePkgFile>) srcFM.DeviceSpecificPackages);
      if (srcFM.Features != null)
      {
        this.Features = new FMFeatures();
        if (srcFM.Features.MSFeatureGroups != null)
          this.Features.MSFeatureGroups = new List<FMFeatureGrouping>((IEnumerable<FMFeatureGrouping>) srcFM.Features.MSFeatureGroups);
        if (srcFM.Features.Microsoft != null)
          this.Features.Microsoft = new List<MSOptionalPkgFile>((IEnumerable<MSOptionalPkgFile>) srcFM.Features.Microsoft);
        if (srcFM.Features.OEM != null)
          this.Features.OEM = new List<OEMOptionalPkgFile>((IEnumerable<OEMOptionalPkgFile>) srcFM.Features.OEM);
        if (srcFM.Features.OEMFeatureGroups != null)
          this.Features.OEMFeatureGroups = new List<FMFeatureGrouping>((IEnumerable<FMFeatureGrouping>) srcFM.Features.OEMFeatureGroups);
      }
      this.IsFromOldFormat = srcFM.IsFromOldFormat;
      if (srcFM.KeyboardPackages != null)
        this.KeyboardPackages = new List<KeyboardPkgFile>((IEnumerable<KeyboardPkgFile>) srcFM.KeyboardPackages);
      if (srcFM.OEMDevicePlatformPackages != null)
        this.OEMDevicePlatformPackages = new List<OEMDevicePkgFile>((IEnumerable<OEMDevicePkgFile>) srcFM.OEMDevicePlatformPackages);
      if (srcFM.PrereleasePackages != null)
        this.PrereleasePackages = new List<PrereleasePkgFile>((IEnumerable<PrereleasePkgFile>) srcFM.PrereleasePackages);
      if (srcFM.ReleasePackages != null)
        this.ReleasePackages = new List<ReleasePkgFile>((IEnumerable<ReleasePkgFile>) srcFM.ReleasePackages);
      if (srcFM.SOCPackages != null)
        this.SOCPackages = new List<SOCPkgFile>((IEnumerable<SOCPkgFile>) srcFM.SOCPackages);
      if (srcFM.SpeechPackages != null)
        this.SpeechPackages = new List<SpeechPkgFile>((IEnumerable<SpeechPkgFile>) srcFM.SpeechPackages);
      if (srcFM.SVPackages != null)
        this.SVPackages = new List<SVPkgFile>((IEnumerable<SVPkgFile>) srcFM.SVPackages);
      this.SourceFile = srcFM.SourceFile;
      this.OemInput = srcFM.OemInput;
    }

    public bool ShouldSerializeCPUPackages() => false;

    private IEnumerable<PkgFile> _allPackages
    {
      get
      {
        List<PkgFile> allPackages = new List<PkgFile>();
        if (this.BootUILanguagePackageFile != null)
          allPackages.Add((PkgFile) this.BootUILanguagePackageFile);
        if (this.BootLocalePackageFile != null)
          allPackages.Add((PkgFile) this.BootLocalePackageFile);
        if (this.BasePackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.BasePackages);
        if (this.Features != null)
        {
          if (this.Features.Microsoft != null)
            allPackages.AddRange((IEnumerable<PkgFile>) this.Features.Microsoft);
          if (this.Features.OEM != null)
            allPackages.AddRange((IEnumerable<PkgFile>) this.Features.OEM);
        }
        if (this.SVPackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.SVPackages);
        if (this.SOCPackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.SOCPackages);
        if (this.OEMDevicePlatformPackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.OEMDevicePlatformPackages);
        if (this.DeviceLayoutPackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.DeviceLayoutPackages);
        if (this.DeviceSpecificPackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.DeviceSpecificPackages);
        if (this.ReleasePackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.ReleasePackages);
        if (this.PrereleasePackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.PrereleasePackages);
        if (this.KeyboardPackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.KeyboardPackages);
        if (this.SpeechPackages != null)
          allPackages.AddRange((IEnumerable<PkgFile>) this.SpeechPackages);
        return (IEnumerable<PkgFile>) allPackages;
      }
    }

    [XmlIgnore]
    public OEMInput OemInput
    {
      get => this._oemInput;
      set
      {
        this._oemInput = value;
        foreach (PkgFile allPackage in this._allPackages)
          allPackage.OemInput = this._oemInput;
      }
    }

    public List<string> GetFeatureIDs(bool fMSFeatures, bool fOEMFeatures)
    {
      List<string> source = new List<string>();
      if (this.Features != null)
      {
        if (this.Features.Microsoft != null && fMSFeatures)
        {
          foreach (OptionalPkgFile optionalPkgFile in this.Features.Microsoft)
            source.AddRange((IEnumerable<string>) optionalPkgFile.FeatureIDs);
        }
        if (this.Features.OEM != null && fOEMFeatures)
        {
          foreach (OptionalPkgFile optionalPkgFile in this.Features.OEM)
            source.AddRange((IEnumerable<string>) optionalPkgFile.FeatureIDs);
        }
      }
      return source.Distinct<string>((IEqualityComparer<string>) FeatureManifest.IgnoreCase).ToList<string>();
    }

    private List<FeatureManifest.FMPkgInfo> GetPackagesFromList(
      PkgFile pkg,
      List<string> listValues)
    {
      List<FeatureManifest.FMPkgInfo> packagesFromList = new List<FeatureManifest.FMPkgInfo>();
      foreach (string listValue in listValues)
      {
        FeatureManifest.FMPkgInfo fmPkgInfo = new FeatureManifest.FMPkgInfo(pkg);
        switch (pkg.FMGroup)
        {
          case FeatureManifest.PackageGroups.BOOTUI:
            fmPkgInfo.PackagePath = pkg.PackagePath.Replace("$(bootuilanguage)", listValue);
            fmPkgInfo.ID = this.BootUILanguagePackageFile.ID.Replace("$(bootuilanguage)", listValue);
            break;
          case FeatureManifest.PackageGroups.BOOTLOCALE:
            fmPkgInfo.PackagePath = this.BootLocalePackageFile.PackagePath.Replace("$(bootlocale)", listValue);
            fmPkgInfo.ID = this.BootLocalePackageFile.ID.Replace("$(bootlocale)", listValue);
            break;
          case FeatureManifest.PackageGroups.KEYBOARD:
          case FeatureManifest.PackageGroups.SPEECH:
            string packagePath = pkg.PackagePath;
            string extension = Path.GetExtension(packagePath);
            string str = packagePath.Replace(extension, PkgFile.DefaultLanguagePattern + listValue + extension);
            fmPkgInfo.PackagePath = str;
            break;
        }
        fmPkgInfo.GroupValue = listValue;
        packagesFromList.Add(fmPkgInfo);
      }
      return packagesFromList;
    }

    private List<FeatureManifest.FMPkgInfo> GetSatellites(
      PkgFile pkg,
      List<string> supportedUILanguages,
      List<string> supportedResolutions,
      string groupValue = null)
    {
      List<FeatureManifest.FMPkgInfo> satellites = new List<FeatureManifest.FMPkgInfo>();
      string str = groupValue ?? pkg.GroupValue;
      if (!string.IsNullOrEmpty(pkg.Language) && supportedUILanguages.Count != 0)
      {
        foreach (string supported in PkgFile.GetSupportedList(pkg.Language, supportedUILanguages))
          satellites.Add(new FeatureManifest.FMPkgInfo(pkg)
          {
            GroupValue = str,
            Language = supported,
            PackagePath = pkg.GetLanguagePackagePath(supported),
            RawBasePath = pkg.RawPackagePath,
            ID = pkg.ID + PkgFile.DefaultLanguagePattern + supported,
            FeatureIdentifierPackage = false
          });
      }
      if (!string.IsNullOrEmpty(pkg.Resolution) && supportedResolutions.Count != 0)
      {
        foreach (string supported in PkgFile.GetSupportedList(pkg.Resolution, supportedResolutions))
          satellites.Add(new FeatureManifest.FMPkgInfo(pkg)
          {
            GroupValue = str,
            Resolution = supported,
            PackagePath = pkg.GetResolutionPackagePath(supported),
            RawBasePath = pkg.RawPackagePath,
            ID = pkg.ID + PkgFile.DefaultResolutionPattern + supported,
            FeatureIdentifierPackage = false
          });
      }
      return satellites;
    }

    public List<string> GetPRSPackages(
      List<string> supportedUILanguages,
      List<string> supportedLocales,
      List<string> supportedResolutions,
      string buildType,
      string cpuType,
      string msPackageRoot)
    {
      List<FeatureManifest.FMPkgInfo> packages = new List<FeatureManifest.FMPkgInfo>();
      this.GetAllPackageByGroups(ref packages, supportedUILanguages, supportedLocales, supportedResolutions, buildType, cpuType, msPackageRoot);
      return packages.Where<FeatureManifest.FMPkgInfo>((Func<FeatureManifest.FMPkgInfo, bool>) (pkg => (!pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.RELEASE) || !string.Equals(pkg.GroupValue, "Test", StringComparison.OrdinalIgnoreCase)) && !pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.OEMFEATURE))).Select<FeatureManifest.FMPkgInfo, string>((Func<FeatureManifest.FMPkgInfo, string>) (pkg => pkg.PackagePath)).Distinct<string>((IEqualityComparer<string>) FeatureManifest.IgnoreCase).ToList<string>();
    }

    public void GetAllPackageByGroups(
      ref List<FeatureManifest.FMPkgInfo> packages,
      List<string> supportedUILanguages,
      List<string> supportedLocales,
      List<string> supportedResolutions,
      string buildType,
      string cpuType,
      string msPackageRoot)
    {
      if (string.IsNullOrEmpty(buildType))
        buildType = Environment.GetEnvironmentVariable("_BUILDTYPE");
      if (string.IsNullOrEmpty(cpuType))
        cpuType = Environment.GetEnvironmentVariable("_BUILDARCH");
      foreach (PkgFile allPackage in this._allPackages)
      {
        if (string.IsNullOrEmpty(cpuType) || string.IsNullOrEmpty(allPackage.CPUType) || string.Equals(allPackage.CPUType, cpuType, StringComparison.OrdinalIgnoreCase))
        {
          switch (allPackage.FMGroup)
          {
            case FeatureManifest.PackageGroups.BOOTUI:
              packages.AddRange((IEnumerable<FeatureManifest.FMPkgInfo>) this.GetPackagesFromList(allPackage, supportedUILanguages));
              continue;
            case FeatureManifest.PackageGroups.BOOTLOCALE:
              packages.AddRange((IEnumerable<FeatureManifest.FMPkgInfo>) this.GetPackagesFromList(allPackage, supportedLocales));
              continue;
            case FeatureManifest.PackageGroups.MSFEATURE:
            case FeatureManifest.PackageGroups.OEMFEATURE:
              using (List<string>.Enumerator enumerator = (allPackage as OptionalPkgFile).FeatureIDs.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  string current = enumerator.Current;
                  packages.Add(new FeatureManifest.FMPkgInfo(allPackage)
                  {
                    GroupValue = current
                  });
                  packages.AddRange((IEnumerable<FeatureManifest.FMPkgInfo>) this.GetSatellites(allPackage, supportedUILanguages, supportedResolutions, current));
                }
                continue;
              }
            case FeatureManifest.PackageGroups.KEYBOARD:
            case FeatureManifest.PackageGroups.SPEECH:
              List<string> supportedList = PkgFile.GetSupportedList(allPackage.Language);
              packages.AddRange((IEnumerable<FeatureManifest.FMPkgInfo>) this.GetPackagesFromList(allPackage, supportedList));
              continue;
            default:
              FeatureManifest.FMPkgInfo fmPkgInfo = new FeatureManifest.FMPkgInfo(allPackage);
              packages.Add(fmPkgInfo);
              packages.AddRange((IEnumerable<FeatureManifest.FMPkgInfo>) this.GetSatellites(allPackage, supportedUILanguages, supportedResolutions));
              continue;
          }
        }
      }
      this.ProcessVariablesForList(ref packages, buildType, cpuType, msPackageRoot);
    }

    public List<string> GetUILangFeatures(List<string> packages)
    {
      List<string> uiLangFeatures = new List<string>();
      PkgFile pkgFile = this._allPackages.First<PkgFile>((Func<PkgFile, bool>) (pkg => pkg.Language != null && pkg.Language.Equals("*")));
      if (pkgFile != null)
        uiLangFeatures = this.GetValuesForPackagesMatchingPattern(pkgFile.ID, packages, PkgFile.DefaultLanguagePattern).ToList<string>();
      return uiLangFeatures;
    }

    public List<string> GetResolutionFeatures(List<string> packages)
    {
      List<string> resolutionFeatures = new List<string>();
      PkgFile pkgFile = this._allPackages.First<PkgFile>((Func<PkgFile, bool>) (pkg => pkg.Resolution != null && pkg.Resolution.Equals("*")));
      if (pkgFile != null)
        resolutionFeatures = this.GetValuesForPackagesMatchingPattern(pkgFile.ID, packages, PkgFile.DefaultResolutionPattern).ToList<string>();
      return resolutionFeatures;
    }

    private List<string> GetValuesForPackagesMatchingPattern(
      string pattern,
      List<string> packages,
      string satelliteStr)
    {
      List<string> packagesMatchingPattern = new List<string>();
      string str = pattern.ToUpper().Replace(PkgConstants.c_strPackageExtension.ToUpper(), "");
      if (!string.IsNullOrEmpty(satelliteStr) && !str.EndsWith(satelliteStr, StringComparison.OrdinalIgnoreCase))
        str += satelliteStr;
      foreach (string package in packages)
      {
        if (package.StartsWith(str, StringComparison.OrdinalIgnoreCase))
          packagesMatchingPattern.Add(package.Substring(str.Length));
      }
      return packagesMatchingPattern;
    }

    public List<FeatureManifest.FMPkgInfo> GetFeatureIdentifierPackages()
    {
      List<FeatureManifest.FMPkgInfo> packages = new List<FeatureManifest.FMPkgInfo>();
      if (this.IsFromOldFormat)
        return packages;
      List<string> supportedUILanguages = new List<string>();
      List<string> supportedLocales = new List<string>();
      List<string> supportedResolutions = new List<string>();
      string cpuType = "";
      string buildType = "";
      string msPackageRoot = "";
      this.GetAllPackageByGroups(ref packages, supportedUILanguages, supportedLocales, supportedResolutions, buildType, cpuType, msPackageRoot);
      return packages.Where<FeatureManifest.FMPkgInfo>((Func<FeatureManifest.FMPkgInfo, bool>) (pkg => pkg.FeatureIdentifierPackage)).ToList<FeatureManifest.FMPkgInfo>();
    }

    private void ProcessVariablesForList(
      ref List<FeatureManifest.FMPkgInfo> packageList,
      string buildType,
      string cpuType,
      string msPackageRoot)
    {
      for (int index = 0; index < packageList.Count; ++index)
      {
        FeatureManifest.FMPkgInfo fmPkgInfo = packageList[index];
        string name = fmPkgInfo.PackagePath;
        if (!string.IsNullOrEmpty(buildType))
          name = name.Replace("$(buildtype)", buildType);
        if (!string.IsNullOrEmpty(cpuType))
          name = name.Replace("$(cputype)", cpuType);
        if (!string.IsNullOrEmpty(msPackageRoot))
          name = name.Replace("$(mspackageroot)", msPackageRoot);
        fmPkgInfo.PackagePath = Environment.ExpandEnvironmentVariables(name);
        packageList[index] = fmPkgInfo;
      }
    }

    public List<string> GetAllPackageFileList(
      List<string> supportedUILanguages,
      List<string> supportedResolutions,
      List<string> supportedLocales,
      string buildType,
      string cpuType,
      string msPackageRoot)
    {
      List<string> source = new List<string>();
      List<FeatureManifest.FMPkgInfo> packages = new List<FeatureManifest.FMPkgInfo>();
      if (string.IsNullOrEmpty(buildType))
      {
        buildType = Environment.GetEnvironmentVariable("_BUILDTYPE");
        if (string.IsNullOrEmpty(buildType))
          buildType = "fre";
      }
      if (string.IsNullOrEmpty(cpuType))
      {
        cpuType = Environment.GetEnvironmentVariable("_BUILDARCH");
        if (string.IsNullOrEmpty(cpuType))
          cpuType = FeatureManifest.CPUType_ARM;
      }
      this.GetAllPackageByGroups(ref packages, supportedUILanguages, supportedLocales, supportedResolutions, buildType, cpuType, msPackageRoot);
      source.AddRange(packages.Select<FeatureManifest.FMPkgInfo, string>((Func<FeatureManifest.FMPkgInfo, string>) (pkg => pkg.PackagePath)));
      return source.Distinct<string>((IEqualityComparer<string>) FeatureManifest.IgnoreCase).ToList<string>();
    }

    public List<string> GetPackageFileList()
    {
      List<string> source = new List<string>();
      List<FeatureManifest.FMPkgInfo> fmPkgInfoList = new List<FeatureManifest.FMPkgInfo>();
      List<FeatureManifest.FMPkgInfo> packagesByGroups = this.GetFilteredPackagesByGroups();
      source.AddRange(packagesByGroups.Select<FeatureManifest.FMPkgInfo, string>((Func<FeatureManifest.FMPkgInfo, string>) (pkg => pkg.PackagePath)));
      return source.Distinct<string>((IEqualityComparer<string>) FeatureManifest.IgnoreCase).ToList<string>();
    }

    public List<FeatureManifest.FMPkgInfo> GetFilteredPackagesByGroups()
    {
      List<FeatureManifest.FMPkgInfo> packages = new List<FeatureManifest.FMPkgInfo>();
      List<string> supportedLocales = new List<string>();
      char[] separators = new char[1]{ ';' };
      if (this._oemInput == null)
        throw new FeatureAPIException("FeatureAPI!GetFilteredPackagesByGroups: The OEMInput reference cannot be null.  Set the OEMInput before calling this function.");
      if (this.Features != null)
        this.Features.ValidateConstraints(this._oemInput.MSFeatureIDs, this._oemInput.OEMFeatureIDs);
      supportedLocales.Add(this._oemInput.BootLocale);
      this.GetAllPackageByGroups(ref packages, this._oemInput.SupportedLanguages.UserInterface, supportedLocales, this._oemInput.Resolutions, this._oemInput.BuildType, this._oemInput.CPUType, this._oemInput.MSPackageRoot);
      return packages.Where<FeatureManifest.FMPkgInfo>((Func<FeatureManifest.FMPkgInfo, bool>) (pkg =>
      {
        if (pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.BASE) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.RELEASE) && string.Equals(pkg.GroupValue, this._oemInput.ReleaseType, StringComparison.OrdinalIgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.PRERELEASE) && this._oemInput.ExcludePrereleaseFeatures && string.Equals(pkg.GroupValue, "replacement", StringComparison.OrdinalIgnoreCase) || !this._oemInput.ExcludePrereleaseFeatures && string.Equals(pkg.GroupValue, "protected", StringComparison.OrdinalIgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.SV) && string.Equals(pkg.GroupValue, this._oemInput.SV, StringComparison.OrdinalIgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.SOC) && string.Equals(pkg.GroupValue, this._oemInput.SOC, StringComparison.OrdinalIgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.DEVICE) && string.Equals(pkg.GroupValue, this._oemInput.Device, StringComparison.OrdinalIgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.DEVICELAYOUT) && string.Equals(pkg.GroupValue, this._oemInput.SOC, StringComparison.OrdinalIgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.OEMDEVICEPLATFORM) && string.Equals(pkg.GroupValue, this._oemInput.Device, StringComparison.OrdinalIgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.BOOTLOCALE) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.BOOTUI) && string.Equals(pkg.GroupValue, this._oemInput.BootUILanguage, StringComparison.OrdinalIgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.KEYBOARD) && this._oemInput.SupportedLanguages.Keyboard.Contains<string>(pkg.GroupValue, (IEqualityComparer<string>) FeatureManifest.IgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.SPEECH) && this._oemInput.SupportedLanguages.Speech.Contains<string>(pkg.GroupValue, (IEqualityComparer<string>) FeatureManifest.IgnoreCase) || pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.MSFEATURE) && this._oemInput.Features.Microsoft != null && this._oemInput.Features.Microsoft.Intersect<string>((IEnumerable<string>) pkg.GroupValue.Split(separators, StringSplitOptions.RemoveEmptyEntries), (IEqualityComparer<string>) FeatureManifest.IgnoreCase).Count<string>() > 0)
          return true;
        return pkg.FMGroup.Equals((object) FeatureManifest.PackageGroups.OEMFEATURE) && this._oemInput.Features.OEM != null && this._oemInput.Features.OEM.Intersect<string>((IEnumerable<string>) pkg.GroupValue.Split(separators, StringSplitOptions.RemoveEmptyEntries), (IEqualityComparer<string>) FeatureManifest.IgnoreCase).Count<string>() > 0;
      })).ToList<FeatureManifest.FMPkgInfo>();
    }

    private void PopulateFromOldStyleSKU(MicrosoftPhoneSKU sourceSku, bool bOEMSKU = false)
    {
      this.IsFromOldFormat = true;
      this.BootUILanguagePackageFile = sourceSku.BootUILanguagePackageFile;
      this.BootLocalePackageFile = sourceSku.BootLocalePackageFile;
      this.BasePackages = sourceSku.BasePackages;
      this.ReleasePackages = sourceSku.ReleasePackages;
      this.PrereleasePackages = sourceSku.PrereleasePackages;
      this.SVPackages = sourceSku.SVPackages;
      this.SOCPackages = sourceSku.SOCPackages;
      this.DeviceLayoutPackages = sourceSku.DeviceLayoutPackages;
      this.OEMDevicePlatformPackages = sourceSku.OEMDevicePlatformPackages;
      this.DeviceSpecificPackages = sourceSku.DeviceSpecificPackages;
      this.SpeechPackages = sourceSku.SpeechPackages;
      this.KeyboardPackages = sourceSku.KeyboardPackages;
      if (this.BasePackages == null)
        this.BasePackages = sourceSku.CPUPackages;
      else
        this.BasePackages.AddRange((IEnumerable<PkgFile>) sourceSku.CPUPackages);
      this.Features = new FMFeatures();
      if (bOEMSKU)
      {
        this.Features.OEM = new List<OEMOptionalPkgFile>();
        if (sourceSku.InternalOptionalFeatures != null)
        {
          foreach (OptionalPkgFile internalOptionalFeature in sourceSku.InternalOptionalFeatures)
            this.Features.OEM.Add(new OEMOptionalPkgFile(internalOptionalFeature));
        }
        if (sourceSku.ProductionOptionalFeatures == null)
          return;
        this.Features.OEM.AddRange((IEnumerable<OEMOptionalPkgFile>) sourceSku.ProductionOptionalFeatures);
      }
      else
      {
        this.Features.Microsoft = new List<MSOptionalPkgFile>();
        if (sourceSku.MSOptionalFeatures != null)
          this.Features.Microsoft.AddRange((IEnumerable<MSOptionalPkgFile>) sourceSku.MSOptionalFeatures);
        if (sourceSku.InternalOptionalFeatures == null)
          return;
        this.Features.Microsoft.AddRange((IEnumerable<MSOptionalPkgFile>) sourceSku.InternalOptionalFeatures);
      }
    }

    public void Merge(FeatureManifest sourceFM)
    {
      if (sourceFM.BootUILanguagePackageFile != null)
        this.BootUILanguagePackageFile = this.BootUILanguagePackageFile == null ? sourceFM.BootUILanguagePackageFile : throw new FeatureAPIException("FeatureAPI!Merge: The source FM and the destination FM cannot both contain BootUILanguagePackageFile. Cannot merge.");
      if (sourceFM.BootLocalePackageFile != null)
        this.BootLocalePackageFile = this.BootLocalePackageFile == null ? sourceFM.BootLocalePackageFile : throw new FeatureAPIException("FeatureAPI!Merge: The source FM and the destination FM cannot both contain BootUILanguagePackageFile. Cannot merge.");
      if (sourceFM.BasePackages != null)
      {
        if (this.BasePackages == null)
          this.BasePackages = sourceFM.BasePackages;
        else
          this.BasePackages.AddRange((IEnumerable<PkgFile>) sourceFM.BasePackages);
      }
      if (sourceFM.ReleasePackages != null)
      {
        if (this.ReleasePackages == null)
          this.ReleasePackages = sourceFM.ReleasePackages;
        else
          this.ReleasePackages.AddRange((IEnumerable<ReleasePkgFile>) sourceFM.ReleasePackages);
      }
      if (sourceFM != null)
      {
        if (this.PrereleasePackages == null)
          this.PrereleasePackages = sourceFM.PrereleasePackages;
        else
          this.PrereleasePackages.AddRange((IEnumerable<PrereleasePkgFile>) sourceFM.PrereleasePackages);
      }
      if (sourceFM.SVPackages != null)
      {
        if (this.SVPackages == null)
          this.SVPackages = sourceFM.SVPackages;
        else
          this.SVPackages.AddRange((IEnumerable<SVPkgFile>) sourceFM.SVPackages);
      }
      if (sourceFM.SOCPackages != null)
      {
        if (this.SOCPackages == null)
          this.SOCPackages = sourceFM.SOCPackages;
        else
          this.SOCPackages.AddRange((IEnumerable<SOCPkgFile>) sourceFM.SOCPackages);
      }
      if (sourceFM.DeviceLayoutPackages != null)
      {
        if (this.DeviceLayoutPackages == null)
          this.DeviceLayoutPackages = sourceFM.DeviceLayoutPackages;
        else
          this.DeviceLayoutPackages.AddRange((IEnumerable<DeviceLayoutPkgFile>) sourceFM.DeviceLayoutPackages);
      }
      if (sourceFM.DeviceSpecificPackages != null)
      {
        if (this.DeviceSpecificPackages == null)
          this.DeviceSpecificPackages = sourceFM.DeviceSpecificPackages;
        else
          this.DeviceSpecificPackages.AddRange((IEnumerable<DevicePkgFile>) sourceFM.DeviceSpecificPackages);
      }
      if (sourceFM.OEMDevicePlatformPackages != null)
      {
        if (this.OEMDevicePlatformPackages == null)
          this.OEMDevicePlatformPackages = sourceFM.OEMDevicePlatformPackages;
        else
          this.OEMDevicePlatformPackages.AddRange((IEnumerable<OEMDevicePkgFile>) sourceFM.OEMDevicePlatformPackages);
      }
      if (sourceFM.Features != null)
      {
        if (this.Features == null)
          this.Features = sourceFM.Features;
        else
          this.Features.Merge(sourceFM.Features);
      }
      if (sourceFM.SpeechPackages != null)
      {
        if (this.SpeechPackages == null)
          this.SpeechPackages = sourceFM.SpeechPackages;
        else
          this.SpeechPackages.AddRange((IEnumerable<SpeechPkgFile>) sourceFM.SpeechPackages);
      }
      if (sourceFM.KeyboardPackages == null)
        return;
      if (this.KeyboardPackages == null)
        this.KeyboardPackages = sourceFM.KeyboardPackages;
      else
        this.KeyboardPackages.AddRange((IEnumerable<KeyboardPkgFile>) sourceFM.KeyboardPackages);
    }

    public void WriteToFile(string fileName)
    {
      TextWriter textWriter = (TextWriter) new StreamWriter(fileName);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (FeatureManifest));
      try
      {
        xmlSerializer.Serialize(textWriter, (object) this);
      }
      catch (Exception ex)
      {
        throw new FeatureAPIException("FeatureAPI!WriteToFile: Unable to write Feature Manifest XML file '" + fileName + "'", ex);
      }
      finally
      {
        textWriter.Close();
      }
    }

    public string GetOEMDevicePlatformPackage(string device)
    {
      string devicePlatformPackage1 = (string) null;
      if (this.OEMDevicePlatformPackages != null)
      {
        foreach (DevicePkgFile devicePlatformPackage2 in this.OEMDevicePlatformPackages)
        {
          if (devicePlatformPackage2.Device.Equals(device, StringComparison.OrdinalIgnoreCase))
          {
            devicePlatformPackage1 = devicePlatformPackage2.PackagePath;
            break;
          }
        }
      }
      if (string.IsNullOrEmpty(devicePlatformPackage1) && this.DeviceSpecificPackages != null)
      {
        DevicePkgFile devicePkgFile = this.DeviceSpecificPackages.Find((Predicate<DevicePkgFile>) (pkg => pkg.FeatureIdentifierPackage && pkg.Device.Equals(device, StringComparison.OrdinalIgnoreCase)));
        if (devicePkgFile != null)
          devicePlatformPackage1 = devicePkgFile.PackagePath;
      }
      return devicePlatformPackage1;
    }

    public string GetDeviceLayoutPackage(string SOC)
    {
      string deviceLayoutPackage1 = (string) null;
      if (this.DeviceLayoutPackages != null)
      {
        foreach (SOCPkgFile deviceLayoutPackage2 in this.DeviceLayoutPackages)
        {
          if (deviceLayoutPackage2.SOC.Equals(SOC, StringComparison.OrdinalIgnoreCase))
          {
            deviceLayoutPackage1 = deviceLayoutPackage2.PackagePath;
            break;
          }
        }
      }
      if (string.IsNullOrEmpty(deviceLayoutPackage1) && this.SOCPackages != null)
      {
        SOCPkgFile socPkgFile = this.SOCPackages.Find((Predicate<SOCPkgFile>) (pkg => pkg.FeatureIdentifierPackage && pkg.SOC.Equals(SOC, StringComparison.OrdinalIgnoreCase)));
        if (socPkgFile != null)
          deviceLayoutPackage1 = socPkgFile.PackagePath;
      }
      return deviceLayoutPackage1;
    }

    public void ProcessVariables()
    {
      foreach (PkgFile allPackage in this._allPackages)
        allPackage.ProcessVariables();
    }

    public static void ValidateAndLoad(
      ref FeatureManifest fm,
      string xmlFile,
      IULogger logger,
      bool bOEMSKU = false)
    {
      MicrosoftPhoneSKU sourceSku = (MicrosoftPhoneSKU) null;
      IULogger logger1 = new IULogger();
      logger1.ErrorLogger = (LogString) null;
      logger1.InformationLogger = (LogString) null;
      XsdValidator xsdValidator = new XsdValidator();
      try
      {
        xsdValidator.ValidateXsd(DevicePaths.FeatureManifestSchema, xmlFile, logger1);
      }
      catch (XsdValidatorException ex1)
      {
        sourceSku = new MicrosoftPhoneSKU();
        try
        {
          xsdValidator.ValidateXsd(DevicePaths.MicrosoftPhoneSKUSchema, xmlFile, logger1);
        }
        catch (XsdValidatorException ex2)
        {
          try
          {
            xsdValidator.ValidateXsd(DevicePaths.FeatureManifestSchema, xmlFile, logger);
          }
          catch (XsdValidatorException ex3)
          {
            throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to validate Feature Manifest XSD for file '" + xmlFile + "'.", (Exception) ex3);
          }
        }
      }
      logger.LogInfo("FeatureAPI: Successfully validated the Feature Manifest XML: {0}", (object) xmlFile);
      TextReader textReader = (TextReader) new StreamReader(xmlFile);
      if (sourceSku == null)
      {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof (FeatureManifest));
        try
        {
          fm = (FeatureManifest) xmlSerializer.Deserialize(textReader);
        }
        catch (Exception ex)
        {
          throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to parse Feature Manifest XML file '" + xmlFile + "'.", ex);
        }
        finally
        {
          textReader.Close();
        }
      }
      else
      {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof (MicrosoftPhoneSKU));
        try
        {
          sourceSku = (MicrosoftPhoneSKU) xmlSerializer.Deserialize(textReader);
        }
        catch (Exception ex)
        {
          throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to parse Microsoft Phone SKU XML file '" + xmlFile + "'.", ex);
        }
        finally
        {
          textReader.Close();
        }
        fm.PopulateFromOldStyleSKU(sourceSku, bOEMSKU);
      }
      if (fm.CPUPackages != null)
      {
        if (fm.BasePackages != null)
          fm.BasePackages.AddRange((IEnumerable<PkgFile>) fm.CPUPackages);
        else
          fm.BasePackages = fm.CPUPackages;
        fm.CPUPackages = new List<PkgFile>();
      }
      if (fm.Features != null && !fm.IsFromOldFormat && fm.Features.Microsoft != null && fm.Features.Microsoft.Count<MSOptionalPkgFile>() > 0 && fm.Features.OEM != null && fm.Features.OEM.Count<OEMOptionalPkgFile>() > 0)
        throw new FeatureAPIException("FeatureAPI!ValidateInput: Feature Manifests can contain only Microsoft or OEM features but not both: '" + xmlFile + "'.");
      fm.SourceFile = Path.GetFileName(xmlFile).ToUpper();
    }

    public void AddPackagesFromMergeResult(
      List<FeatureManifest.FMPkgInfo> packageList,
      List<MergeResult> results,
      List<string> supportedLanguages,
      List<string> supportedResolutions,
      string packageOutputDir,
      string packageOutputDirReplacement)
    {
      if (packageList == null || packageList.Count<FeatureManifest.FMPkgInfo>() == 0)
        return;
      FeatureManifest.PackageGroups fmGroup = packageList[0].FMGroup;
      foreach (MergeResult result in results)
      {
        if ((fmGroup == FeatureManifest.PackageGroups.MSFEATURE || fmGroup == FeatureManifest.PackageGroups.OEMFEATURE) && this.Features == null)
          this.Features = new FMFeatures();
        PkgFile pkgFile = new PkgFile();
        bool flag = false;
        switch (fmGroup)
        {
          case FeatureManifest.PackageGroups.BASE:
            if (this.BasePackages == null)
              this.BasePackages = new List<PkgFile>();
            this.BasePackages.Add(pkgFile);
            break;
          case FeatureManifest.PackageGroups.RELEASE:
            ReleasePkgFile releasePkgFile = new ReleasePkgFile();
            if (this.ReleasePackages == null)
              this.ReleasePackages = new List<ReleasePkgFile>();
            this.ReleasePackages.Add(releasePkgFile);
            pkgFile = (PkgFile) releasePkgFile;
            break;
          case FeatureManifest.PackageGroups.DEVICELAYOUT:
            DeviceLayoutPkgFile deviceLayoutPkgFile = new DeviceLayoutPkgFile();
            if (this.DeviceLayoutPackages == null)
              this.DeviceLayoutPackages = new List<DeviceLayoutPkgFile>();
            deviceLayoutPkgFile.CPUType = ((object) result.PkgInfo.CpuType).ToString();
            this.DeviceLayoutPackages.Add(deviceLayoutPkgFile);
            pkgFile = (PkgFile) deviceLayoutPkgFile;
            flag = true;
            break;
          case FeatureManifest.PackageGroups.OEMDEVICEPLATFORM:
            OEMDevicePkgFile oemDevicePkgFile = new OEMDevicePkgFile();
            if (this.OEMDevicePlatformPackages == null)
              this.OEMDevicePlatformPackages = new List<OEMDevicePkgFile>();
            this.OEMDevicePlatformPackages.Add(oemDevicePkgFile);
            pkgFile = (PkgFile) oemDevicePkgFile;
            break;
          case FeatureManifest.PackageGroups.SV:
            SVPkgFile svPkgFile = new SVPkgFile();
            if (this.SVPackages == null)
              this.SVPackages = new List<SVPkgFile>();
            this.SVPackages.Add(svPkgFile);
            pkgFile = (PkgFile) svPkgFile;
            break;
          case FeatureManifest.PackageGroups.SOC:
            SOCPkgFile socPkgFile = new SOCPkgFile();
            if (this.SOCPackages == null)
              this.SOCPackages = new List<SOCPkgFile>();
            socPkgFile.CPUType = ((object) result.PkgInfo.CpuType).ToString();
            this.SOCPackages.Add(socPkgFile);
            pkgFile = (PkgFile) socPkgFile;
            break;
          case FeatureManifest.PackageGroups.DEVICE:
            DevicePkgFile devicePkgFile = new DevicePkgFile();
            if (this.DeviceSpecificPackages == null)
              this.DeviceSpecificPackages = new List<DevicePkgFile>();
            this.DeviceSpecificPackages.Add(devicePkgFile);
            pkgFile = (PkgFile) devicePkgFile;
            break;
          case FeatureManifest.PackageGroups.MSFEATURE:
            MSOptionalPkgFile msOptionalPkgFile = new MSOptionalPkgFile();
            if (this.Features.Microsoft == null)
              this.Features.Microsoft = new List<MSOptionalPkgFile>();
            this.Features.Microsoft.Add(msOptionalPkgFile);
            pkgFile = (PkgFile) msOptionalPkgFile;
            break;
          case FeatureManifest.PackageGroups.OEMFEATURE:
            OEMOptionalPkgFile oemOptionalPkgFile = new OEMOptionalPkgFile();
            if (this.Features.OEM == null)
              this.Features.OEM = new List<OEMOptionalPkgFile>();
            this.Features.OEM.Add(oemOptionalPkgFile);
            pkgFile = (PkgFile) oemOptionalPkgFile;
            break;
          case FeatureManifest.PackageGroups.KEYBOARD:
            if (this.KeyboardPackages == null)
              this.KeyboardPackages = new List<KeyboardPkgFile>();
            this.KeyboardPackages.Add(pkgFile as KeyboardPkgFile);
            break;
          case FeatureManifest.PackageGroups.SPEECH:
            if (this.SpeechPackages == null)
              this.SpeechPackages = new List<SpeechPkgFile>();
            this.SpeechPackages.Add(pkgFile as SpeechPkgFile);
            break;
          case FeatureManifest.PackageGroups.PRERELEASE:
            PrereleasePkgFile prereleasePkgFile = new PrereleasePkgFile();
            if (this.PrereleasePackages == null)
              this.PrereleasePackages = new List<PrereleasePkgFile>();
            this.PrereleasePackages.Add(prereleasePkgFile);
            pkgFile = (PkgFile) prereleasePkgFile;
            break;
        }
        string groupValue = packageList[0].GroupValue;
        pkgFile.InitializeWithMergeResult(result, fmGroup, groupValue, supportedLanguages, supportedResolutions);
        if (flag)
          pkgFile.FeatureIdentifierPackage = false;
        if (!string.IsNullOrEmpty(packageOutputDirReplacement))
          pkgFile.Directory = pkgFile.Directory.Replace(packageOutputDir, packageOutputDirReplacement);
        if (!pkgFile.Directory.Contains(packageOutputDirReplacement))
        {
          FeatureManifest.FMPkgInfo fmPkgInfo = packageList.Single<FeatureManifest.FMPkgInfo>((Func<FeatureManifest.FMPkgInfo, bool>) (pkg => pkg.PackagePath.StartsWith(pkgFile.PackagePath, StringComparison.OrdinalIgnoreCase)));
          if (fmPkgInfo != null)
            pkgFile.Directory = Path.GetDirectoryName(fmPkgInfo.RawBasePath);
        }
      }
    }

    public static string GetFeatureIDWithFMID(string featureID, string fmID) => string.IsNullOrEmpty(fmID) ? featureID : featureID + "." + fmID;

    public override string ToString() => this.SourceFile;

    public enum PackageGroups
    {
      BASE,
      BOOTUI,
      BOOTLOCALE,
      RELEASE,
      DEVICELAYOUT,
      OEMDEVICEPLATFORM,
      SV,
      SOC,
      DEVICE,
      MSFEATURE,
      OEMFEATURE,
      KEYBOARD,
      SPEECH,
      PRERELEASE,
    }

    public class FMPkgInfo
    {
      public const string ReleaseType_Production = "Production";
      public const string ReleaseType_Test = "Test";
      public static char[] separators = new char[1]{ ';' };
      public static string[] ProductionCoreOptionalFeatures = new string[1]
      {
        "PRODUCTION_CORE"
      };
      public static string[] TestCoreOptionalFeatures = new string[2]
      {
        "MOBLECORE_TEST",
        "BOOTSEQUENCE_TEST"
      };
      public FeatureManifest.PackageGroups FMGroup;
      public string GroupValue;
      public string Language;
      public string Resolution;
      public string PackagePath;
      public string RawBasePath;
      public string ID;
      public bool FeatureIdentifierPackage;
      public string Partition = string.Empty;
      public VersionInfo? Version = new VersionInfo?();

      public FMPkgInfo()
      {
      }

      public FMPkgInfo(PkgFile pkg)
      {
        this.PackagePath = pkg.PackagePath;
        this.RawBasePath = pkg.RawPackagePath;
        this.ID = pkg.ID;
        this.FeatureIdentifierPackage = pkg.FeatureIdentifierPackage;
        this.SetVersion(pkg.Version);
        this.Partition = pkg.Partition;
        this.FMGroup = pkg.FMGroup;
        this.GroupValue = pkg.GroupValue;
      }

      public FMPkgInfo(
        string packagePath,
        string id,
        FeatureManifest.PackageGroups fmGroup,
        string groupValue,
        string partition,
        string language,
        string resolution,
        bool featureIdentifierPackage,
        VersionInfo? version)
      {
        this.PackagePath = packagePath;
        this.ID = id;
        this.FMGroup = fmGroup;
        this.GroupValue = groupValue;
        this.Language = language;
        this.Partition = partition;
        this.Resolution = resolution;
        this.FeatureIdentifierPackage = featureIdentifierPackage;
        this.Version = version;
      }

      public string[] GetGroupValueList()
      {
        string[] groupValueList = new string[0];
        if (this.FMGroup == FeatureManifest.PackageGroups.MSFEATURE || this.FMGroup == FeatureManifest.PackageGroups.OEMFEATURE)
          groupValueList = this.GroupValue.Split(FeatureManifest.FMPkgInfo.separators, StringSplitOptions.RemoveEmptyEntries);
        return groupValueList;
      }

      public bool IsOptionalCore() => this.IsOptionalProductionCore() || this.IsOptionalTestCore();

      private bool IsOptionalProductionCore() => ((IEnumerable<string>) FeatureManifest.FMPkgInfo.ProductionCoreOptionalFeatures).Intersect<string>((IEnumerable<string>) this.GetGroupValueList(), (IEqualityComparer<string>) FeatureManifest.IgnoreCase).Count<string>() > 0;

      private bool IsOptionalTestCore() => ((IEnumerable<string>) FeatureManifest.FMPkgInfo.TestCoreOptionalFeatures).Intersect<string>((IEnumerable<string>) this.GetGroupValueList(), (IEqualityComparer<string>) FeatureManifest.IgnoreCase).Count<string>() > 0;

      public void SetVersion(string versionStr)
      {
        if (!string.IsNullOrEmpty(versionStr))
        {
          VersionInfo versionInfo = new VersionInfo();
          if (VersionInfo.TryParse(versionStr, ref versionInfo))
            this.Version = new VersionInfo?(versionInfo);
          else
            this.Version = new VersionInfo?();
        }
        else
          this.Version = new VersionInfo?();
      }

      public FeatureManifest.FMPkgInfo.CorePackageTypeEnum CorePackageType
      {
        get
        {
          if (this.FMGroup.Equals((object) FeatureManifest.PackageGroups.BASE) || this.FMGroup.Equals((object) FeatureManifest.PackageGroups.KEYBOARD) || this.FMGroup.Equals((object) FeatureManifest.PackageGroups.SPEECH) || this.FMGroup.Equals((object) FeatureManifest.PackageGroups.BOOTUI) || this.FMGroup.Equals((object) FeatureManifest.PackageGroups.BOOTLOCALE))
            return FeatureManifest.FMPkgInfo.CorePackageTypeEnum.RetailCore | FeatureManifest.FMPkgInfo.CorePackageTypeEnum.NonRetailCore | FeatureManifest.FMPkgInfo.CorePackageTypeEnum.TestCore;
          if (this.FMGroup == FeatureManifest.PackageGroups.RELEASE)
          {
            if (string.Equals(this.GroupValue, "Production", StringComparison.OrdinalIgnoreCase))
              return FeatureManifest.FMPkgInfo.CorePackageTypeEnum.RetailCore;
            if (string.Equals(this.GroupValue, "Test", StringComparison.OrdinalIgnoreCase))
              return FeatureManifest.FMPkgInfo.CorePackageTypeEnum.NonRetailCore | FeatureManifest.FMPkgInfo.CorePackageTypeEnum.TestCore;
            throw new FeatureAPIException(string.Format("FeatureAPI!CorePackageType:ReleaseType has wrong value {0}", (object) this.GroupValue));
          }
          if (this.IsOptionalProductionCore())
            return FeatureManifest.FMPkgInfo.CorePackageTypeEnum.NonRetailCore;
          return this.IsOptionalTestCore() ? FeatureManifest.FMPkgInfo.CorePackageTypeEnum.TestCore : FeatureManifest.FMPkgInfo.CorePackageTypeEnum.NonCore;
        }
      }

      public string FeatureID
      {
        get
        {
          string featureId;
          switch (this.FMGroup)
          {
            case FeatureManifest.PackageGroups.BASE:
              featureId = FeatureManifest.PackageGroups.BASE.ToString();
              break;
            case FeatureManifest.PackageGroups.MSFEATURE:
              featureId = "MS_" + this.GroupValue;
              break;
            case FeatureManifest.PackageGroups.OEMFEATURE:
              featureId = "OEM_" + this.GroupValue;
              break;
            default:
              featureId = this.FMGroup.ToString() + "_" + this.GroupValue.ToUpper();
              break;
          }
          return featureId;
        }
      }

      public override string ToString() => string.Format("{0} ({1})", (object) this.ID, (object) this.FeatureID);

      [Flags]
      public enum CorePackageTypeEnum
      {
        NonCore = 0,
        RetailCore = 1,
        NonRetailCore = 2,
        TestCore = 4,
      }
    }
  }
}
