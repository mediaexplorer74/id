// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.BuildSuccessPageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class BuildSuccessPageVM : IDViewModelBase
  {
    public static readonly DependencyProperty FFUImagePathProperty = DependencyProperty.Register(nameof (FFUImagePath), typeof (string), typeof (BuildSuccessPageVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty CanFlashProperty = DependencyProperty.Register(nameof (CanFlash), typeof (bool), typeof (BuildSuccessPageVM), new PropertyMetadata((object) true));
    private IIDCommand<bool> _viewLogCommand;

    internal BuildSuccessPageVM(IDStates mystate)
      : base(mystate)
    {
    }

    internal override bool OnStateEntry() => true;

    protected override void Validate() => this.IsValid = true;

    protected override void ComputeNextState() => this._nextState = new IDStates?(IDStates.FlashImage);

    public string FFUImagePath
    {
      get => (string) this.GetValue(BuildSuccessPageVM.FFUImagePathProperty);
      set => this.SetValue(BuildSuccessPageVM.FFUImagePathProperty, (object) value);
    }

    public bool CanFlash
    {
      get => (bool) this.GetValue(BuildSuccessPageVM.CanFlashProperty);
      set => this.SetValue(BuildSuccessPageVM.CanFlashProperty, (object) value);
    }

    public IIDCommand<bool> ViewLogCommand
    {
      get
      {
        if (this._viewLogCommand == null)
          this._viewLogCommand = (IIDCommand<bool>) new DelegateCommand<bool>((Func<bool>) (() => this.ViewLogInternal()), (Func<bool>) (() => this.CanViewLog));
        return this._viewLogCommand;
      }
    }

    public bool CanViewLog => true;

    private bool ViewLogInternal()
    {
      string str = Path.Combine(this.Context.OutputDir, "Flash.ImageApp.log");
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.FileName = Path.Combine(Environment.GetEnvironmentVariable("windir"), "system32\\notepad.exe");
      startInfo.UseShellExecute = false;
      startInfo.CreateNoWindow = true;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("\"" + str + "\"");
      startInfo.Arguments = stringBuilder.ToString();
      ThreadPool.QueueUserWorkItem((WaitCallback) (param0 => new Process()
      {
        StartInfo = startInfo
      }.Start()));
      return true;
    }
  }
}
