// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPSetting
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.MCSF.Offline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPSetting : WPListItemBase
  {
    public Setting Setting;
    public PolicyGroup PolicyGroup;
    public PolicySetting PolicySetting;
    private List<string> _pathParts;
    private string _finalPathPart;
    public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register(nameof (DefaultValue), typeof (string), typeof (WPSetting), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof (Description), typeof (string), typeof (WPSetting), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty NameForegroundProperty = DependencyProperty.Register(nameof (NameForeground), typeof (string), typeof (WPSetting), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty NameHasWarningsProperty = DependencyProperty.Register(nameof (NameHasWarnings), typeof (bool), typeof (WPSetting), new PropertyMetadata((object) false));
    public static readonly DependencyProperty NameHasErrorsProperty = DependencyProperty.Register(nameof (NameHasErrors), typeof (bool), typeof (WPSetting), new PropertyMetadata((object) false));
    public static readonly DependencyProperty NameWarningsToolTipProperty = DependencyProperty.Register(nameof (NameWarningsToolTip), typeof (string), typeof (WPSetting), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty NameErrorsToolTipProperty = DependencyProperty.Register(nameof (NameErrorsToolTip), typeof (string), typeof (WPSetting), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ValueHasWarningsProperty = DependencyProperty.Register(nameof (ValueHasWarnings), typeof (bool), typeof (WPSetting), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ValueHasErrorsProperty = DependencyProperty.Register(nameof (ValueHasErrors), typeof (bool), typeof (WPSetting), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ValueWarningsToolTipProperty = DependencyProperty.Register(nameof (ValueWarningsToolTip), typeof (string), typeof (WPSetting), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ValueErrorsToolTipProperty = DependencyProperty.Register(nameof (ValueErrorsToolTip), typeof (string), typeof (WPSetting), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ValueForegroundProperty = DependencyProperty.Register(nameof (ValueForeground), typeof (string), typeof (WPSetting), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty ShowValueProperty = DependencyProperty.Register(nameof (ShowValue), typeof (bool), typeof (WPSetting), new PropertyMetadata((object) true));
    public static readonly DependencyProperty NameHasUndefinedMacroProperty = DependencyProperty.Register(nameof (NameHasUndefinedMacro), typeof (bool), typeof (WPSetting), new PropertyMetadata((object) false, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof (Name), typeof (string), typeof (WPSetting), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof (Value), typeof (string), typeof (WPSetting), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty MinNumberProperty = DependencyProperty.Register(nameof (MinNumber), typeof (int), typeof (WPSetting), new PropertyMetadata((object) int.MinValue));
    public static readonly DependencyProperty MaxNumberProperty = DependencyProperty.Register(nameof (MaxNumber), typeof (int), typeof (WPSetting), new PropertyMetadata((object) int.MaxValue));
    public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof (Options), typeof (ObservableCollection<PolicyEnum>), typeof (WPSetting), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty ShowTextEditProperty = DependencyProperty.Register(nameof (ShowTextEdit), typeof (bool), typeof (WPSetting), new PropertyMetadata((object) true));
    public static readonly DependencyProperty ShowOptionListProperty = DependencyProperty.Register(nameof (ShowOptionList), typeof (bool), typeof (WPSetting), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ShowNumbersProperty = DependencyProperty.Register(nameof (ShowNumbers), typeof (bool), typeof (WPSetting), new PropertyMetadata((object) false));

    public WPSetting(Setting setting, bool isIncludedInOutput, WPListItemBase parent)
      : base(parent)
    {
      this.Setting = setting;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.Value = this.Setting.Value;
      this.Name = this.Setting.Name;
      this.PolicyGroup = (parent as WPSettings).PolicyGroup;
      if (this.PolicyGroup != null)
      {
        this.PolicySetting = this.PolicyGroup.SettingByName(setting.Name);
        if (this.PolicySetting != null)
        {
          this.DefaultValue = this.PolicySetting.DefaultValue;
          this.Description = this.PolicySetting.Description;
          this.Sample = this.PolicySetting.SampleValue;
        }
      }
      this.SetValueControls();
      this.Value = this.Setting.Value;
      this.ShowValue = true;
      this.NameHasUndefinedMacro = false;
      this.InitializationComplete();
    }

    public WPSetting(PolicySetting setting, WPListItemBase parent, string macroReplacedName = null)
      : base(parent)
    {
      this.PolicyGroup = (parent as WPSettings).PolicyGroup;
      this.PolicySetting = setting;
      this.SetToPolicySettingValues(setting, macroReplacedName);
      this.InitializationComplete();
    }

    public void SetToPolicySettingValues(PolicySetting policySetting, string macroReplacedName = null)
    {
      this.Setting = new Setting();
      this.IsIncludedInOutput = false;
      this.Setting.Name = policySetting.Name;
      this.Name = macroReplacedName ?? policySetting.Name;
      this.Setting.Value = policySetting.DefaultValue;
      this.Value = policySetting.DefaultValue;
      this.DefaultValue = policySetting.DefaultValue;
      this.Description = policySetting.Description;
      this.IsNewItem = false;
      this._isDirty = false;
      this.IsHideAble = true;
      if (policySetting.AssetInfo == null)
        this.Children = (ObservableCollection<WPListItemBase>) null;
      if (string.IsNullOrEmpty(macroReplacedName) && policySetting.HasOEMMacros)
      {
        this.ShowValue = false;
        this.NameHasUndefinedMacro = true;
      }
      else
      {
        this.ShowValue = true;
        this.NameHasUndefinedMacro = false;
      }
      this.SetValueControls();
      this.ValidateItem();
    }

    public void ResetToPolicySettingValues()
    {
      this._initializing = true;
      this.SetToPolicySettingValues(this.PolicySetting);
      this.InitializationComplete();
    }

    public WPSettingsAssets WPAssets
    {
      get
      {
        WPSettingsAssets wpAssets = (WPSettingsAssets) null;
        if (this.Parent is WPSettings parent)
          wpAssets = parent.wpAssets;
        return wpAssets;
      }
    }

    public bool RefreshOptions()
    {
      if (this.PolicySetting == null || this.PolicySetting.AssetInfo == null)
        return false;
      string str = this.Value;
      List<PolicyEnum> list = new List<PolicyEnum>();
      list.AddRange((IEnumerable<PolicyEnum>) this.PolicySetting.AssetInfo.Presets);
      if (this.WPAssets != null)
      {
        foreach (WPSettingAsset asset in this.WPAssets.GetAssets(this.PolicySetting.AssetInfo.Name))
          list.Add(new PolicyEnum(asset.DisplayText, string.IsNullOrEmpty(asset.TargetFileName) ? Path.GetFileName(asset.Source) : asset.TargetFileName));
      }
      this.Options = new ObservableCollection<PolicyEnum>(list);
      this.Value = str;
      this.ValidateItem();
      return true;
    }

    private void SetValueControls()
    {
      if (this.PolicySetting == null)
      {
        this.ShowNumbers = false;
        this.ShowOptionList = false;
        this.ShowTextEdit = true;
      }
      else
      {
        this.ShowNumbers = false;
        this.ShowOptionList = false;
        this.ShowTextEdit = false;
        if (this.RefreshOptions())
        {
          this.ShowOptionList = true;
        }
        else
        {
          switch ((int) this.PolicySetting.SettingType)
          {
            case 0:
            case 4:
              this.MinNumber = this.PolicySetting.Min;
              this.MaxNumber = this.PolicySetting.Max;
              this.ShowTextEdit = true;
              break;
            case 1:
              this.MinNumber = this.PolicySetting.Min;
              this.MaxNumber = this.PolicySetting.Max;
              this.ShowNumbers = true;
              break;
            case 2:
            case 3:
              this.Options = new ObservableCollection<PolicyEnum>(this.PolicySetting.Options.ToList<PolicyEnum>());
              this.ShowOptionList = true;
              break;
            default:
              this.ShowTextEdit = true;
              break;
          }
        }
      }
    }

    public List<string> PathParts
    {
      get
      {
        if (this._pathParts == null)
          this.SetPathParts(this.Name);
        return this._pathParts;
      }
      private set => this._pathParts = value;
    }

    public string FinalPathPart
    {
      get
      {
        if (this._finalPathPart == null)
          this.SetPathParts(this.Name);
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

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/properties_16xMD.png";

    public override string DisplayText => this.FinalPathPart;

    public string NameToolTip
    {
      get => string.IsNullOrEmpty(this.Description) ? (string) null : this.Description;
      set
      {
      }
    }

    public string ValueLabel
    {
      get
      {
        string fieldName = Tools.GetString("tbCOSSettingValue");
        if (this.PolicySetting != null)
        {
          if (!string.IsNullOrEmpty(this.PolicySetting.FieldName))
          {
            fieldName = this.PolicySetting.FieldName;
          }
          else
          {
            if (this.PolicySetting.AssetInfo != null)
              return fieldName;
            switch ((int) this.PolicySetting.SettingType)
            {
              case 0:
                fieldName = Tools.GetString("tbCOSSettingValueText");
                break;
              case 1:
                fieldName = Tools.GetString("tbCOSSettingValueNumber");
                break;
              case 4:
                fieldName = Tools.GetString("tbCOSSettingValueBinary");
                break;
            }
          }
        }
        return fieldName;
      }
    }

    public string ValueToolTip
    {
      get
      {
        string valueToolTip = "";
        string str = Tools.GetString("ttCOSSettingValueDefaultNone");
        if (this.PolicySetting == null)
          return (string) null;
        switch ((int) this.PolicySetting.SettingType)
        {
          case 0:
            valueToolTip = string.Format(Tools.GetString("ttCOSSettingValueText"), string.IsNullOrEmpty(this.PolicySetting.DefaultValue) ? (object) str : (object) this.PolicySetting.DefaultValue);
            break;
          case 1:
            valueToolTip = string.Format(Tools.GetString("ttCOSSettingValueNumber"), string.IsNullOrEmpty(this.PolicySetting.DefaultValue) ? (object) str : (object) this.PolicySetting.DefaultValue, (object) this.PolicySetting.Min, (object) this.PolicySetting.Max);
            break;
          case 2:
          case 3:
            string newValue = "";
            foreach (PolicyEnum option in this.PolicySetting.Options)
            {
              string friendlyName = option.Value;
              if (!string.IsNullOrEmpty(option.FriendlyName))
                friendlyName = option.FriendlyName;
              newValue = !friendlyName.Equals(this.PolicySetting.DefaultValue) ? newValue + string.Format("\t{0}\n", (object) friendlyName) : newValue + "\t" + string.Format(Tools.GetString("ttCOSSettingValueEnumerationDefaultItem"), (object) friendlyName) + Environment.NewLine;
            }
            valueToolTip = Tools.GetString("ttCOSSettingValueEnumeration").Replace("{0}", newValue);
            break;
          case 4:
            valueToolTip = string.Format(Tools.GetString("ttCOSSettingValueBinary"), string.IsNullOrEmpty(this.PolicySetting.DefaultValue) ? (object) str : (object) this.PolicySetting.DefaultValue);
            break;
        }
        return valueToolTip;
      }
    }

    public string AssetsToolTip
    {
      get
      {
        if (this.PolicySetting == null || this.PolicySetting.AssetInfo == null || this.PolicySetting.AssetInfo.FileTypes.Count<string>() == 0)
          return (string) null;
        string str = "";
        foreach (string fileType in this.PolicySetting.AssetInfo.FileTypes)
          str += string.Format("*{0}", (object) fileType);
        return string.Format(Tools.GetString("ttCOSSettingsAssetFileTypes"), (object) str);
      }
    }

    public string Sample { get; private set; }

    public bool ShowSample => !string.IsNullOrEmpty(this.Sample);

    public override object GetPreviewItem(bool includeAllLevels, int level = 0) => (object) new Setting()
    {
      DefinedInFile = this.Setting.DefinedInFile,
      Name = this.Name,
      Value = this.Value
    };

    protected override bool HasErrors => (this.NameHasErrors || this.ValueHasErrors) && (this.IsHideAble && (this.IsDirty || this.IsNewItem) || !this.IsHideAble) || base.HasErrors;

    private WPSettings _wpSettings
    {
      get
      {
        WPListItemBase parent = this.Parent;
        while (!(parent is WPSettings))
          parent = parent.Parent;
        return parent as WPSettings;
      }
    }

    protected override void ValidateItem()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        bool includChildren = false;
        Settings previewItem1 = this._wpSettings.GetPreviewItem(includChildren, 0) as Settings;
        Setting previewItem2 = this.GetPreviewItem(includChildren, 0) as Setting;
        WPErrors wpErrors1 = new WPErrors(CustomContentGenerator.VerifySettingName(previewItem2, previewItem1, WPListItemBase.PolicyStore, this.PolicyGroup).ToList<CustomizationError>(), (WPListItemBase) this);
        base.ValidateItem();
        WPErrors wpErrors2 = new WPErrors(CustomContentGenerator.VerifySettingValue(previewItem2, previewItem1, WPListItemBase.PolicyStore, this.PolicySetting).ToList<CustomizationError>(), (WPListItemBase) this);
        this.ValueHasWarnings = wpErrors2.HasWarnings;
        this.ValueHasErrors = wpErrors2.HasErrors;
        this.ValueWarningsToolTip = wpErrors2.WarningsMessage;
        this.ValueErrorsToolTip = wpErrors2.ErrorsMessage;
        this._additionalIssues.Add(wpErrors2);
      }
      else
        base.ValidateItem();
    }

    private bool _isValueDirty => this.IsNewItem || string.IsNullOrEmpty(this.Value) != string.IsNullOrEmpty(this.Setting.Value) || this.Setting.Value != null && !this.Value.Equals(this.Setting.Value);

    protected override void UpdateIsDirty()
    {
      if (this._isValueDirty || this.ChildListChanged)
        this._isDirty = true;
      else
        this._isDirty = false;
    }

    public override void UpdateFontSettings()
    {
      this.ValueForeground = !this.HasValidationErrors ? (!this._isValueDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      base.UpdateFontSettings();
    }

    public string DefaultValue
    {
      get => (string) this.GetValue(WPSetting.DefaultValueProperty);
      set => this.SetValue(WPSetting.DefaultValueProperty, (object) value);
    }

    public string Description
    {
      get => (string) this.GetValue(WPSetting.DescriptionProperty);
      set => this.SetValue(WPSetting.DescriptionProperty, (object) value);
    }

    public string NameForeground
    {
      get => (string) this.GetValue(WPSetting.NameForegroundProperty);
      set => this.SetValue(WPSetting.NameForegroundProperty, (object) value);
    }

    public bool NameHasWarnings
    {
      get => (bool) this.GetValue(WPSetting.NameHasWarningsProperty);
      set => this.SetValue(WPSetting.NameHasWarningsProperty, (object) value);
    }

    public bool NameHasErrors
    {
      get => (bool) this.GetValue(WPSetting.NameHasErrorsProperty);
      set => this.SetValue(WPSetting.NameHasErrorsProperty, (object) value);
    }

    public string NameWarningsToolTip
    {
      get => (string) this.GetValue(WPSetting.NameWarningsToolTipProperty);
      set => this.SetValue(WPSetting.NameWarningsToolTipProperty, (object) value);
    }

    public string NameErrorsToolTip
    {
      get => (string) this.GetValue(WPSetting.NameErrorsToolTipProperty);
      set => this.SetValue(WPSetting.NameErrorsToolTipProperty, (object) value);
    }

    public bool ValueHasWarnings
    {
      get => (bool) this.GetValue(WPSetting.ValueHasWarningsProperty);
      set => this.SetValue(WPSetting.ValueHasWarningsProperty, (object) value);
    }

    public bool ValueHasErrors
    {
      get => (bool) this.GetValue(WPSetting.ValueHasErrorsProperty);
      set => this.SetValue(WPSetting.ValueHasErrorsProperty, (object) value);
    }

    public string ValueWarningsToolTip
    {
      get => (string) this.GetValue(WPSetting.ValueWarningsToolTipProperty);
      set => this.SetValue(WPSetting.ValueWarningsToolTipProperty, (object) value);
    }

    public string ValueErrorsToolTip
    {
      get => (string) this.GetValue(WPSetting.ValueErrorsToolTipProperty);
      set => this.SetValue(WPSetting.ValueErrorsToolTipProperty, (object) value);
    }

    public string ValueForeground
    {
      get => (string) this.GetValue(WPSetting.ValueForegroundProperty);
      set => this.SetValue(WPSetting.ValueForegroundProperty, (object) value);
    }

    public static WPListItemBase AddNewItem(
      WPListItemBase parent,
      string name,
      string macroReplacedName = null)
    {
      WPSettings wpSettings = parent as WPSettings;
      wpSettings.LoadPolicy();
      WPSetting setting1 = wpSettings.AllSettings.First<WPSetting>((Func<WPSetting, bool>) (setting => setting.Name.Equals(name)));
      if (!string.IsNullOrEmpty(macroReplacedName))
      {
        setting1 = new WPSetting(setting1.PolicySetting, parent, macroReplacedName);
        wpSettings.AddSettingToPathParts(setting1);
      }
      setting1.IsNewItem = true;
      setting1.IsDirty = true;
      setting1.IsExpanded = true;
      setting1.IsSelected = true;
      setting1.NotifyChanges();
      wpSettings.NotifyChanges();
      wpSettings.ShowAllItems();
      return (WPListItemBase) setting1;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.Setting.Value = this.Value;
        this.Setting.Name = this.Name;
        this.IsHideAble = false;
        this.IsIncludedInOutput = true;
      }
      return base.SaveItem();
    }

    public bool ShowValue
    {
      get => (bool) this.GetValue(WPSetting.ShowValueProperty);
      set => this.SetValue(WPSetting.ShowValueProperty, (object) value);
    }

    public bool NameHasUndefinedMacro
    {
      get => (bool) this.GetValue(WPSetting.NameHasUndefinedMacroProperty);
      set => this.SetValue(WPSetting.NameHasUndefinedMacroProperty, (object) value);
    }

    public string Name
    {
      get => (string) this.GetValue(WPSetting.NameProperty);
      set
      {
        this.SetPathParts(value);
        this.SetValue(WPSetting.NameProperty, (object) value);
      }
    }

    public string Value
    {
      get => (string) this.GetValue(WPSetting.ValueProperty);
      set => this.SetValue(WPSetting.ValueProperty, (object) value);
    }

    public int MinNumber
    {
      get => (int) this.GetValue(WPSetting.MinNumberProperty);
      set => this.SetValue(WPSetting.MinNumberProperty, (object) value);
    }

    public int MaxNumber
    {
      get => (int) this.GetValue(WPSetting.MaxNumberProperty);
      set => this.SetValue(WPSetting.MaxNumberProperty, (object) value);
    }

    public ObservableCollection<PolicyEnum> Options
    {
      get => (ObservableCollection<PolicyEnum>) this.GetValue(WPSetting.OptionsProperty);
      set => this.SetValue(WPSetting.OptionsProperty, (object) value);
    }

    public bool ShowTextEdit
    {
      get => (bool) this.GetValue(WPSetting.ShowTextEditProperty);
      set => this.SetValue(WPSetting.ShowTextEditProperty, (object) value);
    }

    public bool ShowOptionList
    {
      get => (bool) this.GetValue(WPSetting.ShowOptionListProperty);
      set => this.SetValue(WPSetting.ShowOptionListProperty, (object) value);
    }

    public bool ShowNumbers
    {
      get => (bool) this.GetValue(WPSetting.ShowNumbersProperty);
      set => this.SetValue(WPSetting.ShowNumbersProperty, (object) value);
    }
  }
}
