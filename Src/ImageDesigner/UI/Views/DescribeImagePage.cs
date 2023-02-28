// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.DescribeImagePage
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.UI.Common;
using Microsoft.WindowsPhone.ImageDesigner.UI.Controls;
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
  public class DescribeImagePage : UserControl, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal TextBlock tbSelectedImage;
    internal TextBlock tbPageDesc2;
    internal TextBlock tbDescribe1;
    internal TextBlock tbDescribe2;
    internal TextBox tbxDescription;
    internal TextBlock tbPickLanguage;
    internal CheckListBox cbUserInterfaceLanguages;
    internal ComboBox cmbBootLanguage;
    internal ComboBox cmbBootLocale;
    internal CheckListBox cbKeyboardLanguages;
    internal CheckListBox cbSpeechLanguages;
    private bool _contentLoaded;

    public DescribeImagePage() => this.InitializeComponent();

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/describeimagepage.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
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
          this.tbSelectedImage = (TextBlock) target;
          break;
        case 4:
          this.tbPageDesc2 = (TextBlock) target;
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HelpButtonClick);
          break;
        case 6:
          this.tbDescribe1 = (TextBlock) target;
          break;
        case 7:
          this.tbDescribe2 = (TextBlock) target;
          break;
        case 8:
          this.tbxDescription = (TextBox) target;
          break;
        case 9:
          this.tbPickLanguage = (TextBlock) target;
          break;
        case 10:
          this.cbUserInterfaceLanguages = (CheckListBox) target;
          break;
        case 11:
          this.cmbBootLanguage = (ComboBox) target;
          break;
        case 12:
          this.cmbBootLocale = (ComboBox) target;
          break;
        case 13:
          this.cbKeyboardLanguages = (CheckListBox) target;
          break;
        case 14:
          this.cbSpeechLanguages = (CheckListBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
