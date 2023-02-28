// EnumToBooleanConverter.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.EnumToBooleanConverter
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public partial class EnumToBooleanConverter : IValueConverter
  {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return (object)value.Equals(parameter);
        }

        public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      return !value.Equals((object) true) ? Binding.DoNothing : parameter;
    }
  }
}
