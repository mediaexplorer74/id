// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.FlashImagePage
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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public class FlashImagePage : System.Windows.Controls.UserControl, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal TextBlock tbFlashTxt;
    internal System.Windows.Controls.TextBox tbFlashImagePath;
    internal TextBlock tbChangeImageLink;
    internal Hyperlink hlChangeImageLink;
    internal TextBlock tbConnectedDevice;
    internal System.Windows.Controls.ComboBox cmbConnectedDevice;
    internal System.Windows.Controls.Button btRefresh;
    internal System.Windows.Controls.Button btFlash;
    internal Border bdFlashProgress;
    internal System.Windows.Controls.ProgressBar pbFlash;
    internal Border bdFlashComplete;
    internal System.Windows.Controls.TextBox tbFlashComplete;
    private bool _contentLoaded;

    public FlashImagePage()
    {
      this.InitializeComponent();
      Run run = new Run(Tools.GetString("hlFlashImageChangeImageLink"));
      this.hlChangeImageLink.Inlines.Clear();
      this.hlChangeImageLink.Inlines.Add((Inline) run);
    }

    private void hlChangeImageLink_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.CheckFileExists = true;
      openFileDialog.Multiselect = false;
      openFileDialog.Filter = Tools.GetString("txtFFUFilter");
      if (!(this.DataContext is FlashImagePageVM dataContext))
        throw new ArgumentException("DataContext object is null");
      if (dataContext.Context == null)
        throw new ArgumentException("Context object is null");
      string str = dataContext.Context.OutputDir;
      if (string.IsNullOrEmpty(dataContext.Context.OutputDir) || !Directory.Exists(dataContext.Context.OutputDir))
        str = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
      openFileDialog.InitialDirectory = str;
      if (openFileDialog.ShowDialog() != DialogResult.OK)
        return;
      dataContext.FFUFilePath = openFileDialog.FileName;
    }

    private void btRefresh_Click_1(object sender, RoutedEventArgs e) => (this.DataContext as FlashImagePageVM).GetConnectedDevicesCommand.Execute((object) this);

    private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
    {
    }

    private void FlashImageUserControl_Loaded_1(object sender, RoutedEventArgs e)
    {
      this.bdFlashProgress.Visibility = Visibility.Collapsed;
      (this.DataContext as FlashImagePageVM).PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      FlashImagePageVM flashImagePageVm = sender as FlashImagePageVM;
      string propertyName = e.PropertyName;
      if (e.PropertyName.Equals("FlashInProgress"))
      {
        this.Dispatcher.BeginInvoke((Delegate) (() => this.bdFlashProgress.Visibility = Visibility.Visible));
        this.Dispatcher.BeginInvoke((Delegate) (() => this.bdFlashComplete.Visibility = Visibility.Hidden));
      }
      else
      {
        if (!e.PropertyName.Equals("FlashComplete"))
          return;
        this.Dispatcher.BeginInvoke((Delegate) (() => this.bdFlashProgress.Visibility = Visibility.Hidden));
        this.Dispatcher.BeginInvoke((Delegate) (() => this.bdFlashComplete.Visibility = Visibility.Visible));
        string str = Tools.GetString("txtFlashSuccess");
        if (flashImagePageVm.FlashResult != null && flashImagePageVm.FlashResult.Error != 0)
        {
          StringBuilder stringBuilder = new StringBuilder(Tools.GetString("txtFlashError"));
          if (!string.IsNullOrWhiteSpace(flashImagePageVm.FlashResult.ErrorMessage))
          {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(Tools.GetString("txtFlashErrorDetail"), (object) flashImagePageVm.FlashResult.ErrorMessage);
          }
          str = stringBuilder.ToString();
          this.bdFlashComplete.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
        }
        this.tbFlashComplete.Text = str;
      }
    }

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => Microsoft.WindowsPhone.ImageDesigner.UI.Common.HelpProvider.ShowHelp();

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/flashimagepage.xaml", UriKind.Relative));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.FlashImageUserControl_Loaded_1);
          break;
        case 2:
          this.tbPageTitle = (TextBlock) target;
          break;
        case 3:
          this.tbPageDesc = (TextBlock) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HelpButtonClick);
          break;
        case 5:
          this.tbFlashTxt = (TextBlock) target;
          break;
        case 6:
          this.tbFlashImagePath = (System.Windows.Controls.TextBox) target;
          break;
        case 7:
          this.tbChangeImageLink = (TextBlock) target;
          break;
        case 8:
          this.hlChangeImageLink = (Hyperlink) target;
          this.hlChangeImageLink.Click += new RoutedEventHandler(this.hlChangeImageLink_Click);
          break;
        case 9:
          this.tbConnectedDevice = (TextBlock) target;
          break;
        case 10:
          this.cmbConnectedDevice = (System.Windows.Controls.ComboBox) target;
          break;
        case 11:
          this.btRefresh = (System.Windows.Controls.Button) target;
          this.btRefresh.Click += new RoutedEventHandler(this.btRefresh_Click_1);
          break;
        case 12:
          this.btFlash = (System.Windows.Controls.Button) target;
          break;
        case 13:
          this.bdFlashProgress = (Border) target;
          break;
        case 14:
          this.pbFlash = (System.Windows.Controls.ProgressBar) target;
          break;
        case 15:
          this.bdFlashComplete = (Border) target;
          break;
        case 16:
          this.tbFlashComplete = (System.Windows.Controls.TextBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
