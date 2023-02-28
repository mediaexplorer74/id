// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.CustomizeOSPageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.MCSF.Offline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class CustomizeOSPageVM : IDViewModelBase
  {
    public int CurrentErrorIndex;
    public static readonly DependencyProperty ErrorsGroupTitleProperty = DependencyProperty.Register(nameof (ErrorsGroupTitle), typeof (string), typeof (CustomizeOSPageVM), new PropertyMetadata((object) nameof (Errors)));
    public static readonly DependencyProperty ErrorsProperty = DependencyProperty.Register(nameof (Errors), typeof (List<WPErrors>), typeof (CustomizeOSPageVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty HasErrorsProperty = DependencyProperty.Register(nameof (HasErrors), typeof (bool), typeof (CustomizeOSPageVM), new PropertyMetadata((object) false));
    public static readonly DependencyProperty CurrentPathProperty = DependencyProperty.Register(nameof (CurrentPath), typeof (string), typeof (CustomizeOSPageVM), new PropertyMetadata((object) Tools.GetString("tbSettingsPathPartDefault")));
    public static readonly DependencyProperty HasValidationWarningsProperty = DependencyProperty.Register(nameof (HasValidationWarnings), typeof (bool), typeof (CustomizeOSPageVM), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ValidationWarningToolTipProperty = DependencyProperty.Register(nameof (ValidationWarningToolTip), typeof (string), typeof (CustomizeOSPageVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ValidationErrorToolTipProperty = DependencyProperty.Register(nameof (ValidationErrorToolTip), typeof (string), typeof (CustomizeOSPageVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty HasValidationErrorsProperty = DependencyProperty.Register(nameof (HasValidationErrors), typeof (bool), typeof (CustomizeOSPageVM), new PropertyMetadata((object) false));
    private PolicyStore _policyStore;
    public WPImageCustomization WPImageCustomization;
    public WPListItemBase CurrentVariantItem;
    public static readonly DependencyProperty VariantListProperty = DependencyProperty.Register(nameof (VariantList), typeof (ObservableCollection<WPListItemBase>), typeof (CustomizeOSPageVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty CustomizationsProperty = DependencyProperty.Register(nameof (Customizations), typeof (ObservableCollection<WPListItemBase>), typeof (CustomizeOSPageVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty CurrentItemProperty = DependencyProperty.Register(nameof (CurrentItem), typeof (WPListItemBase), typeof (CustomizeOSPageVM), new PropertyMetadata((PropertyChangedCallback) null));

    internal CustomizeOSPageVM(IDStates mystate)
      : base(mystate)
    {
    }

    protected override bool SaveSupported => true;

    protected override void Validate()
    {
      if (this.WPImageCustomization == null)
        this.IsValid = false;
      else
        this.IsValid = !this.HasErrors;
    }

    protected override void ComputeNextState() => this._nextState = new IDStates?(IDStates.BuildImage);

    internal override bool OnStateEntry()
    {
      this.LoadCustomizationPolicy();
      this.PopulateVariants();
      if (this.CheckForErrors())
      {
        this.GoToError(this.CurrentErrorIndex);
        return false;
      }
      this.Validate();
      return true;
    }

    protected override bool SavePage()
    {
      this.Errors = (List<WPErrors>) null;
      this.HasErrors = false;
      if (this.CheckForErrors())
      {
        this.GoToError(this.CurrentErrorIndex);
        int num = (int) MessageBox.Show(Tools.GetString("msgCOSFailedToSave"), Tools.GetString("msgCOSFailedToSaveTitle"));
        return false;
      }
      this.WPImageCustomization.Save();
      ImageCustomizations imageCustomizations = this.WPImageCustomization.ImageCustomizations;
      if (File.Exists(this.Context.CurrentOEMCustomizationFile))
        File.Delete(this.Context.CurrentOEMCustomizationFile);
      Version firmwareVersion = this.GetFirmwareVersion();
      if (firmwareVersion != (Version) null)
        IDContext.Instance.CustomizationPkgVersion = firmwareVersion;
      imageCustomizations.Save(this.Context.CurrentOEMCustomizationFile);
      this.Context.CurrentImageCustomization = imageCustomizations;
      return true;
    }

    public bool CheckForErrors()
    {
      if (this.WPImageCustomization != null && this.WPImageCustomization.AllErrors.Count<WPErrors>() != 0)
      {
        this.CurrentErrorIndex = 0;
        this.HasErrors = true;
        this.Errors = this.WPImageCustomization.AllErrors;
        this.IsValid = false;
        return true;
      }
      this.HasErrors = false;
      this.Errors = (List<WPErrors>) null;
      this.IsValid = true;
      return false;
    }

    public void GoToPreviousError()
    {
      if (this.CurrentErrorIndex > 0)
        --this.CurrentErrorIndex;
      else
        this.CurrentErrorIndex = this.Errors.Count<WPErrors>() - 1;
      this.GoToError(this.CurrentErrorIndex);
    }

    public void GoToNextError()
    {
      if (this.CurrentErrorIndex + 1 < this.Errors.Count<WPErrors>())
        ++this.CurrentErrorIndex;
      else
        this.CurrentErrorIndex = 0;
      this.GoToError(this.CurrentErrorIndex);
    }

    public void GoToError(int index)
    {
      if (index < 0 || index >= this.Errors.Count<WPErrors>())
        return;
      WPListItemBase sourceItem = this.Errors[index].SourceItem;
      if (sourceItem != this.CurrentItem)
      {
        this.CurrentItem = sourceItem;
        sourceItem.IsExpanded = true;
        sourceItem.IsSelected = true;
        sourceItem.OnPropertyChanged("IsSelected");
        sourceItem.OnPropertyChanged("IsExpanded");
      }
      this.ErrorsGroupTitle = string.Format(Tools.GetString("gCOSErrorsTitle"), (object) (index + 1), (object) this.Errors.Count<WPErrors>());
      this.OnPropertyChanged("ErrorsGroupTitle");
    }

    public string ErrorsGroupTitle
    {
      get => (string) this.GetValue(CustomizeOSPageVM.ErrorsGroupTitleProperty);
      set => this.SetValue(CustomizeOSPageVM.ErrorsGroupTitleProperty, (object) value);
    }

    public List<WPErrors> Errors
    {
      get => (List<WPErrors>) this.GetValue(CustomizeOSPageVM.ErrorsProperty);
      set => this.SetValue(CustomizeOSPageVM.ErrorsProperty, (object) value);
    }

    public bool HasErrors
    {
      get => (bool) this.GetValue(CustomizeOSPageVM.HasErrorsProperty);
      set => this.SetValue(CustomizeOSPageVM.HasErrorsProperty, (object) value);
    }

    public string CurrentPath
    {
      get => (string) this.GetValue(CustomizeOSPageVM.CurrentPathProperty);
      set => this.SetValue(CustomizeOSPageVM.CurrentPathProperty, (object) value);
    }

    public bool HasValidationWarnings
    {
      get => (bool) this.GetValue(CustomizeOSPageVM.HasValidationWarningsProperty);
      set => this.SetValue(CustomizeOSPageVM.HasValidationWarningsProperty, (object) value);
    }

    public string ValidationWarningToolTip
    {
      get => (string) this.GetValue(CustomizeOSPageVM.ValidationWarningToolTipProperty);
      set => this.SetValue(CustomizeOSPageVM.ValidationWarningToolTipProperty, (object) value);
    }

    public string ValidationErrorToolTip
    {
      get => (string) this.GetValue(CustomizeOSPageVM.ValidationErrorToolTipProperty);
      set => this.SetValue(CustomizeOSPageVM.ValidationErrorToolTipProperty, (object) value);
    }

    public bool HasValidationErrors
    {
      get => (bool) this.GetValue(CustomizeOSPageVM.HasValidationErrorsProperty);
      set => this.SetValue(CustomizeOSPageVM.HasValidationErrorsProperty, (object) value);
    }

    private void LoadCustomizationPolicy()
    {
      List<FeatureManifest.FMPkgInfo> oemInputPackages = Tools.GetOEMInputPackages(this.Context.SelectedOEMInput, this.Context.BSPRoot, this.Context.AKRoot, this.Context.SelectedOEMInput.IsMMOS ? this.Context.MMOSFMFile : this.Context.MicrosoftPhoneFMFile);
      Tools.RetrieveForMetadataPackages(oemInputPackages, this.Context.PackageMetadataList);
      List<IPkgInfo> ipkgInfoList = new List<IPkgInfo>();
      foreach (FeatureManifest.FMPkgInfo fmPkgInfo in oemInputPackages)
        ipkgInfoList.Add(this.Context.PackageMetadataList[fmPkgInfo.PackagePath].pkgInfo);
      this._policyStore = new PolicyStore();
      List<IPkgInfo> source = new List<IPkgInfo>();
      string str = "";
      foreach (IPkgInfo ipkgInfo in ipkgInfoList)
      {
        if (ipkgInfo.ReleaseType == 2)
        {
          try
          {
            this._policyStore.LoadPolicyFromPackage(ipkgInfo);
          }
          catch (Exception ex)
          {
            str = str + "\t" + ipkgInfo.Name + ":" + Environment.NewLine;
            str = str + "\t\t" + ex.Message + Environment.NewLine;
            source.Add(ipkgInfo);
          }
        }
      }
      if (((IEnumerable<IPkgInfo>) source).Count<IPkgInfo>() == 0)
        return;
      int num = (int) MessageBox.Show(string.Format(Tools.GetString("msgCOSBadPolicyPackagesError"), (object) ((IEnumerable<IPkgInfo>) source).Count<IPkgInfo>(), (object) str), Tools.GetString("msgCOSBadPolicyPackagesErrorHeader"));
    }

    public void OnShowAllChanged()
    {
      if (this.CurrentVariantItem == null || !(this.CurrentVariantItem is WPVariant))
        return;
      WPVariant currentVariantItem = this.CurrentVariantItem as WPVariant;
      currentVariantItem.OnShowAllChanged(!currentVariantItem.ShowAll);
    }

    private void PopulateVariants()
    {
      ObservableCollection<WPListItemBase> observableCollection = new ObservableCollection<WPListItemBase>();
      ImageCustomizations imageCustomizations = this.Context.CurrentImageCustomization;
      bool isIncludedInOutput = true;
      if (imageCustomizations == null)
      {
        imageCustomizations = new ImageCustomizations();
        imageCustomizations.Description = "Customization generated in Image Designer";
        imageCustomizations.Name = "Custom";
        imageCustomizations.Owner = "OEM";
        imageCustomizations.OwnerType = (OwnerType) 2;
        imageCustomizations.DefinedInFile = "Image Designer";
      }
      if (imageCustomizations.StaticVariant == null)
        imageCustomizations.StaticVariant = new StaticVariant();
      this.WPImageCustomization = new WPImageCustomization(imageCustomizations, this._policyStore, isIncludedInOutput, this);
      if (this.WPImageCustomization.Children.Count<WPListItemBase>() != 0 && this.WPImageCustomization.Children.Where<WPListItemBase>((Func<WPListItemBase, bool>) (temp => temp.IsSelectedForListBox)).Count<WPListItemBase>() == 0)
      {
        WPVariant child = this.WPImageCustomization.Children[0] as WPVariant;
        child.IsSelectedForListBox = true;
        child.OnPropertyChanged("IsSelectedForListBox");
        this.VariantSelectionChanged(child);
      }
      this.VariantList = this.WPImageCustomization.Children;
    }

    public void VariantSelectionChanged(WPVariant variant)
    {
      this.ShowCustomizations(variant);
      this.CurrentVariantItem = (WPListItemBase) variant;
    }

    private void ShowCustomizations(WPVariant variant)
    {
      this.Customizations = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) variant.Children);
      if (this.Customizations.Count<WPListItemBase>() == 0 || this.Customizations.Where<WPListItemBase>((Func<WPListItemBase, bool>) (cust => cust.IsSelected)).Count<WPListItemBase>() != 0)
        return;
      this.Customizations[0].IsSelected = true;
    }

    public ObservableCollection<WPListItemBase> VariantList
    {
      get => (ObservableCollection<WPListItemBase>) this.GetValue(CustomizeOSPageVM.VariantListProperty);
      set => this.SetValue(CustomizeOSPageVM.VariantListProperty, (object) value);
    }

    public ObservableCollection<WPListItemBase> Customizations
    {
      get => (ObservableCollection<WPListItemBase>) this.GetValue(CustomizeOSPageVM.CustomizationsProperty);
      set => this.SetValue(CustomizeOSPageVM.CustomizationsProperty, (object) value);
    }

    public WPListItemBase CurrentItem
    {
      get => (WPListItemBase) this.GetValue(CustomizeOSPageVM.CurrentItemProperty);
      set
      {
        this.SetValue(CustomizeOSPageVM.CurrentItemProperty, (object) value);
        if (this.CurrentItem != null)
        {
          this.CurrentPath = this.CurrentItem.DisplayPath;
          if (!(this.CurrentItem is WPSettings))
            return;
          (this.CurrentItem as WPSettings).LoadPolicy();
        }
        else
          this.CurrentPath = Tools.GetString("tbSettingsPathPartDefault");
      }
    }

    public void SelectedCustomizationChanged(object sender)
    {
      if (sender == null)
        return;
      this.CurrentItem = sender as WPListItemBase;
    }

    public void CustomizationChanged()
    {
      if (this.CurrentItem == null)
        return;
      int num = this.CurrentItem.IsDirty ? 1 : 0;
    }

    private Version GetFirmwareVersion()
    {
      Version result = (Version) null;
      List<Settings> settingGroups = ((Variant) this.WPImageCustomization.ImageCustomizations.StaticVariant).SettingGroups;
      if (settingGroups != null)
      {
        Settings settings = ((IEnumerable<Settings>) settingGroups).FirstOrDefault<Settings>((Func<Settings, bool>) (s => s.Path.Equals("DeviceInfo/Static", StringComparison.OrdinalIgnoreCase)));
        if (settings != null)
        {
          Setting setting = ((IEnumerable<Setting>) settings.Items).FirstOrDefault<Setting>((Func<Setting, bool>) (s => s.Name.Equals("PhoneFirmwareRevision")));
          if (setting != null && Version.TryParse(setting.Value, out result))
            result = result.Normalize();
        }
      }
      return result;
    }
  }
}
