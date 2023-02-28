// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.LocaleInfoLookup
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class LocaleInfoLookup
  {
    [XmlElement("Locale", Form = XmlSchemaForm.Unqualified)]
    public List<LocaleInfo> Locales;

    public static LocaleInfoLookup Load(string xmlFile)
    {
      LocaleInfoLookup localeInfoLookup = new LocaleInfoLookup();
      using (TextReader textReader = (TextReader) new StreamReader(xmlFile))
        return (LocaleInfoLookup) new XmlSerializer(typeof (LocaleInfoLookup)).Deserialize(textReader);
    }
  }
}
