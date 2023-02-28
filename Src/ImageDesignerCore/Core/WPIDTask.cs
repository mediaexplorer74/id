// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.WPIDTask
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class WPIDTask
  {
    private IDContext _context;

    public WPIDTask(TaskID taskID)
    {
      this.Status = TaskStatus.NotStarted;
      this._context = IDContext.Instance;
      this.TaskID = taskID;
    }

    public TaskStatus Status { get; protected set; }

    public int ErrorCode { get; protected set; }

    public object Result { get; protected set; }

    public TaskID TaskID { get; private set; }

    public virtual void RunTask() => this.RunTask((object) null);

    public virtual void RunTask(object argument)
    {
      if (this.Task == null)
        return;
      TaskWorkEventArgs e = new TaskWorkEventArgs(argument);
      this.Task((object) this, e);
      this.Result = e.Result;
      this.ErrorCode = e.ErrorCode;
    }

    public virtual event TaskWorkEventHandler Task;
  }
}
