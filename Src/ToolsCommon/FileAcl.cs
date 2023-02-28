// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.FileAcl
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
  public class FileAcl : ResourceAcl
  {
    private FileInfo m_fi;

    public FileAcl()
    {
    }

    public FileAcl(string file, string rootPath)
    {
      if (!LongPathFile.Exists(file))
        throw new FileNotFoundException("Specified file cannot be found", file);
      this.Initialize(new FileInfo(file), rootPath);
    }

    public FileAcl(FileInfo fi, string rootPath)
    {
      if (fi == null)
        throw new ArgumentNullException(nameof (fi));
      this.Initialize(fi, rootPath);
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
        FileSecurity objectSecurity = (FileSecurity) null;
        if (this.m_nos != null)
        {
          objectSecurity = new FileSecurity();
          objectSecurity.SetSecurityDescriptorBinaryForm(this.m_nos.GetSecurityDescriptorBinaryForm());
        }
        return (NativeObjectSecurity) objectSecurity;
      }
    }

    protected override string TypeString => "File";

    protected override string ComputeExplicitDACL()
    {
      FileSecurity accessControl = this.m_fi.GetAccessControl(AccessControlSections.All);
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
      string str = (string) null;
      if (this.DACLProtected || count > 0)
      {
        str = accessControl.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
        if (!string.IsNullOrEmpty(str))
          str = ResourceAcl.regexStripDacl.Replace(str, string.Empty);
      }
      return SddlNormalizer.FixAceSddl(str);
    }

    private void Initialize(FileInfo fi, string rootPath)
    {
      this.m_fi = fi != null ? fi : throw new ArgumentNullException(nameof (fi));
      this.m_nos = (NativeObjectSecurity) fi.GetAccessControl(AccessControlSections.All);
      this.m_fullPath = fi.FullName;
        //RnD
        this.m_path = "";//Path.Combine("\\", this.m_fullPath.Remove(0, rootPath.Length)).ToUpper();
    }
  }
}
