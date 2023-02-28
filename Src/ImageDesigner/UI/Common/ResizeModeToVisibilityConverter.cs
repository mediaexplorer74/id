// ResizeModeToVisibilityConverter.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Common.ResizeModeToVisibilityConverter
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Common
{
  [ValueConversion(typeof (ResizeMode), typeof (Visibility))]
  public class ResizeModeToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
            => (ResizeMode) value == ResizeMode.NoResize ? (object) Visibility.Collapsed 
            : (object) Visibility.Visible;

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      return (Visibility) value == Visibility.Collapsed 
                ? (object) ResizeMode.NoResize 
                : (object) ResizeMode.CanResize;
    }
  }
}
