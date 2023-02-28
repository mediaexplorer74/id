// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.Extensions
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public static class Extensions
  {
    public static void VirtualizedScrollIntoView(
      this ListView control,
      object item,
      bool autoscroll)
    {
      try
      {
        if (!autoscroll)
          return;
        (VisualTreeHelper.GetChild((DependencyObject) control, 0) as ScrollViewer).ScrollToEnd();
      }
      catch (Exception ex)
      {
      }
    }
  }
}
