// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.OEMInput
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  [XmlRoot(ElementName = "OEMInput", IsNullable = false, Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  [XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  public class OEMInput
  {
    public const string FM_MicrosoftPhone = "MICROSOFTPHONEFM.XML";
    private const string SKU_MicrosoftPhone = "MICROSOFTPHONESKU.XML";
    private const string FM_MMOS = "MMOSFM.XML";
    private const string SKU_MMOS = "MMOSSKU.XML";
    private const string ExcludePrereleaseTrueValue = "REPLACEMENT";
    private const string ExcludePrereleaseFalseValue = "PROTECTED";
    public const OEMInput.OEMFeatureTypes ALLFEATURES = (OEMInput.OEMFeatureTypes) 268435455;
    public const OEMInput.OEMFeatureTypes MSONLYFEATURES = (OEMInput.OEMFeatureTypes) 268433407;
    public const OEMInput.OEMFeatureTypes OEMONLYFEATURES = (OEMInput.OEMFeatureTypes) 268434431;
    public static readonly string DefaultProduct = "Windows Phone 8.0";
    public static readonly string MMOSProduct = "Manufacturing OS";
    public static readonly string BuildType_FRE = "fre";
    public static readonly string BuildType_CHK = "chk";
    private string _product = OEMInput.DefaultProduct;
    private static StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;
    public string Description;
    public string SOC;
    public string SV;
    public string Device;
    public string ReleaseType;
    public string BuildType;
    public SupportedLangs SupportedLanguages;
    public string BootUILanguage;
    public string BootLocale;
    [XmlArray]
    [XmlArrayItem(ElementName = "Resolution", IsNullable = false, Type = typeof (string))]
    [DefaultValue(null)]
    public List<string> Resolutions;
    [XmlArray]
    [DefaultValue(null)]
    [XmlArrayItem(ElementName = "AdditionalFM", IsNullable = false, Type = typeof (string))]
    public List<string> AdditionalFMs;
    public OEMInputFeatures Features;
    [XmlIgnore]
    public bool IsFromOldFormat;
    [XmlArrayItem(ElementName = "AdditionalSKU", IsNullable = false, Type = typeof (string))]
    [DefaultValue(null)]
    [XmlArray]
    public List<string> AdditionalSKUs;
    [DefaultValue(null)]
    [XmlArrayItem(ElementName = "OptionalFeature", IsNullable = false, Type = typeof (string))]
    [XmlArray]
    public List<string> InternalOptionalFeatures;
    [XmlArrayItem(ElementName = "OptionalFeatures", IsNullable = false, Type = typeof (string))]
    [XmlArray]
    [DefaultValue(null)]
    public List<string> ProductionOptionalFeatures;
    [XmlArrayItem(ElementName = "OptionalFeature", IsNullable = false, Type = typeof (string))]
    [XmlArray]
    [DefaultValue(null)]
    public List<string> MSOptionalFeatures;
    public UserStoreMapData UserStoreMapData;
    public string FormatDPP;
    [DefaultValue(false)]
    public bool ExcludePrereleaseFeatures;
    [XmlArray]
    [DefaultValue(null)]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (string))]
    public List<string> PackageFiles;
    private string _msPackageRoot;
    [XmlIgnore]
    public string CPUType;

    public OEMInput()
    {
    }

    public OEMInput(OEMInput srcOEMInput)
    {
      if (srcOEMInput == null)
        return;
      this._msPackageRoot = srcOEMInput._msPackageRoot;
      this._product = srcOEMInput._product;
      if (srcOEMInput.AdditionalFMs != null)
      {
        this.AdditionalFMs = new List<string>();
        this.AdditionalFMs.AddRange((IEnumerable<string>) srcOEMInput.AdditionalFMs);
      }
      this.BootLocale = srcOEMInput.BootLocale;
      this.BootUILanguage = srcOEMInput.BootUILanguage;
      this.BuildType = srcOEMInput.BuildType;
      this.CPUType = srcOEMInput.CPUType;
      this.Description = srcOEMInput.Description;
      this.Device = srcOEMInput.Device;
      this.ExcludePrereleaseFeatures = srcOEMInput.ExcludePrereleaseFeatures;
      if (srcOEMInput.Features != null)
      {
        this.Features = new OEMInputFeatures();
        if (srcOEMInput.Features.Microsoft != null)
        {
          this.Features.Microsoft = new List<string>();
          this.Features.Microsoft.AddRange((IEnumerable<string>) srcOEMInput.Features.Microsoft);
        }
        if (srcOEMInput.Features.OEM != null)
        {
          this.Features.OEM = new List<string>();
          this.Features.OEM.AddRange((IEnumerable<string>) srcOEMInput.Features.OEM);
        }
      }
      this.FormatDPP = srcOEMInput.FormatDPP;
      if (srcOEMInput.PackageFiles != null)
      {
        this.PackageFiles = new List<string>();
        this.PackageFiles.AddRange((IEnumerable<string>) srcOEMInput.PackageFiles);
      }
      this.ReleaseType = srcOEMInput.ReleaseType;
      if (srcOEMInput.Resolutions != null)
      {
        this.Resolutions = new List<string>();
        this.Resolutions.AddRange((IEnumerable<string>) srcOEMInput.Resolutions);
      }
      this.SOC = srcOEMInput.SOC;
      if (srcOEMInput.SupportedLanguages != null)
      {
        this.SupportedLanguages = new SupportedLangs();
        if (srcOEMInput.SupportedLanguages.UserInterface != null)
        {
          this.SupportedLanguages.UserInterface = new List<string>();
          this.SupportedLanguages.UserInterface.AddRange((IEnumerable<string>) srcOEMInput.SupportedLanguages.UserInterface);
        }
        if (srcOEMInput.SupportedLanguages.Keyboard != null)
        {
          this.SupportedLanguages.Keyboard = new List<string>();
          this.SupportedLanguages.Keyboard.AddRange((IEnumerable<string>) srcOEMInput.SupportedLanguages.Keyboard);
        }
        if (srcOEMInput.SupportedLanguages.Speech != null)
        {
          this.SupportedLanguages.Speech = new List<string>();
          this.SupportedLanguages.Speech.AddRange((IEnumerable<string>) srcOEMInput.SupportedLanguages.Speech);
        }
      }
      this.SV = srcOEMInput.SV;
      this.UserStoreMapData = srcOEMInput.UserStoreMapData;
    }

    public string Product
    {
      get => this._product;
      set => this._product = value;
    }

    public bool IsMMOS => string.Equals(this.Product, OEMInput.MMOSProduct, StringComparison.OrdinalIgnoreCase);

    [XmlIgnore]
    public string MSPackageRoot
    {
      get => this._msPackageRoot;
      set
      {
        if (value == null)
        {
          this._msPackageRoot = (string) null;
        }
        else
        {
          char[] chArray = new char[1]{ '\\' };
          this._msPackageRoot = value.TrimEnd(chArray);
        }
      }
    }

    [XmlIgnore]
    public List<string> FeatureIDs => this.GetFeatureList();

    [XmlIgnore]
    public List<string> MSFeatureIDs => this.GetFeatureList((OEMInput.OEMFeatureTypes) 268433407);

    [XmlIgnore]
    public List<string> OEMFeatureIDs => this.GetFeatureList((OEMInput.OEMFeatureTypes) 268434431);

    public string ProcessOEMInputVariables(string value) => value.Replace("$(device)", this.Device).Replace("$(releasetype)", this.ReleaseType).Replace("$(buildtype)", this.BuildType).Replace("$(cputype)", this.CPUType).Replace("$(bootuilanguage)", this.BootUILanguage).Replace("$(bootlocale)", this.BootLocale).Replace("$(mspackageroot)", this.MSPackageRoot);

    public static void ValidateInput(
      ref OEMInput xmlInput,
      string xmlFile,
      IULogger logger,
      string msPackageDir,
      string cpuType)
    {
      XsdValidator xsdValidator = new XsdValidator();
      try
      {
        xsdValidator.ValidateXsd(DevicePaths.OEMInputSchema, xmlFile, logger);
      }
      catch (XsdValidatorException ex)
      {
        throw new FeatureAPIException(string.Format("FeatureAPI!ValidateInput: Unable to validate OEM Input XSD for file '{0}'", (object) xmlFile), (Exception) ex);
      }
      logger.LogInfo("FeatureAPI: Successfully validated the OEM Input XML: {0}", (object) xmlFile);
      TextReader textReader = (TextReader) new StreamReader(xmlFile);
      try
      {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof (OEMInput));
        xmlInput = (OEMInput) xmlSerializer.Deserialize(textReader);
        xmlInput.MSPackageRoot = msPackageDir;
        xmlInput.CPUType = cpuType;
        xmlInput.BuildType = Environment.ExpandEnvironmentVariables(xmlInput.BuildType);
        xmlInput.Description = xmlInput.ProcessOEMInputVariables(xmlInput.Description);
        xmlInput.Description = Environment.ExpandEnvironmentVariables(xmlInput.Description);
        if (xmlInput.PackageFiles != null)
        {
          for (int index = 0; index < xmlInput.PackageFiles.Count; ++index)
          {
            xmlInput.PackageFiles[index] = xmlInput.ProcessOEMInputVariables(xmlInput.PackageFiles[index]);
            xmlInput.PackageFiles[index] = Environment.ExpandEnvironmentVariables(xmlInput.PackageFiles[index]);
          }
        }
        if (!xmlInput.IsMMOS && (xmlInput.SupportedLanguages.Keyboard == null || xmlInput.SupportedLanguages.Keyboard.Count == 0))
          throw new FeatureAPIException("FeatureAPI!ValidateInput: Atleast one Keyboard language must be specified.");
        if (xmlInput.AdditionalSKUs != null && xmlInput.AdditionalSKUs.Count != 0)
        {
          xmlInput.IsFromOldFormat = true;
          xmlInput.AdditionalFMs = xmlInput.AdditionalSKUs;
        }
        xmlInput.AdditionalSKUs = (List<string>) null;
        if (xmlInput.AdditionalFMs != null)
        {
          for (int index = 0; index < xmlInput.AdditionalFMs.Count; ++index)
            xmlInput.AdditionalFMs[index] = Environment.ExpandEnvironmentVariables(xmlInput.AdditionalFMs[index]);
        }
        if (xmlInput.Features == null)
          xmlInput.Features = new OEMInputFeatures();
        if (xmlInput.MSOptionalFeatures != null && xmlInput.MSOptionalFeatures.Count != 0)
        {
          xmlInput.IsFromOldFormat = true;
          xmlInput.Features.Microsoft = xmlInput.MSOptionalFeatures;
        }
        xmlInput.MSOptionalFeatures = (List<string>) null;
        xmlInput.MSOptionalFeatures = (List<string>) null;
        if (xmlInput.ProductionOptionalFeatures != null && xmlInput.ProductionOptionalFeatures.Count != 0)
        {
          xmlInput.IsFromOldFormat = true;
          xmlInput.Features.OEM = xmlInput.ProductionOptionalFeatures;
        }
        xmlInput.ProductionOptionalFeatures = (List<string>) null;
        if (xmlInput.InternalOptionalFeatures != null && xmlInput.InternalOptionalFeatures.Count != 0)
        {
          xmlInput.IsFromOldFormat = true;
          if (xmlInput.Features.Microsoft == null)
            xmlInput.Features.Microsoft = new List<string>();
          xmlInput.Features.Microsoft.AddRange((IEnumerable<string>) xmlInput.InternalOptionalFeatures);
          if (xmlInput.Features.OEM == null)
            xmlInput.Features.OEM = new List<string>();
          xmlInput.Features.OEM.AddRange((IEnumerable<string>) xmlInput.InternalOptionalFeatures);
        }
        xmlInput.InternalOptionalFeatures = (List<string>) null;
      }
      catch (Exception ex)
      {
        throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to parse OEM Input XML file.", ex);
      }
      finally
      {
        textReader.Close();
      }
    }

    public void WriteToFile(string fileName)
    {
      using (XmlWriter xmlWriter = XmlWriter.Create(fileName, new XmlWriterSettings()
      {
        Indent = true
      }))
      {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof (OEMInput));
        try
        {
          xmlSerializer.Serialize(xmlWriter, (object) this);
        }
        catch (Exception ex)
        {
          throw new FeatureAPIException("FeatureAPI!WriteToFile: Unable to write OEM Input XML file '" + fileName + "'", ex);
        }
      }
    }

    public static List<string> GetPackagesFromDSMs(List<string> dsmPaths)
    {
      string str1 = ".dsm.xml";
      List<string> source = new List<string>();
      foreach (string dsmPath in dsmPaths)
      {
        foreach (string file in Directory.GetFiles(dsmPath, "*" + str1))
        {
          string fileName = Path.GetFileName(file);
          string str2 = fileName.Substring(0, fileName.Length - str1.Length);
          source.Add(str2);
        }
      }
      return source.Distinct<string>((IEqualityComparer<string>) OEMInput.IgnoreCase).ToList<string>();
    }

    public void InferOEMInputFromPackageList(string msFMPattern, List<string> packages)
    {
      IULogger logger = new IULogger();
      foreach (string xmlFile in !Directory.Exists(msFMPattern) ? Directory.GetFiles(Path.GetDirectoryName(msFMPattern), Path.GetFileName(msFMPattern)) : Directory.GetFiles(msFMPattern))
      {
        FeatureManifest fm = new FeatureManifest();
        try
        {
          FeatureManifest.ValidateAndLoad(ref fm, xmlFile, logger);
        }
        catch
        {
          continue;
        }
        this.InferOEMInput(fm, packages);
      }
    }

    public void InferOEMInput(FeatureManifest fm, List<string> packages)
    {
      if (this.SupportedLanguages == null)
      {
        this.SupportedLanguages = new SupportedLangs();
        this.SupportedLanguages.UserInterface = new List<string>();
        this.SupportedLanguages.Speech = new List<string>();
        this.SupportedLanguages.Keyboard = new List<string>();
      }
      if (this.SupportedLanguages.UserInterface.Count<string>() == 0)
        this.SupportedLanguages.UserInterface = fm.GetUILangFeatures(packages);
      if (this.Resolutions == null)
        this.Resolutions = new List<string>();
      if (this.Resolutions.Count<string>() == 0)
        this.Resolutions = fm.GetResolutionFeatures(packages);
      if (this.Features == null)
      {
        this.Features = new OEMInputFeatures();
        this.Features.Microsoft = new List<string>();
        this.Features.OEM = new List<string>();
      }
      foreach (FeatureManifest.FMPkgInfo identifierPackage in fm.GetFeatureIdentifierPackages())
      {
        string str = identifierPackage.ID;
        if (identifierPackage.FMGroup == FeatureManifest.PackageGroups.KEYBOARD || identifierPackage.FMGroup == FeatureManifest.PackageGroups.SPEECH)
          str = str + PkgFile.DefaultLanguagePattern + identifierPackage.GroupValue;
        if (packages.Contains<string>(str, (IEqualityComparer<string>) OEMInput.IgnoreCase))
          this.SetOEMInputValue(identifierPackage);
      }
      if (fm.BootLocalePackageFile != null)
      {
        string bootLocaleBaseName = fm.BootLocalePackageFile.ID.Replace("$(bootlocale)", "");
        List<string> list = packages.Where<string>((Func<string, bool>) (pkg => pkg.StartsWith(bootLocaleBaseName, StringComparison.OrdinalIgnoreCase))).ToList<string>();
        if (list.Count > 0)
          this.BootLocale = list[0].Substring(bootLocaleBaseName.Length);
      }
      if (fm.BootUILanguagePackageFile == null)
        return;
      string bootLangBaseName = fm.BootUILanguagePackageFile.ID.Replace("$(bootuilanguage)", "");
      List<string> list1 = packages.Where<string>((Func<string, bool>) (pkg => pkg.StartsWith(bootLangBaseName, StringComparison.OrdinalIgnoreCase))).ToList<string>();
      if (list1.Count <= 0)
        return;
      this.BootUILanguage = list1[0].Substring(bootLangBaseName.Length);
    }

    private void SetOEMInputValue(FeatureManifest.FMPkgInfo FeatureIDPkg)
    {
      switch (FeatureIDPkg.FMGroup)
      {
        case FeatureManifest.PackageGroups.RELEASE:
          this.ReleaseType = FeatureIDPkg.GroupValue;
          break;
        case FeatureManifest.PackageGroups.DEVICELAYOUT:
          this.SOC = FeatureIDPkg.GroupValue;
          break;
        case FeatureManifest.PackageGroups.OEMDEVICEPLATFORM:
          this.Device = FeatureIDPkg.GroupValue;
          break;
        case FeatureManifest.PackageGroups.SV:
          this.SV = FeatureIDPkg.GroupValue;
          break;
        case FeatureManifest.PackageGroups.SOC:
          this.SOC = FeatureIDPkg.GroupValue;
          break;
        case FeatureManifest.PackageGroups.DEVICE:
          this.Device = FeatureIDPkg.GroupValue;
          break;
        case FeatureManifest.PackageGroups.MSFEATURE:
          this.Features.Microsoft.Add(FeatureIDPkg.GroupValue);
          break;
        case FeatureManifest.PackageGroups.OEMFEATURE:
          this.Features.OEM.Add(FeatureIDPkg.GroupValue);
          break;
        case FeatureManifest.PackageGroups.KEYBOARD:
          this.SupportedLanguages.Keyboard.Add(FeatureIDPkg.GroupValue);
          break;
        case FeatureManifest.PackageGroups.SPEECH:
          this.SupportedLanguages.Speech.Add(FeatureIDPkg.GroupValue);
          break;
        case FeatureManifest.PackageGroups.PRERELEASE:
          this.ExcludePrereleaseFeatures = FeatureIDPkg.GroupValue.Equals("replacement", StringComparison.OrdinalIgnoreCase);
          break;
      }
    }

    public List<string> GetFeatureList(OEMInput.OEMFeatureTypes forFeatures = (OEMInput.OEMFeatureTypes) 268435455)
    {
      List<string> featureList = new List<string>();
      featureList.Add(FeatureManifest.PackageGroups.BASE.ToString());
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.SV))
        featureList.Add(this.FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.SV, this.SV));
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.SOC))
        featureList.Add(this.FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.SOC, this.SOC));
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.DEVICE))
        featureList.Add(this.FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.DEVICE, this.Device));
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.RELEASE))
        featureList.Add(this.FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.RELEASE, this.ReleaseType));
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.UILANGS) && this.SupportedLanguages != null && this.SupportedLanguages.UserInterface != null)
      {
        foreach (string str in this.SupportedLanguages.UserInterface)
          featureList.Add(this.KeyAndValueToFeatureID(OEMInput.OEMFeatureTypes.UILANGS.ToString(), str));
      }
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.KEYBOARD) && this.SupportedLanguages != null)
      {
        foreach (string str in this.SupportedLanguages.Keyboard)
          featureList.Add(this.FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.KEYBOARD, str));
      }
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.SPEECH) && this.SupportedLanguages != null)
      {
        foreach (string str in this.SupportedLanguages.Speech)
          featureList.Add(this.FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.SPEECH, str));
      }
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.BOOTUI))
        featureList.Add(this.FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.BOOTUI, this.BootUILanguage));
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.BOOTLOCALE))
        featureList.Add(this.FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.BOOTLOCALE, this.BootLocale));
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.RESOULTIONS) && this.Resolutions != null)
      {
        foreach (string resolution in this.Resolutions)
          featureList.Add(this.KeyAndValueToFeatureID(OEMInput.OEMFeatureTypes.RESOULTIONS.ToString(), resolution));
      }
      if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.PRERELEASE) && !this.IsFromOldFormat)
      {
        string str = this.ExcludePrereleaseFeatures ? "REPLACEMENT" : "PROTECTED";
        featureList.Add(this.FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.PRERELEASE, str));
      }
      if (this.Features != null)
      {
        if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.MSFEATURES) && this.Features.Microsoft != null)
          featureList.AddRange(this.Features.Microsoft.Select<string, string>((Func<string, string>) (feature => "MS_" + feature)));
        if (forFeatures.HasFlag((Enum) OEMInput.OEMFeatureTypes.OEMFEATURES) && this.Features.OEM != null)
          featureList.AddRange(this.Features.OEM.Select<string, string>((Func<string, string>) (feature => "OEM_" + feature)));
      }
      return featureList;
    }

    private string FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups group, string value) => this.KeyAndValueToFeatureID(group.ToString(), value);

    private string KeyAndValueToFeatureID(string key, string value)
    {
      string str = key.ToUpper() + "_";
      return !string.IsNullOrEmpty(value) ? str + value.ToUpper() : str + "INVALID";
    }

    public List<string> GetFMs()
    {
      List<string> fms = new List<string>();
      if (this.IsFromOldFormat)
      {
        if (this.IsMMOS)
          fms.Add("MMOSSKU.XML");
        else
          fms.Add("MICROSOFTPHONESKU.XML");
      }
      else if (this.IsMMOS)
        fms.Add("MMOSFM.XML");
      else
        fms.Add("MICROSOFTPHONEFM.XML");
      foreach (string additionalFm in this.AdditionalFMs)
      {
        string upper = Path.GetFileName(additionalFm).ToUpper();
        fms.Add(upper);
      }
      return fms;
    }

    [System.Flags]
    public enum OEMFeatureTypes
    {
      NONE = 0,
      BASE = 1,
      BOOTUI = 2,
      BOOTLOCALE = 4,
      RELEASE = 8,
      SV = 32, // 0x00000020
      SOC = 64, // 0x00000040
      DEVICE = 128, // 0x00000080
      KEYBOARD = 256, // 0x00000100
      SPEECH = 512, // 0x00000200
      MSFEATURES = 1024, // 0x00000400
      OEMFEATURES = 2048, // 0x00000800
      PRERELEASE = 4096, // 0x00001000
      UILANGS = 8192, // 0x00002000
      RESOULTIONS = 16384, // 0x00004000
    }
  }
}
