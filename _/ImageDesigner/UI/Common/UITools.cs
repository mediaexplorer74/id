// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Common.UITools
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using System;
using System.Globalization;
using System.Windows.Controls;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Common
{
  public class UITools
  {
    public static bool IsValidNumber(
      string numberStr,
      int min,
      int max,
      bool skipMinCheck = false,
      bool skipMaxCheck = false)
    {
      bool flag = false;
      if (numberStr.Equals("0x", StringComparison.OrdinalIgnoreCase))
        return true;
      int num;
      try
      {
        if (numberStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
          flag = true;
          num = int.Parse(numberStr.Substring(2), NumberStyles.HexNumber);
        }
        else
          num = int.Parse(numberStr);
      }
      catch
      {
        return false;
      }
      return (flag || num == 0 || numberStr[0] != '0') && (skipMinCheck || num >= min) && (skipMaxCheck || num <= max);
    }

    public static string GetNewTextBasedOnControl(TextBox control, string insertionStr)
    {
      string text = control.Text;
      return control.SelectionLength == 0 ? text.Insert(control.CaretIndex, insertionStr) : text.Remove(control.SelectionStart, control.SelectionLength).Insert(control.SelectionStart, insertionStr);
    }
  }
}
