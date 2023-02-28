// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Common.InverseBooleanToSVConverter
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Common
{
  [ValueConversion(typeof (bool), typeof (ScrollBarVisibility))]
  public class InverseBooleanToSVConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(bool) value ? (object) ScrollBarVisibility.Visible : (object) ScrollBarVisibility.Disabled;

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      ScrollBarVisibility scrollBarVisibility = (ScrollBarVisibility) value;
      return (object) (bool) (scrollBarVisibility == ScrollBarVisibility.Visible ? 0 : (scrollBarVisibility != ScrollBarVisibility.Auto ? 1 : 0));
    }
  }
}
