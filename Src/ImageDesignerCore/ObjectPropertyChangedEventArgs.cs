// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ObjectPropertyChangedEventArgs
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.ComponentModel;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class ObjectPropertyChangedEventArgs : PropertyChangedEventArgs
  {
    private object _item;

    public ObjectPropertyChangedEventArgs(object item, string propertyName)
      : base(propertyName)
    {
      this._item = item;
    }

    public object Item => this._item;
  }
}
