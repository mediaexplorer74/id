// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.ModifyImagePageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class ModifyImagePageVM : IDViewModelBase
  {
    public static readonly DependencyProperty LastSavedProjectPathProperty = DependencyProperty.Register(nameof (LastSavedProjectPath), typeof (string), typeof (ModifyImagePageVM), new PropertyMetadata((PropertyChangedCallback) null));

    internal ModifyImagePageVM(IDStates mystate)
      : base(mystate)
    {
    }

    public string LastSavedProjectPath
    {
      get => (string) this.GetValue(ModifyImagePageVM.LastSavedProjectPathProperty);
      set => this.SetValue(ModifyImagePageVM.LastSavedProjectPathProperty, (object) value);
    }

    internal override bool OnStateEntry()
    {
      this.LastSavedProjectPath = UserConfig.GetUserConfig("LastSavedProject");
      try
      {
        ProjectXml.Validate(this.LastSavedProjectPath);
        this.IsValid = true;
      }
      catch (WPIDException ex)
      {
        int num = (int) MessageBox.Show(ex.Message, Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtError"));
        this.IsValid = false;
      }
      return true;
    }

    internal override bool OnStateExit()
    {
      if (!string.IsNullOrWhiteSpace(this.LastSavedProjectPath))
      {
        if (LongPathFile.Exists(this.LastSavedProjectPath))
        {
          try
          {
            this.Context.Controller.LoadProject(this.LastSavedProjectPath);
          }
          catch
          {
          }
        }
      }
      return true;
    }

    protected override void Validate()
    {
      try
      {
        this.Context.Controller.LoadProject(this.LastSavedProjectPath);
        this.IsValid = true;
      }
      catch
      {
        this.IsValid = false;
      }
    }

    protected override void ComputeNextState() => this._nextState = new IDStates?(IDStates.PickOutputLocation);
  }
}
