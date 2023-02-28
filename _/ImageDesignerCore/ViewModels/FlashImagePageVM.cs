// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.FlashImagePageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using FFUComponents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class FlashImagePageVM : IDViewModelBase
  {
    public string _ffuFilePath = string.Empty;
    private bool _skipFlash;
    public static readonly DependencyProperty ConnectedDevicesProperty = DependencyProperty.Register(nameof (ConnectedDevices), typeof (ObservableCollection<IFFUDevice>), typeof (FlashImagePageVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty SelectedDeviceIndexProperty = DependencyProperty.Register(nameof (SelectedDeviceIndex), typeof (int), typeof (FlashImagePageVM), new PropertyMetadata((object) -1));
    public static readonly DependencyProperty FlashProgressProperty = DependencyProperty.Register(nameof (FlashProgress), typeof (double), typeof (FlashImagePageVM), new PropertyMetadata((object) 0.0));
    public static readonly DependencyProperty ProgressTextProperty = DependencyProperty.Register(nameof (ProgressText), typeof (string), typeof (FlashImagePageVM), new PropertyMetadata((object) "0"));
    public static readonly DependencyProperty FlashResultProperty = DependencyProperty.Register(nameof (FlashResult), typeof (FlashingResult), typeof (FlashImagePageVM), new PropertyMetadata((PropertyChangedCallback) null));
    private bool _canFlash = true;
    private bool _flashInProgress;
    private DelegateCommand<bool> _getConnectedDevicesCommand;
    private DelegateCommand<bool> _beginFlashCommand;
    private DeviceMonitor _devicemon;
    private DeviceFlasher _deviceFlash;

    internal FlashImagePageVM(IDStates mystate)
      : base(mystate)
    {
      this.ConnectedDevices = new ObservableCollection<IFFUDevice>();
      this._devicemon = new DeviceMonitor();
      this._devicemon.OnDeviceArrival += new DeviceMonitorEventHandler(this._devicemon_OnDeviceArrival);
      this._devicemon.OnDeviceRemoval += new DeviceMonitorEventHandler(this._devicemon_OnDeviceRemoval);
    }

    private void _devicemon_OnDeviceArrival(object sender, EventArgs e)
    {
      if (Application.Current != null)
        Application.Current.Dispatcher.Invoke((Delegate) (() => this.GetConnectedDevicesCommand.Execute((object) this)));
      else
        this.GetConnectedDevicesCommand.Execute((object) this);
    }

    private void _devicemon_OnDeviceRemoval(object sender, EventArgs e)
    {
      if (Application.Current != null)
        Application.Current.Dispatcher.Invoke((Delegate) (() => this.GetConnectedDevicesCommand.Execute((object) this)));
      else
        this.GetConnectedDevicesCommand.Execute((object) this);
    }

    private void OnProgressChange(object sender, ProgressChangedEventArgs e)
    {
      if (e.UserState == null)
        return;
      if (Application.Current != null)
      {
        Application.Current.Dispatcher.Invoke((Delegate) (() => this.FlashProgress = (double) e.UserState));
        Application.Current.Dispatcher.Invoke((Delegate) (() => this.ProgressText = string.Format("{0,2:0}", (object) this.FlashProgress)));
      }
      else
      {
        this.FlashProgress = (double) e.UserState;
        this.ProgressText = string.Format("{0,2:0}", (object) this.FlashProgress);
      }
    }

    private void OnFlashingComplete(object sender, RunWorkerCompletedEventArgs e)
    {
      Tools.DispatcherExec<bool>((Func<bool>) (() => this.FlashInProgress = false));
      this.FlashResult = e.Result as FlashingResult;
      this.OnPropertyChanged("FlashComplete");
    }

    protected override void Validate()
    {
      bool flag = false;
      if (this.SkipFlash)
        flag = true;
      else if (!string.IsNullOrEmpty(this.FFUFilePath) && File.Exists(this.FFUFilePath) && this.ConnectedDevices.Count > 0 && this.SelectedDeviceIndex >= 0)
        flag = true;
      this.CanFlash = flag;
    }

    protected override void ComputeNextState() => this._nextState = new IDStates?(IDStates.End);

    internal override bool OnStateEntry()
    {
      this.Initialize();
      return true;
    }

    internal override bool OnStateExit()
    {
      this.Deinitialize();
      return true;
    }

    private void Initialize()
    {
      FFUManager.Start();
      this._deviceFlash = new DeviceFlasher(this.FFUFilePath);
      this._devicemon.StartMonitor();
      this.GetConnectedDevices();
    }

    private void Deinitialize()
    {
      this._devicemon.StopMonitor();
      FFUManager.Stop();
    }

    protected override void OnApplicationExit() => this.Deinitialize();

    public string FFUFilePath
    {
      get
      {
        if (string.IsNullOrEmpty(this._ffuFilePath))
          this._ffuFilePath = this.Context.FFUPath;
        return this._ffuFilePath;
      }
      set
      {
        this._ffuFilePath = value;
        this.Validate();
        this.OnPropertyChanged(nameof (FFUFilePath));
      }
    }

    public bool SkipFlash
    {
      get => this._skipFlash;
      set
      {
        this._skipFlash = value;
        this.Validate();
      }
    }

    public ObservableCollection<IFFUDevice> ConnectedDevices
    {
      get => (ObservableCollection<IFFUDevice>) this.GetValue(FlashImagePageVM.ConnectedDevicesProperty);
      set => this.SetValue(FlashImagePageVM.ConnectedDevicesProperty, (object) value);
    }

    public int SelectedDeviceIndex
    {
      get => (int) this.GetValue(FlashImagePageVM.SelectedDeviceIndexProperty);
      set => this.SetValue(FlashImagePageVM.SelectedDeviceIndexProperty, (object) value);
    }

    public double FlashProgress
    {
      get => (double) this.GetValue(FlashImagePageVM.FlashProgressProperty);
      set => this.SetValue(FlashImagePageVM.FlashProgressProperty, (object) value);
    }

    public string ProgressText
    {
      get => (string) this.GetValue(FlashImagePageVM.ProgressTextProperty);
      set => this.SetValue(FlashImagePageVM.ProgressTextProperty, (object) value);
    }

    public FlashingResult FlashResult
    {
      get => (FlashingResult) this.GetValue(FlashImagePageVM.FlashResultProperty);
      set => this.SetValue(FlashImagePageVM.FlashResultProperty, (object) value);
    }

    public bool CanFlash
    {
      get => this._canFlash;
      set
      {
        if (this._canFlash == value)
          return;
        this._canFlash = value;
        this.OnPropertyChanged(nameof (CanFlash));
      }
    }

    public bool FlashInProgress
    {
      get => this._flashInProgress;
      set
      {
        if (this._flashInProgress == value)
          return;
        this._flashInProgress = value;
        this.OnPropertyChanged(nameof (FlashInProgress));
        this.CanFlash = !value;
      }
    }

    private static void FlashImagePathChanged(
      DependencyObject o,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(o is FlashImagePageVM flashImagePageVm))
        return;
      flashImagePageVm.Validate();
    }

    public DelegateCommand<bool> GetConnectedDevicesCommand
    {
      get
      {
        if (this._getConnectedDevicesCommand == null)
          this._getConnectedDevicesCommand = new DelegateCommand<bool>((Func<bool>) (() => this.GetConnectedDevices()), (Func<bool>) (() => true));
        return this._getConnectedDevicesCommand;
      }
    }

    private bool GetConnectedDevices()
    {
      this.ConnectedDevices.Clear();
      List<IFFUDevice> list = Tools.GetFlashableDevices().ToList<IFFUDevice>();
      if (list.Count > 0)
      {
        foreach (IFFUDevice ffuDevice in list)
          this.ConnectedDevices.Add(ffuDevice);
        this.SelectedDeviceIndex = 0;
      }
      this.Validate();
      return true;
    }

    public DelegateCommand<bool> BeginFlashCommand
    {
      get
      {
        if (this._beginFlashCommand == null)
          this._beginFlashCommand = new DelegateCommand<bool>((Func<bool>) (() => this.BeginFlash()), (Func<bool>) (() => this.CanFlash));
        return this._beginFlashCommand;
      }
    }

    private bool BeginFlash()
    {
      if (!this.SkipFlash)
      {
        int selectedDeviceIndex = this.SelectedDeviceIndex;
        this._deviceFlash.FFUFilePath = this.FFUFilePath;
        this._deviceFlash.FFUDevice = this.ConnectedDevices[selectedDeviceIndex];
        this._deviceFlash.OnFlashProgressChange += new DeviceFlasherProgressEventHandler(this.OnProgressChange);
        this._deviceFlash.OnFlashingComplete += new DeviceFlasherEventHandler(this.OnFlashingComplete);
        this.FlashResult = (FlashingResult) null;
        this._deviceFlash.BeginFlashAsync();
        this.FlashInProgress = true;
        this.OnPropertyChanged("FlashInProgress");
      }
      return true;
    }

    public void WaitForCompletion()
    {
      if (!this.FlashInProgress || this._deviceFlash == null)
        return;
      this._deviceFlash.WaitForCompletion();
    }
  }
}
