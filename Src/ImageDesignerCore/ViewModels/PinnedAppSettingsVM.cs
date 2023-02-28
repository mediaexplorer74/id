// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.PinnedAppSettingsVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Application = Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Application;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class PinnedAppSettingsVM : PinVMBase
  {
    private Application _selectedOEMApplication;
    public static readonly DependencyProperty SelectedAppTypeProperty = DependencyProperty.Register(nameof (SelectedAppType), typeof (AppType), typeof (PinnedAppSettingsVM), new PropertyMetadata((object) AppType.OEMApplication, new PropertyChangedCallback(PinnedAppSettingsVM.OnSelectedAppTypeChanged)));
    public static readonly DependencyProperty AppSettingsProperty = DependencyProperty.Register(nameof (AppSettings), typeof (PinnedAppSettings), typeof (PinnedAppSettingsVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty AppTileSizeListProperty = DependencyProperty.Register(nameof (AppTileSizeList), typeof (List<EnumWrapper>), typeof (PinnedAppSettingsVM), new PropertyMetadata((PropertyChangedCallback) null));

    public PinnedAppSettingsVM(
      WPSettings settings,
      IEnumerable<Application> availableOEMApps,
      IEnumerable<EnumWrapper> availableMSApps)
    {
      this.AvailableOEMApps = availableOEMApps;
      this.AvailableMSApps = availableMSApps;
      this.WPSettings = settings;
      bool flag = settings.IncludedSettings.Count > 0;
      this.SelectedAppType = !flag ? (availableOEMApps.Count<Application>() <= 0 ? (availableMSApps.Count<EnumWrapper>() <= 0 ? AppType.WebLink : AppType.MSApplication) : AppType.OEMApplication) : PinnedAppSettingsVM.GetAppTypeFromSettings(settings);
     
       //RnD     
       switch (this.SelectedAppType)
      {
                // fix it
        case AppType.OEMApplication:
          this.AppSettings = (PinnedAppSettings) new InfusedAppSettings((PinnedAppSettings) null, 
              this.SelectedAppType, /*availableOEMApps*/null, this.AvailableMSApps);
          break;

             // fix it
        case AppType.MSApplication:
          this.AppSettings = (PinnedAppSettings) 
                        new InfusedAppSettings((PinnedAppSettings) null, 
                        this.SelectedAppType, /*availableOEMApps*/null, this.AvailableMSApps);
          break;
        case AppType.WebLink:
          this.AppSettings = (PinnedAppSettings)
                        new WebLinkSettings((PinnedAppSettings) null, this.SelectedAppType);
          break;
      }
      if (flag)
        this.LoadAppSettings(settings);
      this.AppTileSizeList = EnumWrapper.GetEnumList(typeof (AppTileSize));
      this.Initialized = true;
    }

    internal bool Initialized { get; private set; }

    public WPSettings WPSettings { get; private set; }

    public Dictionary<AppType, string> AppTypes => this.AvailableAppTypes;

    public Application SelectedOEMApplication
    {
      get
      {
        if (this.AppSettings != null && this.AppSettings.AppType == AppType.OEMApplication)
          this._selectedOEMApplication = this.AvailableOEMApps.ElementAt<Application>(this.AppSettings.SelectedAppIndex);
        return this._selectedOEMApplication;
      }
    }

    public IEnumerable<Application> AvailableOEMApps { get; private set; }

    public IEnumerable<EnumWrapper> AvailableMSApps { get; private set; }

    public string WebLinkIconFileExtensionsFilter
    {
      get
      {
        WPSetting wpSetting = this.WPSettings.FindByPattern("(StartPrepinnedWebLink)(.*)(Icon)").FirstOrDefault<WPSetting>();
        string extensionsFilter;
        if (wpSetting != null)
          extensionsFilter = WPSettingsAssetsGroup.GetFileFilters(wpSetting.PolicySetting.AssetInfo);
        else
          extensionsFilter = Tools.GetString("txtJPEGFilter") + "|" + Tools.GetString("txtJPGFilter") + "|" + Tools.GetString("txtPNGFilter");
        return extensionsFilter;
      }
    }

    public AppType SelectedAppType
    {
      get => (AppType) this.GetValue(PinnedAppSettingsVM.SelectedAppTypeProperty);
      set => this.SetValue(PinnedAppSettingsVM.SelectedAppTypeProperty, (object) value);
    }

    protected static void OnSelectedAppTypeChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is PinnedAppSettingsVM pinnedAppSettingsVm) || !pinnedAppSettingsVm.Initialized)
        return;
      pinnedAppSettingsVm.AppSettings = PinnedAppSettingsVM.GetAppSettings(pinnedAppSettingsVm.AppSettings, pinnedAppSettingsVm.SelectedAppType, pinnedAppSettingsVm.AvailableOEMApps, pinnedAppSettingsVm.AvailableMSApps);
    }

    public PinnedAppSettings AppSettings
    {
      get => (PinnedAppSettings) this.GetValue(PinnedAppSettingsVM.AppSettingsProperty);
      set => this.SetValue(PinnedAppSettingsVM.AppSettingsProperty, (object) value);
    }

    public List<EnumWrapper> AppTileSizeList
    {
      get => (List<EnumWrapper>) this.GetValue(PinnedAppSettingsVM.AppTileSizeListProperty);
      set => this.SetValue(PinnedAppSettingsVM.AppTileSizeListProperty, (object) value);
    }

    private void LoadAppSettings(WPSettings settings)
    {
      if (settings != null)
        this.AppSettings.LoadFromSettings<SettingAttribute>(settings);
      switch (this.AppSettings.AppType)
      {
        case AppType.OEMApplication:
          WPSetting byName1 = settings.FindByName("StartPrepinnedOEMAppID");
          if (byName1 == null)
            break;
          Application appFromProductId = this.GetConfiguredAppFromProductId(byName1.Value);
          if (appFromProductId == null)
            break;
          this.AppSettings.Name = Path.GetFileName(appFromProductId.Source);
          this.AppSettings.SelectedAppIndex = new List<Application>(this.AvailableOEMApps).IndexOf(appFromProductId);
          break;
        case AppType.MSApplication:
          WPSetting byName2 = settings.FindByName("StartPrepinnedMSAppID");
          if (byName2 == null)
            break;
          this.AppSettings.Name = Enum.GetName(typeof (MSApps), (object) int.Parse(byName2.Value));
          this.AppSettings.SelectedAppIndex = int.Parse(byName2.Value);
          break;
        case AppType.WebLink:
          WebLinkSettings appSettings = this.AppSettings as WebLinkSettings;
          Asset webLinkAsset1 = this.FindWebLinkAsset(settings, "WebLinkSmallIcon");
          if (webLinkAsset1 != null)
          {
            appSettings.WebLinkSmallIcon = webLinkAsset1.Source;
            appSettings.WebLinkSmallIconDisplay = webLinkAsset1.DisplayName;
          }
          Asset webLinkAsset2 = this.FindWebLinkAsset(settings, "WebLinkMediumLargeIcon");
          if (webLinkAsset2 == null)
            break;
          appSettings.WebLinkMediumLargeIcon = webLinkAsset2.Source;
          appSettings.WebLinkMediumLargeIconDisplay = webLinkAsset2.DisplayName;
          break;
      }
    }

    private Asset FindWebLinkAsset(WPSettings settings, string propertyName)
    {
      Asset webLinkAsset = (Asset) null;
      PropertyInfo property = (this.AppSettings as WebLinkSettings).GetType().GetProperty(propertyName);
      if (property != (PropertyInfo) null)
      {
        SettingAttribute customAttribute = property.GetCustomAttribute<SettingAttribute>();
        if (customAttribute != null)
        {
          WPSetting setting = settings.FindByName(customAttribute.Name);
          if ((setting.WPAssets.Children.First<WPListItemBase>((Func<WPListItemBase, bool>) (group => group.DisplayText.Equals(setting.PolicySetting.AssetInfo.Name))) as WPSettingsAssetsGroup).GetPreviewItem(false, 0) is List<Asset> previewItem)
          {
            Func<Asset, bool> predicate = (Func<Asset, bool>) (a =>
            {
              string fileName = Path.GetFileName(a.Source);
              return fileName != null && fileName.Equals(setting.Value, StringComparison.OrdinalIgnoreCase);
            });
            webLinkAsset = ((IEnumerable<Asset>) previewItem).Where<Asset>(predicate).FirstOrDefault<Asset>();
          }
        }
      }
      return webLinkAsset;
    }

    private Application GetConfiguredAppFromProductId(string productID) => this.AvailableOEMApps.Where<Application>((Func<Application, bool>) (app =>
    {
      string applicationProductId = Tools.GetApplicationProductID(app);
      return applicationProductId != null && applicationProductId.Equals(productID, StringComparison.OrdinalIgnoreCase);
    })).FirstOrDefault<Application>();

    private static AppType GetAppTypeFromSettings(WPSettings settings)
    {
      AppType typeFromSettings = AppType.OEMApplication;
      if (settings != null)
      {
        WPSetting wpSetting = settings.FindByPattern("(StartPrepinned)(((OEM|MS)AppID)|(WebLinkURL))", true).FirstOrDefault<WPSetting>();
        if (wpSetting != null)
          typeFromSettings = !wpSetting.Name.Equals("StartPrepinnedOEMAppID", StringComparison.OrdinalIgnoreCase) ? (!wpSetting.Name.Equals("StartPrepinnedMSAppID", StringComparison.OrdinalIgnoreCase) ? AppType.WebLink : AppType.MSApplication) : AppType.OEMApplication;
        else if (settings.FindByPattern("(.*)(WebLink)(.*)", true).FirstOrDefault<WPSetting>() != null)
          typeFromSettings = AppType.WebLink;
      }
      return typeFromSettings;
    }

    //RnD, TODO
    private static PinnedAppSettings GetAppSettings(
      PinnedAppSettings settings,
      AppType appType,
      IEnumerable<Application> availableOEMApps,
      IEnumerable<EnumWrapper> availableMSApps)
    {
      PinnedAppSettings newSettings = (PinnedAppSettings) null;
      switch (appType)
      {
                // fix it
        case AppType.OEMApplication:
          newSettings = (PinnedAppSettings) 
                        new InfusedAppSettings(settings, appType, /*availableOEMApps*/null, availableMSApps);
          break;
        case AppType.MSApplication:
          newSettings = (PinnedAppSettings) 
                        new InfusedAppSettings(settings, appType, /*availableOEMApps*/null, availableMSApps);
          break;
        case AppType.WebLink:
          newSettings = (PinnedAppSettings) 
                        new WebLinkSettings(settings, appType);
          break;
      }
      Tools.DispatcherExec((Action) (() => 
      BindingOperations.GetBindingExpressionBase(
          (DependencyObject) newSettings, 
            PinnedAppSettingsVM.AppSettingsProperty)?.UpdateTarget()));

      return newSettings;
    }

    private Dictionary<AppType, string> AvailableAppTypes
    {
      get
      {
        Dictionary<AppType, string> availableAppTypes = new Dictionary<AppType, string>();
        if (this.AvailableOEMApps.Count<Application>() > 0)
          availableAppTypes.Add(AppType.OEMApplication, Tools.GetString("eAppTypeOEMApplication"));
        if (this.AvailableMSApps.Count<EnumWrapper>() > 0)
          availableAppTypes.Add(AppType.MSApplication, Tools.GetString("eAppTypeMSApplication"));
        availableAppTypes.Add(AppType.WebLink, Tools.GetString("eAppTypeWebLink"));
        return availableAppTypes;
      }
    }
  }
}
