// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.EnumWrapper
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public struct EnumWrapper
  {
    public string ValueName;
    private string _enumName;

    public EnumWrapper(Type type, string valueName)
    {
      this._enumName = type.Name;
      this.ValueName = valueName;
    }

    public override string ToString()
    {
      string valueName = Tools.GetString("e" + this._enumName + this.ValueName);
      if (string.IsNullOrEmpty(valueName))
        valueName = this.ValueName;
      return valueName;
    }

    public string DisplayText => this.ToString();

    public static List<EnumWrapper> GetEnumList(Type type)
    {
      List<EnumWrapper> enumList = new List<EnumWrapper>();
      foreach (string name in Enum.GetNames(type))
        enumList.Add(new EnumWrapper(type, name));
      return enumList;
    }
  }
}
