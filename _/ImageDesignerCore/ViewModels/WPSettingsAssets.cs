// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPSettingsAssets
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.MCSF.Offline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPSettingsAssets : WPListItemBase
  {
    public List<Asset> Assets;
    public PolicyGroup PolicyGroup;

    public WPSettingsAssets(
      List<Asset> assets,
      PolicyGroup policyGroup,
      bool isIncludedInOutput,
      WPListItemBase parent)
      : base(parent)
    {
      this.Assets = assets;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.PolicyGroup = policyGroup;
      ObservableCollection<WPListItemBase> collection = new ObservableCollection<WPListItemBase>();
      Dictionary<string, PolicyAssetInfo> dictionary1 = new Dictionary<string, PolicyAssetInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      Dictionary<string, List<Asset>> dictionary2 = new Dictionary<string, List<Asset>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      if (policyGroup != null)
      {
        foreach (PolicyAssetInfo asset in policyGroup.Assets)
        {
          dictionary1[asset.Name] = asset;
          dictionary2[asset.Name] = new List<Asset>();
        }
      }
      foreach (Asset asset in this.Assets)
      {
        bool flag = false;
        foreach (KeyValuePair<string, PolicyAssetInfo> keyValuePair in dictionary1)
        {
          if (keyValuePair.Value == null && keyValuePair.Key.Equals(asset.Name, StringComparison.OrdinalIgnoreCase) || keyValuePair.Value != null && keyValuePair.Value.IsMatch(asset.Name))
          {
            flag = true;
            dictionary2[keyValuePair.Key].Add(asset);
            break;
          }
        }
        if (!flag)
        {
          dictionary1[asset.Name] = (PolicyAssetInfo) null;
          dictionary2[asset.Name] = new List<Asset>();
          dictionary2[asset.Name].Add(asset);
        }
      }
      foreach (KeyValuePair<string, PolicyAssetInfo> keyValuePair in dictionary1)
      {
        string key = keyValuePair.Key;
        List<Asset> assets1 = dictionary2[keyValuePair.Key];
        PolicyAssetInfo assetInfo = keyValuePair.Value;
        collection.Add((WPListItemBase) new WPSettingsAssetsGroup(assets1, this.PolicyGroup, assetInfo, key, isIncludedInOutput, (WPListItemBase) this));
      }
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) collection);
      this.InitializationComplete();
    }

    public WPSettingsAssets(PolicyGroup policyGroup, WPListItemBase parent)
      : base(parent)
    {
      this.Assets = new List<Asset>();
      this.PolicyGroup = policyGroup;
      ObservableCollection<WPListItemBase> collection = new ObservableCollection<WPListItemBase>();
      bool isIncludedInOutput = false;
      foreach (PolicyAssetInfo asset in this.PolicyGroup.Assets)
        collection.Add((WPListItemBase) new WPSettingsAssetsGroup(this.Assets, this.PolicyGroup, asset, asset.Name, isIncludedInOutput, (WPListItemBase) this));
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) collection);
      this.InitializationComplete();
    }

    public List<WPSettingAsset> GetAssets(string groupName = null)
    {
      List<WPSettingAsset> assets = new List<WPSettingAsset>();
      foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
      {
        WPSettingsAssetsGroup settingsAssetsGroup = child as WPSettingsAssetsGroup;
        if (groupName == null || settingsAssetsGroup.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase))
        {
          assets.AddRange((IEnumerable<WPSettingAsset>) settingsAssetsGroup.GetAssets());
          if (groupName != null)
            break;
        }
      }
      return assets;
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/resource_32xMD.png";

    public override string DisplayText => Tools.GetString("tbCOSSettingsAssets");

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      List<Asset> previewItem1 = new List<Asset>();
      if (this.Children != null)
      {
        foreach (WPListItemBase child1 in (Collection<WPListItemBase>) this.Children)
        {
          foreach (WPListItemBase child2 in (Collection<WPListItemBase>) child1.Children)
          {
            Asset previewItem2 = (child2 as WPSettingAsset).GetPreviewItem(includeAllLevels, level + 1) as Asset;
            previewItem1.Add(previewItem2);
          }
        }
      }
      return (object) previewItem1;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.Assets.Clear();
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          WPSettingsAssetsGroup settingsAssetsGroup = child as WPSettingsAssetsGroup;
          settingsAssetsGroup.Save();
          this.Assets.AddRange((IEnumerable<Asset>) settingsAssetsGroup.Assets);
        }
        this.IsHideAble = false;
      }
      return base.SaveItem();
    }
  }
}
