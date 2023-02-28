// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPDataAssetsGroups
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPDataAssetsGroups : WPListItemBase
  {
    public List<DataAssets> Groups;

    public WPDataAssetsGroups(
      List<DataAssets> groups,
      bool isIncludedInOutput,
      WPListItemBase parent)
      : base(parent)
    {
      this.Groups = groups;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.ToolTip = Tools.GetString("ttCOSDataAssetsGroups");
      ObservableCollection<WPListItemBase> collection = new ObservableCollection<WPListItemBase>();
      foreach (DataAssets group in groups)
        collection.Add((WPListItemBase) new WPDataAssetsGroup(group, isIncludedInOutput, (WPListItemBase) this));
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) collection);
      this.InitializationComplete();
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/folder_closed_32xMD.png";

    public override string DisplayText => Tools.GetString("txtCOSDataAssetsGroups");

    public List<string> GetAvailableAssetTypes()
    {
      List<string> list = WPCustomizationAssetType.GetListNames().ToList<string>();
      List<string> second = new List<string>();
      foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
      {
        WPDataAssetsGroup wpDataAssetsGroup = child as WPDataAssetsGroup;
        second.Add(((object) wpDataAssetsGroup.SelectedType).ToString());
      }
      return list.Except<string>((IEnumerable<string>) second).ToList<string>();
    }

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      List<DataAssets> previewItem1 = new List<DataAssets>();
      if (this.Children != null && (includeAllLevels || !includeAllLevels && level <= 1))
      {
        ++level;
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          DataAssets previewItem2 = child.GetPreviewItem(includeAllLevels, level) as DataAssets;
          previewItem1.Add(previewItem2);
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
        Variant variantItem = (this.Parent as WPVariant).VariantItem;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifyDataAssetGroups((IEnumerable<DataAssets>) (this.GetPreviewItem(includChildren, 0) as List<DataAssets>), variantItem, flag).ToList<CustomizationError>(), (WPListItemBase) this);
      }
      base.ValidateItem();
    }

    public static WPListItemBase AddNewItem(WPListItemBase parent)
    {
      WPDataAssetsGroups newItem = new WPDataAssetsGroups(new List<DataAssets>(), false, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      return (WPListItemBase) newItem;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.Groups.Clear();
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          {
            WPDataAssetsGroup wpDataAssetsGroup = child as WPDataAssetsGroup;
            if (wpDataAssetsGroup.DataAssets != null && wpDataAssetsGroup.DataAssets.Items != null && ((IEnumerable<DataAsset>) wpDataAssetsGroup.DataAssets.Items).Count<DataAsset>() != 0)
              this.Groups.Add(wpDataAssetsGroup.DataAssets);
          }
        }
      }
      return base.SaveItem();
    }
  }
}
