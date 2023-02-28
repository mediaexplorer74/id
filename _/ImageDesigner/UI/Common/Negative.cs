// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Common.Negative
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Common
{
  public class Negative : IValueConverter
  {
    public static Negative Instance => new Negative();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is double num)
        return (object) (-1.0 * num);
      throw new ArgumentException();
    }

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      throw new Exception("The method or operation is not implemented.");
    }
  }
}
