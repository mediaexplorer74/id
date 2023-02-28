// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.MobileCoreVHD
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class MobileCoreVHD : MobileCoreImage
  {
    private const int SLEEP_1000 = 1000;
    private const int MAX_RETRY = 3;
    private IntPtr _hndlVirtDisk = IntPtr.Zero;
    private static readonly object _lockObj = new object();

    internal MobileCoreVHD(string path)
      : base(path)
    {
    }

    private void MountWithRetry(bool readOnly)
    {
      if (this.IsMounted)
        return;
      int num1 = 0;
      int num2 = 3;
      bool flag;
      do
      {
        flag = false;
        try
        {
          this.MountVHD(readOnly);
        }
        catch (Exception ex)
        {
          Debug.WriteLine("[ex] Exception: " + ex.Message);
          ++num1;
          flag = num1 < num2;
          if (!flag)
            throw;
          else
            Thread.Sleep(1000);
        }
      }
      while (flag);
      this.IsMounted = true;
    }

    public override void MountReadOnly() => this.MountWithRetry(true);

    public override void Mount() => this.MountWithRetry(false);

    private void MountVHD(bool readOnly)
    {
      lock (MobileCoreVHD._lockObj)
      {
        this.m_partitions.Clear();
        try
        {
          this._hndlVirtDisk = CommonUtils.MountVHD(this.m_mobileCoreImagePath, readOnly);
          int DiskPathSizeInBytes = 1024;
          StringBuilder DiskPath = new StringBuilder(DiskPathSizeInBytes);
          int diskPhysicalPath = VirtualDiskLib.GetVirtualDiskPhysicalPath(this._hndlVirtDisk, ref DiskPathSizeInBytes, DiskPath);
          if (0 < diskPhysicalPath)
            throw new Win32Exception(diskPhysicalPath);
          this.m_partitions.PopulateFromPhysicalDeviceId(DiskPath.ToString());
          if (this.m_partitions.Count == 0)
            throw new IUException("Could not retrieve logical drive information for {0}", new object[1]
            {
              (object) DiskPath
            });
        }
        catch (Exception ex)
        {
          Debug.WriteLine("[ex] Exception: " + ex.Message);
          this.Unmount();
          throw;
        }
      }
    }

    public override void Unmount()
    {
      lock (MobileCoreVHD._lockObj)
      {
        CommonUtils.DismountVHD(this._hndlVirtDisk);
        this._hndlVirtDisk = IntPtr.Zero;
        this.m_partitions.Clear();
        this.IsMounted = false;
      }
    }
  }
}
