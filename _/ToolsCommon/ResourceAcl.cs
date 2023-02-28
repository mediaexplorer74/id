// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.ResourceAcl
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Globalization;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public abstract class ResourceAcl
  {
    protected string m_explicitDacl;
    protected string m_macLabel;
    protected string m_owner;
    protected string m_elementId;
    protected string m_attributeHash;
    protected string m_path;
    protected bool m_isProtected;
    private static readonly ResourceAclComparer ResourceAclComparer = new ResourceAclComparer();
    [CLSCompliant(false)]
    protected NativeObjectSecurity m_nos;
    [CLSCompliant(false)]
    protected AuthorizationRuleCollection m_accessRules;
    protected string m_fullPath = string.Empty;
    protected static readonly Regex regexExtractMIL = new Regex("(?<MIL>\\(ML[^\\)]*\\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    protected static readonly Regex regexStripDacl = new Regex("^D:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    protected static readonly Regex regexStripDriveLetter = new Regex("^[A-Z]:", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [XmlAttribute("DACL")]
    public string ExplicitDACL
    {
      get
      {
        if (this.m_nos != null)
          this.m_explicitDacl = this.ComputeExplicitDACL();
        return this.m_explicitDacl;
      }
      set => this.m_explicitDacl = value;
    }

    [XmlAttribute("SACL")]
    public abstract string MandatoryIntegrityLabel { get; set; }

    [XmlAttribute("Owner")]
    public string Owner
    {
      get
      {
        if (this.m_nos != null)
        {
          this.m_owner = this.m_nos.GetSecurityDescriptorSddlForm(AccessControlSections.Owner | AccessControlSections.Group);
          this.m_owner = SddlNormalizer.FixOwnerSddl(this.m_owner);
        }
        return this.m_owner;
      }
      set => this.m_owner = value;
    }

    [XmlAttribute]
    public string ElementID
    {
      get
      {
        if (!string.IsNullOrEmpty(this.m_path))
        {
          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.Append(this.TypeString);
          stringBuilder.Append(this.m_path.ToUpper(new CultureInfo("en-US", false)));
          this.m_elementId = CommonUtils.GetSha256Hash(Encoding.Unicode.GetBytes(stringBuilder.ToString()));
        }
        return this.m_elementId;
      }
      set => this.m_elementId = value;
    }

    [XmlAttribute]
    public virtual string AttributeHash
    {
      get
      {
        if (string.IsNullOrEmpty(this.m_attributeHash))
        {
          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.Append(this.TypeString);
          stringBuilder.Append(this.m_path.ToUpper(new CultureInfo("en-US", false)));
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
          this.m_attributeHash = CommonUtils.GetSha256Hash(Encoding.Unicode.GetBytes(stringBuilder.ToString()));
        }
        return this.m_attributeHash;
      }
      set => this.m_attributeHash = value;
    }

    [XmlAttribute]
    public string Path
    {
      get => this.m_path;
      set => this.m_path = value;
    }

    [XmlIgnore]
    public string Protected
    {
      get
      {
        if (this.m_nos != null)
          this.m_isProtected = this.m_nos.AreAccessRulesProtected;
        return !this.m_isProtected ? "No" : "Yes";
      }
      set => this.m_isProtected = value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsEmpty => string.IsNullOrEmpty(this.ExplicitDACL) && string.IsNullOrEmpty(this.MandatoryIntegrityLabel) && !this.DACLProtected;

    public string DACL
    {
      get
      {
        string str = string.Empty;
        if (this.m_nos != null)
        {
          str = this.m_nos.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
          if (!string.IsNullOrEmpty(str))
            str = ResourceAcl.regexStripDacl.Replace(str, string.Empty);
        }
        return SddlNormalizer.FixAceSddl(str);
      }
    }

    public string FullACL
    {
      get
      {
        string fullAcl = string.Empty;
        if (this.m_nos != null)
          fullAcl = this.m_nos.GetSecurityDescriptorSddlForm(AccessControlSections.All);
        return fullAcl;
      }
    }

    public static ResourceAclComparer Comparer => ResourceAcl.ResourceAclComparer;

    public abstract NativeObjectSecurity ObjectSecurity { get; }

    protected abstract string TypeString { get; }

    protected AuthorizationRuleCollection AccessRules
    {
      get
      {
        if (this.m_accessRules == null && this.m_nos != null)
          this.m_accessRules = this.m_nos.GetAccessRules(true, false, typeof (NTAccount));
        return this.m_accessRules;
      }
    }

    public bool PreserveInheritance => this.m_nos != null && this.m_nos.GetAccessRules(false, true, typeof (NTAccount)).Count > 0;

    public bool DACLProtected => this.m_nos != null && this.m_nos.AreAccessRulesProtected;

    protected abstract string ComputeExplicitDACL();
  }
}
