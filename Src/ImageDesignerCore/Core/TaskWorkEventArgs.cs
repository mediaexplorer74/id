// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.TaskWorkEventArgs
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.ComponentModel;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class TaskWorkEventArgs : DoWorkEventArgs
  {
    protected bool _cancelled;

    public TaskWorkEventArgs()
      : base((object) null)
    {
    }

    public TaskWorkEventArgs(object argument)
      : base(argument)
    {
    }

    public int ErrorCode { internal get; set; }

    public bool Cancelled
    {
      set => this._cancelled = value;
      internal get => this._cancelled;
    }
  }
}
