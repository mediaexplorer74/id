// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.SettingUpPage
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public class SettingUpPage : System.Windows.Controls.UserControl, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal TextBlock tbDrvLocTitle;
    internal TextBlock tbDrvLocDesc;
    internal TextBlock tbSectionDesc;
    internal System.Windows.Controls.TextBox tbBspPath;
    internal TextBlock tbChangeBspPathLink;
    internal Hyperlink hlChangeLocation;
    internal TextBlock tbOutputPathTitle;
    internal TextBlock tbOutputPathDesc;
    internal TextBlock tbCurrentOutputPath;
    internal System.Windows.Controls.TextBox tbChangeOutputPath;
    internal TextBlock tbChangeOutputPathLink;
    internal Hyperlink hlChangeOutputPath;
    private bool _contentLoaded;

    public SettingUpPage()
    {
      this.InitializeComponent();
      Run run1 = new Run(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("hlSettingUpChangeBSP"));
      this.hlChangeLocation.Inlines.Clear();
      this.hlChangeLocation.Inlines.Add((Inline) run1);
      Run run2 = new Run(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("hlSettingUpChangeOutputPath"));
      this.hlChangeOutputPath.Inlines.Clear();
      this.hlChangeOutputPath.Inlines.Add((Inline) run2);
    }

    private void hlChangeLocation_Click(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is SettingUpPageVM dataContext))
        throw new ArgumentException("DataContext object is null");
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.CheckFileExists = true;
      openFileDialog.Multiselect = false;
      openFileDialog.Filter = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtConfigFilter");
      string str1 = dataContext.ComponentDriversPath;
      if (string.IsNullOrEmpty(dataContext.ComponentDriversPath) || !Directory.Exists(dataContext.ComponentDriversPath))
      {
        string environmentVariable = Environment.GetEnvironmentVariable("BSPROOT");
        str1 = !Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(environmentVariable) ? Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) : environmentVariable;
      }
      openFileDialog.InitialDirectory = str1;
      if (openFileDialog.ShowDialog() != DialogResult.OK)
        return;
      string fileName = openFileDialog.FileName;
      try
      {
        string akRoot = IDContext.Instance.AKRoot;
        BSPConfig bspConfig = BSPConfig.LoadFromXml(fileName, IDContext.Instance.MSPackageRoot);
        IDContext.Instance.BSPConfig = bspConfig;
        dataContext.ComponentDriversPath = bspConfig.BSPRoot;
        dataContext.BSPConfigFilePath = fileName;
      }
      catch (WPIDException ex)
      {
        string str2 = ex.Message;
        if (ex.InnerException != null)
          str2 = str2 + Environment.NewLine + ex.InnerException.Message;
        string message = string.Format(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtInvalidBspConfigError"), (object) str2);
        CustomMessageBox.ShowMessage(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtError"), message);
      }
    }

    private void hlChangeOutputPath_Click(object sender, RoutedEventArgs e)
    {
      FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
      folderBrowserDialog.ShowNewFolderButton = true;
      if (!(this.DataContext is SettingUpPageVM dataContext))
        throw new ArgumentException("DataContext object is null");
      folderBrowserDialog.SelectedPath = string.IsNullOrEmpty(dataContext.OutputPath) || !Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(dataContext.OutputPath) ? Constants.DEFAULT_OUTPUT_ROOT : dataContext.OutputPath;
      if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
        return;
      dataContext.OutputPath = folderBrowserDialog.SelectedPath;
    }

    private void tbBspPath_MenuItem_Click(object sender, RoutedEventArgs e) => Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CopyTextToClipboard(this.tbBspPath.SelectedText);

    private void tbChangeOutputPath_MenuItem_Click(object sender, RoutedEventArgs e) => Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CopyTextToClipboard(this.tbChangeOutputPath.SelectedText);

    private void tbChangeOutputPath_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.tbChangeOutputPath.SelectAll();

    private void tbBspPath_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.tbBspPath.SelectAll();

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => Microsoft.WindowsPhone.ImageDesigner.UI.Common.HelpProvider.ShowHelp();

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/settinguppage.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.tbPageTitle = (TextBlock) target;
          break;
        case 2:
          this.tbPageDesc = (TextBlock) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HelpButtonClick);
          break;
        case 4:
          this.tbDrvLocTitle = (TextBlock) target;
          break;
        case 5:
          this.tbDrvLocDesc = (TextBlock) target;
          break;
        case 6:
          this.tbSectionDesc = (TextBlock) target;
          break;
        case 7:
          this.tbBspPath = (System.Windows.Controls.TextBox) target;
          this.tbBspPath.MouseDoubleClick += new MouseButtonEventHandler(this.tbBspPath_MouseDoubleClick);
          break;
        case 8:
          ((System.Windows.Controls.MenuItem) target).Click += new RoutedEventHandler(this.tbBspPath_MenuItem_Click);
          break;
        case 9:
          this.tbChangeBspPathLink = (TextBlock) target;
          break;
        case 10:
          this.hlChangeLocation = (Hyperlink) target;
          this.hlChangeLocation.Click += new RoutedEventHandler(this.hlChangeLocation_Click);
          break;
        case 11:
          this.tbOutputPathTitle = (TextBlock) target;
          break;
        case 12:
          this.tbOutputPathDesc = (TextBlock) target;
          break;
        case 13:
          this.tbCurrentOutputPath = (TextBlock) target;
          break;
        case 14:
          this.tbChangeOutputPath = (System.Windows.Controls.TextBox) target;
          this.tbChangeOutputPath.MouseDoubleClick += new MouseButtonEventHandler(this.tbChangeOutputPath_MouseDoubleClick);
          break;
        case 15:
          ((System.Windows.Controls.MenuItem) target).Click += new RoutedEventHandler(this.tbChangeOutputPath_MenuItem_Click);
          break;
        case 16:
          this.tbChangeOutputPathLink = (TextBlock) target;
          break;
        case 17:
          this.hlChangeOutputPath = (Hyperlink) target;
          this.hlChangeOutputPath.Click += new RoutedEventHandler(this.hlChangeOutputPath_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
