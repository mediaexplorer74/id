// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPVariant
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPVariant : WPListItemBase
  {
    public Variant VariantItem;
    private ImageCustomizations _imageCustomizations;
    public bool IsStaticVariant;
    public string Name;
    public static readonly DependencyProperty ShowAllProperty = DependencyProperty.Register(nameof (ShowAll), typeof (bool), typeof (WPVariant), new PropertyMetadata((object) false));

    public WPVariant(string name, Dictionary<string, string> targetValues, WPListItemBase parent)
      : base(parent)
    {
      this.VariantItem = new Variant();
      this.VariantItem.Name = name;
      this.Name = name;
      this.VariantItem.TargetRefs = new List<TargetRef>();
      this.VariantItem.SettingGroups = new List<Settings>();
      this.VariantItem.ApplicationGroups = new List<Applications>();
      this.IsIncludedInOutput = false;
      this.IsStaticVariant = false;
      string id = string.Format("{0} Targets", (object) this.VariantItem.Name);
      this.VariantItem.TargetRefs.Add(new TargetRef(id));
      WPTarget.AddNewItem(id, targetValues, (WPListItemBase) this);
      WPApplicationsGroups.AddNewItem((WPListItemBase) this);
      WPSettingsGroups.AddNewItem((WPListItemBase) this);
      this.InitializationComplete();
    }

    public WPVariant(
      Variant variant,
      ImageCustomizations imageCustomizations,
      bool isIncludedInOutput,
      WPListItemBase parent)
      : base(parent)
    {
      this.VariantItem = variant;
      this._imageCustomizations = imageCustomizations;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.IsStaticVariant = this.VariantItem is StaticVariant;
      ObservableCollection<WPListItemBase> collection = new ObservableCollection<WPListItemBase>();
      this.Name = !this.IsStaticVariant ? this.VariantItem.Name : Tools.GetString("txtCOSStaticVariantName");
      if (!this.IsStaticVariant)
      {
        Target target1 = new Target();
        target1.Id = string.Format("{0} Target", (object) this.VariantItem.Name);
        target1.TargetStates = new List<TargetState>();
        if (this.VariantItem.TargetRefs != null)
        {
          foreach (TargetRef targetRef1 in this.VariantItem.TargetRefs)
          {
            TargetRef targetRef = targetRef1;
            Target target2 = imageCustomizations.Targets.Find((Predicate<Target>) (trig => trig.Id.Equals(targetRef.Id)));
            if (target2 != null)
              target1.TargetStates.AddRange((IEnumerable<TargetState>) target2.TargetStates);
          }
        }
        this.VariantItem.TargetRefs = new List<TargetRef>();
        this.VariantItem.TargetRefs.Add(new TargetRef(target1.Id));
        WPTarget wpTarget = new WPTarget(target1, this.IsIncludedInOutput, (WPListItemBase) this);
        collection.Add((WPListItemBase) wpTarget);
      }
      if (this.VariantItem.ApplicationGroups == null)
        this.VariantItem.ApplicationGroups = new List<Applications>();
      collection.Add((WPListItemBase) new WPApplicationsGroups(this.VariantItem.ApplicationGroups, isIncludedInOutput, (WPListItemBase) this));
      if (this.IsStaticVariant)
      {
        StaticVariant variantItem = this.VariantItem as StaticVariant;
        if (variantItem.DataAssetGroups == null)
          variantItem.DataAssetGroups = new List<DataAssets>();
        collection.Add((WPListItemBase) new WPDataAssetsGroups(variantItem.DataAssetGroups, isIncludedInOutput, (WPListItemBase) this));
      }
      if (this.VariantItem.SettingGroups == null)
        this.VariantItem.SettingGroups = new List<Settings>();
      WPSettingsGroups wpSettingsGroups = new WPSettingsGroups(this.VariantItem.SettingGroups, isIncludedInOutput, (WPListItemBase) this);
      collection.Add((WPListItemBase) wpSettingsGroups);
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) collection);
      this.InitializationComplete();
    }

    public override string DisplayText => this.Name;

    public void OnShowAllChanged(bool showAll)
    {
      this.ShowAll = showAll;
      WPSettingsGroups wpSettingsGroups = this.Children.First<WPListItemBase>((Func<WPListItemBase, bool>) (child => child is WPSettingsGroups)) as WPSettingsGroups;
      wpSettingsGroups.LoadPolicy();
      wpSettingsGroups.ShowAllItems(this.ShowAll);
    }

    public bool ShowAll
    {
      get => (bool) this.GetValue(WPVariant.ShowAllProperty);
      set => this.SetValue(WPVariant.ShowAllProperty, (object) value);
    }

    public Target GetTarget() => this.Children == null || this.Children.Count<WPListItemBase>() == 0 ? (Target) null : (this.Children[0] as WPTarget).Target;

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      Variant previewItem1 = new Variant();
      StaticVariant staticVariant = (StaticVariant) null;
      if (this.IsStaticVariant)
      {
        staticVariant = new StaticVariant();
        staticVariant.DataAssetGroups = new List<DataAssets>();
        previewItem1 = (Variant) staticVariant;
      }
      else
        previewItem1.TargetRefs = new List<TargetRef>();
      previewItem1.ApplicationGroups = new List<Applications>();
      previewItem1.SettingGroups = new List<Settings>();
      previewItem1.DefinedInFile = this.VariantItem.DefinedInFile;
      previewItem1.Name = this.VariantItem.Name;
      if (this.Children != null && (includeAllLevels || !includeAllLevels && level <= 1))
      {
        ++level;
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          object previewItem2 = child.GetPreviewItem(includeAllLevels, level);
          switch (previewItem2)
          {
            case List<DataAssets> _:
              staticVariant.DataAssetGroups = previewItem2 as List<DataAssets>;
              continue;
            case List<Applications> _:
              previewItem1.ApplicationGroups = previewItem2 as List<Applications>;
              continue;
            case List<Settings> _:
              previewItem1.SettingGroups = previewItem2 as List<Settings>;
              continue;
            default:
              continue;
          }
        }
      }
      return (object) previewItem1;
    }

    public override string DisplayPath => this.DisplayText;

    public void ClearValues()
    {
      this._isDirty = true;
      this.VariantItem.SettingGroups = new List<Settings>();
      this.VariantItem.ApplicationGroups = new List<Applications>();
      this.Children = new ObservableCollection<WPListItemBase>();
      WPApplicationsGroups.AddNewItem((WPListItemBase) this);
      if (this.IsStaticVariant)
      {
        (this.VariantItem as StaticVariant).DataAssetGroups = new List<DataAssets>();
        WPDataAssetsGroups.AddNewItem((WPListItemBase) this);
      }
      WPSettingsGroups.AddNewItem((WPListItemBase) this);
      this.Children[0].IsSelected = true;
    }

    protected override bool SaveItem()
    {
      Variant variant = new Variant();
      if (this.IsStaticVariant)
      {
        variant = (Variant) new StaticVariant();
        (variant as StaticVariant).DataAssetGroups = new List<DataAssets>();
      }
      else
      {
        variant.TargetRefs = this.VariantItem.TargetRefs;
        variant.Name = this.VariantItem.Name;
      }
      variant.ApplicationGroups = new List<Applications>();
      variant.SettingGroups = new List<Settings>();
      if (this.Children != null)
      {
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          switch (child)
          {
            case WPApplicationsGroups _:
              using (List<Applications>.Enumerator enumerator = (child as WPApplicationsGroups).Applications.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  Applications current = enumerator.Current;
                  variant.ApplicationGroups.Add(current);
                }
                break;
              }
            case WPSettingsGroups _:
              using (List<Settings>.Enumerator enumerator = (child as WPSettingsGroups).Groups.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  Settings current = enumerator.Current;
                  variant.SettingGroups.Add(current);
                }
                break;
              }
            case WPDataAssetsGroups _:
              StaticVariant staticVariant = variant as StaticVariant;
              using (List<DataAssets>.Enumerator enumerator = (child as WPDataAssetsGroups).Groups.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  DataAssets current = enumerator.Current;
                  staticVariant.DataAssetGroups.Add(current);
                }
                break;
              }
          }
          child.UpdateFontSettings();
          child.NotifyFontChanges();
        }
        this.VariantItem = variant;
      }
      this.IsHideAble = false;
      this.IsIncludedInOutput = true;
      return base.SaveItem();
    }

    public static WPListItemBase AddNewItem(
      WPListItemBase parent,
      string name,
      Dictionary<string, string> targetValues)
    {
      WPVariant newItem = new WPVariant(name, targetValues, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      bool showAll = true;
      newItem.OnShowAllChanged(showAll);
      return (WPListItemBase) newItem;
    }

    public WPSettingsGroups SettingsGroup
    {
      get
      {
        WPSettingsGroups settingsGroup = (WPSettingsGroups) null;
        Queue<WPListItemBase> queue = new Queue<WPListItemBase>();
        this.Children.ToList<WPListItemBase>().ForEach((Action<WPListItemBase>) (c => queue.Enqueue(c)));
        while (queue.Count<WPListItemBase>() > 0)
        {
          WPListItemBase wpListItemBase = queue.Dequeue();
          if (wpListItemBase is WPSettingsGroups)
          {
            settingsGroup = wpListItemBase as WPSettingsGroups;
            break;
          }
          wpListItemBase.Children.ToList<WPListItemBase>().ForEach((Action<WPListItemBase>) (c => queue.Enqueue(c)));
        }
        return settingsGroup;
      }
    }
  }
}
