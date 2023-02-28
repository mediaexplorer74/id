// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.BuildSuccessPage
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using Microsoft.WindowsPhone.ImageDesigner.UI.Common;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public class BuildSuccessPage : UserControl, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal TextBlock tbImageLoc;
    internal TextBlock tbImageLocValue;
    internal TextBlock tbViewLogLink;
    internal Hyperlink hlViewLog;
    internal TextBlock tbNote;
    internal TextBlock tbNoteText;
    internal TextBlock tbInstr;
    internal TextBlock tbInstrLink;
    internal Hyperlink hlViewImageOptionsSummary;
    private bool _contentLoaded;

    public BuildSuccessPage()
    {
      this.InitializeComponent();
      Run run1 = new Run(Tools.GetString("hlBuildSuccessViewLog"));
      this.hlViewLog.Inlines.Clear();
      this.hlViewLog.Inlines.Add((Inline) run1);
      Run run2 = new Run(Tools.GetString("hlBuildSuccessViewImageOptionsSummary"));
      this.hlViewImageOptionsSummary.Inlines.Clear();
      this.hlViewImageOptionsSummary.Inlines.Add((Inline) run2);
    }

    private void hlViewSummary_Click(object sender, RoutedEventArgs e) => HelpProvider.ShowHelp("RetailSigning");

    private void hlViewLog_Click(object sender, RoutedEventArgs e) => (this.DataContext as BuildSuccessPageVM).ViewLogCommand.Execute((object) this);

    private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
    {
    }

    private void BuildSuccessPage_Loaded_1(object sender, RoutedEventArgs e)
    {
      this.tbImageLocValue.DataContext = (object) (this.DataContext as BuildSuccessPageVM).Context;
      Binding binding = new Binding("FFUPath");
      this.tbImageLocValue.SetBinding(TextBlock.TextProperty, (BindingBase) binding);
    }

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/buildsuccesspage.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.BuildSuccessPage_Loaded_1);
          break;
        case 2:
          this.tbPageTitle = (TextBlock) target;
          break;
        case 3:
          this.tbPageDesc = (TextBlock) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HelpButtonClick);
          break;
        case 5:
          this.tbImageLoc = (TextBlock) target;
          break;
        case 6:
          this.tbImageLocValue = (TextBlock) target;
          break;
        case 7:
          this.tbViewLogLink = (TextBlock) target;
          break;
        case 8:
          this.hlViewLog = (Hyperlink) target;
          this.hlViewLog.Click += new RoutedEventHandler(this.hlViewLog_Click);
          break;
        case 9:
          this.tbNote = (TextBlock) target;
          break;
        case 10:
          this.tbNoteText = (TextBlock) target;
          break;
        case 11:
          this.tbInstr = (TextBlock) target;
          break;
        case 12:
          this.tbInstrLink = (TextBlock) target;
          break;
        case 13:
          this.hlViewImageOptionsSummary = (Hyperlink) target;
          this.hlViewImageOptionsSummary.Click += new RoutedEventHandler(this.hlViewSummary_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
