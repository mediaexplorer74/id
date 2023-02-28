// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPCondition
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System.Linq;
using System.Windows;
using Condition = Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Condition;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPCondition : WPListItemBase
  {
    public Condition Condition;
    public static readonly DependencyProperty NameForegroundProperty = DependencyProperty.Register(nameof (NameForeground), typeof (string), typeof (WPCondition), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty ValueForegroundProperty = DependencyProperty.Register(nameof (ValueForeground), typeof (string), typeof (WPCondition), new PropertyMetadata((object) WPListItemBase.DefaultForegroundColor, new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty ValueHasWarningsProperty = DependencyProperty.Register(nameof (ValueHasWarnings), typeof (bool), typeof (WPCondition), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ValueHasErrorsProperty = DependencyProperty.Register(nameof (ValueHasErrors), typeof (bool), typeof (WPCondition), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ValueWarningsToolTipProperty = DependencyProperty.Register(nameof (ValueWarningsToolTip), typeof (string), typeof (WPCondition), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ValueErrorsToolTipProperty = DependencyProperty.Register(nameof (ValueErrorsToolTip), typeof (string), typeof (WPCondition), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof (Name), typeof (string), typeof (WPCondition), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof (Value), typeof (string), typeof (WPCondition), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));

    public WPCondition(
      Condition condition,
      bool isIncludedInOutput,
      WPListItemBase parent,
      bool isNewItem = false)
      : base(parent)
    {
      this.Condition = condition;
      this.Name = condition.Name;
      this.Value = condition.Value;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.IsNewItem = isNewItem;
      this.TreeViewVisibilty = "Collapsed";
      this.InitializationComplete();
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/folder_closed_32xMD.png";

    public override string DisplayText => string.Format("{0}={1}", (object) this.Name, (object) this.Value);

    public override object GetPreviewItem(bool includeAllLevels, int level = 0) => (object) new Condition()
    {
      Name = this.Name,
      Value = this.Value
    };

    protected override bool HasErrors => this.ValueHasErrors && (this.IsHideAble && (this.IsDirty || this.IsNewItem) || !this.IsHideAble) || base.HasErrors;

    protected override void ValidateItem()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        bool includChildren = false;
        WPTarget parent = this.Parent.Parent as WPTarget;
        Condition previewItem1 = this.GetPreviewItem(includChildren, 0) as Condition;
        ImageCustomizations previewItem2 = (this.ListRoot as WPImageCustomization).GetPreviewItem(true, 0) as ImageCustomizations;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifyConditionName(previewItem1, parent.Target).ToList<CustomizationError>(), (WPListItemBase) this);
        base.ValidateItem();
        WPErrors wpErrors = new WPErrors(CustomContentGenerator.VerifyConditionValue(previewItem1, parent.Target, previewItem2).ToList<CustomizationError>(), (WPListItemBase) this);
        this.ValueHasWarnings = wpErrors.HasWarnings;
        this.ValueHasErrors = wpErrors.HasErrors;
        this.ValueWarningsToolTip = wpErrors.WarningsMessage;
        this.ValueErrorsToolTip = wpErrors.ErrorsMessage;
        this._additionalIssues.Add(wpErrors);
      }
      else
        base.ValidateItem();
    }

    protected override string PathPart => this.Name;

    private bool _isNameDirty => this.IsNewItem || !this.Name.Equals(this.Condition.Name);

    private bool _isValueDirty => this.IsNewItem || !this.Value.Equals(this.Condition.Value);

    protected override void UpdateIsDirty()
    {
      if (this._isNameDirty || this._isValueDirty || this.ChildListChanged)
        this._isDirty = true;
      else
        this._isDirty = false;
    }

    public override void UpdateFontSettings()
    {
      this.NameForeground = !this.HasValidationErrors ? (!this._isNameDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      this.ValueForeground = !this.ValueHasErrors ? (!this._isValueDirty ? WPListItemBase.DefaultForegroundColor : WPListItemBase.ModifiedForegroundColor) : WPListItemBase.ErrorForegroundColor;
      base.UpdateFontSettings();
    }

    public string NameForeground
    {
      get => (string) this.GetValue(WPCondition.NameForegroundProperty);
      set => this.SetValue(WPCondition.NameForegroundProperty, (object) value);
    }

    public string ValueForeground
    {
      get => (string) this.GetValue(WPCondition.ValueForegroundProperty);
      set => this.SetValue(WPCondition.ValueForegroundProperty, (object) value);
    }

    public bool ValueHasWarnings
    {
      get => (bool) this.GetValue(WPCondition.ValueHasWarningsProperty);
      set => this.SetValue(WPCondition.ValueHasWarningsProperty, (object) value);
    }

    public bool ValueHasErrors
    {
      get => (bool) this.GetValue(WPCondition.ValueHasErrorsProperty);
      set => this.SetValue(WPCondition.ValueHasErrorsProperty, (object) value);
    }

    public string ValueWarningsToolTip
    {
      get => (string) this.GetValue(WPCondition.ValueWarningsToolTipProperty);
      set => this.SetValue(WPCondition.ValueWarningsToolTipProperty, (object) value);
    }

    public string ValueErrorsToolTip
    {
      get => (string) this.GetValue(WPCondition.ValueErrorsToolTipProperty);
      set => this.SetValue(WPCondition.ValueErrorsToolTipProperty, (object) value);
    }

    public static WPListItemBase AddNewItem(
      WPListItemBase parent,
      string name,
      string value)
    {
      bool isIncludedInOutput = false;
      WPCondition newItem = new WPCondition(new Condition(name, value), isIncludedInOutput, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      return (WPListItemBase) newItem;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.Condition.Name = this.Name;
        this.Condition.Value = this.Value;
      }
      return base.SaveItem();
    }

    public string Name
    {
      get => (string) this.GetValue(WPCondition.NameProperty);
      set => this.SetValue(WPCondition.NameProperty, (object) value);
    }

    public string Value
    {
      get => (string) this.GetValue(WPCondition.ValueProperty);
      set => this.SetValue(WPCondition.ValueProperty, (object) value);
    }
  }
}
