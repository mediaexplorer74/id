// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPTargetState
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPTargetState : WPListItemBase
  {
    public TargetState TargetState;
    private string _name;

    public WPTargetState(
      TargetState targetState,
      bool isIncludedInOutput,
      string name,
      WPListItemBase parent,
      bool isNewItem = false)
      : base(parent)
    {
      this.TargetState = targetState;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.IsNewItem = isNewItem;
      this.ToolTip = Tools.GetString("ttCOSTarget");
      this._name = name;
      ObservableCollection<WPListItemBase> collection = new ObservableCollection<WPListItemBase>();
      foreach (Condition condition in targetState.Items)
        collection.Add((WPListItemBase) new WPCondition(condition, isIncludedInOutput, (WPListItemBase) this, this.IsNewItem));
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) collection);
      this.InitializationComplete();
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/conditions_5855.png";

    public override string DisplayText => this._name;

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      TargetState previewItem1 = new TargetState();
      previewItem1.Items = new List<Condition>();
      if (this.Children != null && (includeAllLevels || !includeAllLevels && level <= 1))
      {
        ++level;
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          Condition previewItem2 = child.GetPreviewItem(includeAllLevels, level) as Condition;
          previewItem1.Items.Add(previewItem2);
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
        WPTarget parent = this.Parent as WPTarget;
        TargetState previewItem1 = this.GetPreviewItem(includChildren, 0) as TargetState;
        ImageCustomizations previewItem2 = (this.ListRoot as WPImageCustomization).GetPreviewItem(true, 0) as ImageCustomizations;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifyConditions(previewItem1, parent.Target, previewItem2, flag).ToList<CustomizationError>(), (WPListItemBase) this);
      }
      base.ValidateItem();
    }

    protected override string PathPart => this.DisplayText;

    public List<string> ConditionNames
    {
      get
      {
        List<string> conditionNames = new List<string>();
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          {
            WPCondition wpCondition = child as WPCondition;
            conditionNames.Add(wpCondition.Name);
          }
        }
        return conditionNames;
      }
    }

    public static WPListItemBase AddNewItem(WPListItemBase parent)
    {
      bool isIncludedInOutput = false;
      TargetState targetState = new TargetState();
      targetState.Items = new List<Condition>();
      int num1 = 1;
      string format = Tools.GetString("txtCOSTargetName");
      int num2 = num1;
      int num3 = num2 + 1;
      // ISSUE: variable of a boxed type
      __Boxed<int> local = (ValueType) num2;
      string newName = string.Format(format, (object) local);
      if (parent.Children != null && parent.Children.Count<WPListItemBase>() != 0)
      {
        while (parent.Children.ToList<WPListItemBase>().Find((Predicate<WPListItemBase>) (child => child.DisplayText.Equals(newName))) != null)
          newName = string.Format(Tools.GetString("txtCOSTargetName"), (object) num3++);
      }
      WPTargetState newItem = new WPTargetState(targetState, isIncludedInOutput, newName, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      return (WPListItemBase) newItem;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
      {
        this.TargetState.Items.Clear();
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          {
            WPCondition wpCondition = child as WPCondition;
            if (wpCondition.Condition != null)
              this.TargetState.Items.Add(wpCondition.Condition);
          }
        }
      }
      return base.SaveItem();
    }
  }
}
