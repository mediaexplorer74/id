// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPSettingsGroups
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.MCSF.Offline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPSettingsGroups : WPListItemBase
  {
    public List<Settings> Groups;
    public List<WPListItemBase> AllChildren;
    private bool _showAll;
    public bool PolicyLoaded;

    public WPSettingsGroups(List<Settings> groups, bool isIncludedInOutput, WPListItemBase parent)
      : base(parent)
    {
      this.Groups = groups;
      this.IsIncludedInOutput = isIncludedInOutput;
      foreach (Settings group in groups)
        this.AddSettingsToPathParts(new WPSettings(group, this, isIncludedInOutput));
      this.ShowAllItems();
      this.InitializationComplete();
    }

    private WPSettingsPathPart FindDeepestPathMatch(WPSettings settings)
    {
      if (this.AllChildren != null && settings.PathParts.Count<string>() != 0)
      {
        foreach (WPListItemBase allChild in this.AllChildren)
        {
          if (allChild is WPSettingsPathPart)
          {
            WPSettingsPathPart matchingChild = (allChild as WPSettingsPathPart).FindMatchingChild(settings.PathParts);
            if (matchingChild != null)
              return matchingChild;
          }
        }
      }
      return (WPSettingsPathPart) null;
    }

    public void AddSettingsToPathParts(WPSettings settings)
    {
      if (this.AllChildren == null)
        this.AllChildren = new List<WPListItemBase>();
      if (settings.PathParts.Count > 0)
      {
        WPSettingsPathPart deepestPathMatch = this.FindDeepestPathMatch(settings);
        if (deepestPathMatch != null)
          deepestPathMatch.AddPathPartChild((WPListItemBase) settings);
        else
          this.AllChildren.Add((WPListItemBase) new WPSettingsPathPart((WPListItemBase) settings, (WPListItemBase) this));
      }
      else
      {
        settings.Parent = (WPListItemBase) this;
        this.AllChildren.Add((WPListItemBase) settings);
      }
    }

    public WPListItemBase AddPolicyGroup(PolicyGroup policyGroup)
    {
      WPSettings settings = new WPSettings(policyGroup);
      this.AddSettingsToPathParts(settings);
      this.InitializationComplete();
      return (WPListItemBase) settings;
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/folder_closed_32xMD.png";

    public override string DisplayText => Tools.GetString("txtCOSSettings");

    public List<string> GroupPaths
    {
      get
      {
        List<string> groupPaths = new List<string>();
        foreach (WPSettings allSettingsGroup in this.AllSettingsGroups)
          groupPaths.Add(allSettingsGroup.Path);
        return groupPaths;
      }
    }

    public override string ToolTip
    {
      get => Tools.GetString("ttCOSSettingsGroup");
      set => base.ToolTip = value;
    }

    public List<WPSettings> IncludedSettingsGroups
    {
      get => this.AllSettingsGroups.Where<WPSettings>((Func<WPSettings, bool>) (setting => !setting.IsHideAble || setting.IsDirty)).ToList<WPSettings>();
      set
      {
      }
    }

    public List<WPSettings> AllSettingsGroups
    {
      get
      {
        List<WPSettings> allSettingsGroups = new List<WPSettings>();
        if (this.AllChildren != null)
        {
          foreach (WPListItemBase allChild in this.AllChildren)
          {
            if (allChild is WPSettings)
            {
              allSettingsGroups.Add(allChild as WPSettings);
            }
            else
            {
              WPSettingsPathPart settingsPathPart = allChild as WPSettingsPathPart;
              allSettingsGroups.AddRange((IEnumerable<WPSettings>) settingsPathPart.AllSettingsGroups);
            }
          }
        }
        return allSettingsGroups;
      }
    }

    public bool HasAssociatePolicy => WPListItemBase.PolicyStore != null;

    public IEnumerable<WPSettings> GetAvailableSettings(string regexPattern = null)
    {
      List<WPSettings> source = new List<WPSettings>();
      if (this.AllSettingsGroups != null)
      {
        if (regexPattern != null)
          source.AddRange(this.AllSettingsGroups.Where<WPSettings>((Func<WPSettings, bool>) (a => Regex.IsMatch(a.Path, regexPattern, RegexOptions.IgnoreCase))));
        else
          source = this.AllSettingsGroups;
      }
      return source.AsEnumerable<WPSettings>();
    }

    public IEnumerable<WPSettings> GetIncludedSettings(string regexPattern = null)
    {
      List<WPSettings> source = new List<WPSettings>();
      if (this.IncludedSettingsGroups != null)
      {
        if (regexPattern != null)
          source.AddRange(this.IncludedSettingsGroups.Where<WPSettings>((Func<WPSettings, bool>) (a => Regex.IsMatch(a.Path, regexPattern, RegexOptions.IgnoreCase))));
        else
          source = this.IncludedSettingsGroups;
      }
      return source.AsEnumerable<WPSettings>();
    }

    public void LoadPolicy()
    {
      if (this.PolicyLoaded)
        return;
      this.PolicyLoaded = true;
      List<string> groupPaths = this.GroupPaths;
      foreach (PolicyGroup policyGroup in (IEnumerable<PolicyGroup>) WPListItemBase.PolicyStore.SettingGroups.OrderBy<PolicyGroup, string>((Func<PolicyGroup, string>) (group => group.Path), (IComparer<string>) Tools.LexicoGraphicComparer))
      {
        if (!groupPaths.Contains<string>(policyGroup.Path, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
          this.AddPolicyGroup(policyGroup);
      }
    }

    public void ShowAllItems() => this.ShowAllItems(this._showAll);

    public void ShowAllItems(bool showHiddenItems)
    {
      if (this.AllChildren == null)
        return;
      List<WPListItemBase> list = new List<WPListItemBase>();
      this._showAll = showHiddenItems;
      foreach (WPListItemBase allChild in this.AllChildren)
      {
        if (allChild is WPSettings)
          (allChild as WPSettings).ShowAllItems(showHiddenItems);
        else
          (allChild as WPSettingsPathPart).ShowAllItems(showHiddenItems);
        if (allChild.IsIncludedInOutput || allChild.IsDirty || allChild.IsNewItem)
          list.Add(allChild);
      }
      if (showHiddenItems)
        this.Children = new ObservableCollection<WPListItemBase>(this.AllChildren);
      else
        this.Children = new ObservableCollection<WPListItemBase>(list);
      this.NotifyChanges();
    }

    public List<string> GetAvailableSettingsPath()
    {
      List<string> availableSettingsPath = new List<string>();
      if (this.PolicyLoaded)
        availableSettingsPath = this.AllSettingsGroups.Where<WPSettings>((Func<WPSettings, bool>) (group => group.IsHideAble && !group.IsDirty)).Select<WPSettings, string>((Func<WPSettings, string>) (group => group.Path)).ToList<string>();
      else if (this.HasAssociatePolicy)
      {
        List<string> list = this.AllSettingsGroups.Where<WPSettings>((Func<WPSettings, bool>) (group =>
        {
          if (!group.IsHideAble)
            return true;
          return group.IsHideAble && group.IsDirty;
        })).Select<WPSettings, string>((Func<WPSettings, string>) (group => group.Path)).ToList<string>();
        availableSettingsPath = WPListItemBase.PolicyStore.SettingGroups.Select<PolicyGroup, string>((Func<PolicyGroup, string>) (group => group.Path)).Except<string>((IEnumerable<string>) list).ToList<string>();
      }
      return availableSettingsPath;
    }

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      List<Settings> previewItem1 = new List<Settings>();
      if (this.Children != null && (includeAllLevels || !includeAllLevels && level <= 1))
      {
        ++level;
        foreach (WPListItemBase includedSettingsGroup in this.IncludedSettingsGroups)
        {
          Settings previewItem2 = includedSettingsGroup.GetPreviewItem(includeAllLevels, level) as Settings;
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
        WPVariant parent = this.Parent as WPVariant;
        Variant variantItem = parent.VariantItem;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifySettingGroups((IEnumerable<Settings>) (this.GetPreviewItem(includChildren, 0) as List<Settings>), parent.VariantItem, (ImageCustomizations) null, WPListItemBase.PolicyStore, flag).ToList<CustomizationError>(), (WPListItemBase) this);
      }
      base.ValidateItem();
    }

    public override void NotifyChanges()
    {
      this.OnPropertyChanged("IncludedSettingsGroups");
      this.OnPropertyChanged("Children");
      base.NotifyChanges();
    }

    public static WPListItemBase AddNewItem(WPListItemBase parent)
    {
      WPSettingsGroups newItem = new WPSettingsGroups(new List<Settings>(), false, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      return (WPListItemBase) newItem;
    }

    public override WPListItemBase SelectedChild => this.IncludedSettingsGroups == null || this.IncludedSettingsGroups.Where<WPSettings>((Func<WPSettings, bool>) (child => child.IsSelectedForListBox)).Count<WPSettings>() == 0 ? (WPListItemBase) null : (WPListItemBase) this.IncludedSettingsGroups.First<WPSettings>((Func<WPSettings, bool>) (child => child.IsSelectedForListBox));

    protected override bool RemoveChild(WPListItemBase item)
    {
      if (this.IncludedSettingsGroups == null)
        return false;
      WPSettings wpSettings = item as WPSettings;
      if (wpSettings.PolicyGroup != null && !wpSettings.PolicyGroup.HasOEMMacros)
      {
        wpSettings.ResetToPolicyGroupValues();
      }
      else
      {
        WPListItemBase wpListItemBase = item;
        while (wpListItemBase.Parent.Children.Count<WPListItemBase>() == 1 && !(wpListItemBase.Parent is WPSettingsGroups))
          wpListItemBase = wpListItemBase.Parent;
        if (wpListItemBase.Parent is WPSettingsGroups)
        {
          WPSettingsGroups parent = wpListItemBase.Parent as WPSettingsGroups;
          parent.AllChildren.Remove(wpListItemBase);
          parent.Children.Remove(wpListItemBase);
          parent.OnPropertyChanged("Children");
          parent.NotifyChanges();
        }
        else
        {
          WPSettingsPathPart parent = wpListItemBase.Parent as WPSettingsPathPart;
          parent.AllChildren.Remove(wpListItemBase);
          parent.Children.Remove(wpListItemBase);
          parent.NotifyChanges();
        }
      }
      this.ShowAllItems();
      this.NotifyChanges();
      this.ChildListChanged = true;
      return true;
    }

    public override bool Save() => this.SaveItem();

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.Groups.Clear();
        if (this.IncludedSettingsGroups != null)
        {
          foreach (WPSettings includedSettingsGroup in this.IncludedSettingsGroups)
          {
            includedSettingsGroup.Save();
            if (includedSettingsGroup.Settings != null && includedSettingsGroup.Settings.Items != null && ((IEnumerable<Setting>) includedSettingsGroup.Settings.Items).Count<Setting>() != 0 || includedSettingsGroup.wpAssets != null && includedSettingsGroup.wpAssets.Assets != null && ((IEnumerable<Asset>) includedSettingsGroup.wpAssets.Assets).Count<Asset>() != 0)
              this.Groups.Add(includedSettingsGroup.Settings);
          }
        }
        this.IsHideAble = false;
      }
      return base.SaveItem();
    }
  }
}
