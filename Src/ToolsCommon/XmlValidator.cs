// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.XmlValidator
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class XmlValidator
  {
    protected ValidationEventHandler _validationEventHandler;
    protected XmlReaderSettings _xmlReaderSettings;

    private static void OnSchemaValidationEvent(object sender, ValidationEventArgs e) => Console.WriteLine(e.Message);

    public XmlValidator()
      : this(new ValidationEventHandler(XmlValidator.OnSchemaValidationEvent))
    {
    }

    public XmlValidator(ValidationEventHandler eventHandler)
    {
      this._validationEventHandler = eventHandler;
      this._xmlReaderSettings = new XmlReaderSettings();
      this._xmlReaderSettings.ValidationType = ValidationType.Schema;
      this._xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessIdentityConstraints;
      this._xmlReaderSettings.ValidationEventHandler += this._validationEventHandler;
    }

    public void AddSchema(XmlSchema schema) => this._xmlReaderSettings.Schemas.Add(schema);

    public void AddSchema(string xsdFile)
    {
      using (Stream xsdStream = (Stream) LongPathFile.OpenRead(xsdFile))
        this.AddSchema(xsdStream);
    }

    public void AddSchema(Stream xsdStream) => this.AddSchema(XmlSchema.Read(xsdStream, this._validationEventHandler));

    public void Validate(string xmlFile)
    {
      using (Stream xmlStream = (Stream) LongPathFile.OpenRead(xmlFile))
        this.Validate(xmlStream);
    }

    public void Validate(Stream xmlStream)
    {
      XmlReader xmlReader = XmlReader.Create(xmlStream, this._xmlReaderSettings);
      do
        ;
      while (xmlReader.Read());
    }

    public void Validate(XElement element)
    {
      while (element != null && element.GetSchemaInfo() == null)
        element = element.Parent;
      IXmlSchemaInfo xmlSchemaInfo = element != null ? element.GetSchemaInfo() : throw new ArgumentException("Argument has no SchemaInfo anywhere in the document. Validate the XDocument first.");
      element.Validate((XmlSchemaObject) xmlSchemaInfo.SchemaElement, this._xmlReaderSettings.Schemas, this._validationEventHandler, true);
    }

    public void Validate(XDocument document) => document.Validate(this._xmlReaderSettings.Schemas, this._validationEventHandler, true);

    public XmlReader GetXmlReader(string xmlFile) => XmlReader.Create((Stream) LongPathFile.OpenRead(xmlFile), this._xmlReaderSettings);

    public XmlReader GetXmlReader(Stream xmlStream) => XmlReader.Create(xmlStream, this._xmlReaderSettings);
  }
}
