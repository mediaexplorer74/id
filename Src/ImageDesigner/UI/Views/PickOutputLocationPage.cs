// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.PickOutputLocationPage
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public partial class PickOutputLocationPage : System.Windows.Controls.UserControl//, IComponentConnector
  {
    //internal TextBlock tbPageTitle;
    //internal TextBlock tbPageDesc;
    //internal System.Windows.Controls.RadioButton rbPickOutputLoc_NewLoc;
    //internal TextBlock tbPickOutputLoc_NewLoc;
    //internal System.Windows.Controls.TextBox tbCurrentOutputLoc;
    //internal TextBlock tbChangeOutputLoc;
    //internal Hyperlink hlChangeOutputLoc;
    //internal System.Windows.Controls.RadioButton rbPickOutputLoc_Overwrite;
    //internal TextBlock tbPickOutputLoc_Overwrite;
    //private bool _contentLoaded;

    public PickOutputLocationPage()
    {
      this.InitializeComponent();
      Run run = new Run(Tools.GetString("hlPickOutputLocationSelect"));
      this.hlChangeOutputLoc.Inlines.Clear();
      this.hlChangeOutputLoc.Inlines.Add((Inline) run);
    }

    private void hlChangeOutputLoc_Click(object sender, RoutedEventArgs e)
    {
      FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
      folderBrowserDialog.ShowNewFolderButton = true;
      folderBrowserDialog.SelectedPath = this.DataContext is PickOutputLocationPageVM dataContext ? dataContext.OutputLocation : throw new ArgumentException("DataContext object is null");
      if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
        return;
      dataContext.OutputLocation = folderBrowserDialog.SelectedPath + "\\";
    }

    private void tbCurrentOutputLoc_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.tbCurrentOutputLoc.SelectAll();

    private void tbCurrentOutputLoc_MenuItem_Click(object sender, RoutedEventArgs e) => Tools.CopyTextToClipboard(this.tbCurrentOutputLoc.SelectedText);

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => Microsoft.WindowsPhone.ImageDesigner.UI.Common.HelpProvider.ShowHelp();

        /*
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/pickoutputlocationpage.xaml", UriKind.Relative));
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
          this.rbPickOutputLoc_NewLoc = (System.Windows.Controls.RadioButton) target;
          break;
        case 5:
          this.tbPickOutputLoc_NewLoc = (TextBlock) target;
          break;
        case 6:
          this.tbCurrentOutputLoc = (System.Windows.Controls.TextBox) target;
          this.tbCurrentOutputLoc.MouseDoubleClick += new MouseButtonEventHandler(this.tbCurrentOutputLoc_MouseDoubleClick);
          break;
        case 7:
          ((System.Windows.Controls.MenuItem) target).Click += new RoutedEventHandler(this.tbCurrentOutputLoc_MenuItem_Click);
          break;
        case 8:
          this.tbChangeOutputLoc = (TextBlock) target;
          break;
        case 9:
          this.hlChangeOutputLoc = (Hyperlink) target;
          this.hlChangeOutputLoc.Click += new RoutedEventHandler(this.hlChangeOutputLoc_Click);
          break;
        case 10:
          this.rbPickOutputLoc_Overwrite = (System.Windows.Controls.RadioButton) target;
          break;
        case 11:
          this.tbPickOutputLoc_Overwrite = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
        */
  }
}
