// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.CustomGetValueVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.Collections.ObjectModel;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class CustomGetValueVM : DependencyObject
  {
    public static readonly DependencyProperty ShowHeaderProperty = DependencyProperty.Register(nameof (ShowHeader), typeof (bool), typeof (CustomGetValueVM), new PropertyMetadata((object) true));
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof (Header), typeof (string), typeof (CustomGetValueVM), new PropertyMetadata((object) nameof (Header)));
    public static readonly DependencyProperty InstructionsProperty = DependencyProperty.Register(nameof (Instructions), typeof (string), typeof (CustomGetValueVM), new PropertyMetadata((object) nameof (Instructions)));
    public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(nameof (Result), typeof (CustomGetValueResult), typeof (CustomGetValueVM), new PropertyMetadata((object) CustomGetValueResult.Cancel));
    public static readonly DependencyProperty ShowDialogNextTimeProperty = DependencyProperty.Register(nameof (ShowDialogNextTime), typeof (bool), typeof (CustomGetValueVM), new PropertyMetadata((object) true));
    public static readonly DependencyProperty DialogTypeProperty = DependencyProperty.Register(nameof (DialogType), typeof (CustomDialogType), typeof (CustomGetValueVM), new PropertyMetadata((object) CustomDialogType.Invalid));
    public static readonly DependencyProperty ValueListProperty = DependencyProperty.Register(nameof (ValueList), typeof (ObservableCollection<string>), typeof (CustomGetValueVM), new PropertyMetadata((object) new ObservableCollection<string>()));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof (Value), typeof (string), typeof (CustomGetValueVM), new PropertyMetadata((object) ""));

    public CustomGetValueVM()
    {
    }

    public CustomGetValueVM(CustomDialogType type) => this.DialogType = type;

    public bool ShowHeader
    {
      get => (bool) this.GetValue(CustomGetValueVM.ShowHeaderProperty);
      set => this.SetValue(CustomGetValueVM.ShowHeaderProperty, (object) value);
    }

    public string Header
    {
      get => (string) this.GetValue(CustomGetValueVM.HeaderProperty);
      set => this.SetValue(CustomGetValueVM.HeaderProperty, (object) value);
    }

    public string Instructions
    {
      get => (string) this.GetValue(CustomGetValueVM.InstructionsProperty);
      set => this.SetValue(CustomGetValueVM.InstructionsProperty, (object) value);
    }

    public CustomGetValueResult Result
    {
      get => (CustomGetValueResult) this.GetValue(CustomGetValueVM.ResultProperty);
      set => this.SetValue(CustomGetValueVM.ResultProperty, (object) value);
    }

    public bool ShowDialogNextTime
    {
      get => (bool) this.GetValue(CustomGetValueVM.ShowDialogNextTimeProperty);
      set => this.SetValue(CustomGetValueVM.ShowDialogNextTimeProperty, (object) value);
    }

    public CustomDialogType DialogType
    {
      get => (CustomDialogType) this.GetValue(CustomGetValueVM.DialogTypeProperty);
      set => this.SetValue(CustomGetValueVM.DialogTypeProperty, (object) value);
    }

    public ObservableCollection<string> ValueList
    {
      get => (ObservableCollection<string>) this.GetValue(CustomGetValueVM.ValueListProperty);
      set => this.SetValue(CustomGetValueVM.ValueListProperty, (object) value);
    }

    public string Value
    {
      get => (string) this.GetValue(CustomGetValueVM.ValueProperty);
      set => this.SetValue(CustomGetValueVM.ValueProperty, (object) value);
    }

    public bool OnLoad() => true;

    public bool OnExit() => true;
  }
}
