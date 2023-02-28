// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.DeviceMonitor
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Management;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class DeviceMonitor : IDisposable
  {
    private const int DEVICE_ARRIVAL = 2;
    private const int DEVICE_REMOVAL = 3;
    private readonly object _lock = new object();
    private AutoResetEvent _are = new AutoResetEvent(false);
    private ManagementEventWatcher _deviceArrivalMonitor = new ManagementEventWatcher();
    private ManagementEventWatcher _deviceRemovalMonitor = new ManagementEventWatcher();
    private WqlEventQuery _arrivalQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'");
    private WqlEventQuery _removalQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'");

    public DeviceMonitor()
    {
      this._deviceArrivalMonitor.EventArrived += new EventArrivedEventHandler(this.DeviceAttached);
      this._deviceArrivalMonitor.Query = (EventQuery) this._arrivalQuery;
      this._deviceRemovalMonitor.EventArrived += new EventArrivedEventHandler(this.DeviceRemoved);
      this._deviceRemovalMonitor.Query = (EventQuery) this._removalQuery;
    }

    ~DeviceMonitor() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool Disposing)
    {
      if (!Disposing)
        return;
      this.StopMonitor();
    }

    public event DeviceMonitorEventHandler OnDeviceArrival;

    public event DeviceMonitorEventHandler OnDeviceRemoval;

    public void StartMonitor() => ThreadPool.QueueUserWorkItem((WaitCallback) (param0 =>
    {
      this._deviceArrivalMonitor.Start();
      this._deviceRemovalMonitor.Start();
      this._are.WaitOne();
    }));

    private void DeviceAttached(object obj, EventArrivedEventArgs e)
    {
      if (this.OnDeviceArrival == null)
        return;
      this.OnDeviceArrival((object) this, EventArgs.Empty);
    }

    private void DeviceRemoved(object obj, EventArrivedEventArgs e)
    {
      if (this.OnDeviceRemoval == null)
        return;
      this.OnDeviceRemoval((object) this, EventArgs.Empty);
    }

    public void StopMonitor()
    {
      this._are.Set();
      this._deviceArrivalMonitor.Stop();
      this._deviceRemovalMonitor.Stop();
    }
  }
}
