// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.MicrosoftPhoneSKU
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  [XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  [XmlRoot(ElementName = "MicrosoftPhoneSKU", IsNullable = false, Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  public class MicrosoftPhoneSKU
  {
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (PkgFile))]
    public List<PkgFile> BasePackages;
    public BootUIPkgFile BootUILanguagePackageFile;
    public BootLocalePkgFile BootLocalePackageFile;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (MSOptionalPkgFile))]
    [XmlArray]
    public List<MSOptionalPkgFile> InternalOptionalFeatures;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (OEMOptionalPkgFile))]
    public List<OEMOptionalPkgFile> ProductionOptionalFeatures;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (MSOptionalPkgFile))]
    [XmlArray]
    public List<MSOptionalPkgFile> MSOptionalFeatures;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (ReleasePkgFile))]
    [XmlArray]
    public List<ReleasePkgFile> ReleasePackages;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (PrereleasePkgFile))]
    [XmlArray]
    public List<PrereleasePkgFile> PrereleasePackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (OEMDevicePkgFile))]
    public List<OEMDevicePkgFile> OEMDevicePlatformPackages;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (DeviceLayoutPkgFile))]
    [XmlArray]
    public List<DeviceLayoutPkgFile> DeviceLayoutPackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (SOCPkgFile))]
    public List<SOCPkgFile> SOCPackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (SVPkgFile))]
    public List<SVPkgFile> SVPackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (DevicePkgFile))]
    public List<DevicePkgFile> DeviceSpecificPackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (PkgFile))]
    public List<PkgFile> CPUPackages;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (SpeechPkgFile))]
    [XmlArray]
    public List<SpeechPkgFile> SpeechPackages;
    [XmlArray]
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (KeyboardPkgFile))]
    public List<KeyboardPkgFile> KeyboardPackages;

    public static void ValidateAndLoad(
      ref MicrosoftPhoneSKU xmlInput,
      string xmlFile,
      IULogger logger)
    {
      XsdValidator xsdValidator = new XsdValidator();
      try
      {
        xsdValidator.ValidateXsd(DevicePaths.MicrosoftPhoneSKUSchema, xmlFile, logger);
      }
      catch (XsdValidatorException ex)
      {
        throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to validate Microsoft Phone SKU XSD.", (Exception) ex);
      }
      logger.LogInfo("FeatureAPI: Successfully validated the Microsoft Phone SKU XML: {0}", (object) xmlFile);
      TextReader textReader = (TextReader) new StreamReader(xmlFile);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (MicrosoftPhoneSKU));
      try
      {
        xmlInput = (MicrosoftPhoneSKU) xmlSerializer.Deserialize(textReader);
      }
      catch (Exception ex)
      {
        throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to parse Microsoft Phone SKU XML file.", ex);
      }
      finally
      {
        textReader.Close();
      }
    }

    public enum PackageGroups
    {
      BASE,
      BOOTUI,
      BOOTLOCALE,
      RELEASE,
      CPU,
      DEVICELAYOUT,
      OEMDEVICEPLATFORM,
      SV,
      SOC,
      DEVICE,
      INTERNALOPTIONAL,
      PRODUCTIONOPTIONAL,
      MSOPTIONAL,
      KEYBOARD,
      SPEECH,
      PRERELEASE,
    }
  }
}
