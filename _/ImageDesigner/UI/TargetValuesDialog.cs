// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.TargetValuesDialog
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
  public class TargetValuesDialog : Window, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageInstructions;
    internal TextBox tbName;
    internal TextBox tbCommon1;
    internal TextBox tbCommon2;
    internal TextBox tbCommon3;
    internal TextBox tbOtherName1;
    internal TextBox tbOther1;
    internal TextBox tbOtherName2;
    internal TextBox tbOther2;
    internal TextBlock tb1;
    internal Button btOK;
    internal Button btCancel;
    private bool _contentLoaded;

    public TargetValuesDialog(TargetValuesDialogVM vm)
    {
      this.InitializeComponent();
      this.DataContext = (object) vm;
    }

    private void btCancel_Click(object sender, RoutedEventArgs e)
    {
      TargetValuesDialogVM dataContext = this.DataContext as TargetValuesDialogVM;
      dataContext.Result = CustomGetValueResult.Cancel;
      if (!dataContext.OnExit())
        return;
      this.DialogResult = new bool?(false);
      this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      TargetValuesDialogVM dataContext = this.DataContext as TargetValuesDialogVM;
      dataContext.OnLoad();
      if (dataContext.DialogType == CustomDialogType.NewVariantDialog)
      {
        this.tbName.Focus();
        this.tbName.SelectAll();
      }
      else
        this.tbCommon1.Focus();
    }

    private void btOK_Click(object sender, RoutedEventArgs e)
    {
      TargetValuesDialogVM dataContext = this.DataContext as TargetValuesDialogVM;
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
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/dialogs/targetvaluesdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
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
          this.tbName = (TextBox) target;
          break;
        case 5:
          this.tbCommon1 = (TextBox) target;
          break;
        case 6:
          this.tbCommon2 = (TextBox) target;
          break;
        case 7:
          this.tbCommon3 = (TextBox) target;
          break;
        case 8:
          this.tbOtherName1 = (TextBox) target;
          break;
        case 9:
          this.tbOther1 = (TextBox) target;
          break;
        case 10:
          this.tbOtherName2 = (TextBox) target;
          break;
        case 11:
          this.tbOther2 = (TextBox) target;
          break;
        case 12:
          this.tb1 = (TextBlock) target;
          break;
        case 13:
          this.btOK = (Button) target;
          this.btOK.Click += new RoutedEventHandler(this.btOK_Click);
          break;
        case 14:
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
