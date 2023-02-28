// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.CustomizationChoicePage
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
  public class CustomizationChoicePage : UserControl, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal RadioButton rbSelectOption_BuildImage;
    internal TextBlock tbSelectOption_BuildImage;
    internal RadioButton rbSelectOption_selectTemplates;
    internal TextBlock tbSelectOption_selectTemplates;
    internal RadioButton rbSelectOption_customizeOS;
    internal TextBlock tbSelectOption_customizeOS;
    private bool _contentLoaded;

    public CustomizationChoicePage() => this.InitializeComponent();

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/customizationchoicepage.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
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
          this.rbSelectOption_BuildImage = (RadioButton) target;
          break;
        case 5:
          this.tbSelectOption_BuildImage = (TextBlock) target;
          break;
        case 6:
          this.rbSelectOption_selectTemplates = (RadioButton) target;
          break;
        case 7:
          this.tbSelectOption_selectTemplates = (TextBlock) target;
          break;
        case 8:
          this.rbSelectOption_customizeOS = (RadioButton) target;
          break;
        case 9:
          this.tbSelectOption_customizeOS = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
