// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.ModifyImagePage
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public class ModifyImagePage : System.Windows.Controls.UserControl, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal TextBlock tbExistingProj;
    internal System.Windows.Controls.TextBox tbProjLoc;
    internal TextBlock tbChangeProj;
    internal Hyperlink hlChangeProj;
    private bool _contentLoaded;

    public ModifyImagePage()
    {
      this.InitializeComponent();
      Run run = new Run(Tools.GetString("hlModifyImageChange"));
      this.hlChangeProj.Inlines.Clear();
      this.hlChangeProj.Inlines.Add((Inline) run);
    }

    private void hlChangeProj_Click(object sender, RoutedEventArgs e)
    {
      ModifyImagePageVM dataContext = this.DataContext as ModifyImagePageVM;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.CheckFileExists = true;
      openFileDialog.Multiselect = false;
      openFileDialog.InitialDirectory = Path.GetDirectoryName(dataContext.LastSavedProjectPath);
      openFileDialog.Filter = Tools.GetString("txtWPIDFilter");
      if (openFileDialog.ShowDialog() != DialogResult.OK)
        return;
      try
      {
        IDContext.Instance.Controller.LoadProject(openFileDialog.FileName);
      }
      catch (WPIDException ex)
      {
        CustomMessageBox.ShowError(ex.Message);
        return;
      }
      dataContext.LastSavedProjectPath = openFileDialog.FileName;
    }

    private void tbProjLoc_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.tbProjLoc.SelectAll();

    private void tbProjLoc_MenuItem_Click(object sender, RoutedEventArgs e) => Tools.CopyTextToClipboard(this.tbProjLoc.SelectedText);

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => Microsoft.WindowsPhone.ImageDesigner.UI.Common.HelpProvider.ShowHelp();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/modifyimagepage.xaml", UriKind.Relative));
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
          this.tbExistingProj = (TextBlock) target;
          break;
        case 5:
          this.tbProjLoc = (System.Windows.Controls.TextBox) target;
          this.tbProjLoc.MouseDoubleClick += new MouseButtonEventHandler(this.tbProjLoc_MouseDoubleClick);
          break;
        case 6:
          ((System.Windows.Controls.MenuItem) target).Click += new RoutedEventHandler(this.tbProjLoc_MenuItem_Click);
          break;
        case 7:
          this.tbChangeProj = (TextBlock) target;
          break;
        case 8:
          this.hlChangeProj = (Hyperlink) target;
          this.hlChangeProj.Click += new RoutedEventHandler(this.hlChangeProj_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
