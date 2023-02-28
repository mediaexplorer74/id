// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPCustomizationAssetType
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System;
using System.Collections.ObjectModel;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPCustomizationAssetType : WPListItemBase
  {
    public CustomizationDataAssetType Value;

    public WPCustomizationAssetType(CustomizationDataAssetType value)
      : base((WPListItemBase) null)
    {
      this.Value = value;
    }

    public override string DisplayText => Enum.GetName(typeof (CustomizationDataAssetType), (object) this.Value);

    public static ObservableCollection<string> GetListNames()
    {
      ObservableCollection<string> listNames = new ObservableCollection<string>();
      foreach (string name in Enum.GetNames(typeof (CustomizationDataAssetType)))
        listNames.Add(name);
      return listNames;
    }

    public static ObservableCollection<WPCustomizationAssetType> GetList(
      CustomizationDataAssetType selectedValue)
    {
      ObservableCollection<WPCustomizationAssetType> list = new ObservableCollection<WPCustomizationAssetType>();
      foreach (CustomizationDataAssetType customizationDataAssetType in Enum.GetValues(typeof (CustomizationDataAssetType)))
      {
        WPCustomizationAssetType customizationAssetType = new WPCustomizationAssetType(customizationDataAssetType);
        if (customizationDataAssetType == selectedValue)
          customizationAssetType.IsSelected = true;
        list.Add(customizationAssetType);
      }
      return list;
    }
  }
}
