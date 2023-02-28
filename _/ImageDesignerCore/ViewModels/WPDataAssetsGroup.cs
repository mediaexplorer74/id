// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPDataAssetsGroup
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPDataAssetsGroup : WPListItemBase
  {
    public DataAssets DataAssets;
    public static readonly DependencyProperty TypeForegroundProperty = DependencyProperty.Register(nameof (TypeForeground), typeof (string), typeof (WPDataAssetsGroup), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty AssetTypesProperty = DependencyProperty.Register(nameof (AssetTypes), typeof (ObservableCollection<WPCustomizationAssetType>), typeof (WPDataAssetsGroup), new PropertyMetadata((object) null, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));

    public WPDataAssetsGroup(DataAssets dataAssets, bool isIncludedInOutput, WPListItemBase parent)
      : base(parent)
    {
      this.DataAssets = dataAssets;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.AssetTypes = WPCustomizationAssetType.GetList(this.DataAssets.Type);
      ObservableCollection<WPListItemBase> collection = new ObservableCollection<WPListItemBase>();
      foreach (DataAsset dataAsset in dataAssets.Items)
        collection.Add((WPListItemBase) new WPDataAsset(dataAsset, isIncludedInOutput, (WPListItemBase) this));
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) collection);
      this.InitializationComplete();
    }

    public override string TreeViewIcon => this.SelectedType == null ? "/ImageDesigner;component/Resources/Images/maptilelayer_32x.png" : "/ImageDesigner;component/Resources/Images/folder_closed_32xMD.png";

    public override string DisplayText => this.AssetTypes.Where<WPCustomizationAssetType>((Func<WPCustomizationAssetType, bool>) (asset => asset.IsSelected)).Count<WPCustomizationAssetType>() != 0 ? this.AssetTypes.First<WPCustomizationAssetType>((Func<WPCustomizationAssetType, bool>) (asset => asset.IsSelected)).DisplayText : ((object) this.DataAssets.Type).ToString();

    public override string ToolTip
    {
      get
      {
        string toolTip = "";
        if (this.SelectedType == 0)
          toolTip = Tools.GetString("ttCOSMapData");
        return toolTip;
      }
      set => base.ToolTip = value;
    }

    public List<string> FileTypes => new List<string>()
    {
      ".*"
    };

    public List<string> InUseSourceFilenames
    {
      get
      {
        List<string> useSourceFilenames = new List<string>();
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          WPDataAsset wpDataAsset = child as WPDataAsset;
          if (Directory.Exists(wpDataAsset.Source))
            useSourceFilenames.Add(wpDataAsset.Source);
          else
            useSourceFilenames.Add(Path.GetFileName(wpDataAsset.Source));
        }
        return useSourceFilenames;
      }
    }

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      DataAssets previewItem1 = new DataAssets();
      previewItem1.Items = new List<DataAsset>();
      previewItem1.Type = this.AssetTypes.First<WPCustomizationAssetType>((Func<WPCustomizationAssetType, bool>) (asset => asset.IsSelected)).Value;
      if (this.Children != null && (includeAllLevels || !includeAllLevels && level <= 1))
      {
        ++level;
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          DataAsset previewItem2 = child.GetPreviewItem(includeAllLevels, level) as DataAsset;
          previewItem1.Items.Add(previewItem2);
        }
      }
      return (object) previewItem1;
    }

    protected override void ValidateItem()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        bool includChildren = false;
        bool flag = false;
        Variant variantItem = (this.Parent.Parent as WPVariant).VariantItem;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifyDataAssetGroup(this.GetPreviewItem(includChildren, 0) as DataAssets, variantItem, flag).ToList<CustomizationError>(), (WPListItemBase) this);
      }
      base.ValidateItem();
    }

    private bool _isTypeDirty
    {
      get
      {
        if (this.IsNewItem)
          return true;
        if (this.AssetTypes.Where<WPCustomizationAssetType>((Func<WPCustomizationAssetType, bool>) (asset => asset.IsSelected)).Count<WPCustomizationAssetType>() != 0)
        {
          WPCustomizationAssetType customizationAssetType = this.AssetTypes.First<WPCustomizationAssetType>((Func<WPCustomizationAssetType, bool>) (asset => asset.IsSelected));
          if (this.DataAssets != null && customizationAssetType.Value != this.DataAssets.Type)
            return true;
        }
        return false;
      }
    }

    public CustomizationDataAssetType SelectedType
    {
      get
      {
        CustomizationDataAssetType type = this.DataAssets.Type;
        if (this.AssetTypes.Where<WPCustomizationAssetType>((Func<WPCustomizationAssetType, bool>) (asset => asset.IsSelected)).Count<WPCustomizationAssetType>() != 0)
          type = this.AssetTypes.First<WPCustomizationAssetType>((Func<WPCustomizationAssetType, bool>) (asset => asset.IsSelected)).Value;
        return type;
      }
    }

    protected override void UpdateIsDirty()
    {
      if (this._isTypeDirty || this.ChildListChanged)
        this._isDirty = true;
      else
        this._isDirty = false;
    }

    public static WPListItemBase AddNewItem(WPListItemBase parent, string assetType)
    {
      bool isIncludedInOutput = false;
      WPDataAssetsGroup newItem = new WPDataAssetsGroup(new DataAssets((CustomizationDataAssetType) Enum.Parse(typeof (CustomizationDataAssetType), assetType))
      {
        Items = new List<DataAsset>()
      }, isIncludedInOutput, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      return (WPListItemBase) newItem;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.DataAssets.Items.Clear();
        if (this.Children != null)
        {
          this.DataAssets.Type = this.AssetTypes.First<WPCustomizationAssetType>((Func<WPCustomizationAssetType, bool>) (asset => asset.IsSelected)).Value;
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
            this.DataAssets.Items.Add((child as WPDataAsset).DataAsset);
        }
      }
      return base.SaveItem();
    }

    public string TypeForeground
    {
      get
      {
        this.TypeForeground = !this._isTypeDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor;
        return (string) this.GetValue(WPDataAssetsGroup.TypeForegroundProperty);
      }
      set => this.SetValue(WPDataAssetsGroup.TypeForegroundProperty, (object) value);
    }

    public ObservableCollection<WPCustomizationAssetType> AssetTypes
    {
      get => (ObservableCollection<WPCustomizationAssetType>) this.GetValue(WPDataAssetsGroup.AssetTypesProperty);
      set => this.SetValue(WPDataAssetsGroup.AssetTypesProperty, (object) value);
    }
  }
}
