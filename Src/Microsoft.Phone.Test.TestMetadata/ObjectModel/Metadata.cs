// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.ObjectModel.Metadata
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

//using Microsoft.Tools.IO;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.Phone.Test.TestMetadata.ObjectModel
{
  [XmlRoot(ElementName = "Metadata", IsNullable = false, Namespace = "http://schemas.microsoft.com/WindowsPhone/2011/TestMetadata.xsd")]
  [Serializable]
  public class Metadata
  {
    private HashSet<Dependency> _dependencies = new HashSet<Dependency>();

    [XmlIgnore]
    public string FileName { get; private set; }

    [XmlArrayItem(typeof (BinaryDependency), ElementName = "Binary")]
    [XmlArrayItem(typeof (RemoteFileDependency), ElementName = "RemoteFile")]
    [XmlArrayItem(typeof (PackageDependency), ElementName = "Package")]
    [XmlArrayItem(typeof (EnvironmentPathDependnecy), ElementName = "EnvironmentPath")]
    public HashSet<Dependency> Dependencies
    {
      get => this._dependencies;
      set => this._dependencies = value;
    }

    [XmlAttribute]
    public bool RequiresReflash { get; set; }

    public void Save(string fileName)
    {
      Metadata.CleanUpEmptyLists((object) this, "Microsoft.Phone.Test.TestMetadata.ObjectModel");
      XmlWriterSettings settings = new XmlWriterSettings()
      {
        Indent = true
      };
      using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
        new XmlSerializer(this.GetType()).Serialize(xmlWriter, (object) this);
      this.FileName = fileName;
    }

    public override string ToString()
    {
      using (StringWriter stringWriter = new StringWriter((IFormatProvider) CultureInfo.InvariantCulture))
      {
        new XmlSerializer(this.GetType()).Serialize((TextWriter) stringWriter, (object) this);
        return stringWriter.ToString();
      }
    }

    public static Metadata Load(string xmlFile)
    {
      XmlSchema schema = XmlSchema.Read(Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Phone.Test.TestMetadata.Schema.testmetadata.xsd") ?? throw new FileNotFoundException("Microsoft.Phone.Test.TestMetadata.Schema.testmetadata.xsd"), (ValidationEventHandler) null);
      XmlReaderSettings settings = new XmlReaderSettings()
      {
        ValidationType = ValidationType.Schema,
        ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ReportValidationWarnings,
        IgnoreComments = true,
        IgnoreWhitespace = true
      };
      settings.Schemas.Add(schema);
      using (FileStream input = /*LongPath*/File.Open(xmlFile, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        using (XmlReader xmlReader = XmlReader.Create((Stream) input, settings))
        {
          Metadata metadata = (Metadata) new XmlSerializer(typeof (Metadata)).Deserialize(xmlReader);
          metadata.FileName = xmlFile;
          return metadata;
        }
      }
    }

    private static bool CleanUpEmptyLists(object root, string objectNamespace)
    {
      if (root == null)
        return false;
      Type type = root.GetType();
      if (root is IList)
      {
        IList list = (IList) root;
        if (list.Count == 0)
          return true;
        foreach (object root1 in (IEnumerable) list)
          Metadata.CleanUpEmptyLists(root1, objectNamespace);
        return false;
      }
      if (root is ISet<Dependency>)
      {
        ISet<Dependency> dependencySet = (ISet<Dependency>) root;
        if (dependencySet.Count == 0)
          return true;
        foreach (object root2 in (IEnumerable<Dependency>) dependencySet)
          Metadata.CleanUpEmptyLists(root2, objectNamespace);
        return false;
      }
      if (type.Namespace != objectNamespace)
        return false;
      foreach (PropertyInfo property in type.GetProperties())
      {
        if (Metadata.CleanUpEmptyLists(property.GetValue(root, (object[]) null), objectNamespace))
          property.SetValue(root, (object) null, (object[]) null);
      }
      return false;
    }
  }
}
