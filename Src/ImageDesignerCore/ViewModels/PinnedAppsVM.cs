// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.PinnedAppsVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using Application = Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Application;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class PinnedAppsVM : PinVMBase
  {
    public static readonly DependencyProperty PinnedAppsProperty = DependencyProperty.Register(nameof (PinnedApps), typeof (ObservableCollection<PinnedAppSettingsVM>), typeof (PinnedAppsVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty EmptySettingsAvailableProperty = DependencyProperty.Register(nameof (EmptySettingsAvailable), typeof (bool), typeof (PinnedAppsVM), new PropertyMetadata((object) true));
    public static readonly DependencyProperty StartTileLayoutProperty = DependencyProperty.Register(nameof (StartTileLayout), typeof (StartLayout), typeof (PinnedAppsVM), new PropertyMetadata((object) StartLayout.LayoutOne));
    public static readonly DependencyProperty FeatureTileLayoutProperty = DependencyProperty.Register(nameof (FeatureTileLayout), typeof (FeatureLayout), typeof (PinnedAppsVM), new PropertyMetadata((object) FeatureLayout.DataSense));
    public static readonly DependencyProperty StartLayoutListProperty = DependencyProperty.Register(nameof (StartLayoutList), typeof (List<EnumWrapper>), typeof (PinnedAppsVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty FeatureTileLayoutListProperty = DependencyProperty.Register(nameof (FeatureTileLayoutList), typeof (List<EnumWrapper>), typeof (PinnedAppsVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty AppTileSizeListProperty = DependencyProperty.Register(nameof (AppTileSizeList), typeof (List<EnumWrapper>), typeof (PinnedAppsVM), new PropertyMetadata((PropertyChangedCallback) null));
    private SortedSet<WPSettings> _availableSettings;
    private SortedSet<WPSettings> _includedSettings;
    private SortedSet<WPSettings> _unconfiguredSettings;

    private PinnedAppsVM()
    {
    }

    public PinnedAppsVM(List<Application> configuredOEMApps)
    {
      this.ConfiguredOEMApps = new List<Application>();
      if (configuredOEMApps != null)
        this.ConfiguredOEMApps.AddRange((IEnumerable<Application>) configuredOEMApps);
      this.PinnedApps = new ObservableCollection<PinnedAppSettingsVM>();
      StartLayout startLayout = StartLayout.LayoutOne;
      List<EnumWrapper> enumWrapperList = new List<EnumWrapper>();
      foreach (string name in Enum.GetNames(startLayout.GetType()))
        enumWrapperList.Add(new EnumWrapper(typeof (StartLayout), name));
      this.StartLayoutList = EnumWrapper.GetEnumList(typeof (StartLayout));
      this.FeatureTileLayoutList = EnumWrapper.GetEnumList(typeof (FeatureLayout));
      this.AppTileSizeList = EnumWrapper.GetEnumList(typeof (AppTileSize));
      this.LoadSettings();
    }

    public ObservableCollection<PinnedAppSettingsVM> PinnedApps
    {
      get => (ObservableCollection<PinnedAppSettingsVM>) this.GetValue(PinnedAppsVM.PinnedAppsProperty);
      set => this.SetValue(PinnedAppsVM.PinnedAppsProperty, (object) value);
    }

    public bool EmptySettingsAvailable
    {
      get => (bool) this.GetValue(PinnedAppsVM.EmptySettingsAvailableProperty);
      set => this.SetValue(PinnedAppsVM.EmptySettingsAvailableProperty, (object) value);
    }

    [Layout("StartLayout")]
    public StartLayout StartTileLayout
    {
      get => (StartLayout) this.GetValue(PinnedAppsVM.StartTileLayoutProperty);
      set => this.SetValue(PinnedAppsVM.StartTileLayoutProperty, (object) value);
    }

    [Layout("FeaturedTile")]
    public FeatureLayout FeatureTileLayout
    {
      get => (FeatureLayout) this.GetValue(PinnedAppsVM.FeatureTileLayoutProperty);
      set => this.SetValue(PinnedAppsVM.FeatureTileLayoutProperty, (object) value);
    }

    public List<EnumWrapper> StartLayoutList
    {
      get => (List<EnumWrapper>) this.GetValue(PinnedAppsVM.StartLayoutListProperty);
      set => this.SetValue(PinnedAppsVM.StartLayoutListProperty, (object) value);
    }

    public List<EnumWrapper> FeatureTileLayoutList
    {
      get => (List<EnumWrapper>) this.GetValue(PinnedAppsVM.FeatureTileLayoutListProperty);
      set => this.SetValue(PinnedAppsVM.FeatureTileLayoutListProperty, (object) value);
    }

    public List<EnumWrapper> AppTileSizeList
    {
      get => (List<EnumWrapper>) this.GetValue(PinnedAppsVM.AppTileSizeListProperty);
      set => this.SetValue(PinnedAppsVM.AppTileSizeListProperty, (object) value);
    }

    public List<Application> ConfiguredOEMApps { get; private set; }

    public IEnumerable<Application> OEMAppsAvailableToPin => this.GetOEMAppsAvailableToPin();

    public IEnumerable<EnumWrapper> MSAppsAvailableToPin => this.GetMSAppsAvailableToPin();

    public WPSettings NextAvailableWPSettings
    {
      get
      {
        WPSettings availableWpSettings = (WPSettings) null;
        int index = -1;
        if (this.PinnedApps.Count < this.AvailableSettings.Count)
          index = this.PinnedApps.Count;
        if (index > -1)
          availableWpSettings = this.AvailableSettings.ElementAt<WPSettings>(index);
        return availableWpSettings;
      }
    }

    internal SortedSet<WPSettings> AvailableSettings
    {
      get => this._availableSettings;
      private set => this._availableSettings = value;
    }

    internal SortedSet<WPSettings> IncludedSettings
    {
      get => this._includedSettings;
      private set => this._includedSettings = value;
    }

    internal SortedSet<WPSettings> UnconfiguredSettings
    {
      get => this._unconfiguredSettings;
      private set => this._unconfiguredSettings = value;
    }

    public void AddPinnedItem(PinnedAppSettingsVM pinnedItem)
    {
      this.PinnedApps.Add(pinnedItem);
      this.EmptySettingsAvailable = this.PinnedApps.Count < this.AvailableSettings.Count;
    }

    public void Delete(int index)
    {
      if (index < 0 || index >= this.PinnedApps.Count<PinnedAppSettingsVM>())
        return;
      PinnedAppSettingsVM pinnedApp = this.PinnedApps[index];
      this.PinnedApps.RemoveAt(index);
      pinnedApp.WPSettings?.ResetToPolicyGroupValues();
    }

    public void GenerateSettings()
    {
      this.GenerateLayoutSettings();
      this.GeneratePinSettings();
    }

    private void LoadSettings()
    {
      this.LoadLayoutSettings();
      this.LoadPinSettings();
    }

    private void LoadLayoutSettings()
    {
      CustomizeOSPageVM currentPage = IDContext.Instance.Controller.CurrentPage as CustomizeOSPageVM;
      WPListItemBase currentItem = currentPage.CurrentItem;
      WPSettingsGroups settingsGroup = currentPage.CurrentItem.CurrentVariant.SettingsGroup;
      if (settingsGroup == null)
        return;
      WPSettings settings = settingsGroup.GetAvailableSettings("StartLayout").FirstOrDefault<WPSettings>();
      if (settings == null)
        return;
      this.LoadFromSettings<LayoutAttribute>(settings);
    }

    private void LoadPinSettings()
    {
      CustomizeOSPageVM currentPage = IDContext.Instance.Controller.CurrentPage as CustomizeOSPageVM;
      WPListItemBase currentItem = currentPage.CurrentItem;
      WPSettingsGroups settingsGroup = currentPage.CurrentItem.CurrentVariant.SettingsGroup;
      if (settingsGroup == null)
        return;
      this.AvailableSettings = new SortedSet<WPSettings>(settingsGroup.GetAvailableSettings("(StartPrepinnedTile[1-9]+[0-4]*)"), (IComparer<WPSettings>) new PinnedAppsVM.FuncComparer<WPSettings>((Comparison<WPSettings>) ((a, b) => SafeNativeMethods.StrCmpLogicalW(a.Path, b.Path))));
      this.IncludedSettings = new SortedSet<WPSettings>(settingsGroup.GetIncludedSettings("(StartPrepinnedTile[1-9]+[0-4]*)"), (IComparer<WPSettings>) new PinnedAppsVM.FuncComparer<WPSettings>((Comparison<WPSettings>) ((a, b) => SafeNativeMethods.StrCmpLogicalW(a.Path, b.Path))));
      this.UnconfiguredSettings = new SortedSet<WPSettings>(this.AvailableSettings.Except<WPSettings>((IEnumerable<WPSettings>) this.IncludedSettings), (IComparer<WPSettings>) new PinnedAppsVM.FuncComparer<WPSettings>((Comparison<WPSettings>) ((a, b) => SafeNativeMethods.StrCmpLogicalW(a.Path, b.Path))));
      this.EmptySettingsAvailable = this.UnconfiguredSettings.Count > 0;
      foreach (WPSettings includedSetting in this.IncludedSettings)
        this.PinnedApps.Add(new PinnedAppSettingsVM(includedSetting, this.OEMAppsAvailableToPin, this.MSAppsAvailableToPin));
    }

    private void GenerateLayoutSettings()
    {
      CustomizeOSPageVM currentPage = IDContext.Instance.Controller.CurrentPage as CustomizeOSPageVM;
      WPListItemBase currentItem = currentPage.CurrentItem;
      WPSettingsGroups settingsGroup = currentPage.CurrentItem.CurrentVariant.SettingsGroup;
      if (settingsGroup == null)
        return;
      WPSettings settings = settingsGroup.GetAvailableSettings("(StartLayout)").FirstOrDefault<WPSettings>();
      if (settings == null)
        return;
      this.SaveToSettings<LayoutAttribute>(settings);
      if (!settings.IsIncludedInOutput)
        settings.IsNewItem = true;
      settings.IsDirty = true;
      settings.IsExpanded = true;
      settings.IsSelected = false;
      settings.NotifyChanges();
      settingsGroup.NotifyChanges();
      settingsGroup.ShowAllItems();
    }

    private void GeneratePinSettings()
    {
      foreach (PinnedAppSettingsVM pinnedApp in (Collection<PinnedAppSettingsVM>) this.PinnedApps)
      {
        PinnedAppSettingsVM pinnedItem = pinnedApp;
        WPSettings wpSettings = pinnedItem.WPSettings;
        PinnedAppSettings appSettings1 = pinnedItem.AppSettings;
        if (wpSettings != null && appSettings1 != null)
        {
          appSettings1.SaveToSettings<SettingAttribute>(wpSettings);
          if (pinnedItem.AppSettings.AppType == AppType.MSApplication)
          {
            string str;
            try
            {
              str = PinnedAppsVM.MSAppsList.Find((Predicate<EnumWrapper>) (app => app.DisplayText.Equals(pinnedItem.AppSettings.Name))).ValueName;
            }
            catch
            {
              str = pinnedItem.AppSettings.Name;
            }
            MSApps result;
            if (Enum.TryParse<MSApps>(str, true, out result))
              wpSettings.FindByNameAndSet("StartPrepinnedMSAppID", ((int) result).ToString());
          }
          else if (pinnedItem.AppSettings.AppType == AppType.OEMApplication)
          {
            Application selectedOemApplication = pinnedItem.SelectedOEMApplication;
            if (selectedOemApplication != null)
            {
              string applicationProductId = Tools.GetApplicationProductID(selectedOemApplication);
              wpSettings.FindByNameAndSet("StartPrepinnedOEMAppID", applicationProductId);
            }
          }
          WebLinkSettings appSettings2 = pinnedItem.AppSettings as WebLinkSettings;
          if (pinnedItem.AppSettings.AppType == AppType.WebLink)
          {
            this.CreateWebLinkAsset(appSettings2, "WebLinkSmallIcon", appSettings2.WebLinkSmallIconDisplay, wpSettings);
            this.CreateWebLinkAsset(appSettings2, "WebLinkMediumLargeIcon", appSettings2.WebLinkMediumLargeIconDisplay, wpSettings);
          }
        }
        if (!wpSettings.IsIncludedInOutput)
          wpSettings.IsNewItem = true;
        wpSettings.IsDirty = true;
        wpSettings.IsExpanded = true;
        wpSettings.IsSelected = false;
        wpSettings.NotifyChanges();
        WPSettingsGroups parent = wpSettings.Parent as WPSettingsGroups;
        parent.NotifyChanges();
        parent.ShowAllItems();
      }
    }

    private void CreateWebLinkAsset(
      WebLinkSettings wbs,
      string propertyName,
      string propertyDisplayValue,
      WPSettings settings)
    {
      PropertyInfo property = wbs.GetType().GetProperty(propertyName);
      if (!(property != (PropertyInfo) null))
        return;
      string propertyValue = property.GetValue((object) wbs, (object[]) null) as string;
      if (string.IsNullOrWhiteSpace(propertyValue))
        return;
      SettingAttribute customAttribute = property.GetCustomAttribute<SettingAttribute>();
      if (customAttribute == null)
        return;
      WPSetting setting = settings.FindByName(customAttribute.Name);
      WPSettingsAssetsGroup parent = setting.WPAssets.Children.First<WPListItemBase>((Func<WPListItemBase, bool>) (group => group.DisplayText.Equals(setting.PolicySetting.AssetInfo.Name))) as WPSettingsAssetsGroup;
      List<Asset> previewItem = parent.GetPreviewItem(false, 0) as List<Asset>;
      if (previewItem != null)
      {
        List<Asset> source = previewItem;
        Func<Asset, bool> predicate = (Func<Asset, bool>) (a => a.Source.Equals(propertyValue, StringComparison.OrdinalIgnoreCase));
        Asset asset;
        if ((asset = ((IEnumerable<Asset>) source).Where<Asset>(predicate).FirstOrDefault<Asset>()) != null)
        {
          setting.Value = asset.DisplayName;
          return;
        }
      }
      WPSettingAsset wpSettingAsset = WPSettingAsset.AddNewItem((WPListItemBase) parent, propertyValue, propertyDisplayValue, parent.GroupName) as WPSettingAsset;
      setting.Value = wpSettingAsset.DisplayName;
    }

    private IEnumerable<Application> GetOEMAppsAvailableToPin()
    {
      HashSet<Application> first = new HashSet<Application>((IEnumerable<Application>) this.ConfiguredOEMApps);
      HashSet<Application> pinnedAppsSet = new HashSet<Application>();
      this.PinnedApps.Where<PinnedAppSettingsVM>((Func<PinnedAppSettingsVM, bool>) (a => a.AppSettings.AppType == AppType.OEMApplication)).ToList<PinnedAppSettingsVM>().ForEach((Action<PinnedAppSettingsVM>) (a => pinnedAppsSet.Add(a.AvailableOEMApps.ElementAt<Application>(a.AppSettings.SelectedAppIndex))));
      return ((IEnumerable<Application>) first).Except<Application>((IEnumerable<Application>) pinnedAppsSet);
    }

    private IEnumerable<EnumWrapper> GetMSAppsAvailableToPin()
    {
      HashSet<string> pinnedMSAppsSet = new HashSet<string>();
      this.PinnedApps.Where<PinnedAppSettingsVM>((Func<PinnedAppSettingsVM, bool>) (a => a.AppSettings.AppType == AppType.MSApplication)).ToList<PinnedAppSettingsVM>().ForEach((Action<PinnedAppSettingsVM>) (a => pinnedMSAppsSet.Add(a.AppSettings.Name)));
      return PinnedAppsVM.MSAppsList.Where<EnumWrapper>((Func<EnumWrapper, bool>) (app => !pinnedMSAppsSet.Contains(app.DisplayText)));
    }

    private static List<EnumWrapper> MSAppsList => EnumWrapper.GetEnumList(typeof (MSApps));

    private class FuncComparer<T> : IComparer<T>
    {
      private readonly Comparison<T> comparison;

      public FuncComparer(Comparison<T> comparison) => this.comparison = comparison;

      public int Compare(T x, T y) => this.comparison(x, y);
    }
  }
}
