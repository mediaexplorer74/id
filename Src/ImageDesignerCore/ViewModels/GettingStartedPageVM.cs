// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.GettingStartedPageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class GettingStartedPageVM : IDViewModelBase
  {
    public static readonly DependencyProperty OptionNewImageProperty = DependencyProperty.Register("OptionNewImage", typeof (Workflow), typeof (GettingStartedPageVM), new PropertyMetadata((object) Workflow.CreateImage, new PropertyChangedCallback(GettingStartedPageVM.OnWorkflowChanged)));

    internal GettingStartedPageVM(IDStates mystate)
      : base(mystate)
    {
    }

    internal override bool OnStateEntry()
    {
      string userConfig = UserConfig.GetUserConfig("LastSavedProject");
      if (!string.IsNullOrWhiteSpace(userConfig))
      {
        try
        {
          ProjectXml.Validate(userConfig);
          this.SelectedStartOption = Workflow.ModifyImage;
        }
        catch
        {
        }
      }
      return true;
    }

    internal override bool OnStateExit() => true;

    protected override void Validate() => this.IsValid = true;

    protected override void ComputeNextState()
    {
      switch (this.SelectedStartOption)
      {
        case Workflow.CreateImage:
          this._nextState = new IDStates?(IDStates.SettingUp);
          break;
        case Workflow.ModifyImage:
          this._nextState = new IDStates?(IDStates.ModifyImage);
          break;
        case Workflow.FlashImage:
          this._nextState = new IDStates?(IDStates.FlashImage);
          break;
      }
    }

    public Workflow SelectedStartOption
    {
      get => (Workflow) this.GetValue(GettingStartedPageVM.OptionNewImageProperty);
      set => this.SetValue(GettingStartedPageVM.OptionNewImageProperty, (object) value);
    }

    private static void OnWorkflowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GettingStartedPageVM gettingStartedPageVm = d as GettingStartedPageVM;
      if ((Workflow) e.NewValue != Workflow.CreateImage)
        return;
      gettingStartedPageVm.Context.Controller.NewProject();
    }
  }
}
