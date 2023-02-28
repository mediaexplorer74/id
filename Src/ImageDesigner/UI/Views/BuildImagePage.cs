// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.BuildImagePage
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using Microsoft.WindowsPhone.ImageDesigner.UI.Common;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public partial class BuildImagePage : UserControl
  {
      
    public BuildImagePage()
    {
      this.InitializeComponent();
      this.LogEntries = new ObservableCollection<LogEntry>();
      Run run = new Run(Tools.GetString(nameof (hlViewImageOptionsSummary)));
      this.hlViewImageOptionsSummary.Inlines.Clear();
      this.hlViewImageOptionsSummary.Inlines.Add((Inline) run);
    }

    private void BuildImageUserControl_Loaded_1(object sender, RoutedEventArgs e)
    {
      this.gb.DataContext = (object) this.LogEntries;
      this.txtCmdLine.DataContext = this.DataContext;
      this.lv.ItemContainerGenerator.StatusChanged += new EventHandler(
          this.ItemContainerGenerator_StatusChanged);
      this.pbBuild.IsIndeterminate = true;
      (this.DataContext as BuildImagePageVM).PropertyChanged 
                += new PropertyChangedEventHandler(this.vm_PropertyChanged);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(
          this.BuildImagePage_DataContextChanged);
    }

    private void BuildImagePage_DataContextChanged(
      object sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != null)
      {
        BuildImagePageVM oldValue = e.OldValue as BuildImagePageVM;
        oldValue.PropertyChanged -= new PropertyChangedEventHandler(
            this.vm_PropertyChanged);
        if (oldValue.ImageBuilder != null)
          oldValue.ImageBuilder.LogOutputChanged -= new ImageBuilderEventHandler(
              this.ImageBuilder_LogOutputChanged);
      }
      if (e.NewValue == null)
        return;
      BuildImagePageVM newValue = e.NewValue as BuildImagePageVM;
      newValue.PropertyChanged += new PropertyChangedEventHandler(
          this.vm_PropertyChanged);
      if (newValue.ImageBuilder == null)
        return;
      newValue.ImageBuilder.LogOutputChanged += new ImageBuilderEventHandler(
          this.ImageBuilder_LogOutputChanged);
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      BuildImagePageVM vm = sender as BuildImagePageVM;
      string propertyName = e.PropertyName;
            
            if (e.PropertyName.Equals("EnableBuildButton"))
            {
                //RnD ; fix it 
                //this.Dispatcher.BeginInvoke((Delegate)
                 //   (() => this.btBuildImage.IsEnabled = vm.EnableBuildButton));
            }

      if (e.PropertyName.Equals("BuildInProgress"))
      {
        bool workInProgress = vm.BuildInProgress || vm.CancelInProgress;
                if (vm.BuildInProgress)
                {
                    this.Dispatcher.BeginInvoke((Action)
                        (
                        () => { this.LogEntries.Clear(); }
                        ));



                    this.Dispatcher.BeginInvoke((Action)
                    (
                      () => 
                      { this.cbAutoScroll.IsChecked = new bool?(true); }
                    ));


          this.Dispatcher.BeginInvoke((Action) (() => (
          (Panel) this.pbBuild.Template.FindName("Animation", 
          (FrameworkElement) this.pbBuild)).Background = (Brush) Brushes.LightBlue));
        }
        else
          this.Dispatcher.BeginInvoke((Action) (
              () => this.cbAutoScroll.IsChecked = new bool?(false)));

        this.Dispatcher.BeginInvoke((Action) (
            () => this.pbBuild.Visibility = workInProgress 
            ? Visibility.Visible 
            : Visibility.Collapsed));
      }
      if (!e.PropertyName.Equals("CancelInProgress"))
        return;
      bool workInProgress1 = vm.BuildInProgress || vm.CancelInProgress;
      if (vm.CancelInProgress)
      {
        this.Dispatcher.BeginInvoke((Action) (
            () => this.cbAutoScroll.IsChecked = new bool?(false)));
        this.Dispatcher.BeginInvoke((Action) (
            () => ((Panel) this.pbBuild.Template.FindName("Animation", 
            (FrameworkElement) this.pbBuild)).Background = (Brush) Brushes.DarkRed));
      }
      this.Dispatcher.BeginInvoke((Action) (
          () => this.pbBuild.Visibility = workInProgress1
          ? Visibility.Visible 
          : Visibility.Collapsed));
    }

    private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
    {
      if (this.lv.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
        return;
      object obj = this.lv.Items[this.lv.Items.Count - 1];
      if (obj == null)
        return;
      this.lv.VirtualizedScrollIntoView(obj, this.cbAutoScroll.IsChecked.Value);
    }

    private void ImageBuilder_LogOutputChanged(object sender, ImageBuilderEventArgs e)
    {
      foreach (LogEntry logEntry in (Collection<LogEntry>) e.logEntries)
      {
        LogEntry le = logEntry;
        this.Dispatcher.BeginInvoke((Action) (() => this.LogEntries.Add(le)));
      }
    }

    private void hlViewSummary_Click(object sender, RoutedEventArgs e)
    {
    }

    public ObservableCollection<LogEntry> LogEntries { get; set; }

    private void Grid_Loaded_1(object sender, RoutedEventArgs e) => (this.DataContext as BuildImagePageVM).ImageBuilder.LogOutputChanged += new ImageBuilderEventHandler(this.ImageBuilder_LogOutputChanged);

    private void btBuildImage_Click_1(object sender, RoutedEventArgs e)
    {
      this.LogEntries.Clear();
      (this.DataContext as BuildImagePageVM).BuildImageCommand.Execute((object) this);
    }

    private void btViewLog_Click_1(object sender, RoutedEventArgs e) => (this.DataContext as BuildImagePageVM).ViewLogCommand.Execute((object) this);

    private void btClearLog_Click_1(object sender, RoutedEventArgs e) => this.LogEntries.Clear();

    private void txtCmdLine_TargetUpdated(object sender, DataTransferEventArgs e) => this.spCmdLine.Visibility = this.txtCmdLine.Text.Trim().Length > 0 ? Visibility.Visible : Visibility.Collapsed;

    private void txtCmdLine_MenuItem_Click(object sender, RoutedEventArgs e) => Tools.CopyTextToClipboard(this.txtCmdLine.SelectedText);

    private void txtCmdLine_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) => this.txtCmdLine.SelectAll();

    private void btOpenFolder_Click_1(object sender, RoutedEventArgs e) => Process.Start(IDContext.Instance.OutputDir);

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp();

        /*
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/buildimagepage.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.BuildImageUserControl = (BuildImagePage) target;
          this.BuildImageUserControl.Loaded += new RoutedEventHandler(this.BuildImageUserControl_Loaded_1);
          break;
        case 2:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.Grid_Loaded_1);
          break;
        case 3:
          this.tbPageTitle = (TextBlock) target;
          break;
        case 4:
          this.tbPageDesc = (TextBlock) target;
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HelpButtonClick);
          break;
        case 6:
          this.tbSummaryLink = (TextBlock) target;
          break;
        case 7:
          this.hlViewImageOptionsSummary = (Hyperlink) target;
          this.hlViewImageOptionsSummary.Click += new RoutedEventHandler(this.hlViewSummary_Click);
          break;
        case 8:
          this.btBuildImage = (Button) target;
          this.btBuildImage.Click += new RoutedEventHandler(this.btBuildImage_Click_1);
          break;
        case 9:
          this.gb = (GroupBox) target;
          break;
        case 10:
          this.cbAutoScroll = (CheckBox) target;
          break;
        case 11:
          this.btViewLog = (Button) target;
          this.btViewLog.Click += new RoutedEventHandler(this.btViewLog_Click_1);
          break;
        case 12:
          this.btClearLog = (Button) target;
          this.btClearLog.Click += new RoutedEventHandler(this.btClearLog_Click_1);
          break;
        case 13:
          this.btOpenOutputFolder = (Button) target;
          this.btOpenOutputFolder.Click += new RoutedEventHandler(this.btOpenFolder_Click_1);
          break;
        case 14:
          this.pbBuild = (ProgressBar) target;
          break;
        case 15:
          this.spCmdLine = (StackPanel) target;
          break;
        case 16:
          this.txtLabelCmdLine = (TextBlock) target;
          break;
        case 17:
          this.txtCmdLine = (TextBox) target;
          this.txtCmdLine.TargetUpdated += new EventHandler<DataTransferEventArgs>(this.txtCmdLine_TargetUpdated);
          this.txtCmdLine.PreviewMouseDoubleClick += new MouseButtonEventHandler(this.txtCmdLine_PreviewMouseDoubleClick);
          break;
        case 18:
          ((MenuItem) target).Click += new RoutedEventHandler(this.txtCmdLine_MenuItem_Click);
          break;
        case 19:
          this.lv = (ListView) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
        */
  }
}
