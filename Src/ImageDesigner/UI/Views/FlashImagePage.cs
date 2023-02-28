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
  public partial class FlashImagePage : System.Windows.Controls.UserControl
  {
      

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

      if (string.IsNullOrEmpty(dataContext.Context.OutputDir) 
                || !Directory.Exists(dataContext.Context.OutputDir))
        str = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

      openFileDialog.InitialDirectory = str;

      if (openFileDialog.ShowDialog() != DialogResult.OK)
        return;
      dataContext.FFUFilePath = openFileDialog.FileName;
    }

    private void btRefresh_Click_1(object sender, RoutedEventArgs e) => 
            (this.DataContext as FlashImagePageVM).GetConnectedDevicesCommand.Execute((object) this);

    private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
    {
    }

    private void FlashImageUserControl_Loaded_1(object sender, RoutedEventArgs e)
    {
      this.bdFlashProgress.Visibility = Visibility.Collapsed;
      (this.DataContext as FlashImagePageVM).PropertyChanged 
                += new PropertyChangedEventHandler(this.vm_PropertyChanged);
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      FlashImagePageVM flashImagePageVm = sender as FlashImagePageVM;
      string propertyName = e.PropertyName;
      if (e.PropertyName.Equals("FlashInProgress"))
      {
        this.Dispatcher.BeginInvoke((Action) (() => 
        this.bdFlashProgress.Visibility = Visibility.Visible));

        this.Dispatcher.BeginInvoke((Action) (() => 
        this.bdFlashComplete.Visibility = Visibility.Hidden));
      }
      else
      {
        if (!e.PropertyName.Equals("FlashComplete"))
          return;
        this.Dispatcher.BeginInvoke((Action) (() => this.bdFlashProgress.Visibility = Visibility.Hidden));
        this.Dispatcher.BeginInvoke((Action) (() => this.bdFlashComplete.Visibility = Visibility.Visible));
        string str = Tools.GetString("txtFlashSuccess");
        if (flashImagePageVm.FlashResult != null && flashImagePageVm.FlashResult.Error != 0)
        {
          StringBuilder stringBuilder = new StringBuilder(Tools.GetString("txtFlashError"));
          if (!string.IsNullOrWhiteSpace(flashImagePageVm.FlashResult.ErrorMessage))
          {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(Tools.GetString("txtFlashErrorDetail"), 
                (object) flashImagePageVm.FlashResult.ErrorMessage);
          }
          str = stringBuilder.ToString();
          this.bdFlashComplete.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
        }
        this.tbFlashComplete.Text = str;
      }
    }

        private void HelpButtonClick(object sender, MouseButtonEventArgs e)
        {
            Microsoft.WindowsPhone.ImageDesigner.UI.Common.HelpProvider.ShowHelp();
        }

     
    }
}
