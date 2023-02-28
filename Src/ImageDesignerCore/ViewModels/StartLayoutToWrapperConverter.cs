// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.StartLayoutToWrapperConverter
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  [ValueConversion(typeof (StartLayout), typeof (string))]
  public class StartLayoutToWrapperConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (object) new EnumWrapper(typeof (StartLayout), ((StartLayout) value).ToString());

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      StartLayout result;
      Enum.TryParse<StartLayout>(((EnumWrapper) value).ValueName, out result);
      return (object) result;
    }
  }
}
