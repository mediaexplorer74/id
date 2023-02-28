// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.BuildImagePageVM
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
  public class BuildImagePageVM : IDViewModelBase
  {
    private bool _enableBuildButton;
    private bool _buildInProgress;
    private bool _cancelInProgress;
    private bool _skipBuild;
    private string _buildCommandLine = string.Empty;
    private DelegateCommand<bool> _buildImageCommand;
    private DelegateCommand<bool> _viewLogCommand;
    private ImageBuilder _imageBuilder;

    internal BuildImagePageVM(IDStates mystate)
      : base(mystate)
    {
    }

    protected override bool CancelSupported => true;

    private void Initialize()
    {
      this._imageBuilder = new ImageBuilder(Path.Combine(this.Context.OutputDir, "Flash.FFU"), this.Context.OEMInputFile);
      this._imageBuilder.AKRoot = this.Context.AKRoot;
      this._imageBuilder.BSPRoot = this.Context.BSPRoot;
      this._imageBuilder.ReadLogOutput = true;
      this._imageBuilder.OEMVersion = IDContext.Instance.CustomizationPkgVersion;
      if (this.Context.SelectedCustomizationChoice != CustomizationChoice.NoCustomization)
        this._imageBuilder.CustomizationFile = this.Context.CurrentOEMCustomizationFile;
      this._imageBuilder.BuildCompleted += new ImageBuilderEventHandler(this._imageBuilder_BuildCompleted);
      this._imageBuilder.BuildStarted += new ImageBuilderEventHandler(this._imageBuilder_BuildStarted);
      this._imageBuilder.CancelStarted += new ImageBuilderEventHandler(this._imageBuilder_CancelStarted);
      this._imageBuilder.CancelCompleted += new ImageBuilderEventHandler(this._imageBuilder_CancelCompleted);
      this.BuildCommandLine = string.Empty;
    }

    public string FFUPath { get; private set; }

    public int LastError
    {
      get
      {
        int lastError = 0;
        if (this._imageBuilder != null)
          lastError = this._imageBuilder.ExitCode;
        return lastError;
      }
    }

    public event BuildImagePageEventHandler BuildCompleted;

    public event BuildImagePageEventHandler BuildStarted;

    public event BuildImagePageEventHandler BuildCancelled;

    internal override bool OnStateEntry()
    {
      this.Initialize();
      this.EnableBuildButton = true;
      return true;
    }

    private void _imageBuilder_BuildStarted(object sender, ImageBuilderEventArgs e)
    {
      if (Application.Current != null)
      {
        this.EnableBuildButton = false;
        this.EnableCancelBuildButton = true;
      }
      this.FFUPath = string.Empty;
      this.BuildInProgress = true;
      this.CancelInProgress = false;
      if (this.BuildStarted == null)
        return;
      this.BuildStarted((object) this, new BuildImagePageEventArgs());
    }

    private string GetBuildCommandLine()
    {
      StringBuilder stringBuilder = new StringBuilder(500);
      stringBuilder.Append("\"" + new FileInfo(Environment.GetCommandLineArgs()[0]).FullName + "\"");
      stringBuilder.Append(" ");
      stringBuilder.Append("GenerateImage");
      stringBuilder.Append(" ");
      stringBuilder.Append("\"" + this._imageBuilder.FFUImagePath + "\"");
      stringBuilder.Append(" ");
      stringBuilder.Append("\"" + this._imageBuilder.OEMInputFile + "\"");
      stringBuilder.Append(" ");
      if (!string.IsNullOrWhiteSpace(this._imageBuilder.CustomizationFile))
      {
        stringBuilder.Append("/CustomizationFile");
        stringBuilder.Append(":");
        stringBuilder.Append("\"" + this._imageBuilder.CustomizationFile + "\"");
        stringBuilder.Append(" ");
      }
      stringBuilder.Append("/MSPackageRoot");
      stringBuilder.Append(":");
      stringBuilder.Append("\"" + this._imageBuilder.MSPackageRoot + "\"");
      stringBuilder.Append(" ");
      stringBuilder.Append("/BSPConfigFile");
      stringBuilder.Append(":");
      stringBuilder.Append("\"" + IDContext.Instance.BSPConfig.BSPConfigFile + "\"");
      return stringBuilder.ToString();
    }

    private void _imageBuilder_BuildCompleted(object sender, ImageBuilderEventArgs e)
    {
      if (Application.Current != null)
      {
        this.EnableBuildButton = true;
        this.EnableCancelBuildButton = false;
      }
      this.BuildInProgress = false;
      this.CancelInProgress = false;
      if (e.ExitCode == 0)
      {
        this.FFUPath = this.ImageBuilder.FFUImagePath;
        Tools.DispatcherExec<bool>((Func<bool>) (() => this.IsValid = true));
      }
      if (this.BuildCompleted == null)
        return;
      this.BuildCompleted((object) this, new BuildImagePageEventArgs(e.ExitCode));
    }

    private void _imageBuilder_CancelStarted(object sender, ImageBuilderEventArgs e)
    {
      if (Application.Current != null)
      {
        this.EnableBuildButton = false;
        this.EnableCancelBuildButton = false;
      }
      this.CancelInProgress = true;
      this.BuildInProgress = false;
    }

    private void _imageBuilder_CancelCompleted(object sender, ImageBuilderEventArgs e)
    {
      if (Application.Current != null)
      {
        this.EnableBuildButton = true;
        this.EnableCancelBuildButton = false;
      }
      this.BuildInProgress = false;
      this.CancelInProgress = false;
      if (this.BuildCancelled == null)
        return;
      this.BuildCancelled((object) this, new BuildImagePageEventArgs(e.ExitCode));
    }

    protected override void Validate() => this.IsValid = this.SkipBuild || this._imageBuilder != null && this._imageBuilder.BuildSucceeded;

    protected override void ComputeNextState() => this._nextState = new IDStates?(IDStates.BuildSuccess);

    public ImageBuilder ImageBuilder => this._imageBuilder;

    public bool EnableBuildButton
    {
      get => this._enableBuildButton;
      private set
      {
        this._enableBuildButton = value;
        this.OnPropertyChanged(nameof (EnableBuildButton));
      }
    }

    public bool EnableCancelBuildButton
    {
      get
      {
        Func<bool> method = (Func<bool>) (() => this.Context.Controller.CanCancel);
        return Application.Current == null ? method() : (bool) Application.Current.Dispatcher.Invoke((Delegate) method);
      }
      private set
      {
        Action method = (Action) (() => this.Context.Controller.CanCancel = value);
        if (Application.Current != null)
          Application.Current.Dispatcher.Invoke((Delegate) method);
        else
          method();
      }
    }

    public bool BuildInProgress
    {
      get => this._buildInProgress;
      private set
      {
        this._buildInProgress = value;
        this.OnPropertyChanged(nameof (BuildInProgress));
      }
    }

    public bool CancelInProgress
    {
      get => this._cancelInProgress;
      private set
      {
        this._cancelInProgress = value;
        this.OnPropertyChanged(nameof (CancelInProgress));
      }
    }

    public bool SkipBuild
    {
      get => this._skipBuild;
      set
      {
        this._skipBuild = value;
        this.Validate();
      }
    }

    public string BuildCommandLine
    {
      get => this._buildCommandLine;
      set
      {
        this._buildCommandLine = value;
        this.OnPropertyChanged(nameof (BuildCommandLine));
      }
    }

    public DelegateCommand<bool> BuildImageCommand
    {
      get
      {
        if (this._buildImageCommand == null)
          this._buildImageCommand = new DelegateCommand<bool>((Func<bool>) (() => this.BuildImageInternal()), (Func<bool>) (() => this.CanBuild));
        return this._buildImageCommand;
      }
    }

    public bool CanBuild => true;

    private bool BuildImageInternal()
    {
      if (!this.SkipBuild)
      {
        this.BuildCommandLine = this.GetBuildCommandLine();
        Console.WriteLine("Image Designer Command line: {0}", (object) this.BuildCommandLine);
        this._imageBuilder.BuildAsync();
      }
      return true;
    }

    public void CancelBuild()
    {
      this._imageBuilder.CancelBuild();
      this.Context.Controller.CanCancel = false;
    }

    public void WaitForBuildCompletion()
    {
      if (this._imageBuilder == null || !this._imageBuilder.BuildInProgress)
        return;
      this._imageBuilder.WaitForCompletion();
    }

    public DelegateCommand<bool> ViewLogCommand
    {
      get
      {
        if (this._viewLogCommand == null)
          this._viewLogCommand = new DelegateCommand<bool>((Func<bool>) (() => this.ViewLogInternal()), (Func<bool>) (() => this.CanViewLog));
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
