// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.ImagePartitionCollection
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.Collections.ObjectModel;
using System.Management;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class ImagePartitionCollection : Collection<ImagePartition>
  {
    private const string WMI_GETPARTITIONS_QUERY = "\\\\.\\root\\cimv2:Win32_DiskDrive.DeviceID='{0}'";
    private const string WMI_DISKPARTITION_CLASS = "Win32_DiskPartition";
    private const string STR_NAME = "Name";

    public void PopulateFromPhysicalDeviceId(string deviceId)
    {
      foreach (ManagementObject managementObject in new ManagementObjectSearcher((ObjectQuery) new RelatedObjectQuery(string.Format("\\\\.\\root\\cimv2:Win32_DiskDrive.DeviceID='{0}'", (object) deviceId), "Win32_DiskPartition")).Get())
        this.Add((ImagePartition) new VHDImagePartition(deviceId, managementObject.GetPropertyValue("Name").ToString()));
    }
  }
}
