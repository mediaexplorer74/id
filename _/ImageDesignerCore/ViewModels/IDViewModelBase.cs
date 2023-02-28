// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.IDViewModelBase
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.ComponentModel;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public abstract class IDViewModelBase : DependencyObject, IIDState, INotifyPropertyChanged
  {
    private bool _isCurrentPage;
    private IDStates _myState;
    protected IDStates? _nextState = new IDStates?();
    private bool _isValid;

    private IDViewModelBase()
    {
    }

    internal IDViewModelBase(IDStates myState) => this._myState = myState;

    public event ValidationEventHandler PageValidPropertyChanged;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsCurrentPage
    {
      get => this._isCurrentPage;
      set
      {
        if (value == this._isCurrentPage)
          return;
        this._isCurrentPage = value;
        this.OnPropertyChanged(nameof (IsCurrentPage));
      }
    }

    public bool IsValid
    {
      get => this._isValid;
      protected set
      {
        this._isValid = value;
        if (this.PageValidPropertyChanged == null)
          return;
        this.PageValidPropertyChanged((object) this, EventArgs.Empty);
      }
    }

    public virtual IDStates CurrentState => this._myState;

    public IDStates NextState
    {
      get
      {
        if (this.IsValid)
          this.ComputeNextState();
        return this._nextState.Value;
      }
    }

    public IDContext Context => IDContext.Instance;

    protected virtual bool SaveSupported { get; private set; }

    protected virtual bool CancelSupported { get; private set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    internal bool OnStateEntryBase()
    {
      this.Context.Controller.CancelSupported = this.CancelSupported;
      this.Context.Controller.SaveSupported = this.SaveSupported;
      this.Validate();
      return this.OnStateEntry();
    }

    internal virtual bool OnStateEntry() => true;

    private bool OnStateExitBase() => this.OnStateExit();

    internal bool Save()
    {
      bool flag = true;
      if (this.SaveSupported)
      {
        flag = this.SavePage();
        this.Context.Controller.SaveProject();
      }
      return flag;
    }

    protected virtual bool SavePage() => true;

    internal virtual bool OnStateExit() => true;

    protected abstract void Validate();

    protected abstract void ComputeNextState();

    internal void OnApplicationExitBase() => this.OnApplicationExit();

    protected virtual void OnApplicationExit()
    {
    }
  }
}
