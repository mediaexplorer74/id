// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.BackgroundTasks
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public static class BackgroundTasks
  {
    public static readonly TaskID SizeCalculatorTask = new TaskID("SizeCalculator");
    public static readonly TaskID CalculateLanguageSizeTask = new TaskID("CalculateLanguageSize");
    private static readonly object _lock = new object();
    private static Dictionary<TaskID, AsyncWPIDTask> _taskMap = new Dictionary<TaskID, AsyncWPIDTask>();

    static BackgroundTasks()
    {
      ImageSizeCalculatorTask sizeCalculatorTask = new ImageSizeCalculatorTask();
      Microsoft.WindowsPhone.ImageDesigner.Core.CalculateLanguageSizeTask languageSizeTask = new Microsoft.WindowsPhone.ImageDesigner.Core.CalculateLanguageSizeTask();
    }

    public static void RegisterTask(AsyncWPIDTask task)
    {
      AsyncWPIDTask asyncWpidTask = (AsyncWPIDTask) null;
      if (BackgroundTasks._taskMap.TryGetValue(task.TaskID, out asyncWpidTask) && asyncWpidTask.Status == TaskStatus.InProgress)
      {
        asyncWpidTask.CancelTask();
        asyncWpidTask.WaitForCompletion();
      }
      lock (BackgroundTasks._lock)
        BackgroundTasks._taskMap[task.TaskID] = task;
    }

    public static List<AsyncWPIDTask> GetAllTasks()
    {
      lock (BackgroundTasks._lock)
        return BackgroundTasks._taskMap.Values.ToList<AsyncWPIDTask>();
    }

    public static void WaitForCompletion(bool cancel = false) => BackgroundTasks.GetAllTasks()?.ForEach((Action<AsyncWPIDTask>) (t => BackgroundTasks.WaitForCompletion(t, cancel)));

    public static void WaitForCompletion(TaskID id, bool cancel = false) => BackgroundTasks.WaitForCompletion(BackgroundTasks.GetTask(id), cancel);

    public static void WaitForCompletion(AsyncWPIDTask task, bool cancel = false)
    {
      if (task == null)
        return;
      if (cancel)
        task.CancelTask();
      task.WaitForCompletion();
    }

    public static AsyncWPIDTask GetTask(TaskID id)
    {
      AsyncWPIDTask task = (AsyncWPIDTask) null;
      lock (BackgroundTasks._lock)
        BackgroundTasks._taskMap.TryGetValue(id, out task);
      return task;
    }

    public static void CancelAll() => BackgroundTasks.GetAllTasks()?.ForEach((Action<AsyncWPIDTask>) (t => t.CancelTask()));
  }
}
