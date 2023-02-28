// App.xaml.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.App
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Microsoft.WindowsPhone.ImageDesigner.UI
{
  public partial class App : Application  {
             

    private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        App.GetParentWindow(sender as DependencyObject)?.DragMove();
    }

    private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
    {
        App.GetParentWindow(sender as DependencyObject)?.Close();
    }

    private void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
    {
      Window parentWindow = App.GetParentWindow(sender as DependencyObject);
      if (parentWindow == null)
        return;
      if (parentWindow.WindowState == WindowState.Normal)
        parentWindow.WindowState = WindowState.Maximized;
      else
        parentWindow.WindowState = WindowState.Normal;
    }

    private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
    {
      Window parentWindow = App.GetParentWindow(sender as DependencyObject);
      if (parentWindow == null)
        return;
      parentWindow.WindowState = WindowState.Minimized;
    }

    private static Window GetParentWindow(DependencyObject child)
    {
      DependencyObject parent = VisualTreeHelper.GetParent(child);
      if (parent == null)
        return (Window) null;
      return parent is Window window ? window : App.GetParentWindow(parent);
    }

    private void cmbLanguageSelector_Loaded(object sender, RoutedEventArgs e)
    {
      ComboBox comboBox = sender as ComboBox;
      comboBox.DataContext = (object) LocalizationVM.Instance;
      comboBox.DisplayMemberPath = "DisplayString";
    }
     
  }
}
