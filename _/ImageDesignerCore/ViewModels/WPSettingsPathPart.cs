// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPSettingsPathPart
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPSettingsPathPart : WPListItemBase
  {
    public List<WPListItemBase> AllChildren;
    public string SettingsPathPart;
    private List<string> _pathParts;

    public WPSettingsPathPart(WPListItemBase child, WPListItemBase parent)
      : base(parent)
    {
      string path = (string) null;
      switch (child)
      {
        case WPSettings _:
          path = (child as WPSettings).Path;
          break;
        case WPSetting _:
          path = (child as WPSetting).Name;
          break;
      }
      this.SettingsPathPart = !(parent is WPSettingsPathPart) ? WPSettingsPathPart.GetPathPart((string) null, path) : WPSettingsPathPart.GetPathPart((parent as WPSettingsPathPart).FullPath, path);
      this.IsIncludedInOutput = child.IsIncludedInOutput;
      this.IsHideAble = child.IsHideAble;
      this.IsNewItem = child.IsNewItem;
      this.AllChildren = new List<WPListItemBase>();
      this.AddPathPartChild(child);
      this.Children = new ObservableCollection<WPListItemBase>(this.AllChildren);
      this.InitializationComplete();
    }

    public string FullPath
    {
      get
      {
        string str = "";
        if (this.Parent is WPSettingsPathPart)
          str = (this.Parent as WPSettingsPathPart).FullPath + "/";
        return str + this.SettingsPathPart;
      }
    }

    public WPListItemBase AddPathPartChild(WPListItemBase child)
    {
      int num;
      switch (child)
      {
        case WPSettings _:
          num = (child as WPSettings).PathParts.Count<string>();
          break;
        case WPSetting _:
          num = (child as WPSetting).PathParts.Count<string>();
          break;
        default:
          return (WPListItemBase) null;
      }
      WPListItemBase wpListItemBase;
      if (this.PathParts.Count<string>() == num)
      {
        child.Parent = (WPListItemBase) this;
        wpListItemBase = child;
      }
      else
        wpListItemBase = (WPListItemBase) new WPSettingsPathPart(child, (WPListItemBase) this);
      this.AllChildren.Add(wpListItemBase);
      return wpListItemBase;
    }

    private static string GetPathPart(string parentPath, string path)
    {
      string pathPart = "";
      List<string> pathParts1 = WPSettingsPathPart.GetPathParts(path);
      if (string.IsNullOrEmpty(parentPath))
      {
        pathPart = pathParts1[0];
      }
      else
      {
        List<string> pathParts2 = WPSettingsPathPart.GetPathParts(parentPath);
        if (pathParts2.Count<string>() != pathParts1.Count<string>())
          pathPart = pathParts1[pathParts2.Count<string>()];
      }
      return pathPart;
    }

    public WPSettingsPathPart FindMatchingChild(
      List<string> pathParts,
      int currentPartIndex = 0)
    {
      WPSettingsPathPart matchingChild1 = (WPSettingsPathPart) null;
      if (this.FindLastMatchingIndex(this.PathParts, pathParts) == this.PathParts.Count<string>() - 1)
      {
        matchingChild1 = this;
        if (this.PathParts.Count<string>() != pathParts.Count<string>())
        {
          foreach (WPListItemBase allChild in this.AllChildren)
          {
            if (allChild is WPSettingsPathPart)
            {
              WPSettingsPathPart settingsPathPart = allChild as WPSettingsPathPart;
              int num = currentPartIndex + 1;
              if (settingsPathPart.PathPart.Equals(pathParts[num], StringComparison.OrdinalIgnoreCase))
              {
                matchingChild1 = settingsPathPart;
                WPSettingsPathPart matchingChild2 = settingsPathPart.InternalFindMatchingChild(pathParts, num);
                if (matchingChild2 != null)
                {
                  matchingChild1 = matchingChild2;
                  break;
                }
              }
            }
          }
        }
      }
      return matchingChild1;
    }

    protected WPSettingsPathPart InternalFindMatchingChild(
      List<string> pathParts,
      int currentPartIndex = 0)
    {
      WPSettingsPathPart matchingChild1 = this;
      if (this.PathParts.Count<string>() != pathParts.Count<string>())
      {
        int num = currentPartIndex + 1;
        foreach (WPListItemBase allChild in this.AllChildren)
        {
          if (allChild is WPSettingsPathPart)
          {
            WPSettingsPathPart settingsPathPart = allChild as WPSettingsPathPart;
            if (settingsPathPart.PathPart.Equals(pathParts[num], StringComparison.OrdinalIgnoreCase))
            {
              matchingChild1 = settingsPathPart;
              WPSettingsPathPart matchingChild2 = settingsPathPart.InternalFindMatchingChild(pathParts, num);
              if (matchingChild2 != null)
              {
                matchingChild1 = matchingChild2;
                break;
              }
            }
          }
        }
      }
      return matchingChild1;
    }

    private int FindLastMatchingIndex(List<string> list1, List<string> list2)
    {
      int lastMatchingIndex = -1;
      int num = Math.Min(list1.Count<string>(), list2.Count<string>());
      for (int index = 0; index < num && list1[index].Equals(list2[index], StringComparison.OrdinalIgnoreCase); ++index)
        lastMatchingIndex = index;
      return lastMatchingIndex;
    }

    public List<string> PathParts
    {
      get
      {
        if (this._pathParts == null)
          this._pathParts = WPSettingsPathPart.GetPathParts(this.FullPath);
        return this._pathParts;
      }
      private set => this._pathParts = value;
    }

    public static List<string> GetPathParts(string path) => ((IEnumerable<string>) path.Split(new char[1]
    {
      '/'
    }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>();

    public List<WPSettings> AllSettingsGroups
    {
      get
      {
        List<WPSettings> allSettingsGroups = new List<WPSettings>();
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
        return allSettingsGroups;
      }
    }

    public List<WPSetting> AllSettings
    {
      get
      {
        List<WPSetting> allSettings = new List<WPSetting>();
        foreach (WPListItemBase allChild in this.AllChildren)
        {
          if (allChild is WPSetting)
          {
            allSettings.Add(allChild as WPSetting);
          }
          else
          {
            WPSettingsPathPart settingsPathPart = allChild as WPSettingsPathPart;
            allSettings.AddRange((IEnumerable<WPSetting>) settingsPathPart.AllSettings);
          }
        }
        return allSettings;
      }
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/folder_closed_32xMD.png";

    public override string DisplayText => this.SettingsPathPart;

    public override bool IsIncludedInOutput
    {
      get => this.AllChildren.Find((Predicate<WPListItemBase>) (child => child.IsIncludedInOutput)) != null || base.IsIncludedInOutput;
      set => base.IsIncludedInOutput = value;
    }

    public override bool IsHideAble
    {
      get => this.AllChildren.Find((Predicate<WPListItemBase>) (child => child.IsHideAble)) != null || base.IsHideAble;
      set => base.IsHideAble = value;
    }

    public void ShowAllItems(bool showHiddenItems)
    {
      List<WPListItemBase> list = new List<WPListItemBase>();
      foreach (WPListItemBase allChild in this.AllChildren)
      {
        if (allChild.IsIncludedInOutput || allChild.IsDirty || allChild.IsNewItem)
          list.Add(allChild);
        if (allChild is WPSettings)
          (allChild as WPSettings).ShowAllItems(showHiddenItems);
        else if (allChild is WPSettingsPathPart)
          (allChild as WPSettingsPathPart).ShowAllItems(showHiddenItems);
      }
      if (showHiddenItems)
        this.Children = new ObservableCollection<WPListItemBase>(this.AllChildren);
      else
        this.Children = new ObservableCollection<WPListItemBase>(list);
    }
  }
}
