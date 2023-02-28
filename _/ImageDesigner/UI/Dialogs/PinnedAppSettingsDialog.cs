// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Dialogs.PinnedAppSettingsDialog
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.Win32;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using Microsoft.WindowsPhone.ImageDesigner.UI.Common;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Dialogs
{
  public class PinnedAppSettingsDialog : Window, IComponentConnector, IStyleConnector
  {
    internal Grid MainGrid;
    internal TextBlock tbCoordinates;
    internal TextBlock tbCoordinateX;
    internal TextBox tboxCoordinateX;
    internal TextBlock tbCoordinateY;
    internal TextBox tboxCoordinateY;
    internal TextBlock tbTileSize;
    internal ComboBox cmbTileSize;
    internal TextBlock tb6ColCoordinates;
    internal TextBlock tb6ColCoordinateX;
    internal TextBox tbox6ColCoordinateX;
    internal TextBlock tb6ColCoordinateY;
    internal TextBox tbox6ColCoordinateY;
    internal TextBlock tb6ColTileSize;
    internal ComboBox cmb6ColTileSize;
    internal TextBlock tbAppType;
    internal ComboBox cmbAppType;
    internal Border bdSettings;
    internal DockPanel dbContent;
    private bool _contentLoaded;

    public PinnedAppSettingsDialog() => this.InitializeComponent();

    private void btCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(false);
      this.Close();
    }

    private void btOK_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(true);
      this.Close();
    }

    private void tboxCoordinateX_GotFocus_1(object sender, RoutedEventArgs e) => this.tboxCoordinateX.SelectAll();

    private void tboxCoordinateY_GotFocus_1(object sender, RoutedEventArgs e) => this.tboxCoordinateY.SelectAll();

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp("AppPinning");

    private void bBrowseSmallIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      PinnedAppSettingsVM dataContext = this.DataContext as PinnedAppSettingsVM;
      WebLinkSettings appSettings = dataContext.AppSettings as WebLinkSettings;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = dataContext.WebLinkIconFileExtensionsFilter;
      bool? nullable = openFileDialog.ShowDialog();
      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
        return;
      appSettings.WebLinkSmallIcon = openFileDialog.FileName;
      appSettings.WebLinkSmallIconDisplay = Path.GetFileName(openFileDialog.FileName);
      TextBox dataTemplateTextBox = this.GetDataTemplateTextBox("WebLinkSettingsTemplate", "tbSmallIcon");
      if (dataTemplateTextBox == null)
        return;
      dataTemplateTextBox.Text = appSettings.WebLinkSmallIconDisplay;
      dataTemplateTextBox.ToolTip = (object) appSettings.WebLinkSmallIcon;
    }

    private void bBrowseMediumLargeIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      PinnedAppSettingsVM dataContext = this.DataContext as PinnedAppSettingsVM;
      WebLinkSettings appSettings = dataContext.AppSettings as WebLinkSettings;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = dataContext.WebLinkIconFileExtensionsFilter;
      bool? nullable = openFileDialog.ShowDialog();
      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
        return;
      appSettings.WebLinkMediumLargeIcon = openFileDialog.FileName;
      appSettings.WebLinkMediumLargeIconDisplay = Path.GetFileName(openFileDialog.FileName);
      TextBox dataTemplateTextBox = this.GetDataTemplateTextBox("WebLinkSettingsTemplate", "tbMediumLargeIcon");
      if (dataTemplateTextBox == null)
        return;
      dataTemplateTextBox.Text = appSettings.WebLinkMediumLargeIconDisplay;
      dataTemplateTextBox.ToolTip = (object) appSettings.WebLinkMediumLargeIcon;
    }

    private TextBox GetDataTemplateTextBox(string templateName, string textBoxName)
    {
      TextBox dataTemplateTextBox = (TextBox) null;
      ContentPresenter typeInVisualTree = this.GetObjectOfTypeInVisualTree<ContentPresenter>((DependencyObject) this.dbContent);
      DataTemplate resource = (DataTemplate) this.MainGrid.Resources[(object) templateName];
      ResourceDictionary resources = this.MainGrid.Resources;
      foreach (object key in (IEnumerable) resources.Keys)
      {
        if (resources[key] is DataTemplate)
        {
          DataTemplate dataTemplate = resources[key] as DataTemplate;
          dataTemplate.DataType.ToString();
          if (dataTemplate.DataType.ToString().Equals(typeof (WebLinkSettings).ToString(), StringComparison.OrdinalIgnoreCase))
            dataTemplateTextBox = (TextBox) dataTemplate.FindName(textBoxName, (FrameworkElement) typeInVisualTree);
        }
      }
      return dataTemplateTextBox;
    }

    private T GetObjectOfTypeInVisualTree<T>(DependencyObject dpob) where T : DependencyObject
    {
      int childrenCount = VisualTreeHelper.GetChildrenCount(dpob);
      for (int childIndex = 0; childIndex < childrenCount; ++childIndex)
      {
        DependencyObject child = VisualTreeHelper.GetChild(dpob, childIndex);
        if (child is T typeInVisualTree1)
          return typeInVisualTree1;
        T typeInVisualTree2 = this.GetObjectOfTypeInVisualTree<T>(child);
        if ((object) typeInVisualTree2 != null)
          return typeInVisualTree2;
      }
      return default (T);
    }

    private void gridWebLinkSmallIcon_Loaded(object sender, RoutedEventArgs e)
    {
      WebLinkSettings appSettings = (this.DataContext as PinnedAppSettingsVM).AppSettings as WebLinkSettings;
      TextBox dataTemplateTextBox = this.GetDataTemplateTextBox("WebLinkSettingsTemplate", "tbSmallIcon");
      if (dataTemplateTextBox == null)
        return;
      dataTemplateTextBox.Text = appSettings.WebLinkSmallIconDisplay;
      if (!string.IsNullOrWhiteSpace(dataTemplateTextBox.Text))
        dataTemplateTextBox.ToolTip = (object) appSettings.WebLinkSmallIcon;
      else
        dataTemplateTextBox.ToolTip = (object) null;
    }

    private void gridWebLinkMediumLargeIcon_Loaded(object sender, RoutedEventArgs e)
    {
      WebLinkSettings appSettings = (this.DataContext as PinnedAppSettingsVM).AppSettings as WebLinkSettings;
      TextBox dataTemplateTextBox = this.GetDataTemplateTextBox("WebLinkSettingsTemplate", "tbMediumLargeIcon");
      if (dataTemplateTextBox == null)
        return;
      dataTemplateTextBox.Text = appSettings.WebLinkMediumLargeIconDisplay;
      if (!string.IsNullOrWhiteSpace(dataTemplateTextBox.Text))
        dataTemplateTextBox.ToolTip = (object) appSettings.WebLinkMediumLargeIcon;
      else
        dataTemplateTextBox.ToolTip = (object) null;
    }

    private void tbox6ColCoordinateX_GotFocus_1(object sender, RoutedEventArgs e) => this.tbox6ColCoordinateX.SelectAll();

    private void tbox6ColCoordinateY_GotFocus_1(object sender, RoutedEventArgs e) => this.tbox6ColCoordinateY.SelectAll();

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      string textBasedOnControl = UITools.GetNewTextBasedOnControl(sender as TextBox, e.Text);
      e.Handled = !UITools.IsValidNumber(textBasedOnControl, 0, 0, true, true);
    }

    private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
      TextBox control = sender as TextBox;
      if (!(e.DataObject.GetData(typeof (string)) is string data))
      {
        e.CancelCommand();
      }
      else
      {
        if (UITools.IsValidNumber(UITools.GetNewTextBasedOnControl(control, data), 0, 0, true, true))
          return;
        e.CancelCommand();
      }
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/dialogs/pinnedappsettingsdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HelpButtonClick);
          break;
        case 2:
          this.MainGrid = (Grid) target;
          break;
        case 7:
          this.tbCoordinates = (TextBlock) target;
          break;
        case 8:
          this.tbCoordinateX = (TextBlock) target;
          break;
        case 9:
          this.tboxCoordinateX = (TextBox) target;
          this.tboxCoordinateX.GotFocus += new RoutedEventHandler(this.tboxCoordinateX_GotFocus_1);
          this.tboxCoordinateX.PreviewTextInput += new TextCompositionEventHandler(this.NumericTextBox_PreviewTextInput);
          this.tboxCoordinateX.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.NumericTextBox_Pasting));
          break;
        case 10:
          this.tbCoordinateY = (TextBlock) target;
          break;
        case 11:
          this.tboxCoordinateY = (TextBox) target;
          this.tboxCoordinateY.GotFocus += new RoutedEventHandler(this.tboxCoordinateY_GotFocus_1);
          this.tboxCoordinateY.PreviewTextInput += new TextCompositionEventHandler(this.NumericTextBox_PreviewTextInput);
          this.tboxCoordinateY.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.NumericTextBox_Pasting));
          break;
        case 12:
          this.tbTileSize = (TextBlock) target;
          break;
        case 13:
          this.cmbTileSize = (ComboBox) target;
          break;
        case 14:
          this.tb6ColCoordinates = (TextBlock) target;
          break;
        case 15:
          this.tb6ColCoordinateX = (TextBlock) target;
          break;
        case 16:
          this.tbox6ColCoordinateX = (TextBox) target;
          this.tbox6ColCoordinateX.GotFocus += new RoutedEventHandler(this.tbox6ColCoordinateX_GotFocus_1);
          this.tbox6ColCoordinateX.PreviewTextInput += new TextCompositionEventHandler(this.NumericTextBox_PreviewTextInput);
          this.tbox6ColCoordinateX.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.NumericTextBox_Pasting));
          break;
        case 17:
          this.tb6ColCoordinateY = (TextBlock) target;
          break;
        case 18:
          this.tbox6ColCoordinateY = (TextBox) target;
          this.tbox6ColCoordinateY.GotFocus += new RoutedEventHandler(this.tbox6ColCoordinateY_GotFocus_1);
          this.tbox6ColCoordinateY.PreviewTextInput += new TextCompositionEventHandler(this.NumericTextBox_PreviewTextInput);
          this.tbox6ColCoordinateY.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.NumericTextBox_Pasting));
          break;
        case 19:
          this.tb6ColTileSize = (TextBlock) target;
          break;
        case 20:
          this.cmb6ColTileSize = (ComboBox) target;
          break;
        case 21:
          this.tbAppType = (TextBlock) target;
          break;
        case 22:
          this.cmbAppType = (ComboBox) target;
          break;
        case 23:
          this.bdSettings = (Border) target;
          break;
        case 24:
          this.dbContent = (DockPanel) target;
          break;
        case 25:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.btOK_Click);
          break;
        case 26:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.btCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 3:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.gridWebLinkSmallIcon_Loaded);
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.bBrowseSmallIcon_MouseLeftButtonUp);
          break;
        case 5:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.gridWebLinkMediumLargeIcon_Loaded);
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.bBrowseMediumLargeIcon_MouseLeftButtonUp);
          break;
      }
    }
  }
}
