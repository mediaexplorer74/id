// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RegistryUtils
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class RegistryUtils
  {
    private const string STR_REG_LOAD = "LOAD {0} {1}";
    private const string STR_REG_EXPORT = "EXPORT {0} {1}";
    private const string STR_REG_UNLOAD = "UNLOAD {0}";
    private const string STR_REGEXE = "%windir%\\System32\\REG.EXE";
    private static Dictionary<string, SystemRegistryHiveFiles> hiveMap = new Dictionary<string, SystemRegistryHiveFiles>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase)
    {
      {
        "SOFTWARE",
        SystemRegistryHiveFiles.SOFTWARE
      },
      {
        "SYSTEM",
        SystemRegistryHiveFiles.SYSTEM
      },
      {
        "DRIVERS",
        SystemRegistryHiveFiles.DRIVERS
      },
      {
        "DEFAULT",
        SystemRegistryHiveFiles.DEFAULT
      },
      {
        "SAM",
        SystemRegistryHiveFiles.SAM
      },
      {
        "SECURITY",
        SystemRegistryHiveFiles.SECURITY
      },
      {
        "BCD",
        SystemRegistryHiveFiles.BCD
      },
      {
        "NTUSER.DAT",
        SystemRegistryHiveFiles.CURRENTUSER
      }
    };
    private static readonly Dictionary<SystemRegistryHiveFiles, string> MountPoints = new Dictionary<SystemRegistryHiveFiles, string>()
    {
      {
        SystemRegistryHiveFiles.SOFTWARE,
        "HKEY_LOCAL_MACHINE\\SOFTWARE"
      },
      {
        SystemRegistryHiveFiles.SYSTEM,
        "HKEY_LOCAL_MACHINE\\SYSTEM"
      },
      {
        SystemRegistryHiveFiles.DRIVERS,
        "HKEY_LOCAL_MACHINE\\DRIVERS"
      },
      {
        SystemRegistryHiveFiles.DEFAULT,
        "HKEY_USERS\\.DEFAULT"
      },
      {
        SystemRegistryHiveFiles.SAM,
        "HKEY_LOCAL_MACHINE\\SAM"
      },
      {
        SystemRegistryHiveFiles.SECURITY,
        "HKEY_LOCAL_MACHINE\\SECURITY"
      },
      {
        SystemRegistryHiveFiles.BCD,
        "HKEY_LOCAL_MACHINE\\BCD"
      },
      {
        SystemRegistryHiveFiles.CURRENTUSER,
        "HKEY_CURRENT_USER"
      }
    };

    public static Dictionary<SystemRegistryHiveFiles, string> KnownMountPoints => RegistryUtils.MountPoints;

    public static void ConvertSystemHiveToRegFile(
      DriveInfo systemDrive,
      SystemRegistryHiveFiles hive,
      string outputRegFile)
    {
      LongPathDirectory.CreateDirectory(LongPath.GetDirectoryName(outputRegFile));
      RegistryUtils.ConvertHiveToRegFile(Path.Combine(Path.Combine(systemDrive.RootDirectory.FullName, "windows\\system32\\config"), Enum.GetName(typeof (SystemRegistryHiveFiles), (object) hive)), RegistryUtils.MapHiveToMountPoint(hive), outputRegFile);
    }

    public static void ConvertHiveToRegFile(
      string inputhive,
      string targetRootKey,
      string outputRegFile)
    {
      OfflineRegUtils.ConvertHiveToReg(inputhive, outputRegFile, targetRootKey);
    }

    public static void LoadHive(string inputhive, string mountpoint)
    {
      string args = string.Format("LOAD {0} {1}", (object) mountpoint, (object) inputhive);
      int error = CommonUtils.RunProcess(Environment.CurrentDirectory, Environment.ExpandEnvironmentVariables("%windir%\\System32\\REG.EXE"), args, true);
      if (0 < error)
        throw new Win32Exception(error);
      Thread.Sleep(500);
    }

    public static void ExportHive(string mountpoint, string outputfile)
    {
      string args = string.Format("EXPORT {0} {1}", (object) mountpoint, (object) outputfile);
      int error = CommonUtils.RunProcess(Environment.CurrentDirectory, Environment.ExpandEnvironmentVariables("%windir%\\System32\\REG.EXE"), args, true);
      if (0 < error)
        throw new Win32Exception(error);
      Thread.Sleep(500);
    }

    public static void UnloadHive(string mountpoint)
    {
      string args = string.Format("UNLOAD {0}", (object) mountpoint);
      int error = CommonUtils.RunProcess(Environment.CurrentDirectory, Environment.ExpandEnvironmentVariables("%windir%\\System32\\REG.EXE"), args, true);
      if (0 < error)
        throw new Win32Exception(error);
    }

    public static string MapHiveToMountPoint(SystemRegistryHiveFiles hive) => RegistryUtils.KnownMountPoints[hive];

    public static string MapHiveFileToMountPoint(string hiveFile)
    {
      if (string.IsNullOrEmpty(hiveFile))
        throw new InvalidOperationException("hiveFile cannot be empty");
      SystemRegistryHiveFiles hive;
      return !RegistryUtils.hiveMap.TryGetValue(Path.GetFileName(hiveFile), out hive) ? "" : RegistryUtils.MapHiveToMountPoint(hive);
    }
  }
}
