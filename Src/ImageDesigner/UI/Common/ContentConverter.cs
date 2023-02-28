// ContentConverter.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Common.ContentConverter
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Common
{
  public class ContentConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      string propName = values[0] as string;
      object obj = (object) null;
      if (((FrameworkElement) values[1]).DataContext is INotifyPropertyChanged dataContext)
      {
        PropertyInfo[] properties = dataContext.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (properties != null)
        {
          PropertyInfo propertyInfo = ((IEnumerable<PropertyInfo>) properties).FirstOrDefault<PropertyInfo>((Func<PropertyInfo, bool>) (p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase)));
          if (propertyInfo != (PropertyInfo) null)
            obj = propertyInfo.GetValue((object) dataContext, (object[]) null);
        }
      }
      return obj;
    }

    public object[] ConvertBack(
      object value,
      Type[] targetTypes,
      object parameter,
      CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
