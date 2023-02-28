// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.SettingUpPageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.IO;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class SettingUpPageVM : IDViewModelBase
  {
    public static readonly DependencyProperty ComponentDriversPathProperty = DependencyProperty.Register(nameof (ComponentDriversPath), typeof (string), typeof (SettingUpPageVM), new PropertyMetadata((object) string.Empty, new PropertyChangedCallback(SettingUpPageVM.OnPropertyChanged)));
    public static readonly DependencyProperty BSPConfigFilePathProperty = DependencyProperty.Register(nameof (BSPConfigFilePath), typeof (string), typeof (SettingUpPageVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty OutputPathProperty = DependencyProperty.Register(nameof (OutputPath), typeof (string), typeof (SettingUpPageVM), new PropertyMetadata((object) string.Empty, new PropertyChangedCallback(SettingUpPageVM.OnPropertyChanged)));

    internal SettingUpPageVM(IDStates mystate)
      : base(mystate)
    {
    }

    protected override bool SaveSupported => true;

    internal override bool OnStateEntry()
    {
      string path = this.OutputPath;
      if (string.IsNullOrWhiteSpace(path))
        path = Constants.DEFAULT_OUTPUT_FOLDER;
      if (Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(path))
      {
        int num = 1;
        do
        {
          path = Path.Combine(Constants.DEFAULT_OUTPUT_ROOT, string.Format("{0}_{1}\\", (object) "Project", (object) num));
          ++num;
        }
        while (Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(path));
      }
      this.OutputPath = path;
      if (this.Context.BSPConfig == null)
      {
        string bspConfigFile = this.GetBSPConfigFile();
        if (!string.IsNullOrEmpty(bspConfigFile))
        {
          this.Context.BSPConfig = BSPConfig.LoadFromXml(bspConfigFile, this.Context.MSPackageRoot);
          this.BSPConfigFilePath = bspConfigFile;
        }
      }
      else
        this.BSPConfigFilePath = this.Context.BSPConfig.BSPConfigFile;
      if (this.Context.BSPConfig == null || !this.Context.BSPConfig.IsValidBSP(this.Context.MSPackageRoot))
        this.IsValid = false;
      else
        this.IsValid = true;
      return true;
    }

    private string GetBSPConfigFile()
    {
      string path = string.Empty;
      bool flag = false;
      if (this.Context.BSPConfig != null)
      {
        path = this.Context.BSPConfig.BSPConfigFile;
        flag = Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(path);
      }
      if (!flag && Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(this.Context.BSPRoot))
      {
        path = Path.Combine(this.Context.BSPRoot, "Bsp.Config.xml");
        flag = Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Exists(path);
      }
      if (!flag)
      {
        path = UserConfig.GetUserConfig("LastUsedBspConfig");
        flag = Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Exists(path);
      }
      if (!flag)
        path = string.Empty;
      return path;
    }

    protected override bool SavePage()
    {
      Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.CreateDirectory(this.OutputPath);
      Directory.Exists(this.OutputPath);
      if (this.Context.Workflow == Workflow.CreateImage)
        this.Context.Controller.NewProject();
      UserConfig.SaveUserConfig("LastUsedBspConfig", this.Context.BSPConfig.BSPConfigFile);
      return true;
    }

    protected override void Validate()
    {
      if (this.Context.BSPConfig == null || string.IsNullOrEmpty(this.Context.BSPConfig.BSPRoot) || !Directory.Exists(this.Context.BSPConfig.BSPRoot))
        this.IsValid = false;
      else if (string.IsNullOrEmpty(this.OutputPath))
        this.IsValid = false;
      else
        this.IsValid = true;
    }

    protected override void ComputeNextState() => this._nextState = new IDStates?(IDStates.SelectImageType);

    public string ComponentDriversPath
    {
      get => (string) this.GetValue(SettingUpPageVM.ComponentDriversPathProperty);
      set => this.SetValue(SettingUpPageVM.ComponentDriversPathProperty, (object) value);
    }

    public string BSPConfigFilePath
    {
      get => (string) this.GetValue(SettingUpPageVM.BSPConfigFilePathProperty);
      set => this.SetValue(SettingUpPageVM.BSPConfigFilePathProperty, (object) value);
    }

    public string OutputPath
    {
      get => (string) this.GetValue(SettingUpPageVM.OutputPathProperty);
      set => this.SetValue(SettingUpPageVM.OutputPathProperty, (object) value);
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as SettingUpPageVM).Validate();
  }
}
