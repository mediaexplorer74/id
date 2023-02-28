// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RegistryStoredAcl
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class RegistryStoredAcl : ResourceAcl
  {
    protected string m_typeName = "Unknown";
    protected byte[] m_rawsd;

    public RegistryStoredAcl()
    {
    }

    public RegistryStoredAcl(string typeName, string path, byte[] rawSecurityDescriptor)
    {
      if (rawSecurityDescriptor == null || string.IsNullOrEmpty(path) || string.IsNullOrEmpty(typeName))
        throw new ArgumentNullException("SDRegValue");
      RegistrySecurity registrySecurity = new RegistrySecurity();
      registrySecurity.SetSecurityDescriptorBinaryForm(rawSecurityDescriptor);
      this.m_rawsd = rawSecurityDescriptor;
      this.m_nos = (NativeObjectSecurity) registrySecurity;
      this.m_path = path;
      this.m_fullPath = path;
      this.m_typeName = typeName;
    }

    [XmlAttribute("Type")]
    public string SDRegValueTypeName
    {
      get => this.m_typeName;
      set => this.m_typeName = value;
    }

    [XmlAttribute("SACL")]
    public override string MandatoryIntegrityLabel
    {
      get
      {
        this.m_macLabel = (string) null;
        string stringSd = SecurityUtils.ConvertSDToStringSD(this.m_rawsd, SecurityInformationFlags.MANDATORY_ACCESS_LABEL);
        if (!string.IsNullOrEmpty(stringSd))
        {
          Match match = ResourceAcl.regexExtractMIL.Match(stringSd);
          if (match.Success)
          {
            Group group = match.Groups["MIL"];
            if (group != null)
              this.m_macLabel = SddlNormalizer.FixAceSddl(group.Value);
          }
        }
        return this.m_macLabel;
      }
      set => this.m_macLabel = value;
    }

    [XmlAttribute]
    public override string AttributeHash
    {
      get
      {
        if (string.IsNullOrEmpty(this.m_attributeHash))
        {
          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.Append(this.TypeString);
          stringBuilder.Append(this.m_path);
          stringBuilder.Append(this.Protected);
          string owner = this.Owner;
          if (!string.IsNullOrEmpty(owner))
            stringBuilder.Append(owner);
          string explicitDacl = this.ExplicitDACL;
          if (!string.IsNullOrEmpty(explicitDacl))
            stringBuilder.Append(explicitDacl);
          string mandatoryIntegrityLabel = this.MandatoryIntegrityLabel;
          if (!string.IsNullOrEmpty(mandatoryIntegrityLabel))
            stringBuilder.Append(mandatoryIntegrityLabel);
          stringBuilder.Append(this.SDRegValueTypeName);
          this.m_attributeHash = CommonUtils.GetSha256Hash(Encoding.Unicode.GetBytes(stringBuilder.ToString()));
        }
        return this.m_attributeHash;
      }
      set => this.m_attributeHash = value;
    }

    public override NativeObjectSecurity ObjectSecurity
    {
      get
      {
        RegistrySecurity objectSecurity = new RegistrySecurity();
        objectSecurity.SetSecurityDescriptorBinaryForm(this.m_rawsd);
        return (NativeObjectSecurity) objectSecurity;
      }
    }

    protected override string TypeString => "SDRegValue";

    protected override string ComputeExplicitDACL()
    {
      RegistrySecurity registrySecurity = new RegistrySecurity();
      registrySecurity.SetSecurityDescriptorBinaryForm(this.m_rawsd);
      AuthorizationRuleCollection accessRules = registrySecurity.GetAccessRules(true, false, typeof (NTAccount));
      int count = accessRules.Count;
      foreach (RegistryAccessRule rule in (ReadOnlyCollectionBase) accessRules)
      {
        if (rule.IsInherited)
        {
          registrySecurity.RemoveAccessRule(rule);
          --count;
        }
      }
      if (this.DACLProtected && registrySecurity.AreAccessRulesCanonical)
        registrySecurity.SetAccessRuleProtection(true, this.PreserveInheritance);
      string input = (string) null;
      if (this.DACLProtected || count > 0)
      {
        input = registrySecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
        if (!string.IsNullOrEmpty(input))
          input = SddlNormalizer.FixAceSddl(ResourceAcl.regexStripDacl.Replace(input, string.Empty));
      }
      return input;
    }
  }
}
