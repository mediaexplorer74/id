// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.CustomMessageBoxVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class CustomMessageBoxVM : DependencyObject
  {
    public static readonly DependencyProperty ShowHeaderProperty = DependencyProperty.Register(nameof (ShowHeader), typeof (bool), typeof (CustomMessageBoxVM), new PropertyMetadata((object) true));
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof (Header), typeof (string), typeof (CustomMessageBoxVM), new PropertyMetadata((object) nameof (Header)));
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof (Message), typeof (string), typeof (CustomMessageBoxVM), new PropertyMetadata((object) nameof (Message)));
    public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(nameof (Result), typeof (CustomMessageBoxResult), typeof (CustomMessageBoxVM), new PropertyMetadata((object) CustomMessageBoxResult.No));
    public static readonly DependencyProperty ShowDialogNextTimeProperty = DependencyProperty.Register(nameof (ShowDialogNextTime), typeof (bool), typeof (CustomMessageBoxVM), new PropertyMetadata((object) true));
    public static readonly DependencyProperty ShowDialogNextTimeVisibleProperty = DependencyProperty.Register(nameof (ShowDialogNextTimeVisible), typeof (bool), typeof (CustomMessageBoxVM), new PropertyMetadata((object) false));
    public static readonly DependencyProperty DialogTypeProperty = DependencyProperty.Register(nameof (DialogType), typeof (CustomDialogType), typeof (CustomMessageBoxVM), new PropertyMetadata((object) CustomDialogType.Invalid));
    public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(nameof (Width), typeof (int), typeof (CustomMessageBoxVM), new PropertyMetadata((object) 250));
    public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(nameof (Height), typeof (int), typeof (CustomMessageBoxVM), new PropertyMetadata((object) 300));

    private CustomMessageBoxVM()
    {
    }

    public CustomMessageBoxVM(CustomDialogType type) => this.DialogType = type;

    public bool ShowHeader
    {
      get => (bool) this.GetValue(CustomMessageBoxVM.ShowHeaderProperty);
      set => this.SetValue(CustomMessageBoxVM.ShowHeaderProperty, (object) value);
    }

    public string Header
    {
      get => (string) this.GetValue(CustomMessageBoxVM.HeaderProperty);
      set => this.SetValue(CustomMessageBoxVM.HeaderProperty, (object) value);
    }

    public string Message
    {
      get => (string) this.GetValue(CustomMessageBoxVM.MessageProperty);
      set => this.SetValue(CustomMessageBoxVM.MessageProperty, (object) value);
    }

    public CustomMessageBoxResult Result
    {
      get => (CustomMessageBoxResult) this.GetValue(CustomMessageBoxVM.ResultProperty);
      set => this.SetValue(CustomMessageBoxVM.ResultProperty, (object) value);
    }

    public bool ShowDialogNextTime
    {
      get => (bool) this.GetValue(CustomMessageBoxVM.ShowDialogNextTimeProperty);
      set => this.SetValue(CustomMessageBoxVM.ShowDialogNextTimeProperty, (object) value);
    }

    public bool ShowDialogNextTimeVisible
    {
      get => (bool) this.GetValue(CustomMessageBoxVM.ShowDialogNextTimeVisibleProperty);
      set => this.SetValue(CustomMessageBoxVM.ShowDialogNextTimeVisibleProperty, (object) value);
    }

    public CustomDialogType DialogType
    {
      get => (CustomDialogType) this.GetValue(CustomMessageBoxVM.DialogTypeProperty);
      set => this.SetValue(CustomMessageBoxVM.DialogTypeProperty, (object) value);
    }

    public int Width
    {
      get => (int) this.GetValue(CustomMessageBoxVM.WidthProperty);
      set => this.SetValue(CustomMessageBoxVM.WidthProperty, (object) value);
    }

    public int Height
    {
      get => (int) this.GetValue(CustomMessageBoxVM.HeightProperty);
      set => this.SetValue(CustomMessageBoxVM.HeightProperty, (object) value);
    }

    public virtual bool OnLoad() => true;

    public virtual bool OnExit() => true;
  }
}
