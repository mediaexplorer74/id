// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.CustomMessageBox
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI
{
  public partial class CustomMessageBox : Window//, IComponentConnector
  {
        /*
    internal TextBlock tbPageTitle;
    internal TextBlock tbPageMessage;
    internal TextBlock tb1;
    internal CheckBox cbDoNotAskAgain;
    internal Button btYes;
    internal Button btNo;
    internal Button btOK;
    private bool _contentLoaded;
        */

    public CustomMessageBox(CustomMessageBoxVM vm)
    {
      this.InitializeComponent();
      this.DataContext = (object) vm;
    }

    public static bool? ShowMessage(string title, string message) 
            => new CustomMessageBox(new CustomMessageBoxVM(CustomDialogType.OKDialog)
    {
      Header = title,
      Message = message,
      Width = 300
    }).ShowDialog();

    public static CustomMessageBoxResult ShowYesNoMessage(
      string title,
      string message)
    {
      CustomMessageBoxVM vm = new CustomMessageBoxVM(CustomDialogType.YesNoDialog);
      vm.Header = title;
      vm.Message = message;
      new CustomMessageBox(vm).ShowDialog();
      return vm.Result;
    }

    public static bool? ShowError(Exception e)
    {
      StringBuilder stringBuilder = new StringBuilder(1000);
      stringBuilder.Append(e.Message);
      stringBuilder.Append("\n");
      stringBuilder.Append("Stacktrace:");
      stringBuilder.Append(e.StackTrace);
      return CustomMessageBox.ShowError(stringBuilder.ToString());
    }

    public static bool? ShowError(WPIDException e) 
            => CustomMessageBox.ShowError(e.MessageTrace);

    public static bool? ShowError(string message)
    {
      Console.WriteLine(message);
      return CustomMessageBox.ShowMessage(Tools.GetString("txtError"), message);
    }

    public static bool? ShowWarning(string message)
    {
      Console.WriteLine(message);
      return CustomMessageBox.ShowMessage(Tools.GetString("txtWarning"), message);
    }

    public static bool? ShowMessage(string header, string message, 
        int height = 400, int width = 400) => 
            new CustomMessageBox(new CustomMessageBoxVM(CustomDialogType.OKDialog)
    {
      Header = header,
      Message = message,
      Height = height,
      Width = width
    }).ShowDialog();

    private void btNo_Click(object sender, RoutedEventArgs e)
    {
      CustomMessageBoxVM dataContext = this.DataContext as CustomMessageBoxVM;
      dataContext.Result = CustomMessageBoxResult.No;
      if (!dataContext.OnExit())
        return;
      this.DialogResult = new bool?(false);
      this.Close();
    }

    private void btYes_Click(object sender, RoutedEventArgs e)
    {
      CustomMessageBoxVM dataContext = this.DataContext as CustomMessageBoxVM;
      dataContext.Result = CustomMessageBoxResult.Yes;
      if (!dataContext.OnExit())
        return;
      this.DialogResult = new bool?(true);
      this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) 
            => (this.DataContext as CustomMessageBoxVM).OnLoad();

    private void btOK_Click(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext as CustomMessageBoxVM).OnExit())
        return;
      this.DialogResult = new bool?(true);
      this.Close();
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.Clear();
      Clipboard.SetText(this.tbPageMessage.Text);
    }

        /*
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/dialogs/custommessagebox.xaml", UriKind.Relative));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.Window_Loaded);
          break;
        case 2:
          this.tbPageTitle = (TextBlock) target;
          break;
        case 3:
          this.tbPageMessage = (TextBlock) target;
          break;
        case 4:
          ((MenuItem) target).Click += new RoutedEventHandler(this.MenuItem_Click);
          break;
        case 5:
          this.tb1 = (TextBlock) target;
          break;
        case 6:
          this.cbDoNotAskAgain = (CheckBox) target;
          break;
        case 7:
          this.btYes = (Button) target;
          this.btYes.Click += new RoutedEventHandler(this.btYes_Click);
          break;
        case 8:
          this.btNo = (Button) target;
          this.btNo.Click += new RoutedEventHandler(this.btNo_Click);
          break;
        case 9:
          this.btOK = (Button) target;
          this.btOK.Click += new RoutedEventHandler(this.btOK_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
        */
  }
}
