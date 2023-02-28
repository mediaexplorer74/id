// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.XsdValidator
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class XsdValidator
  {
    private bool _fileIsValid = true;
    private IULogger _logger;

    public void ValidateXsd(string xsdFile, string xmlFile, IULogger logger)
    {
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
      string name = string.Empty;
      if (!File.Exists(xmlFile))
        throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: XML file was not found: " + xmlFile);
      this._logger = logger;
      foreach (string str in manifestResourceNames)
      {
        if (str.Contains(xsdFile))
        {
          name = str;
          break;
        }
      }
      if (string.IsNullOrEmpty(name))
        throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: XSD resource was not found: " + xsdFile);
      this._fileIsValid = true;
      using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(name))
      {
        if (manifestResourceStream == null)
          throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: Failed to load the embeded schema file: " + name);
        XmlDocument xmlDocument;
        try
        {
          XmlSchema schema = XmlSchema.Read(manifestResourceStream, new ValidationEventHandler(this.ValidationHandler));
          xmlDocument = new XmlDocument();
          xmlDocument.Schemas.Add(schema);
        }
        catch (XmlSchemaException ex)
        {
          throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: Unable to use the schema provided " + name, (Exception) ex);
        }
        try
        {
          xmlDocument.Load(xmlFile);
          xmlDocument.Validate(new ValidationEventHandler(this.ValidationHandler));
        }
        catch (Exception ex)
        {
          throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: There was a problem validating the XML file " + xmlFile, ex);
        }
      }
      if (!this._fileIsValid)
        throw new XsdValidatorException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "ToolsCommon!XsdValidator::ValidateXsd: Validation of {0} failed", new object[1]
        {
          (object) xmlFile
        }));
    }

    private void ValidationHandler(object sender, ValidationEventArgs args)
    {
      string format = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\nToolsCommon!XsdValidator::ValidateXsd: XML Validation {0}: {1}", new object[2]
      {
        (object) args.Severity,
        (object) args.Message
      });
      if (args.Severity == XmlSeverityType.Error)
      {
        if (this._logger != null)
          this._logger.LogError(format);
        this._fileIsValid = false;
      }
      else
      {
        if (this._logger == null)
          return;
        this._logger.LogWarning(format);
      }
    }
  }
}
