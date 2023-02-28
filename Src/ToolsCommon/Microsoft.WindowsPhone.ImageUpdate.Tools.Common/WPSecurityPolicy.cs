// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.WPSecurityPolicy
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [XmlRoot("PhoneSecurityPolicy")]
  public class WPSecurityPolicy
  {
    private const string WP8PolicyNamespace = "urn:Microsoft.WindowsPhone/PhoneSecurityPolicyInternal.v8.00";
    private string m_descr = "Mobile Core Policy";
    private string m_vendor = "Microsoft";
    private string m_OSVersion = "8.00";
    private string m_fileVersion = "8.00";
    private string m_hashType = "Sha2";
    private string m_packageId = "";
    private AclCollection m_aclCollection = new AclCollection();

    public WPSecurityPolicy()
    {
    }

    public WPSecurityPolicy(string packageName) => this.m_packageId = packageName;

    [XmlAttribute]
    public string Description
    {
      get => this.m_descr;
      set => this.m_descr = value;
    }

    [XmlAttribute]
    public string Vendor
    {
      get => this.m_vendor;
      set => this.m_vendor = value;
    }

    [XmlAttribute]
    public string RequiredOSVersion
    {
      get => this.m_OSVersion;
      set => this.m_OSVersion = value;
    }

    [XmlAttribute]
    public string FileVersion
    {
      get => this.m_fileVersion;
      set => this.m_fileVersion = value;
    }

    [XmlAttribute]
    public string HashType
    {
      get => this.m_hashType;
      set => this.m_hashType = value;
    }

    [XmlAttribute]
    public string PackageID
    {
      get => this.m_packageId;
      set => this.m_packageId = value;
    }

    [XmlArrayItem(typeof (RegAclWithFullAcl), ElementName = "RegKeyFullACL")]
    [XmlArrayItem(typeof (DirectoryAcl), ElementName = "Directory")]
    [XmlArrayItem(typeof (RegistryStoredAcl), ElementName = "SDRegValue")]
    [XmlArrayItem(typeof (FileAcl), ElementName = "File")]
    [XmlArrayItem(typeof (RegistryAcl), ElementName = "RegKey")]
    public ResourceAcl[] Rules
    {
      get => this.m_aclCollection.ToArray<ResourceAcl>();
      set
      {
        this.m_aclCollection.Clear();
        this.m_aclCollection.UnionWith((IEnumerable<ResourceAcl>) value);
      }
    }

    public void Add(IEnumerable<ResourceAcl> acls) => this.m_aclCollection.UnionWith(acls);

    public void SaveToXml(string policyFile)
    {
      using (TextWriter textWriter = (TextWriter) new StreamWriter((Stream) LongPathFile.OpenWrite(policyFile)))
        new XmlSerializer(typeof (WPSecurityPolicy), "urn:Microsoft.WindowsPhone/PhoneSecurityPolicyInternal.v8.00").Serialize(textWriter, (object) this);
    }

    public static WPSecurityPolicy LoadFromXml(string policyFile)
    {
      if (!LongPathFile.Exists(policyFile))
        throw new FileNotFoundException(string.Format("Policy file {0} does not exist, or cannot be read", (object) policyFile), policyFile);
      using (TextReader textReader = (TextReader) new StreamReader((Stream) LongPathFile.OpenRead(policyFile)))
        return (WPSecurityPolicy) new XmlSerializer(typeof (WPSecurityPolicy), "urn:Microsoft.WindowsPhone/PhoneSecurityPolicyInternal.v8.00").Deserialize(textReader);
    }
  }
}
