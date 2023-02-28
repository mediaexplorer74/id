// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.DirectoryAcl
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class DirectoryAcl : ResourceAcl
  {
    private bool m_isRoot;
    private DirectoryInfo m_di;

    public DirectoryAcl()
    {
    }

    public DirectoryAcl(string directory, string rootPath)
    {
      if (!LongPathDirectory.Exists(directory))
        throw new DirectoryNotFoundException(string.Format("Folder {0} cannot be found", (object) directory));
      this.Initialize(new DirectoryInfo(directory), rootPath);
    }

    public DirectoryAcl(DirectoryInfo di, string rootPath)
    {
      if (di == null)
        throw new ArgumentNullException(nameof (di));
      this.Initialize(di, rootPath);
    }

    [XmlAttribute("SACL")]
    public override string MandatoryIntegrityLabel
    {
      get
      {
        if (this.m_nos != null)
        {
          this.m_macLabel = SecurityUtils.GetFileSystemMandatoryLevel(this.m_fullPath);
          if (string.IsNullOrEmpty(this.m_macLabel))
            this.m_macLabel = (string) null;
          else
            this.m_macLabel = SddlNormalizer.FixAceSddl(this.m_macLabel);
        }
        return this.m_macLabel;
      }
      set => this.m_macLabel = value;
    }

    public override NativeObjectSecurity ObjectSecurity
    {
      get
      {
        DirectorySecurity objectSecurity = (DirectorySecurity) null;
        if (this.m_nos != null)
        {
          objectSecurity = new DirectorySecurity();
          objectSecurity.SetSecurityDescriptorBinaryForm(this.m_nos.GetSecurityDescriptorBinaryForm());
        }
        return (NativeObjectSecurity) objectSecurity;
      }
    }

    protected override string TypeString => "Directory";

    protected override string ComputeExplicitDACL()
    {
      string str = (string) null;
      if (this.m_isRoot)
      {
        str = this.m_nos.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
      }
      else
      {
        DirectorySecurity accessControl = this.m_di.GetAccessControl(AccessControlSections.All);
        AuthorizationRuleCollection accessRules = accessControl.GetAccessRules(true, false, typeof (NTAccount));
        int count = accessRules.Count;
        foreach (FileSystemAccessRule rule in (ReadOnlyCollectionBase) accessRules)
        {
          if (rule.IsInherited)
          {
            accessControl.RemoveAccessRule(rule);
            --count;
          }
        }
        if (this.DACLProtected && accessControl.AreAccessRulesCanonical)
          accessControl.SetAccessRuleProtection(true, this.PreserveInheritance);
        if (this.DACLProtected || count > 0)
          str = accessControl.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
      }
      if (!string.IsNullOrEmpty(str))
        str = ResourceAcl.regexStripDacl.Replace(str, string.Empty);
      return SddlNormalizer.FixAceSddl(str);
    }

    private void Initialize(DirectoryInfo di, string rootPath)
    {
      this.m_di = di != null ? di : throw new ArgumentNullException(nameof (di));
      this.m_nos = (NativeObjectSecurity) di.GetAccessControl(AccessControlSections.All);
      this.m_fullPath = di.FullName;
      this.m_isRoot = string.Equals(di.FullName, rootPath, StringComparison.OrdinalIgnoreCase);

        //RnD
        this.m_path = "";//Path.Combine("\\", di.FullName.Remove(0, rootPath.Length)).ToUpper();
    }
  }
}
