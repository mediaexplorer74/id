// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.SecurityUtils
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class SecurityUtils
  {
    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private static readonly Regex regexExtractMIL = new Regex("(?<MIL>\\(ML[^\\)]*\\))", RegexOptions.Compiled);

    public static string GetFileSystemMandatoryLevel(string resourcePath)
    {
      string empty = string.Empty;
      string stringSd = SecurityUtils.ConvertSDToStringSD(SecurityUtils.GetSecurityDescriptor(resourcePath, SecurityInformationFlags.MANDATORY_ACCESS_LABEL), SecurityInformationFlags.MANDATORY_ACCESS_LABEL);
      if (!string.IsNullOrEmpty(stringSd))
      {
        string input = stringSd.TrimEnd(new char[1]);
        Match match = SecurityUtils.regexExtractMIL.Match(input);
        if (match.Success)
          empty = match.Groups["MIL"].Value;
      }
      return empty;
    }

    [CLSCompliant(false)]
    public static byte[] GetSecurityDescriptor(string resourcePath, SecurityInformationFlags flags)
    {
      byte[] destination = (byte[]) null;
      int lpnLengthNeeded = 0;
      NativeSecurityMethods.GetFileSecurity(resourcePath, flags, IntPtr.Zero, 0, ref lpnLengthNeeded);
      int lastWin32Error = Marshal.GetLastWin32Error();
      if (lastWin32Error != 122)
        throw new Win32Exception(lastWin32Error);
      int num1 = lpnLengthNeeded;
      IntPtr num2 = Marshal.AllocHGlobal(num1);
      try
      {
        if (!NativeSecurityMethods.GetFileSecurity(resourcePath, flags, num2, num1, ref lpnLengthNeeded))
          throw new Win32Exception(Marshal.GetLastWin32Error());
        destination = new byte[lpnLengthNeeded];
        Marshal.Copy(num2, destination, 0, lpnLengthNeeded);
      }
      finally
      {
        Marshal.FreeHGlobal(num2);
      }
      return destination;
    }

    [CLSCompliant(false)]
    public static string ConvertSDToStringSD(
      byte[] securityDescriptor,
      SecurityInformationFlags flags)
    {
      string empty = string.Empty;
      IntPtr StringSecurityDescriptor;
      int StringSecurityDescriptorLen;
      bool securityDescriptor1 = NativeSecurityMethods.ConvertSecurityDescriptorToStringSecurityDescriptor(securityDescriptor, 1, flags, out StringSecurityDescriptor, out StringSecurityDescriptorLen);
      try
      {
        if (!securityDescriptor1)
          throw new Win32Exception(Marshal.GetLastWin32Error());
        return Marshal.PtrToStringUni(StringSecurityDescriptor, StringSecurityDescriptorLen);
      }
      finally
      {
        if (StringSecurityDescriptor != IntPtr.Zero)
          Marshal.FreeHGlobal(StringSecurityDescriptor);
        IntPtr zero = IntPtr.Zero;
      }
    }

    public static AclCollection GetFileSystemACLs(string rootDir)
    {
      if (rootDir == null)
        throw new ArgumentNullException(nameof (rootDir));
      if (!LongPathDirectory.Exists(rootDir))
        throw new ArgumentException(string.Format("Directory {0} does not exist", (object) rootDir));
      AclCollection accesslist = new AclCollection();
      DirectoryInfo directoryInfo = new DirectoryInfo(rootDir);
      DirectoryAcl directoryAcl = new DirectoryAcl(directoryInfo, rootDir);
      if (!directoryAcl.IsEmpty)
        accesslist.Add((ResourceAcl) directoryAcl);
      SecurityUtils.GetFileSystemACLsRecursive(directoryInfo, rootDir, accesslist);
      return accesslist;
    }

    public static AclCollection GetRegistryACLs(string hiveRoot)
    {
      if (hiveRoot == null)
        throw new ArgumentNullException(nameof (hiveRoot));
      if (!LongPathDirectory.Exists(hiveRoot))
        throw new ArgumentException(string.Format("Directory {0} does not exist", (object) hiveRoot));
      AclCollection accesslist = new AclCollection();
      foreach (SystemRegistryHiveFiles hive in Enum.GetValues(typeof (SystemRegistryHiveFiles)))
      {
        using (ORRegistryKey parent = ORRegistryKey.OpenHive(Path.Combine(hiveRoot, Enum.GetName(typeof (SystemRegistryHiveFiles), (object) hive)), RegistryUtils.MapHiveToMountPoint(hive)))
          SecurityUtils.GetRegistryACLsRecursive(parent, accesslist);
      }
      return accesslist;
    }

    private static void GetFileSystemACLsRecursive(
      DirectoryInfo rootdi,
      string rootDir,
      AclCollection accesslist)
    {
      foreach (DirectoryInfo directory in rootdi.GetDirectories())
      {
        SecurityUtils.GetFileSystemACLsRecursive(directory, rootDir, accesslist);
        DirectoryAcl directoryAcl = new DirectoryAcl(directory, rootDir);
        if (!directoryAcl.IsEmpty)
          accesslist.Add((ResourceAcl) directoryAcl);
      }
      foreach (FileInfo file in rootdi.GetFiles())
      {
        FileAcl fileAcl = new FileAcl(file, rootDir);
        if (!fileAcl.IsEmpty)
          accesslist.Add((ResourceAcl) fileAcl);
      }
    }

    public static void GetRegistryACLsRecursive(ORRegistryKey parent, AclCollection accesslist)
    {
      RegistryAcl registryAcl1 = new RegistryAcl(parent);
      foreach (string subKey in parent.SubKeys)
      {
        using (ORRegistryKey orRegistryKey = parent.OpenSubKey(subKey))
        {
          SecurityUtils.GetRegistryACLsRecursive(orRegistryKey, accesslist);
          RegistryAcl registryAcl2 = new RegistryAcl(orRegistryKey);
          if (!registryAcl2.IsEmpty)
            accesslist.Add((ResourceAcl) registryAcl2);
        }
      }
    }
  }
}
