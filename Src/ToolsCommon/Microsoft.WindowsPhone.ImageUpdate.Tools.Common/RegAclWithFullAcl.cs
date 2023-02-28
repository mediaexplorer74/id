// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RegAclWithFullAcl
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.Security.AccessControl;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class RegAclWithFullAcl : RegistryAcl
  {
    public RegAclWithFullAcl()
    {
    }

    public RegAclWithFullAcl(NativeObjectSecurity nos) => this.m_nos = nos;

    [XmlAttribute("FullACL")]
    public string FullRegACL
    {
      get => this.FullACL;
      set
      {
      }
    }
  }
}
