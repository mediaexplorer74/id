// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPSettingAsset
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
using System.Text;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPSettingAsset : WPListItemBase
  {
    public Asset Asset;
    public PolicyAssetInfo AssetInfo;
    public static readonly DependencyProperty SelectedOwnerTypeProperty = DependencyProperty.Register(nameof (SelectedOwnerType), typeof (CustomizationAssetOwner), typeof (WPSettingAsset), new PropertyMetadata((object) (CustomizationAssetOwner) 0));
    public static readonly DependencyProperty SourceForegroundProperty = DependencyProperty.Register(nameof (SourceForeground), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty DisplayNameForegroundProperty = DependencyProperty.Register(nameof (DisplayNameForeground), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty TargetForegroundProperty = DependencyProperty.Register(nameof (TargetForeground), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty OwnerForegroundProperty = DependencyProperty.Register(nameof (OwnerForeground), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty OwnersProperty = DependencyProperty.Register(nameof (Owners), typeof (ObservableCollection<WPCustomizationAssetOwner>), typeof (WPSettingAsset), new PropertyMetadata((object) null, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof (Source), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(nameof (DisplayName), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty DisplayNameHasWarningsProperty = DependencyProperty.Register(nameof (DisplayNameHasWarnings), typeof (bool), typeof (WPSettingAsset), new PropertyMetadata((object) false));
    public static readonly DependencyProperty DisplayNameHasErrorsProperty = DependencyProperty.Register(nameof (DisplayNameHasErrors), typeof (bool), typeof (WPSettingAsset), new PropertyMetadata((object) false));
    public static readonly DependencyProperty DisplayNameWarningsToolTipProperty = DependencyProperty.Register(nameof (DisplayNameWarningsToolTip), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty DisplayNameErrorsToolTipProperty = DependencyProperty.Register(nameof (DisplayNameErrorsToolTip), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty TargetFileNameProperty = DependencyProperty.Register(nameof (TargetFileName), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty TargetFileNameHasWarningsProperty = DependencyProperty.Register(nameof (TargetFileNameHasWarnings), typeof (bool), typeof (WPSettingAsset), new PropertyMetadata((object) false));
    public static readonly DependencyProperty TargetFileNameHasErrorsProperty = DependencyProperty.Register(nameof (TargetFileNameHasErrors), typeof (bool), typeof (WPSettingAsset), new PropertyMetadata((object) false));
    public static readonly DependencyProperty TargetFileNameWarningsToolTipProperty = DependencyProperty.Register(nameof (TargetFileNameWarningsToolTip), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty TargetFileNameErrorsToolTipProperty = DependencyProperty.Register(nameof (TargetFileNameErrorsToolTip), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty OwnerHasWarningsProperty = DependencyProperty.Register(nameof (OwnerHasWarnings), typeof (bool), typeof (WPSettingAsset), new PropertyMetadata((object) false));
    public static readonly DependencyProperty OwnerHasErrorsProperty = DependencyProperty.Register(nameof (OwnerHasErrors), typeof (bool), typeof (WPSettingAsset), new PropertyMetadata((object) false));
    public static readonly DependencyProperty OwnerWarningsToolTipProperty = DependencyProperty.Register(nameof (OwnerWarningsToolTip), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty OwnerErrorsToolTipProperty = DependencyProperty.Register(nameof (OwnerErrorsToolTip), typeof (string), typeof (WPSettingAsset), new PropertyMetadata((object) ""));

    public WPSettingAsset(
      Asset asset,
      PolicyAssetInfo assetInfo,
      bool isIncludedInOutput,
      WPListItemBase parent)
      : base(parent)
    {
      this.Asset = asset;
      this.AssetInfo = assetInfo;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.Owners = new WPCustomizationAssetOwner(this.Asset.Type).GetList(this.Asset.Type);
      this.SelectedOwnerType = this.Asset.Type;
      this.Source = this.Asset.Source;
      this.TargetFileName = this.Asset.TargetFileName;
      this.DisplayName = this.Asset.DisplayName;
      this.InitializationComplete();
    }

    public CustomizationAssetOwner SelectedOwnerType
    {
      get => (CustomizationAssetOwner) this.GetValue(WPSettingAsset.SelectedOwnerTypeProperty);
      set => this.SetValue(WPSettingAsset.SelectedOwnerTypeProperty, (object) value);
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/resource_32xMD.png";

    public override string DisplayText
    {
      get
      {
        string displayText = "";
        if (!string.IsNullOrEmpty(this.DisplayName))
          displayText = this.DisplayName;
        else if (!string.IsNullOrEmpty(this.TargetFileName))
          displayText = this.TargetFileName;
        else if (!string.IsNullOrEmpty(this.Source))
          displayText = Path.GetFileName(this.Source);
        if (this.AssetInfo != null && this.AssetInfo.HasOEMMacros)
          displayText += this.GetMacroDisplayValues(this.AssetInfo, this.Asset.Name);
        return displayText;
      }
    }

    public List<string> FileTypes => this.AssetInfo == null ? new List<string>() : this.AssetInfo.FileTypes.ToList<string>();

    public string GetMacroDisplayValues(PolicyAssetInfo assetInfo, string name)
    {
      if (assetInfo == null || !assetInfo.IsMatch(name))
        return "";
      PolicyMacroTable policyMacroTable = new PolicyMacroTable(assetInfo.Name, name);
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string key in policyMacroTable.macros.Keys)
        stringBuilder.AppendFormat(assetInfo.OEMMacros.Count > 1 ? "{0}={1}; " : "{1}", (object) key, (object) policyMacroTable.macros[key]);
      return string.Format(" ({0})", (object) stringBuilder.ToString().TrimEnd(' ', ';'));
    }

    public override object GetPreviewItem(bool includeAllLevels, int level = 0) => (object) new Asset()
    {
      TargetFileName = this.TargetFileName,
      DisplayName = this.DisplayName,
      Source = this.Source,
      Name = this.Asset.Name,
      Type = this.Owners.First<WPCustomizationAssetOwner>((Func<WPCustomizationAssetOwner, bool>) (asset => asset.IsSelected)).Value
    };

    protected override bool HasErrors => (this.DisplayNameHasErrors || this.OwnerHasErrors || this.TargetFileNameHasErrors) && (this.IsHideAble && (this.IsDirty || this.IsNewItem) || !this.IsHideAble) || base.HasErrors;

    protected override void ValidateItem()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        bool includChildren = false;
        Asset previewItem1 = this.GetPreviewItem(includChildren, 0) as Asset;
        Settings previewItem2 = (this.Parent.Parent.Parent as WPSettings).GetPreviewItem(includChildren, 0) as Settings;
        ImageCustomizations previewItem3 = (this.ListRoot as WPImageCustomization).GetPreviewItem(true, 0) as ImageCustomizations;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifyAssetSource(previewItem1, previewItem2, previewItem3, WPListItemBase.PolicyStore, (PolicyAssetInfo) null).ToList<CustomizationError>(), (WPListItemBase) this);
        base.ValidateItem();
        WPErrors wpErrors1 = new WPErrors(CustomContentGenerator.VerifyAssetDisplayName(previewItem1, previewItem2).ToList<CustomizationError>(), (WPListItemBase) this);
        this.DisplayNameHasWarnings = wpErrors1.HasWarnings;
        this.DisplayNameHasErrors = wpErrors1.HasErrors;
        this.DisplayNameWarningsToolTip = wpErrors1.WarningsMessage;
        this.DisplayNameErrorsToolTip = wpErrors1.ErrorsMessage;
        this._additionalIssues.Add(wpErrors1);
        WPErrors wpErrors2 = new WPErrors(CustomContentGenerator.VerifyAssetTargetFileName(previewItem1, previewItem2, previewItem3, WPListItemBase.PolicyStore, (PolicyAssetInfo) null).ToList<CustomizationError>(), (WPListItemBase) this);
        this.TargetFileNameHasWarnings = wpErrors2.HasWarnings;
        this.TargetFileNameHasErrors = wpErrors2.HasErrors;
        this.TargetFileNameWarningsToolTip = wpErrors2.WarningsMessage;
        this.TargetFileNameErrorsToolTip = wpErrors2.ErrorsMessage;
        this._additionalIssues.Add(wpErrors2);
        WPErrors wpErrors3 = new WPErrors(CustomContentGenerator.VerifyAssetType(previewItem1, previewItem2, WPListItemBase.PolicyStore, (PolicyAssetInfo) null).ToList<CustomizationError>(), (WPListItemBase) this);
        this.OwnerHasWarnings = wpErrors3.HasWarnings;
        this.OwnerHasErrors = wpErrors3.HasErrors;
        this.OwnerWarningsToolTip = wpErrors3.WarningsMessage;
        this.OwnerErrorsToolTip = wpErrors3.ErrorsMessage;
        this._additionalIssues.Add(wpErrors3);
      }
      else
        base.ValidateItem();
    }

    private bool _isSourceDirty => this.IsNewItem || !this.Source.Equals(this.Asset.Source);

    private bool _isDisplayNameDirty => string.IsNullOrEmpty(this.DisplayName) != string.IsNullOrEmpty(this.Asset.DisplayName) || !string.IsNullOrEmpty(this.DisplayName) && !this.DisplayName.Equals(this.Asset.DisplayName);

    private bool _isTargetFilenameDirty => this.IsNewItem || (!string.IsNullOrEmpty(this.TargetFileName) || !string.IsNullOrEmpty(this.Asset.TargetFileName)) && !this.TargetFileName.Equals(this.Asset.TargetFileName);

    private bool _isOwnerDirty => this.IsNewItem || this.Owners.Where<WPCustomizationAssetOwner>((Func<WPCustomizationAssetOwner, bool>) (owner => owner.IsSelected)).Count<WPCustomizationAssetOwner>() != 0 && this.Owners.First<WPCustomizationAssetOwner>((Func<WPCustomizationAssetOwner, bool>) (owner => owner.IsSelected)).Value != this.Asset.Type;

    protected override void UpdateIsDirty()
    {
      if (this._isSourceDirty || this._isTargetFilenameDirty || this._isOwnerDirty || this._isDisplayNameDirty || this.ChildListChanged)
        this._isDirty = true;
      else
        this._isDirty = false;
    }

    public override void UpdateFontSettings()
    {
      this.SourceForeground = !this.HasValidationErrors ? (!this._isSourceDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      this.DisplayNameForeground = !this.DisplayNameHasErrors ? (!this._isDisplayNameDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      this.TargetForeground = !this.TargetFileNameHasErrors ? (!this._isTargetFilenameDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      this.OwnerForeground = !this.OwnerHasErrors ? (!this._isOwnerDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      base.UpdateFontSettings();
    }

    public string SourceForeground
    {
      get => (string) this.GetValue(WPSettingAsset.SourceForegroundProperty);
      set => this.SetValue(WPSettingAsset.SourceForegroundProperty, (object) value);
    }

    public string DisplayNameForeground
    {
      get => (string) this.GetValue(WPSettingAsset.DisplayNameForegroundProperty);
      set => this.SetValue(WPSettingAsset.DisplayNameForegroundProperty, (object) value);
    }

    public string TargetForeground
    {
      get => (string) this.GetValue(WPSettingAsset.TargetForegroundProperty);
      set => this.SetValue(WPSettingAsset.TargetForegroundProperty, (object) value);
    }

    public string OwnerForeground
    {
      get => (string) this.GetValue(WPSettingAsset.OwnerForegroundProperty);
      set => this.SetValue(WPSettingAsset.OwnerForegroundProperty, (object) value);
    }

    public static WPListItemBase AddNewItem(
      WPListItemBase parent,
      string source,
      string displayName,
      string groupName)
    {
      bool isIncludedInOutput = false;
      Asset asset = new Asset();
      asset.Source = source;
      asset.TargetFileName = (string) null;
      asset.DisplayName = displayName;
      asset.Type = (CustomizationAssetOwner) 0;
      asset.Name = groupName;
      WPSettingsAssetsGroup settingsAssetsGroup = parent as WPSettingsAssetsGroup;
      WPSettingAsset newItem = new WPSettingAsset(asset, settingsAssetsGroup.AssetInfo, isIncludedInOutput, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      newItem.IsExpanded = true;
      newItem.IsSelected = true;
      return (WPListItemBase) newItem;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.Asset.Type = this.Owners.First<WPCustomizationAssetOwner>((Func<WPCustomizationAssetOwner, bool>) (owner => owner.IsSelected)).Value;
        this.Asset.Source = this.Source;
        this.Asset.DisplayName = !string.IsNullOrEmpty(this.DisplayName) ? this.DisplayName : (string) null;
        this.Asset.TargetFileName = !string.IsNullOrEmpty(this.TargetFileName) ? this.TargetFileName : (string) null;
      }
      return base.SaveItem();
    }

    public ObservableCollection<WPCustomizationAssetOwner> Owners
    {
      get => (ObservableCollection<WPCustomizationAssetOwner>) this.GetValue(WPSettingAsset.OwnersProperty);
      set => this.SetValue(WPSettingAsset.OwnersProperty, (object) value);
    }

    public string Source
    {
      get => (string) this.GetValue(WPSettingAsset.SourceProperty);
      set => this.SetValue(WPSettingAsset.SourceProperty, (object) value);
    }

    public string DisplayName
    {
      get => (string) this.GetValue(WPSettingAsset.DisplayNameProperty);
      set => this.SetValue(WPSettingAsset.DisplayNameProperty, (object) value);
    }

    public bool DisplayNameHasWarnings
    {
      get => (bool) this.GetValue(WPSettingAsset.DisplayNameHasWarningsProperty);
      set => this.SetValue(WPSettingAsset.DisplayNameHasWarningsProperty, (object) value);
    }

    public bool DisplayNameHasErrors
    {
      get => (bool) this.GetValue(WPSettingAsset.DisplayNameHasErrorsProperty);
      set => this.SetValue(WPSettingAsset.DisplayNameHasErrorsProperty, (object) value);
    }

    public string DisplayNameWarningsToolTip
    {
      get => (string) this.GetValue(WPSettingAsset.DisplayNameWarningsToolTipProperty);
      set => this.SetValue(WPSettingAsset.DisplayNameWarningsToolTipProperty, (object) value);
    }

    public string DisplayNameErrorsToolTip
    {
      get => (string) this.GetValue(WPSettingAsset.DisplayNameErrorsToolTipProperty);
      set => this.SetValue(WPSettingAsset.DisplayNameErrorsToolTipProperty, (object) value);
    }

    public string TargetFileName
    {
      get => (string) this.GetValue(WPSettingAsset.TargetFileNameProperty);
      set => this.SetValue(WPSettingAsset.TargetFileNameProperty, (object) value);
    }

    public bool TargetFileNameHasWarnings
    {
      get => (bool) this.GetValue(WPSettingAsset.TargetFileNameHasWarningsProperty);
      set => this.SetValue(WPSettingAsset.TargetFileNameHasWarningsProperty, (object) value);
    }

    public bool TargetFileNameHasErrors
    {
      get => (bool) this.GetValue(WPSettingAsset.TargetFileNameHasErrorsProperty);
      set => this.SetValue(WPSettingAsset.TargetFileNameHasErrorsProperty, (object) value);
    }

    public string TargetFileNameWarningsToolTip
    {
      get => (string) this.GetValue(WPSettingAsset.TargetFileNameWarningsToolTipProperty);
      set => this.SetValue(WPSettingAsset.TargetFileNameWarningsToolTipProperty, (object) value);
    }

    public string TargetFileNameErrorsToolTip
    {
      get => (string) this.GetValue(WPSettingAsset.TargetFileNameErrorsToolTipProperty);
      set => this.SetValue(WPSettingAsset.TargetFileNameErrorsToolTipProperty, (object) value);
    }

    public bool OwnerHasWarnings
    {
      get => (bool) this.GetValue(WPSettingAsset.OwnerHasWarningsProperty);
      set => this.SetValue(WPSettingAsset.OwnerHasWarningsProperty, (object) value);
    }

    public bool OwnerHasErrors
    {
      get => (bool) this.GetValue(WPSettingAsset.OwnerHasErrorsProperty);
      set => this.SetValue(WPSettingAsset.OwnerHasErrorsProperty, (object) value);
    }

    public string OwnerWarningsToolTip
    {
      get => (string) this.GetValue(WPSettingAsset.OwnerWarningsToolTipProperty);
      set => this.SetValue(WPSettingAsset.OwnerWarningsToolTipProperty, (object) value);
    }

    public string OwnerErrorsToolTip
    {
      get => (string) this.GetValue(WPSettingAsset.OwnerErrorsToolTipProperty);
      set => this.SetValue(WPSettingAsset.OwnerErrorsToolTipProperty, (object) value);
    }
  }
}
