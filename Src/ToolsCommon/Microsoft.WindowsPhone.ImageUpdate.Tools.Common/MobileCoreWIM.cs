// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.MobileCoreWIM
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class MobileCoreWIM : MobileCoreImage
  {
    private const int SLEEP_1000 = 1000;
    private const int MAX_RETRY = 3;
    private bool _commitChanges;
    private string _tmpDir;
    private string _mountPoint;
    private static readonly object _lockObj = new object();

    internal MobileCoreWIM(string path)
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
          this.MountWIM(readOnly);
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

    private void MountWIM(bool readOnly)
    {
      lock (MobileCoreWIM._lockObj)
      {
        this.m_partitions.Clear();
        string str = string.Empty;
        if (!readOnly)
          str = FileUtils.GetTempDirectory();
        string tempDirectory = FileUtils.GetTempDirectory();
        if (CommonUtils.MountWIM(this.m_mobileCoreImagePath, tempDirectory, str))
        {
          this.m_partitions.Add(new ImagePartition("WIM", tempDirectory));
          this._tmpDir = str;
          this._mountPoint = tempDirectory;
          this._commitChanges = !readOnly;
        }
        else
        {
          FileUtils.DeleteTree(tempDirectory);
          if (string.IsNullOrEmpty(str))
            return;
          FileUtils.DeleteTree(str);
        }
      }
    }

    public override void Unmount()
    {
      lock (MobileCoreWIM._lockObj)
      {
        CommonUtils.DismountWIM(this.m_mobileCoreImagePath, this._mountPoint, this._commitChanges);
        this.m_partitions.Clear();
        this.IsMounted = false;
        if (!string.IsNullOrEmpty(this._tmpDir))
          FileUtils.DeleteTree(this._tmpDir);
        this._tmpDir = (string) null;
        this._mountPoint = (string) null;
      }
    }
  }
}
