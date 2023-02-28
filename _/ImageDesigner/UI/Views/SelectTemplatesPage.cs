// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.SelectTemplatesPage
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using Microsoft.WindowsPhone.ImageDesigner.UI.Common;
using Microsoft.WindowsPhone.ImageDesigner.UI.Controls;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public class SelectTemplatesPage : UserControl, IComponentConnector
  {
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageDesc;
    internal TextBlock tbSelectTemplate;
    internal TextBlock tbSelectTemplatePriorityOrder;
    internal CheckListBox lbFrom;
    internal Button bAdd;
    internal Button bRemove;
    internal CheckListBox lbTo;
    internal TextBlock tbViewMergedLink;
    internal Hyperlink hlViewMerged;
    internal Image bMoveUp;
    internal Image bMoveDown;
    private bool _contentLoaded;

    public SelectTemplatesPage()
    {
      this.InitializeComponent();
      this.EnableDisablePriorityButtons();
      this.EnableDisableAddRemoveButtons();
      Run run = new Run(Tools.GetString("hlViewMergedTemplates"));
      this.hlViewMerged.Inlines.Clear();
      this.hlViewMerged.Inlines.Add((Inline) run);
    }

    private void MoveDownButtonClick(object sender, RoutedEventArgs e)
    {
      if (this.lbTo.SelectedItems.Count != 1)
        return;
      (this.DataContext as SelectTemplatesPageVM).MoveDown();
      this.EnableDisablePriorityButtons();
    }

    private void EnableDisablePriorityButtons()
    {
      if (this.lbTo.SelectedItems.Count == 1)
      {
        if (this.lbTo.SelectedIndex == this.lbTo.Items.Count - 1)
          this.bMoveDown.Visibility = Visibility.Hidden;
        else
          this.bMoveDown.Visibility = Visibility.Visible;
        if (this.lbTo.SelectedIndex == 0)
          this.bMoveUp.Visibility = Visibility.Hidden;
        else
          this.bMoveUp.Visibility = Visibility.Visible;
      }
      else
      {
        this.bMoveDown.Visibility = Visibility.Hidden;
        this.bMoveUp.Visibility = Visibility.Hidden;
      }
    }

    private void ToListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      this.EnableDisablePriorityButtons();
      this.EnableDisableAddRemoveButtons();
    }

    private void EnableDisableAddRemoveButtons()
    {
      if (this.lbTo.SelectedItems.Count == 0)
        this.bRemove.IsEnabled = false;
      else
        this.bRemove.IsEnabled = true;
      if (this.lbFrom.SelectedItems.Count == 0)
        this.bAdd.IsEnabled = false;
      else
        this.bAdd.IsEnabled = true;
    }

    private void FromListSelectionChanged(object sender, SelectionChangedEventArgs e) => this.EnableDisableAddRemoveButtons();

    private void AddButtonClick(object sender, RoutedEventArgs e)
    {
      (this.DataContext as SelectTemplatesPageVM).DoAdd();
      this.EnableDisablePriorityButtons();
      this.EnableDisableAddRemoveButtons();
    }

    private void RemoveButtonClick(object sender, RoutedEventArgs e)
    {
      (this.DataContext as SelectTemplatesPageVM).DoRemove();
      this.EnableDisablePriorityButtons();
      this.EnableDisableAddRemoveButtons();
    }

    private void hlViewMerged_Click(object sender, RoutedEventArgs e)
    {
      if (this.lbTo.Items.Count == 0)
        return;
      Tools.ViewInBrowser((this.DataContext as SelectTemplatesPageVM).GetTempMergedTemplateFile());
    }

    private void MoveUpButtonClick(object sender, MouseButtonEventArgs e)
    {
      if (this.lbTo.SelectedItems.Count != 1)
        return;
      (this.DataContext as SelectTemplatesPageVM).MoveUp();
      this.EnableDisablePriorityButtons();
    }

    private void lbTo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.lbTo.SelectedIndex = -1;

    private void lbFrom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.lbFrom.SelectedIndex = -1;

    private void lb_ToMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      (this.DataContext as SelectTemplatesPageVM).DoRemove();
      this.EnableDisablePriorityButtons();
      this.EnableDisableAddRemoveButtons();
    }

    private void lb_FromMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      (this.DataContext as SelectTemplatesPageVM).DoAdd();
      this.EnableDisablePriorityButtons();
      this.EnableDisableAddRemoveButtons();
    }

    private void HelpButtonClick(object sender, MouseButtonEventArgs e) => HelpProvider.ShowHelp();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/views/selecttemplatespage.xaml", UriKind.Relative));
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
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
          this.tbSelectTemplate = (TextBlock) target;
          break;
        case 5:
          this.tbSelectTemplatePriorityOrder = (TextBlock) target;
          break;
        case 6:
          this.lbFrom = (CheckListBox) target;
          break;
        case 7:
          this.bAdd = (Button) target;
          this.bAdd.Click += new RoutedEventHandler(this.AddButtonClick);
          break;
        case 8:
          this.bRemove = (Button) target;
          this.bRemove.Click += new RoutedEventHandler(this.RemoveButtonClick);
          break;
        case 9:
          this.lbTo = (CheckListBox) target;
          break;
        case 10:
          this.tbViewMergedLink = (TextBlock) target;
          break;
        case 11:
          this.hlViewMerged = (Hyperlink) target;
          this.hlViewMerged.Click += new RoutedEventHandler(this.hlViewMerged_Click);
          break;
        case 12:
          this.bMoveUp = (Image) target;
          this.bMoveUp.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoveUpButtonClick);
          break;
        case 13:
          this.bMoveDown = (Image) target;
          this.bMoveDown.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoveDownButtonClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
