// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Controls.CheckListBox
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Controls
{
  public partial class CheckListBox :  UserControl 
  {
    public static readonly DependencyProperty 
            ItemsSourceProperty = DependencyProperty.Register(nameof (ItemsSource), typeof (IEnumerable), typeof (CheckListBox), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty 
            DisplayNameProperty = DependencyProperty.Register(nameof (DisplayName), typeof (string), typeof (CheckListBox), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty 
            SecondColumnTextProperty = DependencyProperty.Register(nameof (SecondColumnText), typeof (string), typeof (CheckListBox), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty 
            ShowSecondColumnTextProperty = DependencyProperty.Register(nameof (ShowSecondColumnText), typeof (bool), typeof (CheckListBox), new PropertyMetadata((object) false));
    public static readonly DependencyProperty 
            HeaderProperty = DependencyProperty.Register(nameof (Header), typeof (string), typeof (CheckListBox), new PropertyMetadata((object) nameof (Header)));
    public static readonly DependencyProperty 
            ShowCheckBoxProperty = DependencyProperty.Register(nameof (ShowCheckBox), typeof (bool), typeof (CheckListBox), new PropertyMetadata((object) true));
    
    //internal CheckListBox This;
    //internal ListBox lb;
    //private bool _contentLoaded;

    public CheckListBox()
    {
      this.InitializeComponent();
      this.BorderThickness = new Thickness(1.0);
    }

    [Bindable(true)]
    public IEnumerable ItemsSource
    {
      get => (IEnumerable) this.GetValue(CheckListBox.ItemsSourceProperty);
      set => this.SetValue(CheckListBox.ItemsSourceProperty, (object) value);
    }

    [Bindable(true)]
    public string DisplayName
    {
      get => (string) this.GetValue(CheckListBox.DisplayNameProperty);
      set => this.SetValue(CheckListBox.DisplayNameProperty, (object) value);
    }

    [Bindable(true)]
    public string SecondColumnText
    {
      get => (string) this.GetValue(CheckListBox.SecondColumnTextProperty);
      set => this.SetValue(CheckListBox.SecondColumnTextProperty, (object) value);
    }

    public bool ShowSecondColumnText
    {
      get => (bool) this.GetValue(CheckListBox.ShowSecondColumnTextProperty);
      set => this.SetValue(CheckListBox.ShowSecondColumnTextProperty, (object) value);
    }

    public string Header
    {
      get => (string) this.GetValue(CheckListBox.HeaderProperty);
      set => this.SetValue(CheckListBox.HeaderProperty, (object) value);
    }

    public event SelectionChangedEventHandler SelectionChanged;

    public int SelectedIndex
    {
      get => this.lb.SelectedIndex;
      set => this.lb.SelectedIndex = value;
    }

    public IList SelectedItems => this.lb.SelectedItems;

    public IList Items => (IList) this.lb.Items;

    public bool ShowCheckBox
    {
      get => (bool) this.GetValue(CheckListBox.ShowCheckBoxProperty);
      set => this.SetValue(CheckListBox.ShowCheckBoxProperty, (object) value);
    }

    private void c_Checked(object sender, RoutedEventArgs e)
    {
      if (!(sender is CheckBox checkBox) || this.SelectionChanged == null)
        return;
      ISelectable dataContext = checkBox.DataContext as ISelectable;
      List<ISelectable> addedItems = new List<ISelectable>()
      {
        dataContext
      };
      List<ISelectable> removedItems = new List<ISelectable>();
      this.lb.SelectedItems.Add((object) dataContext);
      if (this.PropertyChanged != null)
        this.PropertyChanged((object) this, new PropertyChangedEventArgs("SelectedItems"));
      this.SelectionChanged((object) this.lb, new SelectionChangedEventArgs(e.RoutedEvent, (IList) removedItems, (IList) addedItems));
    }

    private void c_Unchecked(object sender, RoutedEventArgs e)
    {
      if (!(sender is CheckBox checkBox) || this.SelectionChanged == null)
        return;
      ISelectable dataContext = checkBox.DataContext as ISelectable;
      List<ISelectable> addedItems = new List<ISelectable>();
      List<ISelectable> removedItems = new List<ISelectable>()
      {
        dataContext
      };
      this.lb.SelectedItems.Remove((object) dataContext);
      if (this.PropertyChanged != null)
        this.PropertyChanged((object) this, new PropertyChangedEventArgs("SelectedItems"));
      this.SelectionChanged((object) this.lb, new SelectionChangedEventArgs(e.RoutedEvent, (IList) removedItems, (IList) addedItems));
    }

    private void c_Loaded(object sender, RoutedEventArgs e)
    {
      object dataContext = (sender as CheckBox).DataContext;
    }

    private void t_Loaded(object sender, RoutedEventArgs e)
    {
      TextBlock textBlock = sender as TextBlock;
      INotifyPropertyChanged dataContext = textBlock.DataContext as INotifyPropertyChanged;
      textBlock.SetBinding(TextBlock.TextProperty, (BindingBase) new Binding()
      {
        Source = (object) dataContext,
        Path = new PropertyPath(this.SecondColumnText, new object[0])
      });
    }

    private void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.SelectionChanged == null)
        return;
      foreach (ISelectable addedItem in (IEnumerable) e.AddedItems)
        addedItem.IsSelected = true;
      foreach (ISelectable removedItem in (IEnumerable) e.RemovedItems)
        removedItem.IsSelected = false;
      this.SelectionChanged((object) this, e);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void lb_Loaded(object sender, RoutedEventArgs e) => ((INotifyCollectionChanged) this.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(this.CheckListBox_CollectionChanged);

    private void CheckListBox_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.NewItems == null)
        return;
      foreach (ISelectable newItem in (IEnumerable) e.NewItems)
      {
        if (newItem.IsSelected)
          this.lb.SelectedItems.Add((object) newItem);
      }
    }

    private void lb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is ListBoxItem listBoxItem && listBoxItem.DataContext is ISelectable dataContext)
      {
        dataContext.IsSelected = true;
        this.lb.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateSource();
      }
      e.Handled = true;
    }

        /*
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/controls/checklistbox.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.This = (CheckListBox) target;
          break;
        case 2:
          this.lb = (ListBox) target;
          this.lb.SelectionChanged += new SelectionChangedEventHandler(this.lb_SelectionChanged);
          this.lb.Loaded += new RoutedEventHandler(this.lb_Loaded);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 3:
          ((Style) target).Setters.Add((SetterBase) new EventSetter()
          {
            Event = Control.MouseDoubleClickEvent,
            Handler = (Delegate) new MouseButtonEventHandler(this.lb_MouseDoubleClick)
          });
          break;
        case 4:
          ((ToggleButton) target).Checked += new RoutedEventHandler(this.c_Checked);
          ((ToggleButton) target).Unchecked += new RoutedEventHandler(this.c_Unchecked);
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.c_Loaded);
          break;
        case 5:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.t_Loaded);
          break;
      }
    }
        */
  }
}
