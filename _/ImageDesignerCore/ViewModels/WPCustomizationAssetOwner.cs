// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPCustomizationAssetOwner
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System;
using System.Collections.ObjectModel;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPCustomizationAssetOwner : WPListItemBase
  {
    public CustomizationAssetOwner Value;

    public WPCustomizationAssetOwner(CustomizationAssetOwner value)
      : base((WPListItemBase) null)
    {
      this.Value = value;
    }

    public override string DisplayText => Enum.GetName(typeof (CustomizationAssetOwner), (object) this.Value);

    public static ObservableCollection<string> GetListNames()
    {
      ObservableCollection<string> listNames = new ObservableCollection<string>();
      foreach (string name in Enum.GetNames(typeof (CustomizationAssetOwner)))
        listNames.Add(name);
      return listNames;
    }

    public ObservableCollection<WPCustomizationAssetOwner> GetList(
      CustomizationAssetOwner selectedValue)
    {
      ObservableCollection<WPCustomizationAssetOwner> list = new ObservableCollection<WPCustomizationAssetOwner>();
      foreach (CustomizationAssetOwner customizationAssetOwner1 in Enum.GetValues(typeof (CustomizationAssetOwner)))
      {
        WPCustomizationAssetOwner customizationAssetOwner2 = new WPCustomizationAssetOwner(customizationAssetOwner1);
        if (customizationAssetOwner1 == selectedValue)
          customizationAssetOwner2.IsSelected = true;
        list.Add(customizationAssetOwner2);
      }
      return list;
    }
  }
}
