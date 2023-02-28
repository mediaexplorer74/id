// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.CustomizationChoicePageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System.IO;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class CustomizationChoicePageVM : IDViewModelBase
  {
    public static readonly DependencyProperty SelectedCustomizationChoiceProperty = DependencyProperty.Register(nameof (SelectedCustomizationChoice), typeof (CustomizationChoice), typeof (CustomizationChoicePageVM), new PropertyMetadata((object) CustomizationChoice.NoCustomization));

    internal CustomizationChoicePageVM(IDStates mystate)
      : base(mystate)
    {
    }

    protected override bool SaveSupported => true;

    internal override bool OnStateExit()
    {
      if (this.SelectedCustomizationChoice == CustomizationChoice.FullCustomization)
      {
        this.Context.CurrentImageCustomization = (ImageCustomizations) null;
        if (File.Exists(this.Context.CurrentOEMCustomizationFile))
          File.Delete(this.Context.CurrentOEMCustomizationFile);
      }
      return base.OnStateExit();
    }

    protected override void Validate() => this.IsValid = true;

    protected override void ComputeNextState()
    {
      switch (this.SelectedCustomizationChoice)
      {
        case CustomizationChoice.NoCustomization:
          this._nextState = new IDStates?(IDStates.BuildImage);
          break;
        case CustomizationChoice.CustomizeFromTemplates:
          this._nextState = new IDStates?(IDStates.SelectTemplates);
          break;
        case CustomizationChoice.FullCustomization:
          this._nextState = new IDStates?(IDStates.CustomizeOS);
          break;
        default:
          this._nextState = new IDStates?(IDStates.Invalid);
          break;
      }
    }

    public CustomizationChoice SelectedCustomizationChoice
    {
      get => (CustomizationChoice) this.GetValue(CustomizationChoicePageVM.SelectedCustomizationChoiceProperty);
      set => this.SetValue(CustomizationChoicePageVM.SelectedCustomizationChoiceProperty, (object) value);
    }
  }
}
