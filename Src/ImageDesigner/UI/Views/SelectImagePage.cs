// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.SelectImagePage
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.UI.Common;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public partial class SelectImagePage : UserControl//, IComponentConnector
  {
        /*
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal RadioButton rbSelectOption_TestImage;
    internal TextBlock tbSelectOption_TestImage;
    internal RadioButton rbSelectOption_ProductionImage;
    internal TextBlock tbSelectOption_ProductionImage;
    internal RadioButton rbSelectOption_RetailImage;
    internal TextBlock tbSelectOption_RetailImage;
    internal RadioButton rbSelectOption_RetailManufacturingImage;
    internal TextBlock tbSelectOption_RetailManufacturingImage;
    internal RadioButton rbSelectOption_MMOSImage;
    internal TextBlock tbSelectOption_MMOSImage;
    private bool _contentLoaded;
        */

    public SelectImagePage() => this.InitializeComponent();

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp();

        /*
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/selectimagepage.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.tbPageTitle = (TextBlock) target;
          break;
        case 2:
          this.tbPageDesc = (TextBlock) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HelpButtonClick);
          break;
        case 4:
          this.rbSelectOption_TestImage = (RadioButton) target;
          break;
        case 5:
          this.tbSelectOption_TestImage = (TextBlock) target;
          break;
        case 6:
          this.rbSelectOption_ProductionImage = (RadioButton) target;
          break;
        case 7:
          this.tbSelectOption_ProductionImage = (TextBlock) target;
          break;
        case 8:
          this.rbSelectOption_RetailImage = (RadioButton) target;
          break;
        case 9:
          this.tbSelectOption_RetailImage = (TextBlock) target;
          break;
        case 10:
          this.rbSelectOption_RetailManufacturingImage = (RadioButton) target;
          break;
        case 11:
          this.tbSelectOption_RetailManufacturingImage = (TextBlock) target;
          break;
        case 12:
          this.rbSelectOption_MMOSImage = (RadioButton) target;
          break;
        case 13:
          this.tbSelectOption_MMOSImage = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
        */
  }
}
