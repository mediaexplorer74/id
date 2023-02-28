// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.AsyncWPIDTask
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Threading;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class AsyncWPIDTask : WPIDTask
  {
    protected ManualResetEvent _taskWaitHandle = new ManualResetEvent(false);
    protected ManualResetEvent _cancelWaitHandle = new ManualResetEvent(false);

    public AsyncWPIDTask(TaskID taskId)
      : base(taskId)
    {
      BackgroundTasks.RegisterTask(this);
    }

    public event TaskEventHandler TaskInProgressEvent;

    public event TaskEventHandler TaskCompletedEvent;

    public override event TaskWorkEventHandler Task;

    public bool CancelPending { get; protected set; }

    public AutoResetEvent CancelWaitHandler { get; protected set; }

    public virtual void RunTaskAsync() => this.RunTaskAsync((object) null);

    public virtual void RunTaskAsync(object argument)
    {
      if (this.Status == TaskStatus.InProgress)
        this.WaitForCompletion();
      if (this.Task == null)
        return;
      this._taskWaitHandle.Reset();
      this.Status = TaskStatus.Queued;
      ThreadPool.QueueUserWorkItem(new WaitCallback(this.TaskHandler), argument);
    }

    public void CancelTask()
    {
      if (this.Status != TaskStatus.InProgress)
        return;
      this.CancelPending = true;
      this._cancelWaitHandle.Set();
    }

    public bool WaitForCompletion() => this._taskWaitHandle.WaitOne();

    public bool WaitForCompletion(int millisec) => this._taskWaitHandle.WaitOne(millisec);

    protected virtual void TaskHandler(object argument)
    {
      this.Status = TaskStatus.InProgress;
      TaskEventArgs e1 = new TaskEventArgs(this.Status);
      if (this.TaskInProgressEvent != null)
        this.TaskInProgressEvent((object) this, e1);
      TaskWorkEventArgs e2 = new TaskWorkEventArgs(argument);
      this.Task((object) this, e2);
      this.ErrorCode = e2.ErrorCode;
      this.Result = e2.Result;
      if (e2.Cancelled)
      {
        this.Status = TaskStatus.Cancelled;
        this.CancelPending = false;
      }
      else
        this.Status = this.ErrorCode == 0 ? TaskStatus.CompleteSucceeded : TaskStatus.CompleteFailed;
      if (this.TaskCompletedEvent != null)
      {
        e1.Status = this.Status;
        if (Application.Current != null)
          Application.Current.Dispatcher.Invoke((Delegate) this.TaskCompletedEvent, (object) this, (object) e1);
        else
          this.TaskCompletedEvent((object) this, e1);
      }
      this._taskWaitHandle.Set();
    }
  }
}
