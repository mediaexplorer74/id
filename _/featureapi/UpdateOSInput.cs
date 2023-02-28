// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.UpdateOSInput
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
  [XmlRoot(ElementName = "UpdateOSInput", IsNullable = false, Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  [XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  public class UpdateOSInput
  {
    public string Description;
    public string DateTime;
    [XmlArrayItem(ElementName = "PackageFile", IsNullable = false, Type = typeof (string))]
    [XmlArray]
    public List<string> PackageFiles;

    public static void ValidateInput(ref UpdateOSInput xmlInput, string inputFile, IULogger logger)
    {
      XsdValidator xsdValidator = new XsdValidator();
      try
      {
        xsdValidator.ValidateXsd(DevicePaths.UpdateOSInputSchema, inputFile, logger);
      }
      catch (XsdValidatorException ex)
      {
        throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to validate update input XSD.", (Exception) ex);
      }
      logger.LogInfo("FeatureAPI: Successfully validated the update input XML: {0}", (object) inputFile);
      TextReader textReader = (TextReader) new StreamReader(inputFile);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (UpdateOSInput));
      try
      {
        xmlInput = (UpdateOSInput) xmlSerializer.Deserialize(textReader);
        for (int index = 0; index < xmlInput.PackageFiles.Count; ++index)
        {
          xmlInput.PackageFiles[index] = Environment.ExpandEnvironmentVariables(xmlInput.PackageFiles[index]);
          xmlInput.PackageFiles[index] = xmlInput.PackageFiles[index].Trim();
        }
      }
      catch (Exception ex)
      {
        throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to parse Update OS Input XML file.", ex);
      }
      finally
      {
        textReader.Close();
      }
    }

    public void WriteToFile(string fileName)
    {
      TextWriter textWriter = (TextWriter) new StreamWriter(fileName);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (UpdateOSInput));
      try
      {
        xmlSerializer.Serialize(textWriter, (object) this);
      }
      catch (Exception ex)
      {
        throw new FeatureAPIException("FeatureAPI!WriteToFile: Unable to write Update OS Input XML file '" + fileName + "'", ex);
      }
      finally
      {
        textWriter.Close();
      }
    }
  }
}
