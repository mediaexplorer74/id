// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPListItemBase
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.MCSF.Offline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPListItemBase : DependencyObject, INotifyPropertyChanged
  {
    protected static string ModifiedForegroundColor = "Blue";
    protected static string ErrorForegroundColor = "Red";
    protected static string DefaultForegroundColor = "Black";
    public WPListItemBase Parent;
    private bool _isSelectedForListBox;
    private bool _isSelected;
    private bool _IsHideAble;
    private string _treeViewVisiblity = "Visible";
    private bool _isExpanded;
    protected bool _isDirty;
    public static PolicyStore PolicyStore = (PolicyStore) null;
    protected WPErrors _validationIssues;
    protected List<WPErrors> _additionalIssues = new List<WPErrors>();
    public static readonly DependencyProperty HasValidationWarningsProperty = DependencyProperty.Register(nameof (HasValidationWarnings), typeof (bool), typeof (WPListItemBase), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ValidationWarningToolTipProperty = DependencyProperty.Register(nameof (ValidationWarningToolTip), typeof (string), typeof (WPListItemBase), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty ValidationErrorToolTipProperty = DependencyProperty.Register(nameof (ValidationErrorToolTip), typeof (string), typeof (WPListItemBase), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty HasValidationErrorsProperty = DependencyProperty.Register(nameof (HasValidationErrors), typeof (bool), typeof (WPListItemBase), new PropertyMetadata((object) false));
    protected bool _childListChanged;
    protected static CustomizeOSPageVM VM;
    protected bool _initializing = true;
    private string _fontWeight = "Normal";
    private string _foreground = WPListItemBase.DefaultForegroundColor;
    private string _childListForeground = WPListItemBase.DefaultForegroundColor;
    private bool _isIncludedInOutput;

    public virtual void OnPropertyChanged(string propertyName)
    {
      if (this.PropertyChanged == null)
        return;
      this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    public WPListItemBase(WPListItemBase parent) => this.Parent = parent;

    public WPListItemBase GetParentOfType(Type type)
    {
      WPListItemBase parentOfType = this;
      while (parentOfType != null)
      {
        parentOfType = parentOfType.Parent;
        if (parentOfType != null && parentOfType.GetType() == type)
          break;
      }
      return parentOfType;
    }

    public virtual List<WPListItemBase> AllListItems
    {
      get
      {
        List<WPListItemBase> allListItems = new List<WPListItemBase>();
        allListItems.Add(this);
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
            allListItems.AddRange((IEnumerable<WPListItemBase>) child.AllListItems);
        }
        return allListItems;
      }
    }

    public WPListItemBase ListRoot
    {
      get
      {
        WPListItemBase listRoot = this;
        while (listRoot.Parent != null)
          listRoot = listRoot.Parent;
        return listRoot;
      }
    }

    public bool ShowTreeViewIcon => !string.IsNullOrEmpty(this.TreeViewIcon);

    public virtual string TreeViewIcon => "";

    public virtual string DisplayText => "";

    public override string ToString() => this.DisplayText + string.Format(" ( {0} {1})", this.IsHideAble ? (object) "HideAble" : (object) "", this.IsDirty ? (object) "Dirty" : (object) "");

    public virtual string DisplayPath
    {
      get
      {
        string str = "";
        if (this.Parent != null)
          str = this.Parent.DisplayPath;
        if (!string.IsNullOrEmpty(str))
          str += "-";
        return str + this.PathPart;
      }
    }

    protected virtual string PathPart => this.DisplayText;

    public virtual string ToolTip
    {
      get => (string) null;
      set
      {
      }
    }

    public bool IsSelectedForListBox
    {
      get => this._isSelectedForListBox;
      set
      {
        if (value == this._isSelectedForListBox)
          return;
        this._isSelectedForListBox = value;
        this.OnPropertyChanged(nameof (IsSelectedForListBox));
      }
    }

    public bool IsSelected
    {
      get => this._isSelected;
      set
      {
        if (value != this._isSelected)
        {
          this._isSelected = value;
          this.OnPropertyChanged(nameof (IsSelected));
        }
        if (this._isSelected && this.Parent != null)
          this.Parent.IsExpanded = true;
        WPListItemBase parent = this.Parent;
        while (true)
        {
          switch (parent)
          {
            case WPVariant _:
            case null:
              goto label_7;
            default:
              parent = parent.Parent;
              continue;
          }
        }
label_7:
        if (parent == null)
          return;
        parent.IsSelectedForListBox = true;
      }
    }

    public bool IsNewItem { get; set; }

    public virtual bool IsHideAble
    {
      get => this._IsHideAble;
      set => this._IsHideAble = value;
    }

    public string TreeViewVisibilty
    {
      get => this._treeViewVisiblity;
      set
      {
        this.OnPropertyChanged(nameof (TreeViewVisibilty));
        this._treeViewVisiblity = value;
      }
    }

    public bool IsExpanded
    {
      get => this._isExpanded;
      set
      {
        if (value != this._isExpanded)
        {
          this._isExpanded = value;
          this.OnPropertyChanged(nameof (IsExpanded));
        }
        if (this._isExpanded && this.Parent != null)
          this.Parent.IsExpanded = true;
        this.OnExpandedChange(value);
      }
    }

    protected virtual void OnExpandedChange(bool expanded)
    {
    }

    public virtual bool IsDirty
    {
      get
      {
        this.UpdateIsDirty();
        if (this._isDirty || this.ChildListChanged || this.IsNewItem)
          return true;
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          {
            if (child.IsDirty)
              return true;
          }
        }
        return false;
      }
      set
      {
        if (value == this._isDirty)
          return;
        this._isDirty = value;
        this.NotifyChanges();
      }
    }

    public virtual object GetPreviewItem(bool includChildren, int level = 0) => (object) null;

    protected void Validate()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        this.ValidateItem();
        if (this.Children == null)
          return;
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          child.ValidateItem();
      }
      else
      {
        this._additionalIssues.Clear();
        this._validationIssues = (WPErrors) null;
      }
    }

    protected virtual void ValidateItem()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        if (this._validationIssues != null)
        {
          this.HasValidationWarnings = this._validationIssues.HasWarnings;
          this.HasValidationErrors = this._validationIssues.HasErrors;
          this.ValidationWarningToolTip = this._validationIssues.WarningsMessage;
          this.ValidationErrorToolTip = this._validationIssues.ErrorsMessage;
        }
        else
        {
          this.HasValidationWarnings = false;
          this.HasValidationErrors = false;
        }
      }
      else
        this._validationIssues = (WPErrors) null;
      this._additionalIssues.Clear();
    }

    public List<WPErrors> AllErrors => this.AllIssues.Where<WPErrors>((Func<WPErrors, bool>) (issue => issue.HasErrors)).ToList<WPErrors>();

    public List<WPErrors> AllWarnings => this.AllIssues.Where<WPErrors>((Func<WPErrors, bool>) (issue => issue.HasWarnings)).ToList<WPErrors>();

    public virtual List<WPErrors> AllIssues
    {
      get
      {
        List<WPErrors> allIssues = new List<WPErrors>();
        if (this._validationIssues != null)
        {
          if (this._validationIssues.HasErrors || this._validationIssues.HasWarnings)
            allIssues.Add(this._validationIssues);
          allIssues.AddRange((IEnumerable<WPErrors>) this._additionalIssues.Where<WPErrors>((Func<WPErrors, bool>) (list => list.HasErrors || list.HasWarnings)).ToList<WPErrors>());
        }
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          {
            if (child.IsIncludedInOutput || child.IsDirty || child.IsNewItem)
              allIssues.AddRange((IEnumerable<WPErrors>) child.AllIssues);
          }
        }
        return allIssues;
      }
    }

    public bool HasValidationWarnings
    {
      get => (bool) this.GetValue(WPListItemBase.HasValidationWarningsProperty);
      set => this.SetValue(WPListItemBase.HasValidationWarningsProperty, (object) value);
    }

    public string ValidationWarningToolTip
    {
      get => (string) this.GetValue(WPListItemBase.ValidationWarningToolTipProperty);
      set => this.SetValue(WPListItemBase.ValidationWarningToolTipProperty, (object) value);
    }

    public bool HasValidationErrors
    {
      get => (bool) this.GetValue(WPListItemBase.HasValidationErrorsProperty);
      set => this.SetValue(WPListItemBase.HasValidationErrorsProperty, (object) value);
    }

    public string ValidationErrorToolTip
    {
      get => (string) this.GetValue(WPListItemBase.ValidationErrorToolTipProperty);
      set => this.SetValue(WPListItemBase.ValidationErrorToolTipProperty, (object) value);
    }

    public bool ChildListChanged
    {
      get => this._childListChanged;
      set
      {
        this.Validate();
        this._childListChanged = value;
        this.NotifyChanges();
      }
    }

    protected void InitializationComplete()
    {
      this._initializing = false;
      this.ValidateItem();
      this.UpdateFontSettings();
      this.NotifyFontChanges();
      this.OnPropertyChanged("Children");
    }

    public virtual void NotifyChanges()
    {
      if (this._initializing)
        return;
      this.ValidateItem();
      WPListItemBase.VM.CheckForErrors();
      if (WPListItemBase.VM.CurrentItem == this)
        WPListItemBase.VM.CurrentPath = this.DisplayPath;
      this.UpdateFontSettings();
      this.NotifyFontChanges();
      if (this.Parent != null)
        this.Parent.NotifyChanges();
      this.OnPropertyChanged("Children");
    }

    public virtual void NotifyFontChanges()
    {
      this.OnPropertyChanged("FontWeight");
      this.OnPropertyChanged("Foreground");
      this.OnPropertyChanged("DisplayText");
      this.OnPropertyChanged("CurrentPath");
      this.OnPropertyChanged("ChildListForeground");
    }

    protected virtual bool HasErrors
    {
      get
      {
        if (this.HasValidationErrors && (this.IsHideAble && (this.IsDirty || this.IsNewItem) || !this.IsHideAble))
          return true;
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          {
            if ((child.IsNewItem || child.IsDirty || this.IsIncludedInOutput) && child.HasErrors)
              return true;
          }
        }
        return false;
      }
    }

    public virtual void UpdateFontSettings()
    {
      this.FontWeight = !this.IsIncludedInOutput ? "Normal" : "Bold";
      this.Foreground = !this.HasErrors ? (this.IsDirty || this.IsNewItem ? WPListItemBase.ModifiedForegroundColor : WPListItemBase.DefaultForegroundColor) : WPListItemBase.ErrorForegroundColor;
      if (this.ChildListChanged)
        this.ChildListForeground = WPListItemBase.ModifiedForegroundColor;
      else
        this.ChildListForeground = WPListItemBase.DefaultForegroundColor;
    }

    protected virtual void UpdateIsDirty()
    {
    }

    protected static void OnPropertyChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      WPListItemBase wpListItemBase = d as WPListItemBase;
      wpListItemBase.UpdateIsDirty();
      wpListItemBase.NotifyChanges();
    }

    public ObservableCollection<WPListItemBase> Children { get; set; }

    public string FontWeight
    {
      get => this._fontWeight;
      set => this._fontWeight = value;
    }

    public string Foreground
    {
      get => this._foreground;
      set => this._foreground = value;
    }

    public string ChildListForeground
    {
      get => this._childListForeground;
      set => this._childListForeground = value;
    }

    public virtual bool IsIncludedInOutput
    {
      get => this._isIncludedInOutput;
      set => this._isIncludedInOutput = value;
    }

    public virtual bool Save()
    {
      if (this.Children != null)
      {
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          if (child.IsDirty && !child.Save())
            return false;
        }
      }
      return this.SaveItem();
    }

    protected virtual bool SaveItem()
    {
      if (this._isDirty || this.ChildListChanged || this.IsNewItem)
      {
        this.IsIncludedInOutput = true;
        this.IsNewItem = false;
        this.ChildListChanged = false;
      }
      this._isDirty = false;
      this.UpdateIsDirty();
      this.UpdateFontSettings();
      return true;
    }

    public static WPListItemBase AddNewItem(
      WPListItemBase parent,
      WPListItemBase newItem)
    {
      newItem.IsIncludedInOutput = false;
      newItem.IsDirty = true;
      newItem.IsNewItem = true;
      newItem.IsExpanded = true;
      newItem.IsSelected = true;
      parent.AddChild(newItem);
      newItem.NotifyChanges();
      return newItem;
    }

    public virtual bool AddChild(WPListItemBase item)
    {
      if (this.Children == null)
        this.Children = new ObservableCollection<WPListItemBase>();
      item.Parent = this;
      item.IsNewItem = true;
      this.Children.Add(item);
      this.ChildListChanged = true;
      return true;
    }

    public virtual WPListItemBase SelectedChild => this.Children == null || this.Children.Where<WPListItemBase>((Func<WPListItemBase, bool>) (child => child.IsSelectedForListBox)).Count<WPListItemBase>() == 0 ? (WPListItemBase) null : this.Children.First<WPListItemBase>((Func<WPListItemBase, bool>) (child => child.IsSelectedForListBox));

    public bool IsAChildSelected => this.SelectedChild != null;

    public virtual bool RemoveSelectedChild()
    {
      WPListItemBase selectedChild = this.SelectedChild;
      return selectedChild != null && this.RemoveChild(selectedChild);
    }

    protected virtual bool RemoveChild(WPListItemBase item)
    {
      if (this.Children == null || !this.Children.Remove(item))
        return false;
      this.ChildListChanged = true;
      return true;
    }

    public WPVariant CurrentVariant
    {
      get
      {
        WPVariant currentVariant = (WPVariant) null;
        for (WPListItemBase parent = this.Parent; parent != null; parent = parent.Parent)
        {
          if (parent is WPVariant)
          {
            currentVariant = parent as WPVariant;
            break;
          }
        }
        return currentVariant;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
