// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.FileExistsConverter
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class FileExistsConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool flag = false;
      string path = value as string;
      if (!string.IsNullOrWhiteSpace(path))
        flag = LongPathFile.Exists(path);
      return (object) flag;
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
