// PinnedAppsDialog.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Dialogs.PinnedAppsDialog
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using Microsoft.WindowsPhone.ImageDesigner.UI.Common;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Dialogs
{
  public partial class PinnedAppsDialog : Window//, IComponentConnector, IStyleConnector
  {
    //internal TextBlock tbLayout;
    //internal ComboBox cmbLayout;
    //internal TextBlock tbFeaturedTileLayout;
    //internal ComboBox cmbFeaturedTileLayout;
    //internal DataGrid gridPinnedApps;
    //private bool _contentLoaded;

    public PinnedAppsDialog() => this.InitializeComponent();

    private void btCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(false);
      this.Close();
    }

    private void btOk_Click(object sender, RoutedEventArgs e)
    {
      (this.DataContext as PinnedAppsVM).GenerateSettings();
      this.DialogResult = new bool?(true);
      this.Close();
    }

    private void btAdd_Click(object sender, RoutedEventArgs e)
    {
      PinnedAppsVM dataContext = this.DataContext as PinnedAppsVM;
      List<Application> availableOEMApps = new List<Application>(dataContext.OEMAppsAvailableToPin);
      List<EnumWrapper> availableMSApps = new List<EnumWrapper>(dataContext.MSAppsAvailableToPin);
      PinnedAppSettingsVM pinnedItem = new PinnedAppSettingsVM(dataContext.NextAvailableWPSettings, (IEnumerable<Application>) availableOEMApps, (IEnumerable<EnumWrapper>) availableMSApps);
      PinnedAppSettingsDialog appSettingsDialog = new PinnedAppSettingsDialog();
      appSettingsDialog.Owner = (Window) this;
      appSettingsDialog.DataContext = (object) pinnedItem;
      bool? nullable = appSettingsDialog.ShowDialog();
      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
        return;
      (this.DataContext as PinnedAppsVM).AddPinnedItem(pinnedItem);
    }

    private void btEdit_Click_1(object sender, RoutedEventArgs e)
    {
      object dataContext = this.DataContext;
      PinnedAppSettingsVM currentItem = this.gridPinnedApps.CurrentItem as PinnedAppSettingsVM;
      PinnedAppSettingsDialog appSettingsDialog = new PinnedAppSettingsDialog();
      appSettingsDialog.Owner = (Window) this;
      appSettingsDialog.DataContext = (object) currentItem;
      appSettingsDialog.ShowDialog();
    }

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp("AppPinning");

    private void btRemove_Click_1(object sender, RoutedEventArgs e) => (this.DataContext as PinnedAppsVM).Delete(this.gridPinnedApps.SelectedIndex);

        /*
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/dialogs/pinnedappsdialog.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
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
          this.tbLayout = (TextBlock) target;
          break;
        case 3:
          this.cmbLayout = (ComboBox) target;
          break;
        case 4:
          this.tbFeaturedTileLayout = (TextBlock) target;
          break;
        case 5:
          this.cmbFeaturedTileLayout = (ComboBox) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HelpButtonClick);
          break;
        case 7:
          this.gridPinnedApps = (DataGrid) target;
          break;
        case 10:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.btAdd_Click);
          break;
        case 11:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.btOk_Click);
          break;
        case 12:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.btCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 8:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.btEdit_Click_1);
          break;
        case 9:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.btRemove_Click_1);
          break;
      }
    }
        */
  }
}
