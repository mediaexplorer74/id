// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.ConstValue`1
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class ConstValue<T>
  {
    public ConstValue(T value) => this.Value = value;

    public T Value { private set; get; }
  }
}
