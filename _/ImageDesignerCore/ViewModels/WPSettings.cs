// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPSettings
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
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPSettings : WPListItemBase
  {
    public Settings Settings;
    public PolicyGroup PolicyGroup;
    public List<WPListItemBase> AllChildren;
    public WPSettingsAssets wpAssets;
    private string _finalPathPart;
    public bool PolicyLoaded;
    private bool _showAll;
    public static readonly DependencyProperty ShowListProperty = DependencyProperty.Register(nameof (ShowList), typeof (bool), typeof (WPSettings), new PropertyMetadata((object) true));
    public static readonly DependencyProperty PathHasUndefinedMacroProperty = DependencyProperty.Register(nameof (PathHasUndefinedMacro), typeof (bool), typeof (WPSettings), new PropertyMetadata((object) false, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty PathForegroundProperty = DependencyProperty.Register(nameof (PathForeground), typeof (string), typeof (WPSettings), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty PathHasWarningsProperty = DependencyProperty.Register(nameof (PathHasWarnings), typeof (bool), typeof (WPSettings), new PropertyMetadata((object) false));
    public static readonly DependencyProperty PathHasErrorsProperty = DependencyProperty.Register(nameof (PathHasErrors), typeof (bool), typeof (WPSettings), new PropertyMetadata((object) false));
    public static readonly DependencyProperty PathWarningsToolTipProperty = DependencyProperty.Register(nameof (PathWarningsToolTip), typeof (string), typeof (WPSettings), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty PathErrorsToolTipProperty = DependencyProperty.Register(nameof (PathErrorsToolTip), typeof (string), typeof (WPSettings), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty PathProperty = DependencyProperty.Register(nameof (Path), typeof (string), typeof (WPSettings), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));

    private WPSettingsGroups _wpSettingsGroups
    {
      get
      {
        WPListItemBase parent = this.Parent;
        while (!(parent is WPSettingsGroups))
          parent = parent.Parent;
        return parent as WPSettingsGroups;
      }
    }

    public WPSettings(Settings settings, WPSettingsGroups groups, bool isIncludedInOutput)
      : base((WPListItemBase) groups)
    {
      this.Settings = settings;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.Path = this.Settings.Path;
      this.SetPathParts(this.Path);
      this.PolicyGroup = WPListItemBase.PolicyStore.SettingGroupByPath(this.Settings.Path);
      if (settings.Assets != null && ((IEnumerable<Asset>) settings.Assets).Count<Asset>() != 0)
        this.wpAssets = new WPSettingsAssets(settings.Assets, this.PolicyGroup, this.IsIncludedInOutput, (WPListItemBase) this);
      else if (this.PolicyGroup != null && this.PolicyGroup.Assets != null && this.PolicyGroup.Assets.Count<PolicyAssetInfo>() != 0)
        this.wpAssets = new WPSettingsAssets(new List<Asset>(), this.PolicyGroup, this.IsIncludedInOutput, (WPListItemBase) this);
      foreach (Setting setting in settings.Items)
        this.AddSettingToPathParts(new WPSetting(setting, isIncludedInOutput, (WPListItemBase) this));
      if (this.Children == null)
        this.Children = new ObservableCollection<WPListItemBase>();
      if (this.wpAssets != null)
        this.Children.Add((WPListItemBase) this.wpAssets);
      this.ShowList = true;
      this.PathHasUndefinedMacro = false;
      this.InitializationComplete();
    }

    public WPSettings(PolicyGroup policyGroup, string macroPath, bool includeChildren = true)
      : base((WPListItemBase) null)
    {
      this.PolicyGroup = policyGroup;
      this.SetToPolicyGroupValues(policyGroup, includeChildren);
      this.Path = macroPath;
      this.SetPathParts(this.Path);
      this.InitializationComplete();
    }

    public WPSettings(PolicyGroup policyGroup, bool includeChildren = true)
      : base((WPListItemBase) null)
    {
      this.PolicyGroup = policyGroup;
      this.Settings = new Settings();
      this.Settings.Path = policyGroup.Path;
      this.SetPathParts(this.PolicyGroup.Path);
      if (policyGroup.HasOEMMacros)
      {
        this.ShowList = false;
        this.PathHasUndefinedMacro = true;
        includeChildren = false;
      }
      this.SetToPolicyGroupValues(policyGroup, includeChildren);
      this.InitializationComplete();
    }

    public List<string> PathParts { get; private set; }

    public string FinalPathPart
    {
      get
      {
        if (this._finalPathPart == null)
          this.SetPathParts(this.Path);
        return this._finalPathPart;
      }
      private set => this._finalPathPart = value;
    }

    private void SetPathParts(string path)
    {
      this.PathParts = WPSettingsPathPart.GetPathParts(path);
      int index = this.PathParts.Count<string>() - 1;
      this.FinalPathPart = this.PathParts[index];
      this.PathParts.RemoveAt(index);
    }

    public void AddSettingToPathParts(WPSetting setting)
    {
      if (this.AllChildren == null)
        this.AllChildren = new List<WPListItemBase>();
      if (setting.PathParts.Count > 0)
      {
        WPSettingsPathPart deepestPathMatch = this.FindDeepestPathMatch(setting);
        if (deepestPathMatch != null)
          deepestPathMatch.AddPathPartChild((WPListItemBase) setting);
        else
          this.AllChildren.Add((WPListItemBase) new WPSettingsPathPart((WPListItemBase) setting, (WPListItemBase) this));
      }
      else
      {
        setting.Parent = (WPListItemBase) this;
        this.AllChildren.Add((WPListItemBase) setting);
      }
    }

    private WPSettingsPathPart FindDeepestPathMatch(WPSetting setting)
    {
      if (this.AllChildren != null && setting.PathParts.Count<string>() != 0)
      {
        foreach (WPListItemBase allChild in this.AllChildren)
        {
          if (allChild is WPSettingsPathPart)
          {
            WPSettingsPathPart settingsPathPart = allChild as WPSettingsPathPart;
            if (settingsPathPart.PathParts[0].Equals(setting.PathParts[0], StringComparison.OrdinalIgnoreCase))
            {
              WPSettingsPathPart deepestPathMatch = settingsPathPart;
              WPSettingsPathPart matchingChild = settingsPathPart.FindMatchingChild(setting.PathParts);
              if (matchingChild != null)
                deepestPathMatch = matchingChild;
              return deepestPathMatch;
            }
          }
        }
      }
      return (WPSettingsPathPart) null;
    }

    public WPSetting FindByName(string name, bool searchIncludedSettingsOnly = false) => (searchIncludedSettingsOnly ? (IEnumerable<WPSetting>) this.IncludedSettings : (IEnumerable<WPSetting>) this.AllSettings).Where<WPSetting>((Func<WPSetting, bool>) (s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))).FirstOrDefault<WPSetting>();

    public IEnumerable<WPSetting> FindByPattern(
      string regexPattern,
      bool searchIncludedSettingsOnly = false)
    {
      return (searchIncludedSettingsOnly ? (IEnumerable<WPSetting>) this.IncludedSettings : (IEnumerable<WPSetting>) this.AllSettings).Where<WPSetting>((Func<WPSetting, bool>) (s => Regex.IsMatch(s.Name, regexPattern, RegexOptions.IgnoreCase)));
    }

    public List<string> GetAvailableSettings()
    {
      List<string> availableSettings = new List<string>();
      if (this.PolicyLoaded)
        availableSettings = this.AllSettings.Where<WPSetting>((Func<WPSetting, bool>) (setting => setting.IsHideAble && !setting.IsDirty)).Select<WPSetting, string>((Func<WPSetting, string>) (setting => setting.Name)).ToList<string>();
      else if (this.HasAssociatePolicy)
      {
        List<string> list = this.AllSettings.Where<WPSetting>((Func<WPSetting, bool>) (setting =>
        {
          if (!setting.IsHideAble)
            return true;
          return setting.IsHideAble && setting.IsDirty;
        })).Select<WPSetting, string>((Func<WPSetting, string>) (setting => setting.Name)).ToList<string>();
        availableSettings = this.PolicyGroup.Settings.Select<PolicySetting, string>((Func<PolicySetting, string>) (setting => setting.Name)).Except<string>((IEnumerable<string>) list).ToList<string>();
      }
      return availableSettings;
    }

    public List<string> SettingNames
    {
      get
      {
        List<string> settingNames = new List<string>();
        if (this.AllSettings != null)
        {
          foreach (WPSetting allSetting in this.AllSettings)
            settingNames.Add(allSetting.Name);
        }
        return settingNames;
      }
    }

    protected override void OnExpandedChange(bool expanded)
    {
      if (!expanded)
        return;
      this.LoadPolicy();
    }

    public void LoadPolicy()
    {
      if (this.PolicyLoaded)
        return;
      this.PolicyLoaded = true;
      if (this.PolicyGroup == null || this.PathHasUndefinedMacro || this.GetParentOfType(typeof (WPVariant)) is WPVariant parentOfType && !parentOfType.IsStaticVariant && this.PolicyGroup.ImageTimeOnly)
        return;
      if (this.AllChildren == null && this.PolicyGroup.Settings.Count<PolicySetting>() > 0)
        this.AllChildren = new List<WPListItemBase>();
      List<string> settingNames = this.SettingNames;
      foreach (PolicySetting setting in (IEnumerable<PolicySetting>) this.PolicyGroup.Settings.OrderBy<PolicySetting, string>((Func<PolicySetting, string>) (setting => setting.Name), (IComparer<string>) Tools.LexicoGraphicComparer))
      {
        if (!settingNames.Contains<string>(setting.Name, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
          this.AddSettingToPathParts(new WPSetting(setting, (WPListItemBase) this));
      }
      this.ShowAllItems();
      this.ValidateItem();
    }

    public bool HasAssociatePolicy => this.PolicyGroup != null;

    public void SetToPolicyGroupValues(PolicyGroup policyGroup, bool includeChildren = true)
    {
      this.Settings = new Settings();
      this.Settings.Path = policyGroup.Path;
      this.Path = policyGroup.Path;
      this.Settings.Items = new List<Setting>();
      this.Settings.DefinedInFile = "";
      this.PolicyGroup = policyGroup;
      if (policyGroup.Atomic)
        this.ToolTip = Tools.GetString("ttCOSSettingsGroupAtomic");
      this.IsIncludedInOutput = false;
      this.IsHideAble = true;
      this.IsNewItem = false;
      if (this.PolicyGroup.Assets != null && this.PolicyGroup.Assets.Count<PolicyAssetInfo>() != 0 && !this.PathHasUndefinedMacro)
      {
        this.wpAssets = new WPSettingsAssets(this.PolicyGroup, (WPListItemBase) this);
        if (this.Children == null)
          this.Children = new ObservableCollection<WPListItemBase>();
        this.Children.Add((WPListItemBase) this.wpAssets);
      }
      else
        this.wpAssets = (WPSettingsAssets) null;
      if (!includeChildren || this.PathHasUndefinedMacro)
        return;
      this.LoadPolicy();
    }

    public void ResetToPolicyGroupValues()
    {
      this._initializing = true;
      this.SetToPolicyGroupValues(this.PolicyGroup);
      foreach (WPSetting includedSetting in this.IncludedSettings)
        includedSetting.ResetToPolicySettingValues();
      this._childListChanged = false;
      this._isDirty = false;
      this._wpSettingsGroups.ShowAllItems();
      this.InitializationComplete();
    }

    public List<WPSetting> IncludedSettings
    {
      get => this.AllChildren == null ? new List<WPSetting>() : this.AllSettings.Where<WPSetting>((Func<WPSetting, bool>) (setting => !setting.IsHideAble || setting.IsDirty)).ToList<WPSetting>();
      set
      {
      }
    }

    public List<WPSetting> AllSettings
    {
      get
      {
        List<WPSetting> allSettings = new List<WPSetting>();
        if (this.AllChildren != null)
        {
          foreach (WPListItemBase allChild in this.AllChildren)
          {
            if (allChild is WPSetting)
              allSettings.Add(allChild as WPSetting);
            else if (allChild is WPSettingsPathPart)
            {
              foreach (WPListItemBase allSetting in (allChild as WPSettingsPathPart).AllSettings)
              {
                if (allSetting is WPSetting)
                  allSettings.Add(allSetting as WPSetting);
              }
            }
          }
        }
        return allSettings;
      }
    }

    public bool FindByNameAndSet(string settingName, string value, bool searchIncludedSettingsOnly = false)
    {
      bool byNameAndSet = false;
      WPSetting wpSetting = settingName != null ? this.FindByName(settingName, searchIncludedSettingsOnly) : throw new ArgumentNullException(nameof (settingName));
      if (wpSetting != null)
      {
        wpSetting.Value = value;
        byNameAndSet = true;
      }
      return byNameAndSet;
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/gear_32xMD.png";

    public override string DisplayText => this.FinalPathPart;

    public void ShowAllItems() => this.ShowAllItems(this._showAll);

    public void ShowAllItems(bool showHiddenItems)
    {
      List<WPListItemBase> list1 = new List<WPListItemBase>();
      this._showAll = showHiddenItems;
      if (this.wpAssets != null)
        list1.Add((WPListItemBase) this.wpAssets);
      if (this.AllChildren == null)
        return;
      foreach (WPListItemBase allChild in this.AllChildren)
      {
        if (allChild is WPSettingsPathPart)
          (allChild as WPSettingsPathPart).ShowAllItems(showHiddenItems);
        if (allChild.IsIncludedInOutput || allChild.IsDirty || allChild.IsNewItem)
          list1.Add(allChild);
      }
      if (showHiddenItems)
      {
        List<WPListItemBase> list2 = new List<WPListItemBase>();
        if (this.wpAssets != null)
          list2.Add((WPListItemBase) this.wpAssets);
        list2.AddRange((IEnumerable<WPListItemBase>) this.AllChildren);
        this.Children = new ObservableCollection<WPListItemBase>(list2);
      }
      else
        this.Children = new ObservableCollection<WPListItemBase>(list1);
      this.NotifyChanges();
    }

    public override WPListItemBase SelectedChild => this.IncludedSettings == null || this.IncludedSettings.Where<WPSetting>((Func<WPSetting, bool>) (child => child.IsSelectedForListBox)).Count<WPSetting>() == 0 ? (WPListItemBase) null : (WPListItemBase) this.IncludedSettings.First<WPSetting>((Func<WPSetting, bool>) (child => child.IsSelectedForListBox));

    protected override bool RemoveChild(WPListItemBase item)
    {
      if (this.IncludedSettings == null)
        return false;
      WPSetting wpSetting = item as WPSetting;
      if (wpSetting.PolicySetting != null && !wpSetting.PolicySetting.HasOEMMacros)
        wpSetting.ResetToPolicySettingValues();
      else if (wpSetting.Parent != this)
      {
        WPSettingsPathPart parent = wpSetting.Parent as WPSettingsPathPart;
        parent.AllChildren.Remove((WPListItemBase) wpSetting);
        parent.Children.Remove((WPListItemBase) wpSetting);
        while (parent.Children.Count<WPListItemBase>() == 0)
        {
          WPListItemBase wpListItemBase = (WPListItemBase) parent;
          if (parent.Parent is WPSettingsPathPart)
          {
            parent = parent.Parent as WPSettingsPathPart;
            parent.AllChildren.Remove(wpListItemBase);
            parent.Children.Remove(wpListItemBase);
          }
          else
          {
            this.AllChildren.Remove(wpListItemBase);
            this.Children.Remove(wpListItemBase);
            break;
          }
        }
      }
      else
      {
        this.AllChildren.Remove((WPListItemBase) wpSetting);
        this.Children.Remove((WPListItemBase) wpSetting);
      }
      this.ChildListChanged = true;
      this.ShowAllItems();
      this.NotifyChanges();
      return true;
    }

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      Settings previewItem1 = new Settings();
      previewItem1.Items = new List<Setting>();
      previewItem1.DefinedInFile = this.Settings.DefinedInFile;
      previewItem1.Path = this.Path;
      if (this.Children != null && (includeAllLevels || !includeAllLevels && level <= 1))
      {
        ++level;
        foreach (WPListItemBase includedSetting in this.IncludedSettings)
        {
          Setting previewItem2 = includedSetting.GetPreviewItem(includeAllLevels, level) as Setting;
          previewItem1.Items.Add(previewItem2);
        }
      }
      if (this.wpAssets != null)
        previewItem1.Assets = this.wpAssets.GetPreviewItem(includeAllLevels, 0) as List<Asset>;
      return (object) previewItem1;
    }

    protected override bool HasErrors => this.PathHasErrors && (this.IsHideAble && (this.IsDirty || this.IsNewItem) || !this.IsHideAble) || base.HasErrors;

    protected override void ValidateItem()
    {
      if (this.Parent == null)
        return;
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        bool includChildren = false;
        WPListItemBase parent = this.Parent;
        while (!(parent is WPVariant))
          parent = parent.Parent;
        Variant previewItem1 = (parent as WPVariant).GetPreviewItem(includChildren, 0) as Variant;
        Settings previewItem2 = this.GetPreviewItem(includChildren, 0) as Settings;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifySettingsGroupList(previewItem2, WPListItemBase.PolicyStore).ToList<CustomizationError>(), (WPListItemBase) this);
        base.ValidateItem();
        WPErrors wpErrors = new WPErrors(CustomContentGenerator.VerifySettingsGroupPath(previewItem2, previewItem1, WPListItemBase.PolicyStore).ToList<CustomizationError>(), (WPListItemBase) this);
        this.PathHasWarnings = wpErrors.HasWarnings;
        this.PathHasErrors = wpErrors.HasErrors;
        this.PathWarningsToolTip = wpErrors.WarningsMessage;
        this.PathErrorsToolTip = wpErrors.ErrorsMessage;
        this._additionalIssues.Add(wpErrors);
      }
      else
        base.ValidateItem();
    }

    protected override void UpdateIsDirty()
    {
      if (this.ChildListChanged || this.wpAssets != null && this.wpAssets.IsDirty)
        this._isDirty = true;
      else
        this._isDirty = false;
    }

    public override void UpdateFontSettings()
    {
      this.PathForeground = !this.HasValidationErrors ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ErrorForegroundColor;
      base.UpdateFontSettings();
    }

    public bool ShowList
    {
      get => (bool) this.GetValue(WPSettings.ShowListProperty);
      set => this.SetValue(WPSettings.ShowListProperty, (object) value);
    }

    public bool PathHasUndefinedMacro
    {
      get => (bool) this.GetValue(WPSettings.PathHasUndefinedMacroProperty);
      set => this.SetValue(WPSettings.PathHasUndefinedMacroProperty, (object) value);
    }

    public string PathForeground
    {
      get => (string) this.GetValue(WPSettings.PathForegroundProperty);
      set => this.SetValue(WPSettings.PathForegroundProperty, (object) value);
    }

    public bool PathHasWarnings
    {
      get => (bool) this.GetValue(WPSettings.PathHasWarningsProperty);
      set => this.SetValue(WPSettings.PathHasWarningsProperty, (object) value);
    }

    public bool PathHasErrors
    {
      get => (bool) this.GetValue(WPSettings.PathHasErrorsProperty);
      set => this.SetValue(WPSettings.PathHasErrorsProperty, (object) value);
    }

    public string PathWarningsToolTip
    {
      get => (string) this.GetValue(WPSettings.PathWarningsToolTipProperty);
      set => this.SetValue(WPSettings.PathWarningsToolTipProperty, (object) value);
    }

    public string PathErrorsToolTip
    {
      get => (string) this.GetValue(WPSettings.PathErrorsToolTipProperty);
      set => this.SetValue(WPSettings.PathErrorsToolTipProperty, (object) value);
    }

    public static WPListItemBase AddNewItem(
      WPListItemBase parent,
      string path,
      string macroPath = null)
    {
      WPSettingsGroups wpSettingsGroups = parent as WPSettingsGroups;
      bool flag = false;
      WPSettings settings;
      if (string.IsNullOrEmpty(macroPath))
      {
        settings = wpSettingsGroups.AllSettingsGroups.Find((Predicate<WPSettings>) (group => group.Path.Equals(path)));
        if (settings == null)
        {
          settings = new WPSettings(WPListItemBase.PolicyStore.SettingGroupByPath(path));
          settings.IsHideAble = true;
        }
        else
          flag = true;
      }
      else
      {
        settings = new WPSettings(WPListItemBase.PolicyStore.SettingGroupByPath(macroPath), macroPath);
        settings.IsHideAble = false;
      }
      settings.IsNewItem = true;
      settings.IsDirty = true;
      if (!flag)
        wpSettingsGroups.AddSettingsToPathParts(settings);
      settings.IsExpanded = true;
      settings.IsSelected = true;
      settings.NotifyChanges();
      wpSettingsGroups.NotifyChanges();
      wpSettingsGroups.ShowAllItems();
      return (WPListItemBase) settings;
    }

    public override void NotifyChanges()
    {
      this.OnPropertyChanged("IncludedSettings");
      this.OnPropertyChanged("Children");
      base.NotifyChanges();
    }

    public override bool Save() => this.SaveItem();

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.Settings.Items.Clear();
        this.Settings.Path = this.Path;
        if (this.IncludedSettings != null)
        {
          foreach (WPSetting includedSetting in this.IncludedSettings)
          {
            includedSetting.Save();
            this.Settings.Items.Add(includedSetting.Setting);
          }
        }
        this.IsHideAble = false;
        if (this.wpAssets != null)
        {
          this.wpAssets.Save();
          this.Settings.Assets = this.wpAssets.GetAssets().Select<WPSettingAsset, Asset>((Func<WPSettingAsset, Asset>) (x => x.Asset)).ToList<Asset>();
        }
      }
      return base.SaveItem();
    }

    public string Path
    {
      get => (string) this.GetValue(WPSettings.PathProperty);
      set => this.SetValue(WPSettings.PathProperty, (object) value);
    }
  }
}
