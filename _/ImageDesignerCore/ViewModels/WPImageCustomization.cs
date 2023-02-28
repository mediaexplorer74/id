// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPImageCustomization
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.MCSF.Offline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPImageCustomization : WPListItemBase
  {
    public ImageCustomizations ImageCustomizations;

    public WPImageCustomization(
      ImageCustomizations imageCustomizations,
      PolicyStore policyStore,
      bool isIncludedInOutput,
      CustomizeOSPageVM vm)
      : base((WPListItemBase) null)
    {
      this.ImageCustomizations = imageCustomizations;
      WPListItemBase.PolicyStore = policyStore;
      this.IsIncludedInOutput = isIncludedInOutput;
      WPListItemBase.VM = vm;
      ObservableCollection<WPListItemBase> observableCollection = new ObservableCollection<WPListItemBase>();
      if (this.ImageCustomizations.StaticVariant != null)
        observableCollection.Add((WPListItemBase) new WPVariant((Variant) this.ImageCustomizations.StaticVariant, this.ImageCustomizations, isIncludedInOutput, (WPListItemBase) this));
      foreach (Variant variant in this.ImageCustomizations.Variants)
        observableCollection.Add((WPListItemBase) new WPVariant(variant, this.ImageCustomizations, isIncludedInOutput, (WPListItemBase) this));
      if (observableCollection.Count<WPListItemBase>() != 0 && observableCollection.Where<WPListItemBase>((Func<WPListItemBase, bool>) (temp => temp.IsSelected)).Count<WPListItemBase>() == 0)
        observableCollection[0].IsSelected = true;
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) observableCollection);
      this.InitializationComplete();
    }

    public override string DisplayText => Tools.GetString("txtCOSDefaultImageCustomizationName");

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      ImageCustomizations previewItem1 = new ImageCustomizations();
      previewItem1.StaticVariant = new StaticVariant();
      previewItem1.Variants = new List<Variant>();
      previewItem1.Targets = new List<Target>();
      previewItem1.DefinedInFile = this.ImageCustomizations.DefinedInFile;
      previewItem1.Description = this.ImageCustomizations.Description;
      previewItem1.Name = this.ImageCustomizations.Name;
      previewItem1.Owner = this.ImageCustomizations.Owner;
      previewItem1.OwnerType = this.ImageCustomizations.OwnerType;
      if (this.Children != null && (includeAllLevels || !includeAllLevels && level <= 1))
      {
        ++level;
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          object previewItem2 = child.GetPreviewItem(includeAllLevels, level);
          switch (previewItem2)
          {
            case StaticVariant _:
              previewItem1.StaticVariant = previewItem2 as StaticVariant;
              continue;
            case Variant _:
              previewItem1.Variants.Add(previewItem2 as Variant);
              continue;
            default:
              previewItem1.Targets = previewItem2 as List<Target>;
              continue;
          }
        }
      }
      return (object) previewItem1;
    }

    public override string DisplayPath => "";

    protected override bool SaveItem()
    {
      ImageCustomizations imageCustomizations = new ImageCustomizations();
      imageCustomizations.Description = this.ImageCustomizations.Description;
      imageCustomizations.Name = this.ImageCustomizations.Name;
      imageCustomizations.Owner = this.ImageCustomizations.Owner;
      imageCustomizations.OwnerType = this.ImageCustomizations.OwnerType;
      imageCustomizations.Targets = new List<Target>();
      foreach (WPListItemBase wpListItemBase in this.Children.Where<WPListItemBase>((Func<WPListItemBase, bool>) (item => item.GetType() == typeof (WPVariant))))
      {
        WPVariant wpVariant = wpListItemBase as WPVariant;
        if (wpVariant.VariantItem.ApplicationGroups != null && ((IEnumerable<Applications>) wpVariant.VariantItem.ApplicationGroups).Count<Applications>() == 0)
          wpVariant.VariantItem.ApplicationGroups = (List<Applications>) null;
        if (wpVariant.VariantItem.SettingGroups != null && ((IEnumerable<Settings>) wpVariant.VariantItem.SettingGroups).Count<Settings>() == 0)
          wpVariant.VariantItem.SettingGroups = (List<Settings>) null;
        if (wpVariant.IsStaticVariant)
        {
          imageCustomizations.StaticVariant = wpVariant.VariantItem as StaticVariant;
          if (imageCustomizations.StaticVariant.DataAssetGroups != null && ((IEnumerable<DataAssets>) imageCustomizations.StaticVariant.DataAssetGroups).Count<DataAssets>() == 0)
            imageCustomizations.StaticVariant.DataAssetGroups = (List<DataAssets>) null;
        }
        else
        {
          if (imageCustomizations.Variants == null)
            imageCustomizations.Variants = new List<Variant>();
          imageCustomizations.Variants.Add(wpVariant.VariantItem);
          Target target = wpVariant.GetTarget();
          if (target != null)
            imageCustomizations.Targets.Add(target);
        }
        wpListItemBase.UpdateFontSettings();
        wpListItemBase.NotifyFontChanges();
      }
      this.ImageCustomizations = imageCustomizations;
      return base.SaveItem();
    }
  }
}
