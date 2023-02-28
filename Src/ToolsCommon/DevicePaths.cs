// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.DevicePaths
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class DevicePaths
  {
    public const string MAINOS_PARTITION_NAME = "MainOS";
    public const string MMOS_PARTITION_NAME = "MMOS";
    private static string _imageUpdatePath = "Windows\\ImageUpdate";
    private static string _updateFilesPath = "SharedData\\DuShared";
    private static string _registryHivePath = "Windows\\System32\\Config";
    private static string _x86BCDHivePath = "boot";
    private static string _armBCDHivePath = "efi\\Microsoft\\boot";
    private static string _dsmPath = DevicePaths._imageUpdatePath;
    private static string _UpdateOSPath = "PROGRAMS\\UpdateOS\\";
    private static string _FMFilesDirectory = "FeatureManifest";
    private static string _OEMInputPath = "OEMInput";
    private static string _OEMInputFile = "OEMInput.xml";
    private static string _deviceLayoutFileName = "DeviceLayout.xml";
    private static string _oemDevicePlatformFileName = "OEMDevicePlatform.xml";
    private static string _updateOutputFile = "UpdateOutput.xml";
    private static string _updateHistoryFile = "UpdateHistory.xml";
    private static string _updateOSWIMName = "UpdateOS.wim";
    private static string _mmosWIMName = "MMOS.wim";

    public static string ImageUpdatePath => DevicePaths._imageUpdatePath;

    public static string DeviceLayoutFileName => DevicePaths._deviceLayoutFileName;

    public static string DeviceLayoutFilePath => Path.Combine(DevicePaths.ImageUpdatePath, DevicePaths.DeviceLayoutFileName);

    public static string OemDevicePlatformFileName => DevicePaths._oemDevicePlatformFileName;

    public static string OemDevicePlatformFilePath => Path.Combine(DevicePaths.ImageUpdatePath, DevicePaths.OemDevicePlatformFileName);

    public static string UpdateOutputFile => DevicePaths._updateOutputFile;

    public static string UpdateOutputFilePath => Path.Combine(DevicePaths._updateFilesPath, DevicePaths._updateOutputFile);

    public static string UpdateHistoryFile => DevicePaths._updateHistoryFile;

    public static string UpdateHistoryFilePath => Path.Combine(DevicePaths._imageUpdatePath, DevicePaths._updateHistoryFile);

    public static string UpdateOSWIMName => DevicePaths._updateOSWIMName;

    public static string UpdateOSWIMFilePath => Path.Combine(DevicePaths._UpdateOSPath, DevicePaths.UpdateOSWIMName);

    public static string MMOSWIMName => DevicePaths._mmosWIMName;

    public static string MMOSWIMFilePath => DevicePaths.MMOSWIMName;

    public static string RegistryHivePath => DevicePaths._registryHivePath;

    public static string GetBCDHivePath(bool isArm) => !isArm ? DevicePaths._x86BCDHivePath : DevicePaths._armBCDHivePath;

    public static string GetRegistryHiveFilePath(SystemRegistryHiveFiles hiveType, bool isArm = true)
    {
      string registryHiveFilePath = "";
      switch (hiveType)
      {
        case SystemRegistryHiveFiles.SYSTEM:
          registryHiveFilePath = Path.Combine(DevicePaths.RegistryHivePath, "SYSTEM");
          break;
        case SystemRegistryHiveFiles.SOFTWARE:
          registryHiveFilePath = Path.Combine(DevicePaths.RegistryHivePath, "SOFTWARE");
          break;
        case SystemRegistryHiveFiles.DEFAULT:
          registryHiveFilePath = Path.Combine(DevicePaths.RegistryHivePath, "DEFAULT");
          break;
        case SystemRegistryHiveFiles.DRIVERS:
          registryHiveFilePath = Path.Combine(DevicePaths.RegistryHivePath, "DRIVERS");
          break;
        case SystemRegistryHiveFiles.SAM:
          registryHiveFilePath = Path.Combine(DevicePaths.RegistryHivePath, "SAM");
          break;
        case SystemRegistryHiveFiles.SECURITY:
          registryHiveFilePath = Path.Combine(DevicePaths.RegistryHivePath, "SECURITY");
          break;
        case SystemRegistryHiveFiles.BCD:
          registryHiveFilePath = Path.Combine(DevicePaths.GetBCDHivePath(isArm), "BCD");
          break;
      }
      return registryHiveFilePath;
    }

    public static string DeviceLayoutSchema => "DeviceLayout.xsd";

    public static string UpdateOSInputSchema => "UpdateOSInput.xsd";

    public static string OEMInputSchema => "OEMInput.xsd";

    public static string FeatureManifestSchema => "FeatureManifest.xsd";

    public static string MicrosoftPhoneSKUSchema => "MicrosoftPhoneSKU.xsd";

    public static string UpdateOSOutputSchema => "UpdateOSOutput.xsd";

    public static string UpdateHistorySchema => "UpdateHistory.xsd";

    public static string OEMDevicePlatformSchema => "OEMDevicePlatform.xsd";

    public static string MSFMPath => Path.Combine(DevicePaths.ImageUpdatePath, DevicePaths._FMFilesDirectory, "Microsoft");

    public static string MSFMPathOld => DevicePaths.ImageUpdatePath;

    public static string OEMFMPath => Path.Combine(DevicePaths.ImageUpdatePath, DevicePaths._FMFilesDirectory, "OEM");

    public static string OEMInputPath => Path.Combine(DevicePaths.ImageUpdatePath, DevicePaths._OEMInputPath);

    public static string OEMInputFile => Path.Combine(DevicePaths.OEMInputPath, DevicePaths._OEMInputFile);
  }
}
