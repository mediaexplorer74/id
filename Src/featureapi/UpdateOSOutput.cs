// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.UpdateOSOutput
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  [XmlRoot(ElementName = "UpdateOSOutput", IsNullable = false, Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  [XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
  public class UpdateOSOutput
  {
    public string Description;
    public int OverallResult;
    public string UpdateState;
    [XmlArrayItem(ElementName = "Package", IsNullable = false, Type = typeof (UpdateOSOutputPackage))]
    [XmlArray]
    public List<UpdateOSOutputPackage> Packages;

    public static void ValidateOutput(
      ref UpdateOSOutput xmlOutput,
      string outputFile,
      IULogger logger)
    {
      XsdValidator xsdValidator = new XsdValidator();
      try
      {
        xsdValidator.ValidateXsd(DevicePaths.UpdateOSOutputSchema, outputFile, logger);
      }
      catch (XsdValidatorException ex)
      {
        throw new FeatureAPIException("FeatureAPI!ValidateOutput: Unable to validate Update OS Output XSD.", (Exception) ex);
      }
      logger.LogInfo("FeatureAPI: Successfully validated the Update OS Output XML: {0}", (object) outputFile);
      TextReader textReader = (TextReader) new StreamReader(outputFile);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (UpdateOSOutput));
      try
      {
        xmlOutput = (UpdateOSOutput) xmlSerializer.Deserialize(textReader);
      }
      catch (Exception ex)
      {
        throw new FeatureAPIException("FeatureAPI!ValidateOutput: Unable to parse Update OS Output XML file.", ex);
      }
      finally
      {
        textReader.Close();
      }
    }

    public void WriteToFile(string fileName)
    {
      XmlWriterSettings settings = new XmlWriterSettings()
      {
        Indent = true,
        NewLineOnAttributes = true
      };
      using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
      {
        XmlAttributeOverrides attributeOverrides = new XmlAttributeOverrides();
        new XmlSerializer(typeof (UpdateOSOutput)).Serialize(xmlWriter, (object) this);
      }
    }
  }
}
