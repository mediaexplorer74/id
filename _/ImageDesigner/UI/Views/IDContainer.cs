// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Views.IDContainer
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
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Views
{
  public class IDContainer : System.Windows.Controls.UserControl, IComponentConnector
  {
    private IDController _control;
    internal IDContainer UserControl;
    internal Grid LayoutRoot;
    internal DockPanel dbContent;
    internal Button btStartOver;
    internal Button btRestartCustomization;
    internal Button btCancel;
    internal Button btSave;
    internal Button btBuild;
    internal Button btFlash;
    internal Button btNext;
    internal StatusBar sb;
    private bool _contentLoaded;

    public IDContainer() => this.InitializeComponent();

    private void IDContainer_Loaded_1(object sender, RoutedEventArgs e)
    {
      this._control = this.DataContext as IDController;
      if (this._control == null)
        return;
      this._control.OnStateEntry += new IDControllerEventHandler(this.control_OnStateEntry);
      this._control.OnStateExit += new IDControllerEventHandler(this._control_OnStateExit);
      if (!(this._control.GetVM(IDStates.BuildImage) is BuildImagePageVM vm))
        return;
      this.btBuild.DataContext = (object) vm;
    }

    private void _control_OnStateExit(object sender, IDControllerEventHandlerArgs e)
    {
      object dataContext = this.DataContext;
      if (e.currentState != IDStates.CustomizeOS)
        return;
      this.btRestartCustomization.Visibility = Visibility.Collapsed;
    }

    private void control_OnStateEntry(object sender, IDControllerEventHandlerArgs e)
    {
      object dataContext = this.DataContext;
      switch (e.currentState)
      {
        case IDStates.CustomizeOS:
          this.btRestartCustomization.Visibility = Visibility.Visible;
          break;
        case IDStates.BuildImage:
          this.btNext.Visibility = Visibility.Collapsed;
          this.btFlash.Visibility = Visibility.Collapsed;
          this.btBuild.Visibility = Visibility.Visible;
          break;
        case IDStates.BuildSuccess:
          this.btNext.Visibility = Visibility.Collapsed;
          this.btBuild.Visibility = Visibility.Collapsed;
          this.btFlash.Visibility = Visibility.Visible;
          if (!(this._control.GetVM(IDStates.BuildSuccess) is BuildSuccessPageVM vm1))
            break;
          this.btFlash.DataContext = (object) vm1;
          break;
        case IDStates.FlashImage:
          this.btNext.Visibility = Visibility.Collapsed;
          this.btBuild.Visibility = Visibility.Collapsed;
          this.btFlash.Visibility = Visibility.Visible;
          if (!(this._control.GetVM(IDStates.FlashImage) is FlashImagePageVM vm2))
            break;
          this.btFlash.DataContext = (object) vm2;
          break;
        default:
          this.btNext.Visibility = Visibility.Visible;
          this.btBuild.Visibility = Visibility.Collapsed;
          this.btFlash.Visibility = Visibility.Collapsed;
          break;
      }
    }

    private void btNext_Click(object sender, RoutedEventArgs e) => this._control.MoveToNextCommand.Execute((object) this);

    private void btBuild_Click(object sender, RoutedEventArgs e) => (this._control.GetVM(IDStates.BuildImage) as BuildImagePageVM).BuildImageCommand.Execute((object) this);

    private void btFlash_Click(object sender, RoutedEventArgs e)
    {
      if (this._control.CurrentState == IDStates.FlashImage)
        (this._control.GetVM(IDStates.FlashImage) as FlashImagePageVM).BeginFlashCommand.Execute((object) this);
      else
        this.btNext_Click(sender, e);
    }

    private void btStartOver_Click(object sender, RoutedEventArgs e)
    {
      bool flag1 = true;
      bool flag2 = true;
      string userConfig = UserConfig.GetUserConfig("ShowConfirmStartOverDialog");
      if (!string.IsNullOrWhiteSpace(userConfig))
        flag2 = Convert.ToBoolean(userConfig);
      if (flag2)
      {
        flag1 = false;
        CustomMessageBoxVM vm = (CustomMessageBoxVM) new ConfirmStartOverDialogVM();
        CustomMessageBox customMessageBox = new CustomMessageBox(vm);
        vm.Header = Tools.GetString("txtStartOverMessageHeader");
        vm.Message = Tools.GetString("txtStartOverMessage");
        customMessageBox.ShowDialog();
        if (vm.Result == CustomMessageBoxResult.Yes)
          flag1 = true;
      }
      if (!flag1)
        return;
      this._control.MoveToStartCommand.Execute((object) this);
    }

    private void btSave_Click(object sender, RoutedEventArgs e)
    {
      bool flag = true;
      string userConfig = UserConfig.GetUserConfig("ShowSaveSuccessDialog");
      if (!string.IsNullOrWhiteSpace(userConfig))
        flag = Convert.ToBoolean(userConfig);
      if (!this._control.SaveCommand.Execute((object) this) || !flag)
        return;
      CustomMessageBoxVM vm = (CustomMessageBoxVM) new SaveSuccessDialogVM();
      CustomMessageBox customMessageBox = new CustomMessageBox(vm);
      vm.Header = Tools.GetString("txtSaveSuccessDlgHeader");
      vm.Message = Tools.GetString("txtSaveSuccessDlgText");
      customMessageBox.ShowDialog();
    }

    private void btCancel_Click(object sender, RoutedEventArgs e) => this._control.CancelCommand.Execute((object) this);

    private void btRestartCustomization_Click(object sender, RoutedEventArgs e)
    {
      CustomMessageBoxVM vm = (CustomMessageBoxVM) new ConfirmStartOverDialogVM();
      CustomMessageBox customMessageBox = new CustomMessageBox(vm);
      vm.ShowDialogNextTimeVisible = false;
      vm.Header = Tools.GetString("txtRestartCustomizationHeader");
      vm.Message = Tools.GetString("txtRestartCustomizationText");
      customMessageBox.ShowDialog();
      if (vm.Result != CustomMessageBoxResult.Yes)
        return;
      this._control.MoveToState(IDStates.CustomizationChoice, false);
    }

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/ImageDesigner;V8.0.0.0;component/dialogs/idcontainer.xaml", UriKind.Relative));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.UserControl = (IDContainer) target;
          this.UserControl.Loaded += new RoutedEventHandler(this.IDContainer_Loaded_1);
          break;
        case 2:
          this.LayoutRoot = (Grid) target;
          break;
        case 3:
          this.dbContent = (DockPanel) target;
          break;
        case 4:
          this.btStartOver = (Button) target;
          this.btStartOver.Click += new RoutedEventHandler(this.btStartOver_Click);
          break;
        case 5:
          this.btRestartCustomization = (Button) target;
          this.btRestartCustomization.Click += new RoutedEventHandler(this.btRestartCustomization_Click);
          break;
        case 6:
          this.btCancel = (Button) target;
          this.btCancel.Click += new RoutedEventHandler(this.btCancel_Click);
          break;
        case 7:
          this.btSave = (Button) target;
          this.btSave.Click += new RoutedEventHandler(this.btSave_Click);
          break;
        case 8:
          this.btBuild = (Button) target;
          this.btBuild.Click += new RoutedEventHandler(this.btBuild_Click);
          break;
        case 9:
          this.btFlash = (Button) target;
          this.btFlash.Click += new RoutedEventHandler(this.btFlash_Click);
          break;
        case 10:
          this.btNext = (Button) target;
          this.btNext.Click += new RoutedEventHandler(this.btNext_Click);
          break;
        case 11:
          this.sb = (StatusBar) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
