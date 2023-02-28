// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.CustomizeOSPage
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.Win32;
using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using Microsoft.WindowsPhone.ImageDesigner.UI.Common;
using Microsoft.WindowsPhone.ImageDesigner.UI.Dialogs;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.MCSF.Offline;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public class CustomizeOSPage : UserControl, IComponentConnector, IStyleConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal Button bPreviousError;
    internal Button bNextError;
    internal Image iError;
    internal Image iWarning;
    internal ListBox lbVariants;
    internal Button bAddVariant;
    internal Button bRemoveVariant;
    internal TextBlock tbShowAllLink;
    internal Hyperlink hlShowAll;
    internal TreeView tvCustomizations;
    private bool _contentLoaded;

    public CustomizeOSPage()
    {
      this.InitializeComponent();
      this.SetShowAllText();
    }

    private void hlShowAll_Click(object sender, RoutedEventArgs e) => this.OnShowAllChanged();

    private void OnShowAllChanged()
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      using (new WaitCursor())
      {
        dataContext.OnShowAllChanged();
        this.SetShowAllText();
      }
    }

    private void SetShowAllText()
    {
      if (!(this.DataContext is CustomizeOSPageVM dataContext) || !(dataContext.CurrentVariantItem is WPVariant))
        return;
      string text = Tools.GetString("hlCOSShowAll");
      if ((dataContext.CurrentVariantItem as WPVariant).ShowAll)
        text = Tools.GetString("hlCOSHideUnset");
      Run run = new Run(text);
      this.hlShowAll.Inlines.Clear();
      this.hlShowAll.Inlines.Add((Inline) run);
    }

    private void bAddVariant_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      TargetValuesDialogVM vm = new TargetValuesDialogVM(CustomDialogType.NewVariantDialog);
      TargetValuesDialog targetValuesDialog = new TargetValuesDialog(vm);
      vm.Header = Tools.GetString("dlgHdrCOSAddNewVariant");
      vm.Instructions = Tools.GetString("dlgInstructionsCOSAddNewVariant");
      vm.SetCommon(new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase)
      {
        {
          "MNC",
          ""
        },
        {
          "MCC",
          ""
        }
      });
      List<string> stringList = new List<string>();
      foreach (WPVariant wpVariant in (IEnumerable) this.lbVariants.Items)
        stringList.Add(wpVariant.Name);
      vm.ExistingVariantNames = stringList;
      vm.OtherCount = 2;
      targetValuesDialog.ShowDialog();
      if (vm.Result != CustomGetValueResult.OK)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      using (new WaitCursor())
        WPVariant.AddNewItem((WPListItemBase) dataContext.WPImageCustomization, vm.Name, vm.Results);
      dataContext.HasValidationErrors = false;
      dataContext.ValidationErrorToolTip = "";
    }

    private void bRemoveVariant_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      WPVariant selectedItem = this.lbVariants.SelectedItem as WPVariant;
      CustomMessageBoxVM vm = new CustomMessageBoxVM(CustomDialogType.YesNoDialog);
      CustomMessageBox customMessageBox = new CustomMessageBox(vm);
      if (selectedItem.IsStaticVariant)
      {
        vm.Header = Tools.GetString("msgCOSCantRemoveDefaultVariantTitle");
        vm.Message = Tools.GetString("msgCOSCantRemoveDefaultVariant");
        vm.ShowDialogNextTime = false;
        customMessageBox.ShowDialog();
        if (vm.Result != CustomMessageBoxResult.Yes)
          return;
        selectedItem.ClearValues();
        dataContext.VariantSelectionChanged(selectedItem);
        dataContext.CheckForErrors();
      }
      else
      {
        vm.Header = Tools.GetString("msgCOSRemovetVariantAreYouSureTitle");
        vm.Message = Tools.GetString("msgCOSRemovetVariantAreYouSure");
        vm.ShowDialogNextTime = false;
        customMessageBox.ShowDialog();
        if (vm.Result == CustomMessageBoxResult.No)
          return;
        int selectedIndex = this.lbVariants.SelectedIndex;
        dataContext.WPImageCustomization.RemoveSelectedChild();
        dataContext.CheckForErrors();
        if (selectedIndex == this.lbVariants.Items.Count)
          --selectedIndex;
        this.lbVariants.SelectedIndex = selectedIndex;
      }
    }

    private void lbVariant_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      if (this.lbVariants.SelectedItem != null)
      {
        WPVariant selectedItem = this.lbVariants.SelectedItem as WPVariant;
        dataContext.VariantSelectionChanged(selectedItem);
        this.SetShowAllText();
        this.bRemoveVariant.IsEnabled = true;
      }
      else
        this.bRemoveVariant.IsEnabled = false;
    }

    private void lbCustomizations_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      object dataContext = this.DataContext;
    }

    private void lbEditCustomization_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      object dataContext = this.DataContext;
    }

    private void tvCustomizations_SelectedItemChanged(
      object sender,
      RoutedPropertyChangedEventArgs<object> e)
    {
      if (this.DataContext == null)
        return;
      (this.DataContext as CustomizeOSPageVM).SelectedCustomizationChanged((sender as TreeView).SelectedItem);
    }

    private void tbCustomizationChanged(object sender, TextChangedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      object dataContext = this.DataContext;
    }

    private void lbCustomizationChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      (this.DataContext as CustomizeOSPageVM).CustomizationChanged();
    }

    private void cbCustomizationChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      if (dataContext.CurrentItem == null)
        return;
      dataContext.CurrentItem.NotifyChanges();
    }

    private void bEditRemove_Click(object sender, RoutedEventArgs e) => (this.tvCustomizations.SelectedItem as WPListItemBase).RemoveSelectedChild();

    private void bEditSettingsAdd_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomGetValueVM vm1 = new CustomGetValueVM(CustomDialogType.GetValueComboBoxDialog);
      CustomGetValueDialog customGetValueDialog = new CustomGetValueDialog(vm1);
      vm1.Header = Tools.GetString("msgCOSAddNewSettingTitle");
      vm1.Instructions = Tools.GetString("msgCOSAddNewSetting");
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      WPSettings currentItem = dataContext.CurrentItem as WPSettings;
      if (!currentItem.HasAssociatePolicy)
      {
        CustomMessageBoxVM vm2 = new CustomMessageBoxVM(CustomDialogType.OKDialog);
        CustomMessageBox customMessageBox = new CustomMessageBox(vm2);
        vm2.Header = Tools.GetString("msgCOSAddNewSettingErrorTitle");
        vm2.Message = Tools.GetString("msgCOSAddNewSettingError");
        vm2.ShowDialogNextTime = false;
        customMessageBox.ShowDialog();
      }
      else
      {
        List<string> availableSettings = currentItem.GetAvailableSettings();
        availableSettings.Sort((IComparer<string>) Tools.LexicoGraphicComparer);
        ObservableCollection<string> source = new ObservableCollection<string>(availableSettings);
        if (source.Count<string>() == 0)
        {
          CustomMessageBoxVM vm3 = new CustomMessageBoxVM(CustomDialogType.OKDialog);
          CustomMessageBox customMessageBox = new CustomMessageBox(vm3);
          vm3.Header = Tools.GetString("msgCOSAddNewSettingNoMoreErrorTitle");
          vm3.Message = Tools.GetString("msgCOSAddNewSettingNoMoreError");
          vm3.ShowDialogNextTime = false;
          customMessageBox.ShowDialog();
        }
        else
        {
          vm1.ValueList = source;
          customGetValueDialog.ShowDialog();
          if (vm1.Result != CustomGetValueResult.OK)
            return;
          List<string> stringList = PolicyMacroTable.OEMMacroList(vm1.Value);
          if (stringList.Count<string>() > 0)
          {
            string macroReplacedName = vm1.Value;
            if (!this.RequestMacroValues(ref macroReplacedName, stringList))
              return;
            if (currentItem.SettingNames.Contains<string>(macroReplacedName, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
              CustomMessageBox.ShowError(Tools.GetString("txtMacroPathAlreadyExists"));
            else
              dataContext.CurrentItem = WPSetting.AddNewItem((WPListItemBase) currentItem, vm1.Value, macroReplacedName);
          }
          else
            WPSetting.AddNewItem(dataContext.CurrentItem, vm1.Value);
        }
      }
    }

    private void bEditSettingAssetAdd_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      WPSettingsAssetsGroup currentItem = dataContext.CurrentItem as WPSettingsAssetsGroup;
      string groupName = currentItem.GroupName;
      if (currentItem.AssetInfo != null && currentItem.AssetInfo.HasOEMMacros)
      {
        string macroExpandedValue = currentItem.AssetInfo.Name;
        if (!this.RequestMacroValues(ref macroExpandedValue, currentItem.AssetInfo.OEMMacros))
          return;
        if (currentItem.GetAssets().Find((Predicate<WPSettingAsset>) (asst => asst.Asset.Name.Equals(macroExpandedValue, StringComparison.OrdinalIgnoreCase))) != null)
        {
          CustomMessageBox.ShowError(Tools.GetString("txtMacroPathAlreadyExists"));
          return;
        }
        groupName = macroExpandedValue;
      }
      CustomGetFileVM vm = new CustomGetFileVM(CustomDialogType.GetFileDialog);
      CustomGetFileDialog customGetFileDialog = new CustomGetFileDialog(vm);
      vm.Header = Tools.GetString("msgCOSAddNewSettingAssetTitle");
      vm.Instructions = Tools.GetString("msgCOSAddNewSettingAsset");
      vm.Filename = Tools.GetString("strCOSAddNewSettingAssetDefaultSource");
      vm.FileExtensionsFilter = currentItem.FileExtensionsFilter;
      vm.ShowDisplayName = true;
      vm.DisplayNamesAlreadyInUseList = currentItem.InUseDisplayNames;
      vm.FilenamesAlreadyInUseList = currentItem.InUseFilenames;
      customGetFileDialog.ShowDialog();
      if (vm.Result != CustomGetFileResult.OK)
        return;
      WPSettingAsset.AddNewItem(dataContext.CurrentItem, vm.Filename, vm.DisplayName, groupName);
    }

    private void bEditApplicationsAdd_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      bool flag = false;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      if (dataContext.CurrentItem.GetParentOfType(typeof (WPVariant)) is WPVariant parentOfType)
        flag = parentOfType.IsStaticVariant;
      CustomGetFileVM vm = new CustomGetFileVM(CustomDialogType.GetFileDialog);
      CustomGetFileDialog customGetFileDialog = new CustomGetFileDialog(vm);
      vm.Header = Tools.GetString("msgCOSAddNewApplicationTitle");
      WPApplicationsGroups currentItem = dataContext.CurrentItem as WPApplicationsGroups;
      if (flag)
      {
        vm.Instructions = Tools.GetString("msgCOSAddNewApplication");
        vm.FileExtensionsFilter = Tools.GetFileFiterByType(Tools.FileFilterType.Application);
        vm.Filename = Tools.GetString("strCOSAddNewApplicationSource");
      }
      else
      {
        vm.Instructions = Tools.GetString("msgCOSAddNewApplicationWithProvXML");
        vm.FileExtensionsFilter = Tools.GetFileFiterByType(Tools.FileFilterType.ApplicationAndProvXML);
        vm.Filename = Tools.GetString("strCOSAddNewApplicationSourceOrProvXML");
      }
      vm.FilenamesAlreadyInUseList = currentItem.InUseSourceFilenames;
      customGetFileDialog.ShowDialog();
      if (vm.Result != CustomGetFileResult.OK)
        return;
      bool useAsProvXML = false;
      if (!flag && Path.GetExtension(vm.Filename).EndsWith("xml", StringComparison.OrdinalIgnoreCase))
        useAsProvXML = true;
      WPApplication.AddNewItem(dataContext.CurrentItem, vm.Filename, useAsProvXML);
    }

    private void bEditDataAssetsGroupAdd_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomGetFileVM vm = new CustomGetFileVM(CustomDialogType.GetFileDialog);
      CustomGetFileDialog customGetFileDialog = new CustomGetFileDialog(vm);
      vm.Header = Tools.GetString("msgCOSAddNewDataAssetTitle");
      vm.Instructions = Tools.GetString("msgCOSAddNewDataAsset");
      vm.Filename = Tools.GetString("strCOSAddNewDataAssetSource");
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      WPDataAssetsGroup currentItem = dataContext.CurrentItem as WPDataAssetsGroup;
      vm.FileExtensionsFilter = Tools.GetFileFiterFromExtension(currentItem.FileTypes);
      vm.FilenamesAlreadyInUseList = currentItem.InUseSourceFilenames;
      vm.AllowDirectorySelection = true;
      customGetFileDialog.ShowDialog();
      if (vm.Result != CustomGetFileResult.OK)
        return;
      string source = vm.Filename;
      if (File.Exists(vm.Filename) && CustomMessageBox.ShowYesNoMessage(Tools.GetString("txtIncludeAllAssetsInDirectoryTitle"), Tools.GetString("txtIncludeAllAssetsInDirectory")) == CustomMessageBoxResult.Yes)
      {
        source = Path.GetDirectoryName(vm.Filename);
        if (vm.FilenamesAlreadyInUseList.Contains<string>(source, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
        {
          int num = (int) MessageBox.Show(string.Format(Tools.GetString("msgCOSAddFileDuplicateNameError"), (object) source), Tools.GetString("msgCOSAddFileDuplicateNameErrorTitle"));
          return;
        }
      }
      WPDataAsset.AddNewItem(dataContext.CurrentItem, source);
    }

    private void bEditSettingsGroupsAdd_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomGetValueVM vm1 = new CustomGetValueVM(CustomDialogType.GetValueComboBoxDialog);
      CustomGetValueDialog customGetValueDialog = new CustomGetValueDialog(vm1);
      vm1.Header = Tools.GetString("msgCOSAddNewSettingsTitle");
      vm1.Instructions = Tools.GetString("msgCOSAddNewSettings");
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      List<string> availableSettingsPath = (dataContext.CurrentItem as WPSettingsGroups).GetAvailableSettingsPath();
      availableSettingsPath.Sort((IComparer<string>) Tools.LexicoGraphicComparer);
      ObservableCollection<string> source = new ObservableCollection<string>(availableSettingsPath);
      if (source.Count<string>() == 0)
      {
        CustomMessageBoxVM vm2 = new CustomMessageBoxVM(CustomDialogType.OKDialog);
        CustomMessageBox customMessageBox = new CustomMessageBox(vm2);
        vm2.Header = Tools.GetString("msgCOSAddNewSettingsNoMorePathErrorTitle");
        vm2.Message = Tools.GetString("msgCOSAddNewSettingsNoMorePathError");
        vm2.ShowDialogNextTime = false;
        customMessageBox.ShowDialog();
      }
      else
      {
        vm1.ValueList = source;
        customGetValueDialog.ShowDialog();
        if (vm1.Result != CustomGetValueResult.OK)
          return;
        List<string> stringList = PolicyMacroTable.OEMMacroList(vm1.Value);
        if (stringList.Count<string>() > 0)
        {
          string macroExpandedValue = vm1.Value;
          if (!this.RequestMacroValues(ref macroExpandedValue, stringList))
            return;
          if ((dataContext.CurrentItem as WPSettingsGroups).GetIncludedSettings().ToList<WPSettings>().Find((Predicate<WPSettings>) (settings => settings.Path.Equals(macroExpandedValue, StringComparison.OrdinalIgnoreCase))) != null)
            CustomMessageBox.ShowError(Tools.GetString("txtMacroPathAlreadyExists"));
          else
            WPSettings.AddNewItem(dataContext.CurrentItem, vm1.Value, macroExpandedValue);
        }
        else
          WPSettings.AddNewItem(dataContext.CurrentItem, vm1.Value);
      }
    }

    private bool RequestMacroValues(ref string value, List<string> macros)
    {
      bool flag1 = true;
      string str1 = value;
      foreach (string macro in macros)
      {
        bool flag2 = false;
        string str2 = "";
        do
        {
          CustomGetValueVM vm = new CustomGetValueVM(CustomDialogType.GetValueEditBoxDialog);
          CustomGetValueDialog customGetValueDialog = new CustomGetValueDialog(vm);
          vm.Header = Tools.GetString("msgCOSAddNewSettingMacroTitle");
          vm.Instructions = string.Format(Tools.GetString("msgCOSAddNewSettingMacro"), (object) macro);
          if (!string.IsNullOrEmpty(str2))
            vm.Value = str2;
          customGetValueDialog.ShowDialog();
          if (vm.Result == CustomGetValueResult.Cancel)
          {
            flag1 = false;
            break;
          }
          str2 = vm.Value;
          if (str2.Contains<char>('~'))
            CustomMessageBox.ShowError(Tools.GetString("txtMacroStillContainsTildeCharacter"));
          else
            flag2 = true;
        }
        while (!flag2);
        str1 = str1.Replace(macro, str2);
      }
      if (flag1)
        value = str1;
      return flag1;
    }

    private void bEditDataAssetsGroupsAdd_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      ObservableCollection<string> source = new ObservableCollection<string>((dataContext.CurrentItem as WPDataAssetsGroups).GetAvailableAssetTypes());
      if (source.Count<string>() == 0)
      {
        CustomMessageBoxVM vm = new CustomMessageBoxVM(CustomDialogType.OKDialog);
        CustomMessageBox customMessageBox = new CustomMessageBox(vm);
        vm.Header = Tools.GetString("msgCOSAddNewDataAssetsGroupNoMoreTypeErrorTitle");
        vm.Message = Tools.GetString("msgCOSAddNewDataAssetsGroupNoMoreTypeError");
        vm.ShowDialogNextTime = false;
        customMessageBox.ShowDialog();
      }
      else
      {
        CustomGetValueVM vm = new CustomGetValueVM(CustomDialogType.GetValueComboBoxDialog);
        CustomGetValueDialog customGetValueDialog = new CustomGetValueDialog(vm);
        vm.Header = Tools.GetString("msgCOSAddNewDataAssetGroupTitle");
        vm.Instructions = Tools.GetString("msgCOSAddNewDataAssetGroup");
        vm.ValueList = source;
        customGetValueDialog.ShowDialog();
        if (vm.Result != CustomGetValueResult.OK)
          return;
        WPDataAssetsGroup.AddNewItem(dataContext.CurrentItem, vm.Value);
      }
    }

    private void bEditTargetsAdd_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      WPTargetState.AddNewItem((this.DataContext as CustomizeOSPageVM).CurrentItem);
    }

    private void bEditConditionsAdd_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      TargetValuesDialogVM vm = new TargetValuesDialogVM(CustomDialogType.EditTargetDialog);
      TargetValuesDialog targetValuesDialog = new TargetValuesDialog(vm);
      vm.Header = Tools.GetString("msgCOSAddNewTargetConditionTitle");
      vm.Instructions = Tools.GetString("msgCOSAddNewTargetCondition");
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      WPTargetState currentItem = dataContext.CurrentItem as WPTargetState;
      List<string> conditionNames = currentItem.ConditionNames;
      vm.ExistingConditionNames = conditionNames;
      vm.OtherCount = 1;
      vm.OtherGroupHeader = Tools.GetString("gCOSAddTargetStateCondition");
      targetValuesDialog.ShowDialog();
      if (vm.Result != CustomGetValueResult.OK || vm.Results.Count<KeyValuePair<string, string>>() <= 0)
        return;
      string str = vm.Results.Keys.First<string>();
      string result = vm.Results[str];
      WPListItemBase wpListItemBase = WPCondition.AddNewItem((WPListItemBase) currentItem, str, result);
      wpListItemBase.IsSelected = false;
      dataContext.HasValidationErrors = false;
      dataContext.ValidationErrorToolTip = "";
      currentItem.IsSelected = true;
      wpListItemBase.IsSelectedForListBox = true;
    }

    private void bNextError_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      (this.DataContext as CustomizeOSPageVM).GoToNextError();
    }

    private void bPreviousError_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      (this.DataContext as CustomizeOSPageVM).GoToPreviousError();
    }

    private void lb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (!((sender as ListBox).SelectedItem is WPListItemBase selectedItem))
        return;
      selectedItem.IsExpanded = true;
      selectedItem.IsSelected = true;
    }

    private void iError_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Image image) || image.ToolTip == null)
        return;
      CustomMessageBox.ShowError(image.ToolTip.ToString());
    }

    private void iWarning_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Image image) || image.ToolTip == null)
        return;
      CustomMessageBox.ShowWarning(image.ToolTip.ToString());
    }

    private void tbEditSettingValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      if (this.DataContext == null)
        return;
      WPSetting currentItem = (this.DataContext as CustomizeOSPageVM).CurrentItem as WPSetting;
      string textBasedOnControl = UITools.GetNewTextBasedOnControl(sender as TextBox, e.Text);
      if (textBasedOnControl.Length >= currentItem.MinNumber && textBasedOnControl.Length <= currentItem.MaxNumber)
        return;
      e.Handled = true;
    }

    private void tbEditSettingValueNumber_PreviewTextInput(
      object sender,
      TextCompositionEventArgs e)
    {
      if (this.DataContext == null)
        return;
      WPSetting currentItem = (this.DataContext as CustomizeOSPageVM).CurrentItem as WPSetting;
      string textBasedOnControl = UITools.GetNewTextBasedOnControl(sender as TextBox, e.Text);
      bool skipMinCheck = true;
      e.Handled = !UITools.IsValidNumber(textBasedOnControl, currentItem.MinNumber, currentItem.MaxNumber, skipMinCheck);
    }

    private void tbEditSettingValueNumbers_Pasting(object sender, DataObjectPastingEventArgs e)
    {
      WPSetting currentItem = (this.DataContext as CustomizeOSPageVM).CurrentItem as WPSetting;
      TextBox control = sender as TextBox;
      if (!(e.DataObject.GetData(typeof (string)) is string data))
      {
        e.CancelCommand();
      }
      else
      {
        if (UITools.IsValidNumber(UITools.GetNewTextBasedOnControl(control, data), currentItem.MinNumber, currentItem.MaxNumber))
          return;
        e.CancelCommand();
      }
    }

    private void bApplicationSourceBrowseFilename_MouseLeftButtonUp(
      object sender,
      MouseButtonEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      WPApplication currentItem = dataContext.CurrentItem as WPApplication;
      string fileFiterByType = Tools.GetFileFiterByType(Tools.FileFilterType.Application);
      openFileDialog.Filter = fileFiterByType;
      bool? nullable = openFileDialog.ShowDialog();
      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
        return;
      currentItem.Source = openFileDialog.FileName;
    }

    private void bApplicationLicenseBrowseFilename_MouseLeftButtonUp(
      object sender,
      MouseButtonEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      WPApplication currentItem = dataContext.CurrentItem as WPApplication;
      string fileFiterByType = Tools.GetFileFiterByType(Tools.FileFilterType.License);
      openFileDialog.Filter = fileFiterByType;
      bool? nullable = openFileDialog.ShowDialog();
      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
        return;
      currentItem.License = openFileDialog.FileName;
    }

    private void bApplicationProvXMLBrowseFilename_MouseLeftButtonUp(
      object sender,
      MouseButtonEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      WPApplication currentItem = dataContext.CurrentItem as WPApplication;
      string fileFiterByType = Tools.GetFileFiterByType(Tools.FileFilterType.ProvXML);
      openFileDialog.Filter = fileFiterByType;
      bool? nullable = openFileDialog.ShowDialog();
      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
        return;
      currentItem.ProvXML = openFileDialog.FileName;
    }

    private void bDataAssetSourceBrowseFilename_MouseLeftButtonUp(
      object sender,
      MouseButtonEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      WPDataAsset currentItem = dataContext.CurrentItem as WPDataAsset;
      WPDataAssetsGroup parent = currentItem.Parent as WPDataAssetsGroup;
      string fiterFromExtension = Tools.GetFileFiterFromExtension(parent.FileTypes);
      openFileDialog.Filter = fiterFromExtension;
      bool? nullable = openFileDialog.ShowDialog();
      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
        return;
      string str = openFileDialog.FileName;
      if (CustomMessageBox.ShowYesNoMessage(Tools.GetString("txtIncludeAllAssetsInDirectoryTitle"), Tools.GetString("txtIncludeAllAssetsInDirectory")) == CustomMessageBoxResult.Yes)
        str = Path.GetDirectoryName(openFileDialog.FileName);
      List<string> source = new List<string>((IEnumerable<string>) parent.InUseSourceFilenames);
      if (Directory.Exists(currentItem.Source))
        source.Remove(currentItem.Source);
      else
        source.Remove(Path.GetFileName(currentItem.Source));
      if (source.Contains<string>(str, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
      {
        int num = (int) MessageBox.Show(string.Format(Tools.GetString("msgCOSAddFileDuplicateNameError"), (object) str), Tools.GetString("msgCOSAddFileDuplicateNameErrorTitle"));
      }
      else
        currentItem.Source = str;
    }

    private void bSettingAssetSourceBrowseFilename_MouseLeftButtonUp(
      object sender,
      MouseButtonEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      WPSettingAsset currentItem = dataContext.CurrentItem as WPSettingAsset;
      string fiterFromExtension = Tools.GetFileFiterFromExtension(currentItem.FileTypes);
      openFileDialog.Filter = fiterFromExtension;
      bool? nullable = openFileDialog.ShowDialog();
      if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
        return;
      currentItem.Source = openFileDialog.FileName;
    }

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp();

    private void hlPinToStart_Click(object sender, RoutedEventArgs e)
    {
      CustomizeOSPageVM currentPage = IDContext.Instance.Controller.CurrentPage as CustomizeOSPageVM;
      WPApplicationsGroups currentItem = currentPage.CurrentItem as WPApplicationsGroups;
      List<Application> configuredOEMApps = new List<Application>();
      if (currentItem.GetPreviewItem(false, 0) is List<Applications> previewItem)
        configuredOEMApps.AddRange(((IEnumerable<Application>) previewItem[0].Items).Where<Application>((Func<Application, bool>) (a => a.TargetPartition.Equals(PkgConstants.c_strMainOsPartition))));
      currentPage.CurrentItem.CurrentVariant.SettingsGroup.LoadPolicy();
      PinnedAppsVM pinnedAppsVm = new PinnedAppsVM(configuredOEMApps);
      PinnedAppsDialog pinnedAppsDialog = new PinnedAppsDialog();
      pinnedAppsDialog.Owner = Application.Current.MainWindow;
      pinnedAppsDialog.DataContext = (object) pinnedAppsVm;
      pinnedAppsDialog.ShowDialog();
    }

    private void bEditSettingsDefineMacros_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      WPSettings currentItem = dataContext.CurrentItem as WPSettings;
      List<string> macros = PolicyMacroTable.OEMMacroList(currentItem.Path);
      string macroExpandedValue = currentItem.Path;
      if (!this.RequestMacroValues(ref macroExpandedValue, macros))
        return;
      WPSettingsGroups parent1 = (WPSettingsGroups) null;
      for (WPListItemBase parent2 = dataContext.CurrentItem.Parent; parent2 != null; parent2 = parent2.Parent)
      {
        if (parent2 is WPSettingsGroups)
        {
          parent1 = parent2 as WPSettingsGroups;
          break;
        }
      }
      if (parent1.GetIncludedSettings().ToList<WPSettings>().Find((Predicate<WPSettings>) (setting => setting.Path.Equals(macroExpandedValue, StringComparison.OrdinalIgnoreCase))) != null)
        CustomMessageBox.ShowError(Tools.GetString("txtMacroPathAlreadyExists"));
      else
        dataContext.CurrentItem = WPSettings.AddNewItem((WPListItemBase) parent1, currentItem.Path, macroExpandedValue);
    }

    private void bEditSettingDefineMacros_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null)
        return;
      CustomizeOSPageVM dataContext = this.DataContext as CustomizeOSPageVM;
      WPSetting currentItem = dataContext.CurrentItem as WPSetting;
      List<string> macros = PolicyMacroTable.OEMMacroList(currentItem.Name);
      string name = currentItem.Name;
      if (!this.RequestMacroValues(ref name, macros))
        return;
      WPSettings parent1 = (WPSettings) null;
      for (WPListItemBase parent2 = dataContext.CurrentItem.Parent; parent2 != null; parent2 = parent2.Parent)
      {
        if (parent2 is WPSettings)
        {
          parent1 = parent2 as WPSettings;
          break;
        }
      }
      if (parent1.SettingNames.Contains<string>(name, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
        CustomMessageBox.ShowError(Tools.GetString("txtMacroPathAlreadyExists"));
      else
        dataContext.CurrentItem = WPSetting.AddNewItem((WPListItemBase) parent1, currentItem.Name, name);
    }

    private void OnLoadPinToStart(object sender, RoutedEventArgs e)
    {
      Run run = new Run(Tools.GetString("hlPinApplicationsToStart"));
      Hyperlink hyperlink = sender as Hyperlink;
      hyperlink.Inlines.Clear();
      hyperlink.Inlines.Add((Inline) run);
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/customizeospage.xaml", UriKind.Relative));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
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
          this.bPreviousError = (Button) target;
          this.bPreviousError.Click += new RoutedEventHandler(this.bPreviousError_Click);
          break;
        case 4:
          this.bNextError = (Button) target;
          this.bNextError.Click += new RoutedEventHandler(this.bNextError_Click);
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HelpButtonClick);
          break;
        case 6:
          this.iError = (Image) target;
          this.iError.MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 7:
          this.iWarning = (Image) target;
          this.iWarning.MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 8:
          this.lbVariants = (ListBox) target;
          this.lbVariants.SelectionChanged += new SelectionChangedEventHandler(this.lbVariant_SelectionChanged);
          break;
        case 9:
          this.bAddVariant = (Button) target;
          this.bAddVariant.Click += new RoutedEventHandler(this.bAddVariant_Click);
          break;
        case 10:
          this.bRemoveVariant = (Button) target;
          this.bRemoveVariant.Click += new RoutedEventHandler(this.bRemoveVariant_Click);
          break;
        case 11:
          this.tbShowAllLink = (TextBlock) target;
          break;
        case 12:
          this.hlShowAll = (Hyperlink) target;
          this.hlShowAll.Click += new RoutedEventHandler(this.hlShowAll_Click);
          break;
        case 13:
          this.tvCustomizations = (TreeView) target;
          this.tvCustomizations.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(this.tvCustomizations_SelectedItemChanged);
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
        case 14:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 15:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 16:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.lbCustomizationChanged);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.lb_MouseDoubleClick);
          break;
        case 17:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditApplicationsAdd_Click);
          break;
        case 18:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditRemove_Click);
          break;
        case 19:
          ((Hyperlink) target).Click += new RoutedEventHandler(this.hlPinToStart_Click);
          ((FrameworkContentElement) target).Loaded += new RoutedEventHandler(this.OnLoadPinToStart);
          break;
        case 20:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          break;
        case 21:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.bApplicationSourceBrowseFilename_MouseLeftButtonUp);
          break;
        case 22:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 23:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 24:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          break;
        case 25:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.bApplicationLicenseBrowseFilename_MouseLeftButtonUp);
          break;
        case 26:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 27:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 28:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          break;
        case 29:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.bApplicationProvXMLBrowseFilename_MouseLeftButtonUp);
          break;
        case 30:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 31:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 32:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 33:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 34:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.lbCustomizationChanged);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.lb_MouseDoubleClick);
          break;
        case 35:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditDataAssetsGroupsAdd_Click);
          break;
        case 36:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditRemove_Click);
          break;
        case 37:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 38:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 39:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.lbCustomizationChanged);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.lb_MouseDoubleClick);
          break;
        case 40:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditDataAssetsGroupAdd_Click);
          break;
        case 41:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditRemove_Click);
          break;
        case 42:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          break;
        case 43:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.bDataAssetSourceBrowseFilename_MouseLeftButtonUp);
          break;
        case 44:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 45:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 46:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 47:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 48:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.lbCustomizationChanged);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.lb_MouseDoubleClick);
          break;
        case 49:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditSettingsGroupsAdd_Click);
          break;
        case 50:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditRemove_Click);
          break;
        case 51:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 52:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 53:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditSettingsDefineMacros_Click);
          break;
        case 54:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 55:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 56:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.lbCustomizationChanged);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.lb_MouseDoubleClick);
          break;
        case 57:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditSettingsAdd_Click);
          break;
        case 58:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditRemove_Click);
          break;
        case 59:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 60:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 61:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.lbCustomizationChanged);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.lb_MouseDoubleClick);
          break;
        case 62:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditSettingAssetAdd_Click);
          break;
        case 63:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditRemove_Click);
          break;
        case 64:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          break;
        case 65:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 66:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 67:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          break;
        case 68:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.bSettingAssetSourceBrowseFilename_MouseLeftButtonUp);
          break;
        case 69:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 70:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 71:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.cbCustomizationChanged);
          break;
        case 72:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 73:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 74:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          break;
        case 75:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 76:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 77:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditSettingDefineMacros_Click);
          break;
        case 78:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 79:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 80:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          ((UIElement) target).PreviewTextInput += new TextCompositionEventHandler(this.tbEditSettingValue_PreviewTextInput);
          break;
        case 81:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          ((UIElement) target).PreviewTextInput += new TextCompositionEventHandler(this.tbEditSettingValueNumber_PreviewTextInput);
          ((UIElement) target).AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.tbEditSettingValueNumbers_Pasting));
          break;
        case 82:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.cbCustomizationChanged);
          break;
        case 83:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 84:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 85:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 86:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 87:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.lbCustomizationChanged);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.lb_MouseDoubleClick);
          break;
        case 88:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditTargetsAdd_Click);
          break;
        case 89:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditRemove_Click);
          break;
        case 90:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 91:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 92:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.lbCustomizationChanged);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.lb_MouseDoubleClick);
          break;
        case 93:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditConditionsAdd_Click);
          break;
        case 94:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.bEditRemove_Click);
          break;
        case 95:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          break;
        case 96:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 97:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
        case 98:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.tbCustomizationChanged);
          break;
        case 99:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iError_MouseLeftButtonUp);
          break;
        case 100:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.iWarning_MouseLeftButtonUp);
          break;
      }
    }
  }
}
