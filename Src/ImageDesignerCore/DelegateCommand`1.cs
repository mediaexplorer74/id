// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.DelegateCommand`1
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class DelegateCommand<T> : IIDCommand<T>
  {
    private readonly Func<bool> _canExecute;
    private readonly Func<T> _execute;

    public event EventHandler CanExecuteChanged;

    public DelegateCommand(Func<T> execute)
      : this(execute, (Func<bool>) null)
    {
    }

    public DelegateCommand(Func<T> execute, Func<bool> canExecute)
    {
      this._execute = execute;
      this._canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => this._canExecute == null || this._canExecute();

    public T Execute(object parameter) => this._execute();

    public void RaiseCanExecuteChanged()
    {
      if (this.CanExecuteChanged == null)
        return;
      this.CanExecuteChanged((object) this, EventArgs.Empty);
    }
  }
}
