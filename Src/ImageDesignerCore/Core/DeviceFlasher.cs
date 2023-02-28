// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.DeviceFlasher
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using FFUComponents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class DeviceFlasher
  {
    private BackgroundWorker _bw;
    private static int endlineWritten;
    private static long bytesFlashed;
    private static AutoResetEvent progressFinishedEvent;

    public DeviceFlasher(string ffuFilePath)
    {
      this.FFUFilePath = ffuFilePath;
      DeviceFlasher.progressFinishedEvent = new AutoResetEvent(false);
    }

    public IFFUDevice FFUDevice { get; set; }

    public string FFUFilePath { get; internal set; }

    public bool ShowConsoleOutput { get; set; }

    public event DeviceFlasherEventHandler OnFlashingComplete;

    public event DeviceFlasherProgressEventHandler OnFlashProgressChange;

    public int BeginFlashAsync()
    {
      this._bw = new BackgroundWorker();
      this._bw.DoWork += new DoWorkEventHandler(this.DoWorkAsync);
      this._bw.WorkerReportsProgress = true;
      this._bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.FlashAsyncCompleted);
      this._bw.RunWorkerAsync((object) this);
      return 0;
    }

    public void WaitForCompletion() => DeviceFlasher.progressFinishedEvent.WaitOne();

    private void DoWorkAsync(object sender, DoWorkEventArgs e)
    {
      IFFUDevice ffuDevice = this.FFUDevice;
      FlashingResult flashingResult = new FlashingResult();
      e.Result = (object) flashingResult;
      if (ffuDevice == null)
      {
        List<IFFUDevice> list = Tools.GetFlashableDevices().ToList<IFFUDevice>();
        if (list.Count > 0)
        {
          ffuDevice = list[0];
        }
        else
        {
          flashingResult.Error = 1;
          flashingResult.ErrorMessage = Tools.GetString("txtNoDeviceFoundError");
        }
        if (this.ShowConsoleOutput)
        {
          Console.WriteLine("Found device:\nName:\t{0}\nID:\t{1}\n", (object) ffuDevice.DeviceFriendlyName, (object) ffuDevice.DeviceUniqueID);
          Console.WriteLine("Flashing: {0:s}", (object) Path.GetFileName(this.FFUFilePath));
        }
      }
      ffuDevice.ProgressEvent += new EventHandler<ProgressEventArgs>(this.FFUProgressEvent);
      try
      {
        ffuDevice.EndTransfer();
        ffuDevice.FlashFFUFile(this.FFUFilePath);
      }
      catch (Exception ex)
      {
        flashingResult.Error = 1;
        flashingResult.Exception = ex;
        flashingResult.ErrorMessage = ex.GetExceptionMessage();
        Console.WriteLine(flashingResult.ErrorMessage);
      }
    }

    private void FFUProgressEvent(object sender, ProgressEventArgs e)
    {
      double userState = (double) e.Position / (double) e.Length * 100.0;
      if (this.OnFlashProgressChange != null)
        this.OnFlashProgressChange((object) this, new ProgressChangedEventArgs(0, (object) userState));
      if (!this.ShowConsoleOutput)
        return;
      double progressFraction = (double) e.Position / (double) e.Length;
      if (progressFraction > 1.0)
        progressFraction = 1.0;
      Console.Write(DeviceFlasher.CreateProgressDisplay(progressFraction, Console.WindowWidth));
      if (1.0 != progressFraction || 1 != Interlocked.Increment(ref DeviceFlasher.endlineWritten))
        return;
      DeviceFlasher.bytesFlashed = e.Length;
      Console.WriteLine();
      DeviceFlasher.progressFinishedEvent.Set();
    }

    private void FlashAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      if (this.OnFlashingComplete == null)
        return;
      this.OnFlashingComplete((object) this, e);
    }

    private static string CreateProgressDisplay(double progressFraction, int width)
    {
      int num = (int) Math.Floor(50.0 * progressFraction);
      StringBuilder stringBuilder = new StringBuilder(2 * width);
      for (int index = 0; index < width; ++index)
        stringBuilder.Append('\b');
      stringBuilder.Append('[');
      for (int index = 0; index < num; ++index)
        stringBuilder.Append('=');
      if (num < 50)
      {
        stringBuilder.Append('>');
        ++num;
      }
      for (int index = num; index < 50; ++index)
        stringBuilder.Append(' ');
      stringBuilder.Append("]  ");
      stringBuilder.AppendFormat("{0:0.00%}", (object) progressFraction);
      return stringBuilder.ToString();
    }
  }
}
