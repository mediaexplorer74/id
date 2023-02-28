// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.GettingStartedPage
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
  public class GettingStartedPage : UserControl, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal RadioButton rbSelectOption_CreateImage;
    internal TextBlock tbSelectOption_CreateImage;
    internal RadioButton rbSelectOption_ModifyImage;
    internal TextBlock tbSelectOption_ModifyImage;
    internal RadioButton rbSelectOption_FlashImage;
    internal TextBlock tbSelectOption_FlashImage;
    private bool _contentLoaded;

    public GettingStartedPage() => this.InitializeComponent();

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp();

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/gettingstartedpage.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
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
          this.rbSelectOption_CreateImage = (RadioButton) target;
          break;
        case 5:
          this.tbSelectOption_CreateImage = (TextBlock) target;
          break;
        case 6:
          this.rbSelectOption_ModifyImage = (RadioButton) target;
          break;
        case 7:
          this.tbSelectOption_ModifyImage = (TextBlock) target;
          break;
        case 8:
          this.rbSelectOption_FlashImage = (RadioButton) target;
          break;
        case 9:
          this.tbSelectOption_FlashImage = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
