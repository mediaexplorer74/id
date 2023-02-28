// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.CustomGetValueDialog
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI
{
  public class CustomGetValueDialog : Window, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageInstructions;
    internal TextBox tbPageEdit;
    internal ComboBox cbPageValue;
    internal TextBlock tb1;
    internal Button btOK;
    internal Button btCancel;
    private bool _contentLoaded;

    public CustomGetValueDialog(CustomGetValueVM vm)
    {
      this.InitializeComponent();
      this.DataContext = (object) vm;
    }

    private void btCancel_Click(object sender, RoutedEventArgs e)
    {
      CustomGetValueVM dataContext = this.DataContext as CustomGetValueVM;
      dataContext.Result = CustomGetValueResult.Cancel;
      if (!dataContext.OnExit())
        return;
      this.DialogResult = new bool?(false);
      this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      CustomGetValueVM dataContext = this.DataContext as CustomGetValueVM;
      dataContext.OnLoad();
      if (dataContext.DialogType == CustomDialogType.GetValueComboBoxDialog)
      {
        if (this.cbPageValue.SelectedIndex == -1)
          this.cbPageValue.SelectedIndex = 0;
        this.cbPageValue.Focus();
      }
      else
      {
        this.tbPageEdit.Focus();
        this.tbPageEdit.SelectAll();
      }
    }

    private void btOK_Click(object sender, RoutedEventArgs e)
    {
      CustomGetValueVM dataContext = this.DataContext as CustomGetValueVM;
      dataContext.Result = CustomGetValueResult.OK;
      if (!dataContext.OnExit())
        return;
      this.DialogResult = new bool?(true);
      this.Close();
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/dialogs/customgetvaluedialog.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.Window_Loaded);
          break;
        case 2:
          this.tbPageTitle = (TextBlock) target;
          break;
        case 3:
          this.tbPageInstructions = (TextBlock) target;
          break;
        case 4:
          this.tbPageEdit = (TextBox) target;
          break;
        case 5:
          this.cbPageValue = (ComboBox) target;
          break;
        case 6:
          this.tb1 = (TextBlock) target;
          break;
        case 7:
          this.btOK = (Button) target;
          this.btOK.Click += new RoutedEventHandler(this.btOK_Click);
          break;
        case 8:
          this.btCancel = (Button) target;
          this.btCancel.Click += new RoutedEventHandler(this.btCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
