// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RegistryAcl
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class RegistryAcl : ResourceAcl
  {
    private ORRegistryKey m_key;

    public RegistryAcl()
    {
    }

    public RegistryAcl(ORRegistryKey key)
    {
      this.m_key = key != null ? key : throw new ArgumentNullException(nameof (key));
      this.m_nos = (NativeObjectSecurity) key.RegistrySecurity;
      this.m_path = key.FullName;
      this.m_fullPath = key.FullName;
    }

    [XmlAttribute("SACL")]
    public override string MandatoryIntegrityLabel
    {
      get
      {
        if (this.m_nos != null)
        {
          this.m_macLabel = (string) null;
          string stringSd = SecurityUtils.ConvertSDToStringSD(this.m_nos.GetSecurityDescriptorBinaryForm(), SecurityInformationFlags.SACL_SECURITY_INFORMATION | SecurityInformationFlags.MANDATORY_ACCESS_LABEL);
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
        }
        return this.m_macLabel;
      }
      set => this.m_macLabel = value;
    }

    public override NativeObjectSecurity ObjectSecurity
    {
      get
      {
        RegistrySecurity objectSecurity = (RegistrySecurity) null;
        if (this.m_nos != null)
        {
          objectSecurity = new RegistrySecurity();
          objectSecurity.SetSecurityDescriptorBinaryForm(this.m_nos.GetSecurityDescriptorBinaryForm());
        }
        return (NativeObjectSecurity) objectSecurity;
      }
    }

    protected override string TypeString => "RegKey";

    protected override string ComputeExplicitDACL()
    {
      RegistrySecurity registrySecurity = this.m_key.RegistrySecurity;
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
      string str = (string) null;
      if (this.DACLProtected || count > 0)
      {
        str = registrySecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
        if (!string.IsNullOrEmpty(str))
          str = ResourceAcl.regexStripDacl.Replace(str, string.Empty);
      }
      return SddlNormalizer.FixAceSddl(str);
    }
  }
}
