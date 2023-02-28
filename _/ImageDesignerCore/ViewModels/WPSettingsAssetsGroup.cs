// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPSettingsAssetsGroup
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.MCSF.Offline;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPSettingsAssetsGroup : WPListItemBase
  {
    public List<Asset> Assets;
    public PolicyGroup PolicyGroup;
    public PolicyAssetInfo AssetInfo;

    public WPSettingsAssetsGroup(
      List<Asset> assets,
      PolicyGroup policyGroup,
      PolicyAssetInfo assetInfo,
      string groupName,
      bool isIncludedInOutput,
      WPListItemBase parent)
      : base(parent)
    {
      this.Assets = assets;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.PolicyGroup = policyGroup;
      this.AssetInfo = assetInfo;
      this.GroupName = this.AssetInfo == null ? groupName : this.AssetInfo.Name;
      ObservableCollection<WPListItemBase> collection = new ObservableCollection<WPListItemBase>();
      foreach (Asset asset in this.Assets)
        collection.Add((WPListItemBase) new WPSettingAsset(asset, this.AssetInfo, isIncludedInOutput, (WPListItemBase) this));
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) collection);
      this.InitializationComplete();
    }

    public WPSettingsAssetsGroup(
      PolicyGroup policyGroup,
      PolicyAssetInfo policyAssetInfo,
      WPListItemBase parent)
      : base(parent)
    {
      this.Assets = new List<Asset>();
      this.PolicyGroup = policyGroup;
      this.AssetInfo = policyAssetInfo;
      this.InitializationComplete();
    }

    public List<string> InUseDisplayNames
    {
      get
      {
        List<string> inUseDisplayNames = new List<string>();
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          {
            WPSettingAsset wpSettingAsset = child as WPSettingAsset;
            if (!string.IsNullOrEmpty(wpSettingAsset.DisplayName))
              inUseDisplayNames.Add(wpSettingAsset.DisplayName);
          }
        }
        return inUseDisplayNames;
      }
    }

    public List<string> InUseFilenames
    {
      get
      {
        List<string> inUseFilenames = new List<string>();
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          {
            WPSettingAsset wpSettingAsset = child as WPSettingAsset;
            if (!string.IsNullOrEmpty(wpSettingAsset.Source))
              inUseFilenames.Add(Path.GetFileName(wpSettingAsset.Source));
          }
        }
        return inUseFilenames;
      }
    }

    public string FileExtensionsFilter
    {
      get
      {
        string extensionsFilter = "";
        if (this.AssetInfo != null)
          extensionsFilter = WPSettingsAssetsGroup.GetFileFilters(this.AssetInfo);
        return extensionsFilter;
      }
    }

    public static string GetFileFilters(PolicyAssetInfo assetInfo)
    {
      string str1 = "";
      string str2 = "";
      foreach (string fileType in assetInfo.FileTypes)
        str2 = str2 + ";*" + fileType;
      string str3 = str2.Trim(';');
      return (str1 + string.Format("|{0} ({1})|{1}", (object) assetInfo.Name, (object) str3)).Trim('|');
    }

    public List<WPSettingAsset> GetAssets()
    {
      List<WPSettingAsset> assets = new List<WPSettingAsset>();
      foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
      {
        WPSettingAsset wpSettingAsset = child as WPSettingAsset;
        assets.Add(wpSettingAsset);
      }
      return assets;
    }

    public override void NotifyChanges()
    {
      WPSettings parent = this.Parent.Parent as WPSettings;
      if (parent.Children != null)
      {
        foreach (WPListItemBase child in (Collection<WPListItemBase>) parent.Children)
        {
          if (child is WPSetting wpSetting)
            wpSetting.RefreshOptions();
        }
      }
      base.NotifyChanges();
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/folder_closed_32xMD.png";

    public override string DisplayText => this.GroupName;

    public string GroupName { get; set; }

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      List<Asset> previewItem1 = new List<Asset>();
      if (this.Children != null)
      {
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          Asset previewItem2 = (child as WPSettingAsset).GetPreviewItem(includeAllLevels, level + 1) as Asset;
          previewItem1.Add(previewItem2);
        }
      }
      return (object) previewItem1;
    }

    protected override void ValidateItem()
    {
      WPErrors wpErrors = new WPErrors(CustomContentGenerator.VerifyAssets((this.Parent.Parent as WPSettings).GetPreviewItem(true, 0) as Settings, this.PolicyGroup).ToList<CustomizationError>(), (WPListItemBase) this);
      base.ValidateItem();
    }

    protected override void UpdateIsDirty()
    {
      if (this.ChildListChanged)
        this._isDirty = true;
      else
        this._isDirty = false;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.Assets.Clear();
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          WPSettingAsset wpSettingAsset = child as WPSettingAsset;
          wpSettingAsset.Save();
          this.Assets.Add(wpSettingAsset.Asset);
        }
        this.IsHideAble = false;
      }
      return base.SaveItem();
    }
  }
}
