// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.IDController
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class IDController : DependencyObject, IIDControl
  {
    public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(nameof (CurrentPage), typeof (IDViewModelBase), typeof (IDController), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty IsLastPageProperty = DependencyProperty.Register(nameof (IsLastPage), typeof (bool), typeof (IDController), new PropertyMetadata((object) false));
    public static readonly DependencyProperty IsFirstPageProperty = DependencyProperty.Register(nameof (IsFirstPage), typeof (bool), typeof (IDController), new PropertyMetadata((object) true, new PropertyChangedCallback(IDController.IsFirstPageChanged)));
    public static readonly DependencyProperty EnableNextButtonProperty = DependencyProperty.Register(nameof (EnableNextButton), typeof (bool), typeof (IDController), new PropertyMetadata((object) false));
    public static readonly DependencyProperty SaveSupportedProperty = DependencyProperty.Register(nameof (SaveSupported), typeof (bool), typeof (IDController), new PropertyMetadata((object) false));
    public static readonly DependencyProperty CanSaveProperty = DependencyProperty.Register(nameof (CanSave), typeof (bool), typeof (IDController), new PropertyMetadata((object) true));
    public static readonly DependencyProperty CancelSupportedProperty = DependencyProperty.Register(nameof (CancelSupported), typeof (bool), typeof (IDController), new PropertyMetadata((object) false));
    public static readonly DependencyProperty CanCancelProperty = DependencyProperty.Register(nameof (CanCancel), typeof (bool), typeof (IDController), new PropertyMetadata((object) false));
    public static readonly DependencyProperty CanMoveToStartPageProperty = DependencyProperty.Register(nameof (CanMoveToStartPage), typeof (bool), typeof (IDController), new PropertyMetadata((object) false));
    private DelegateCommand<bool> _moveToNextCommand;
    private DelegateCommand<bool> _moveToStartCommand;
    private DelegateCommand<bool> _moveToPreviousCommand;
    private DelegateCommand<bool> _saveCommand;
    private DelegateCommand<bool> _cancelCommand;
    private Dictionary<IDStates, IDViewModelBase> _stateMap = new Dictionary<IDStates, IDViewModelBase>();
    private IDStates _initialState;
    private IDStates _currentState;

    public IDController() => this.Initialize();

    public event IDControllerEventHandler OnStateEntry;

    public event IDControllerEventHandler OnStateExit;

    public bool MoveToNextPage()
    {
      bool nextPage = false;
      DelegateCommand<bool> moveToNextCommand = this.MoveToNextCommand;
      if (moveToNextCommand.CanExecute((object) null))
        nextPage = moveToNextCommand.Execute((object) null);
      return nextPage;
    }

    public bool MoveToPrevPage() => throw new NotSupportedException("This functionality is not currently supported");

    public bool MoveToStartPage()
    {
      bool startPage = false;
      DelegateCommand<bool> moveToStartCommand = this.MoveToStartCommand;
      if (moveToStartCommand.CanExecute((object) null))
        startPage = moveToStartCommand.Execute((object) null);
      return startPage;
    }

    public bool Save()
    {
      bool flag = false;
      DelegateCommand<bool> saveCommand = this.SaveCommand;
      if (saveCommand.CanExecute((object) null))
        flag = saveCommand.Execute((object) null);
      return flag;
    }

    public bool Build()
    {
      bool flag = false;
      if (this.CurrentState == IDStates.BuildImage)
      {
        DelegateCommand<bool> buildImageCommand = (this.CurrentPage as BuildImagePageVM).BuildImageCommand;
        if (buildImageCommand.CanExecute((object) null))
          flag = buildImageCommand.Execute((object) null);
      }
      return flag;
    }

    public bool FlashImage()
    {
      bool flag = false;
      if (this.CurrentState == IDStates.FlashImage)
      {
        DelegateCommand<bool> beginFlashCommand = (this.CurrentPage as FlashImagePageVM).BeginFlashCommand;
        if (beginFlashCommand.CanExecute((object) null))
          flag = beginFlashCommand.Execute((object) null);
      }
      return flag;
    }

    public IDViewModelBase GetVM(IDStates state)
    {
      IDViewModelBase vm = (IDViewModelBase) null;
      if (this._stateMap.ContainsKey(state))
        vm = this._stateMap[state];
      return vm;
    }

    public void NewProject()
    {
      string outputDir = IDContext.Instance.OutputDir;
      IDController controller = IDContext.Instance.Controller;
      IDContext newInstance = IDContext.NewInstance;
      newInstance.Workflow = Workflow.CreateImage;
      newInstance.Controller = controller;
      newInstance.OutputDir = outputDir;
      newInstance.BSPConfig = IDContext.Instance.BSPConfig;
      if (newInstance == null)
        return;
      this.SwitchContext(newInstance);
    }

    public void LoadProject(string projectFile)
    {
      IDContext newContext = ProjectXml.Load(projectFile);
      if (newContext == null)
        return;
      this.SwitchContext(newContext);
    }

    public void SaveProject() => this.Context.ProjectXml.Save(Path.Combine(this.Context.OutputDir, Path.ChangeExtension("Project", "wpid.xml")));

    public void OnApplicationExit()
    {
      foreach (IDViewModelBase idViewModelBase in this._stateMap.Values)
        idViewModelBase.OnApplicationExitBase();
    }

    public IDContext Context => IDContext.Instance;

    public IDStates CurrentState => this._currentState;

    public IDViewModelBase CurrentPage
    {
      get => (IDViewModelBase) this.GetValue(IDController.CurrentPageProperty);
      set => this.SetValue(IDController.CurrentPageProperty, (object) value);
    }

    public bool IsLastPage
    {
      get => (bool) this.GetValue(IDController.IsLastPageProperty);
      set => this.SetValue(IDController.IsLastPageProperty, (object) value);
    }

    public bool IsFirstPage
    {
      get => (bool) this.GetValue(IDController.IsFirstPageProperty);
      set => this.SetValue(IDController.IsFirstPageProperty, (object) value);
    }

    private static void IsFirstPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is IDController idController))
        return;
      idController.CanMoveToStartPage = !(bool) e.NewValue;
    }

    public bool EnableNextButton
    {
      get => (bool) this.GetValue(IDController.EnableNextButtonProperty);
      set => this.SetValue(IDController.EnableNextButtonProperty, (object) value);
    }

    public bool MoveToState(IDStates state, bool saveCurrentState = true)
    {
      if (this.CurrentPage.OnStateExit())
      {
        IDStates currentState = this._currentState;
        if (this.OnStateExit != null)
          this.OnStateExit((object) this, new IDControllerEventHandlerArgs()
          {
            previousState = IDStates.Invalid,
            currentState = this._currentState,
            nextState = this._stateMap[this._currentState].NextState
          });
        if (saveCurrentState)
          this.SaveCommand.Execute((object) this);
        this._currentState = state;
        this.CurrentPage = this._stateMap[this._currentState];
        this.CurrentPage.OnStateEntryBase();
        if (this.OnStateEntry != null)
        {
          IDControllerEventHandlerArgs e = new IDControllerEventHandlerArgs();
          e.currentState = this._currentState;
          e.previousState = currentState;
          e.nextState = IDStates.Invalid;
          using (new WaitCursor())
            this.OnStateEntry((object) this, e);
        }
      }
      this.EnableNextButton = this.CanMoveToNextPage;
      this.IsFirstPage = false;
      return true;
    }

    public DelegateCommand<bool> MoveToNextCommand
    {
      get
      {
        if (this._moveToNextCommand == null)
          this._moveToNextCommand = new DelegateCommand<bool>((Func<bool>) (() => this.MoveToNextPageInternal()), (Func<bool>) (() => this.CanMoveToNextPage));
        return this._moveToNextCommand;
      }
    }

    public bool CanMoveToNextPage
    {
      get
      {
        bool canMoveToNextPage = false;
        if (this._currentState != IDStates.Invalid)
        {
          IDViewModelBase state = this._stateMap[this._currentState];
          if (state.IsValid && state.NextState != IDStates.End)
            canMoveToNextPage = true;
        }
        return canMoveToNextPage;
      }
    }

    private void MoveToNextState()
    {
      this._currentState = this._stateMap[this._currentState].NextState;
      this.CurrentPage = this._stateMap[this._currentState];
    }

    private bool MoveToNextPageInternal()
    {
      if (this.CanMoveToNextPage)
      {
        using (new WaitCursor())
        {
          if (this.CurrentPage.OnStateExit())
          {
            IDStates currentState = this._currentState;
            if (this.OnStateExit != null)
              this.OnStateExit((object) this, new IDControllerEventHandlerArgs()
              {
                previousState = IDStates.Invalid,
                currentState = this._currentState,
                nextState = this._stateMap[this._currentState].NextState
              });
            if (!this.SaveCommand.Execute((object) this))
              return false;
            this.MoveToNextState();
            this.CurrentPage.OnStateEntryBase();
            if (this.OnStateEntry != null)
              this.OnStateEntry((object) this, new IDControllerEventHandlerArgs()
              {
                currentState = this._currentState,
                previousState = currentState,
                nextState = IDStates.Invalid
              });
          }
        }
      }
      this.EnableNextButton = this.CanMoveToNextPage;
      this.IsFirstPage = false;
      return true;
    }

    public bool SaveSupported
    {
      get => (bool) this.GetValue(IDController.SaveSupportedProperty);
      set => this.SetValue(IDController.SaveSupportedProperty, (object) value);
    }

    public DelegateCommand<bool> SaveCommand
    {
      get
      {
        if (this._saveCommand == null)
          this._saveCommand = new DelegateCommand<bool>((Func<bool>) (() => this.SaveInternal()), (Func<bool>) (() => this.CanSave));
        return this._saveCommand;
      }
    }

    private bool SaveInternal() => this.CurrentPage.Save();

    public bool CanSave
    {
      get => (bool) this.GetValue(IDController.CanSaveProperty);
      set => this.SetValue(IDController.CanSaveProperty, (object) value);
    }

    public bool CancelSupported
    {
      get => (bool) this.GetValue(IDController.CancelSupportedProperty);
      set => this.SetValue(IDController.CancelSupportedProperty, (object) value);
    }

    public DelegateCommand<bool> CancelCommand
    {
      get
      {
        if (this._cancelCommand == null)
          this._cancelCommand = new DelegateCommand<bool>((Func<bool>) (() => this.CancelInternal()), (Func<bool>) (() => this.CanCancel));
        return this._cancelCommand;
      }
    }

    private bool CancelInternal()
    {
      if (this.CurrentState == IDStates.BuildImage)
        (this.CurrentPage as BuildImagePageVM).CancelBuild();
      return true;
    }

    public bool CanCancel
    {
      get => (bool) this.GetValue(IDController.CanCancelProperty);
      set => this.SetValue(IDController.CanCancelProperty, (object) value);
    }

    public DelegateCommand<bool> MoveToStartCommand
    {
      get
      {
        if (this._moveToStartCommand == null)
          this._moveToStartCommand = new DelegateCommand<bool>((Func<bool>) (() => this.MoveToStartPageInternal()), (Func<bool>) (() => this.CanMoveToStartPage));
        return this._moveToStartCommand;
      }
    }

    public bool CanMoveToStartPage
    {
      get => (bool) this.GetValue(IDController.CanMoveToStartPageProperty);
      set => this.SetValue(IDController.CanMoveToStartPageProperty, (object) value);
    }

    private bool MoveToStartPageInternal()
    {
      if (this._currentState != this._initialState && this.CurrentPage.OnStateExit())
      {
        IDStates currentState = this._currentState;
        if (this.OnStateExit != null)
          this.OnStateExit((object) this, new IDControllerEventHandlerArgs()
          {
            previousState = IDStates.Invalid,
            currentState = this._currentState,
            nextState = this._initialState
          });
        BackgroundTasks.CancelAll();
        this.MoveToFirstState();
        this.CurrentPage.OnStateEntryBase();
        if (this.OnStateEntry != null)
          this.OnStateEntry((object) this, new IDControllerEventHandlerArgs()
          {
            currentState = this._currentState,
            previousState = currentState,
            nextState = IDStates.Invalid
          });
        this.EnableNextButton = this.CanMoveToNextPage;
        this.IsFirstPage = true;
        this.CanMoveToStartPage = !this.IsFirstPage;
      }
      return true;
    }

    private void MoveToFirstState()
    {
      this._currentState = this._initialState;
      this.CurrentPage = this._stateMap[this._currentState];
    }

    public DelegateCommand<bool> MoveToPreviousCommand
    {
      get
      {
        if (this._moveToPreviousCommand == null)
          this._moveToPreviousCommand = new DelegateCommand<bool>((Func<bool>) (() => this.MoveToPreviousPageInternal()), (Func<bool>) (() => this.CanMoveToPreviousPage));
        return this._moveToPreviousCommand;
      }
    }

    public bool CanMoveToPreviousPage => false;

    public bool MoveToPreviousPage() => throw new NotImplementedException();

    private bool MoveToPreviousPageInternal() => throw new NotImplementedException();

    internal Dictionary<IDStates, IDViewModelBase> StateMap => this._stateMap;

    private void Initialize()
    {
      IDContext.Instance.Controller = this;
      this.AddState((IDViewModelBase) new GettingStartedPageVM(IDStates.GettingStarted));
      this.AddState((IDViewModelBase) new SettingUpPageVM(IDStates.SettingUp));
      this.AddState((IDViewModelBase) new SelectImagePageVM(IDStates.SelectImageType));
      this.AddState((IDViewModelBase) new DescribeImagePageVM(IDStates.DescribeImage));
      this.AddState((IDViewModelBase) new SelectTemplatesPageVM(IDStates.SelectTemplates));
      this.AddState((IDViewModelBase) new CustomizationChoicePageVM(IDStates.CustomizationChoice));
      this.AddState((IDViewModelBase) new CustomizeOSPageVM(IDStates.CustomizeOS));
      this.AddState((IDViewModelBase) new BuildImagePageVM(IDStates.BuildImage));
      this.AddState((IDViewModelBase) new BuildSuccessPageVM(IDStates.BuildSuccess));
      this.AddState((IDViewModelBase) new FlashImagePageVM(IDStates.FlashImage));
      this.AddState((IDViewModelBase) new ModifyImagePageVM(IDStates.ModifyImage));
      this.AddState((IDViewModelBase) new PickOutputLocationPageVM(IDStates.PickOutputLocation));
      foreach (IDViewModelBase idViewModelBase in this._stateMap.Values)
        idViewModelBase.PageValidPropertyChanged += new ValidationEventHandler(this.PageValidPropertyChanged);
      this._initialState = IDStates.GettingStarted;
      this._currentState = this._initialState;
      this.CurrentPage = this._stateMap[this._currentState];
      this.CurrentPage.OnStateEntryBase();
      if (this.OnStateEntry != null)
        this.OnStateEntry((object) this, new IDControllerEventHandlerArgs()
        {
          previousState = IDStates.Invalid,
          currentState = IDStates.BuildSuccess,
          nextState = IDStates.Invalid
        });
      this.EnableNextButton = this.CanMoveToNextPage;
      BuildImagePageVM state = this._stateMap[IDStates.BuildImage] as BuildImagePageVM;
      state.BuildCompleted += new BuildImagePageEventHandler(this.buildVM_BuildCompleted);
      state.BuildStarted += new BuildImagePageEventHandler(this.buildVM_BuildStarted);
      state.BuildCancelled += new BuildImagePageEventHandler(this.buildVM_BuildCancelled);
    }

    private void AddState(IDViewModelBase state) => this._stateMap.Add(state.CurrentState, state);

    private void SwitchContext(IDContext newContext) => IDContext.SwitchInstance(newContext);

    private void buildVM_BuildCancelled(object sender, BuildImagePageEventArgs e)
    {
      Action method = (Action) (() => this.CanMoveToStartPage = true);
      if (Application.Current != null)
        Application.Current.Dispatcher.Invoke((Delegate) method);
      else
        method();
    }

    private void buildVM_BuildStarted(object sender, BuildImagePageEventArgs e)
    {
      Action method = (Action) (() => this.CanMoveToStartPage = false);
      if (Application.Current != null)
        Application.Current.Dispatcher.Invoke((Delegate) method);
      else
        method();
    }

    private void buildVM_BuildCompleted(object sender, BuildImagePageEventArgs e)
    {
      if (Application.Current == null)
        return;
      if (e != null && e.ErrorCode == 0)
        Tools.DispatcherExec<bool>((Func<bool>) (() => this.MoveToNextCommand.Execute((object) this)));
      Tools.DispatcherExec<bool>((Func<bool>) (() => this.CanMoveToStartPage = true));
    }

    private void PageValidPropertyChanged(object sender, EventArgs e)
    {
      if (this._stateMap[this._currentState] != sender as IDViewModelBase || Application.Current == null)
        return;
      this.EnableNextButton = this.CanMoveToNextPage;
    }
  }
}
