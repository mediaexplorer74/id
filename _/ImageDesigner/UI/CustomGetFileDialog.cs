// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.CustomGetFileDialog
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.Win32;
using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI
{
  public class CustomGetFileDialog : Window, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageInstructions;
    internal TextBlock tbPageDisplayNameLabel;
    internal TextBox tbPageDisplayName;
    internal TextBlock tbPageFileLabel;
    internal TextBox tbPageFile;
    internal Image bBrowseFilename;
    internal TextBlock tb1;
    internal Button btOK;
    internal Button btCancel;
    private bool _contentLoaded;

    public CustomGetFileDialog(CustomGetFileVM vm)
    {
      this.InitializeComponent();
      this.DataContext = (object) vm;
    }

    private void btCancel_Click(object sender, RoutedEventArgs e)
    {
      CustomGetFileVM dataContext = this.DataContext as CustomGetFileVM;
      dataContext.Result = CustomGetFileResult.Cancel;
      if (!dataContext.OnExit())
        return;
      this.DialogResult = new bool?(false);
      this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      (this.DataContext as CustomGetFileVM).OnLoad();
      this.tbPageFile.Focus();
      this.tbPageFile.SelectAll();
    }

    private void btOK_Click(object sender, RoutedEventArgs e)
    {
      CustomGetFileVM dataContext = this.DataContext as CustomGetFileVM;
      dataContext.Result = CustomGetFileResult.OK;
      if (!dataContext.OnExit())
        return;
      this.DialogResult = new bool?(true);
      this.Close();
    }

    private void bBrowseFilename_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomGetFileVM dataContext = this.DataContext as CustomGetFileVM;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      if (!string.IsNullOrEmpty(dataContext.FileExtensionsFilter))
        openFileDialog.Filter = dataContext.FileExtensionsFilter;
      else
        openFileDialog.Filter = Tools.GetString("txtAllFilesFilter");
      bool? nullable = openFileDialog.ShowDialog();
      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
        return;
      dataContext.Filename = openFileDialog.FileName;
      string[] strArray = openFileDialog.Filter.Split('|');
      dataContext.FilterGroupName = strArray[(openFileDialog.FilterIndex - 1) * 2];
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/dialogs/customgetfiledialog.xaml", UriKind.Relative));
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
          this.tbPageDisplayNameLabel = (TextBlock) target;
          break;
        case 5:
          this.tbPageDisplayName = (TextBox) target;
          break;
        case 6:
          this.tbPageFileLabel = (TextBlock) target;
          break;
        case 7:
          this.tbPageFile = (TextBox) target;
          break;
        case 8:
          this.bBrowseFilename = (Image) target;
          this.bBrowseFilename.MouseLeftButtonUp += new MouseButtonEventHandler(this.bBrowseFilename_MouseLeftButtonUp);
          break;
        case 9:
          this.tb1 = (TextBlock) target;
          break;
        case 10:
          this.btOK = (Button) target;
          this.btOK.Click += new RoutedEventHandler(this.btOK_Click);
          break;
        case 11:
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
