// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.TooltipBehavior
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.Windows;
using System.Windows.Data;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class TooltipBehavior
  {
    public static readonly DependencyProperty AutoTooltipProperty = DependencyProperty.RegisterAttached("AutoTooltip", typeof (bool), typeof (TooltipBehavior), (PropertyMetadata) new FrameworkPropertyMetadata((object) false, new PropertyChangedCallback(TooltipBehavior.OnAutoTooltip)));

    public static void SetAutoTooltip(FrameworkElement element, bool value) => element.SetValue(TooltipBehavior.AutoTooltipProperty, (object) value);

    public static bool GetAutoTooltip(FrameworkElement element) => (bool) element.GetValue(TooltipBehavior.AutoTooltipProperty);

    public static void OnAutoTooltip(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
    {
      if (!(depObj is FrameworkElement target) || string.IsNullOrWhiteSpace(target.Name) || !(e.NewValue is bool))
        return;
      if ((bool) e.NewValue)
      {
        if (BindingOperations.GetBinding((DependencyObject) target, FrameworkElement.ToolTipProperty) != null)
          return;
        target.SetBinding(FrameworkElement.ToolTipProperty, (BindingBase) new Binding()
        {
          Source = (object) TooltipStrings.Instance,
          Converter = (IValueConverter) new TooltipConverter(),
          ConverterParameter = (object) target
        });
      }
      else
        BindingOperations.ClearBinding((DependencyObject) target, FrameworkElement.ToolTipProperty);
    }
  }
}
