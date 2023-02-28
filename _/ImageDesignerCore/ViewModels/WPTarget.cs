// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPTarget
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPTarget : WPListItemBase
  {
    public Target Target;
    public Variant Variant;
    public TargetRef TargetRef;
    private bool _savedOnce;

    public WPTarget(Target target, bool isIncludedInOutput, WPListItemBase parent, bool isNewItem = false)
      : base(parent)
    {
      this.Target = target;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.IsNewItem = isNewItem;
      this.ToolTip = Tools.GetString("ttCOSTarget");
      ObservableCollection<WPListItemBase> collection = new ObservableCollection<WPListItemBase>();
      int num = 1;
      foreach (TargetState targetState in this.Target.TargetStates)
      {
        string name = string.Format(Tools.GetString("txtCOSTargetName"), (object) num++);
        collection.Add((WPListItemBase) new WPTargetState(targetState, isIncludedInOutput, name, (WPListItemBase) this, this.IsNewItem));
      }
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) collection);
      this.InitializationComplete();
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/folder_closed_32xMD.png";

    public override string DisplayText => Tools.GetString("txtCOSTargets");

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      Target previewItem1 = new Target();
      previewItem1.TargetStates = new List<TargetState>();
      previewItem1.DefinedInFile = this.Target.DefinedInFile;
      previewItem1.Id = this.Target.Id;
      if (this.Children != null && (includeAllLevels || !includeAllLevels && level <= 1))
      {
        ++level;
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          TargetState previewItem2 = child.GetPreviewItem(includeAllLevels, level) as TargetState;
          previewItem1.TargetStates.Add(previewItem2);
        }
      }
      return (object) previewItem1;
    }

    public static WPTarget AddNewItem(
      string id,
      Dictionary<string, string> targetValues,
      WPListItemBase parent)
    {
      bool isIncludedInOutput = false;
      bool isNewItem = true;
      Target target = new Target();
      target.Id = id;
      target.TargetStates = new List<TargetState>();
      TargetState targetState = new TargetState();
      foreach (string key in targetValues.Keys)
        targetState.Items.Add(new Condition(key, targetValues[key]));
      target.TargetStates.Add(targetState);
      WPTarget newItem = new WPTarget(target, isIncludedInOutput, parent, isNewItem);
      return WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem) as WPTarget;
    }

    protected override void ValidateItem()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        bool includChildren = false;
        bool flag = false;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifyTargetList(this.GetPreviewItem(includChildren, 0) as Target, flag).ToList<CustomizationError>(), (WPListItemBase) this);
      }
      base.ValidateItem();
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty || !this._savedOnce)
      {
        this._savedOnce = true;
        this.Target.TargetStates.Clear();
        if (this.Children != null)
        {
          foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
          {
            WPTargetState wpTargetState = child as WPTargetState;
            if (wpTargetState.TargetState != null && wpTargetState.TargetState.Items != null && ((IEnumerable<Condition>) wpTargetState.TargetState.Items).Count<Condition>() != 0)
              this.Target.TargetStates.Add(wpTargetState.TargetState);
          }
        }
      }
      return base.SaveItem();
    }
  }
}
