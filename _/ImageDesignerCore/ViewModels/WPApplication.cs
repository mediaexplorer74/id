// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPApplication
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPApplication : WPListItemBase
  {
    public Application Application;
    private bool _isStaticVariant;
    public static readonly DependencyProperty SourceForegroundProperty = DependencyProperty.Register(nameof (SourceForeground), typeof (string), typeof (WPApplication), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty LicenseForegroundProperty = DependencyProperty.Register(nameof (LicenseForeground), typeof (string), typeof (WPApplication), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty ProvXMLForegroundProperty = DependencyProperty.Register(nameof (ProvXMLForeground), typeof (string), typeof (WPApplication), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty LicenseHasWarningsProperty = DependencyProperty.Register(nameof (LicenseHasWarnings), typeof (bool), typeof (WPApplication), new PropertyMetadata((object) false));
    public static readonly DependencyProperty LicenseHasErrorsProperty = DependencyProperty.Register(nameof (LicenseHasErrors), typeof (bool), typeof (WPApplication), new PropertyMetadata((object) false));
    public static readonly DependencyProperty LicenseWarningsToolTipProperty = DependencyProperty.Register(nameof (LicenseWarningsToolTip), typeof (string), typeof (WPApplication), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty LicenseErrorsToolTipProperty = DependencyProperty.Register(nameof (LicenseErrorsToolTip), typeof (string), typeof (WPApplication), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ProvXMLHasWarningsProperty = DependencyProperty.Register(nameof (ProvXMLHasWarnings), typeof (bool), typeof (WPApplication), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ProvXMLHasErrorsProperty = DependencyProperty.Register(nameof (ProvXMLHasErrors), typeof (bool), typeof (WPApplication), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ProvXMLWarningsToolTipProperty = DependencyProperty.Register(nameof (ProvXMLWarningsToolTip), typeof (string), typeof (WPApplication), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ProvXMLErrorsToolTipProperty = DependencyProperty.Register(nameof (ProvXMLErrorsToolTip), typeof (string), typeof (WPApplication), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty TargetPartitionForegroundProperty = DependencyProperty.Register(nameof (TargetPartitionForeground), typeof (string), typeof (WPApplication), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof (Source), typeof (string), typeof (WPApplication), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty LicenseProperty = DependencyProperty.Register(nameof (License), typeof (string), typeof (WPApplication), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty ProvXMLProperty = DependencyProperty.Register(nameof (ProvXML), typeof (string), typeof (WPApplication), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty TargetPartitionProperty = DependencyProperty.Register(nameof (TargetPartition), typeof (AppPartition), typeof (WPApplication), new PropertyMetadata((object) AppPartition.MainOS));

    public WPApplication(Application application, bool isIncludedInOutput, WPListItemBase parent)
      : base(parent)
    {
      this.Application = application;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.Source = this.Application.Source;
      this.License = this.Application.License;
      this.ProvXML = this.Application.ProvXML;
      if (this.GetParentOfType(typeof (WPVariant)) is WPVariant parentOfType)
        this._isStaticVariant = parentOfType.IsStaticVariant;
      this.TargetPartition = !isIncludedInOutput ? (!this.CurrentVariant.IsStaticVariant ? AppPartition.Data : AppPartition.MainOS) : WPApplication.StringToAppPartition(this.Application.TargetPartition);
      this.InitializationComplete();
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/application_32xMD.png";

    public override string DisplayText
    {
      get
      {
        string path = this.Source;
        if (!this._isStaticVariant && string.IsNullOrEmpty(path))
          path = this.ProvXML;
        string displayText = Path.GetFileName(path);
        if (string.IsNullOrEmpty(displayText))
          displayText = path;
        if (string.IsNullOrEmpty(displayText))
          displayText = !this._isStaticVariant ? Tools.GetString("txtApplicationSourceOrProvxmlRequired") : Tools.GetString("txtApplicationSourceRequired");
        return displayText;
      }
    }

    protected override string PathPart
    {
      get
      {
        string pathPart = "";
        if (this.Source.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        {
          int num = this.Source.LastIndexOf('.');
          if (num != -1)
            pathPart = this.Source.Substring(num + 1);
        }
        else
          pathPart = Path.GetFileName(this.Source);
        return pathPart;
      }
    }

    public override object GetPreviewItem(bool includeAllLevels, int level = 0) => (object) new Application()
    {
      Source = this.Source,
      License = this.License,
      ProvXML = this.ProvXML,
      TargetPartition = WPApplication.AppPartitionToString(this.TargetPartition)
    };

    protected override bool HasErrors => (this.LicenseHasErrors || this.ProvXMLHasErrors) && (this.IsHideAble && (this.IsDirty || this.IsNewItem) || !this.IsHideAble) || base.HasErrors;

    protected override void ValidateItem()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        Application previewItem = this.GetPreviewItem(false, 0) as Application;
        WPVariant parent = this.Parent.Parent as WPVariant;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifyApplicationSource(previewItem, parent.VariantItem).ToList<CustomizationError>(), (WPListItemBase) this);
        base.ValidateItem();
        WPErrors wpErrors1 = new WPErrors(CustomContentGenerator.VerifyApplicationLicense(previewItem).ToList<CustomizationError>(), (WPListItemBase) this);
        this.LicenseHasWarnings = wpErrors1.HasWarnings;
        this.LicenseHasErrors = wpErrors1.HasErrors;
        this.LicenseWarningsToolTip = wpErrors1.WarningsMessage;
        this.LicenseErrorsToolTip = wpErrors1.ErrorsMessage;
        this._additionalIssues.Add(wpErrors1);
        WPErrors wpErrors2 = new WPErrors(CustomContentGenerator.VerifyApplicationProvXML(previewItem, parent.VariantItem).ToList<CustomizationError>(), (WPListItemBase) this);
        this.ProvXMLHasWarnings = wpErrors2.HasWarnings;
        this.ProvXMLHasErrors = wpErrors2.HasErrors;
        this.ProvXMLWarningsToolTip = wpErrors2.WarningsMessage;
        this.ProvXMLErrorsToolTip = wpErrors2.ErrorsMessage;
        this._additionalIssues.Add(wpErrors2);
      }
      else
        base.ValidateItem();
    }

    private bool _isSourceDirty => this.IsNewItem || !this.Source.Equals(this.Application.Source);

    private bool _isLicenseDirty => this.IsNewItem || !this.License.Equals(this.Application.License);

    private bool _isProvXMLDirty => this.IsNewItem || !this.ProvXML.Equals(this.Application.ProvXML);

    private bool _isTargetPartitionDirty => this.IsNewItem || !WPApplication.AppPartitionToString(this.TargetPartition).Equals(this.Application.TargetPartition, StringComparison.OrdinalIgnoreCase);

    protected override void UpdateIsDirty()
    {
      if (this._isSourceDirty || this._isLicenseDirty || this._isProvXMLDirty || this._isTargetPartitionDirty || this.ChildListChanged)
        this._isDirty = true;
      else
        this._isDirty = false;
    }

    public override void UpdateFontSettings()
    {
      this.SourceForeground = !this.HasValidationErrors ? (!this._isSourceDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      this.LicenseForeground = !this.LicenseHasErrors ? (!this._isLicenseDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      this.ProvXMLForeground = !this.ProvXMLHasErrors ? (!this._isProvXMLDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      this.TargetPartitionForeground = !this._isTargetPartitionDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor;
      base.UpdateFontSettings();
    }

    public string SourceForeground
    {
      get => (string) this.GetValue(WPApplication.SourceForegroundProperty);
      set => this.SetValue(WPApplication.SourceForegroundProperty, (object) value);
    }

    public string LicenseForeground
    {
      get => (string) this.GetValue(WPApplication.LicenseForegroundProperty);
      set => this.SetValue(WPApplication.LicenseForegroundProperty, (object) value);
    }

    public string ProvXMLForeground
    {
      get => (string) this.GetValue(WPApplication.ProvXMLForegroundProperty);
      set => this.SetValue(WPApplication.ProvXMLForegroundProperty, (object) value);
    }

    public bool LicenseHasWarnings
    {
      get => (bool) this.GetValue(WPApplication.LicenseHasWarningsProperty);
      set => this.SetValue(WPApplication.LicenseHasWarningsProperty, (object) value);
    }

    public bool LicenseHasErrors
    {
      get => (bool) this.GetValue(WPApplication.LicenseHasErrorsProperty);
      set => this.SetValue(WPApplication.LicenseHasErrorsProperty, (object) value);
    }

    public string LicenseWarningsToolTip
    {
      get => (string) this.GetValue(WPApplication.LicenseWarningsToolTipProperty);
      set => this.SetValue(WPApplication.LicenseWarningsToolTipProperty, (object) value);
    }

    public string LicenseErrorsToolTip
    {
      get => (string) this.GetValue(WPApplication.LicenseErrorsToolTipProperty);
      set => this.SetValue(WPApplication.LicenseErrorsToolTipProperty, (object) value);
    }

    public bool ProvXMLHasWarnings
    {
      get => (bool) this.GetValue(WPApplication.ProvXMLHasWarningsProperty);
      set => this.SetValue(WPApplication.ProvXMLHasWarningsProperty, (object) value);
    }

    public bool ProvXMLHasErrors
    {
      get => (bool) this.GetValue(WPApplication.ProvXMLHasErrorsProperty);
      set => this.SetValue(WPApplication.ProvXMLHasErrorsProperty, (object) value);
    }

    public string ProvXMLWarningsToolTip
    {
      get => (string) this.GetValue(WPApplication.ProvXMLWarningsToolTipProperty);
      set => this.SetValue(WPApplication.ProvXMLWarningsToolTipProperty, (object) value);
    }

    public string ProvXMLErrorsToolTip
    {
      get => (string) this.GetValue(WPApplication.ProvXMLErrorsToolTipProperty);
      set => this.SetValue(WPApplication.ProvXMLErrorsToolTipProperty, (object) value);
    }

    public string TargetPartitionForeground
    {
      get => (string) this.GetValue(WPApplication.TargetPartitionForegroundProperty);
      set => this.SetValue(WPApplication.TargetPartitionForegroundProperty, (object) value);
    }

    public static WPListItemBase AddNewItem(
      WPListItemBase parent,
      string file,
      bool useAsProvXML)
    {
      bool isIncludedInOutput = false;
      Application application = new Application();
      if (useAsProvXML)
      {
        application.Source = "";
        application.ProvXML = file;
      }
      else
      {
        application.Source = file;
        application.ProvXML = "";
      }
      application.License = "";
      application.TargetPartition = WPApplication.AppPartitionToString(AppPartition.MainOS);
      WPApplication newItem = new WPApplication(application, isIncludedInOutput, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      return (WPListItemBase) newItem;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.Application.Source = this.Source;
        this.Application.License = this.License;
        this.Application.ProvXML = this.ProvXML;
        this.Application.TargetPartition = WPApplication.AppPartitionToString(this.TargetPartition);
        this.Application.StaticApp = this.CurrentVariant.IsStaticVariant;
      }
      return base.SaveItem();
    }

    public string Source
    {
      get => (string) this.GetValue(WPApplication.SourceProperty);
      set => this.SetValue(WPApplication.SourceProperty, (object) value);
    }

    public string License
    {
      get => (string) this.GetValue(WPApplication.LicenseProperty);
      set => this.SetValue(WPApplication.LicenseProperty, (object) value);
    }

    public string ProvXML
    {
      get => (string) this.GetValue(WPApplication.ProvXMLProperty);
      set => this.SetValue(WPApplication.ProvXMLProperty, (object) value);
    }

    public AppPartition TargetPartition
    {
      get => (AppPartition) this.GetValue(WPApplication.TargetPartitionProperty);
      set => this.SetValue(WPApplication.TargetPartitionProperty, (object) value);
    }

    private static string AppPartitionToString(AppPartition partition)
    {
      string empty = string.Empty;
      switch (partition)
      {
        case AppPartition.MainOS:
          return PkgConstants.c_strMainOsPartition;
        case AppPartition.Data:
          return PkgConstants.c_strDataPartition;
        default:
          throw new ArgumentException("Invalid AppPartition");
      }
    }

    private static AppPartition StringToAppPartition(string strValue)
    {
      if (strValue.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase))
        return AppPartition.MainOS;
      if (strValue.Equals(PkgConstants.c_strDataPartition, StringComparison.OrdinalIgnoreCase))
        return AppPartition.Data;
      throw new ArgumentException("Value {0} cannot be converted to type AppPartition", strValue);
    }
  }
}
