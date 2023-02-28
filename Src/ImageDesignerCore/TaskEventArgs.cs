// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.TaskEventArgs
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class TaskEventArgs : EventArgs
  {
    protected TaskStatus _status;

    public TaskEventArgs(TaskStatus status) => this._status = status;

    public TaskStatus Status
    {
      get => this._status;
      internal set => this._status = value;
    }
  }
}
