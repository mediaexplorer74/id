// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.VHDImagePartition
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.IO;
using System.Management;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class VHDImagePartition : ImagePartition
  {
    private const string WMI_GETPARTITIONS_QUERY = "Select * from Win32_DiskPartition where Name='{0}'";
    private const string WMI_DISKPARTITION_CLASS = "Win32_DiskPartition";
    private const string WMI_LOGICALDISK_CLASS = "Win32_LogicalDisk";
    private const string STR_NAME = "Name";
    private const int MAX_RETRY = 10;
    private const int SLEEP_500 = 500;

    public VHDImagePartition(string deviceId, string partitionId)
    {
      this.PhysicalDeviceId = deviceId;
      this.Name = partitionId;
      string empty = string.Empty;
      int num1 = 10;
      int num2 = 0;
      bool flag;
      string logicalDriveFromWmi;
      do
      {
        flag = false;
        logicalDriveFromWmi = this.GetLogicalDriveFromWMI(deviceId, partitionId);
        if (string.IsNullOrEmpty(logicalDriveFromWmi))
        {
          Console.WriteLine("  ImagePartition.GetLogicalDriveFromWMI({0}, {1}) not found, sleeping...", (object) deviceId, (object) partitionId);
          ++num2;
          flag = num2 < num1;
          Thread.Sleep(500);
        }
      }
      while (flag);
      if (string.IsNullOrEmpty(logicalDriveFromWmi))
        throw new IUException("Failed to retrieve logical drive name of partition {0} using WMI", new object[1]
        {
          (object) partitionId
        });
      if (string.Compare(logicalDriveFromWmi, "NONE", true) == 0)
        return;
      this.MountedDriveInfo = new DriveInfo(Path.GetPathRoot(logicalDriveFromWmi));
      this.Root = this.MountedDriveInfo.RootDirectory.FullName;
    }

    private string GetLogicalDriveFromWMI(string deviceId, string partitionId)
    {
      string logicalDriveFromWmi = string.Empty;
      bool flag = false;
      using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(string.Format("Select * from Win32_DiskPartition where Name='{0}'", (object) partitionId)))
      {
        foreach (ManagementObject managementObject in managementObjectSearcher.Get())
        {
          Console.WriteLine("  ImagePartition.GetLogicalDriveFromWMI: Path={0}", (object) managementObject.Path.ToString());
          if (string.Compare(managementObject.GetPropertyValue("Type").ToString(), "unknown", true) == 0)
          {
            logicalDriveFromWmi = "NONE";
            break;
          }
          using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = new ManagementObjectSearcher((ObjectQuery) new RelatedObjectQuery(managementObject.Path.ToString(), "Win32_LogicalDisk")).Get().GetEnumerator())
          {
            if (enumerator.MoveNext())
            {
              logicalDriveFromWmi = enumerator.Current.GetPropertyValue("Name").ToString();
              flag = true;
            }
          }
          if (flag)
            break;
        }
      }
      return logicalDriveFromWmi;
    }
  }
}
