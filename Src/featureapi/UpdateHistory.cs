// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.UpdateHistory
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  [XmlRoot(ElementName = "UpdateHistory", IsNullable = false, Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  [XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  public class UpdateHistory
  {
    private static int SUCCESS;
    [XmlArray]
    [XmlArrayItem(ElementName = "UpdateEvent", IsNullable = false, Type = typeof (UpdateEvent))]
    public List<UpdateEvent> UpdateEvents;

    public static void ValidateUpdateHistory(
      ref UpdateHistory xmlHistory,
      string xmlFile,
      IULogger logger)
    {
      XsdValidator xsdValidator = new XsdValidator();
      try
      {
        xsdValidator.ValidateXsd(DevicePaths.UpdateHistorySchema, xmlFile, logger);
      }
      catch (XsdValidatorException ex)
      {
        throw new FeatureAPIException("FeatureAPI!ValidateUpdateHistory: Unable to validate Update History XSD.", (Exception) ex);
      }
      logger.LogInfo("FeatureAPI: Successfully validated the Update History XML");
      TextReader textReader = (TextReader) new StreamReader(xmlFile);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (UpdateHistory));
      try
      {
        xmlHistory = (UpdateHistory) xmlSerializer.Deserialize(textReader);
      }
      catch (Exception ex)
      {
        throw new FeatureAPIException("FeatureAPI!ValidateUpdateHistory: Unable to parse Update History XML file.", ex);
      }
      finally
      {
        textReader.Close();
      }
    }

    public UpdateOSOutput GetPackageList()
    {
      UpdateOSOutput packageList = new UpdateOSOutput();
      List<UpdateEvent> list = this.UpdateEvents.Where<UpdateEvent>((Func<UpdateEvent, bool>) (ue => ue.UpdateResults.OverallResult == UpdateHistory.SUCCESS)).OrderBy<UpdateEvent, int>((Func<UpdateEvent, int>) (ue => ue.Sequence)).ToList<UpdateEvent>();
      Dictionary<string, UpdateOSOutputPackage> dictionary = new Dictionary<string, UpdateOSOutputPackage>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (UpdateEvent updateEvent in list)
      {
        foreach (UpdateOSOutputPackage package in updateEvent.UpdateResults.Packages)
        {
          if (package.IsRemoval)
            dictionary.Remove(package.Name);
          else
            dictionary[package.Name] = package;
        }
        packageList.OverallResult = updateEvent.UpdateResults.OverallResult;
        packageList.UpdateState = updateEvent.UpdateResults.UpdateState;
      }
      packageList.Packages = new List<UpdateOSOutputPackage>((IEnumerable<UpdateOSOutputPackage>) dictionary.Values);
      packageList.Description = "List of packages currently in this image or on this device";
      return packageList;
    }
  }
}
