// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ImageBuilderEventArgs
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.ObjectModel;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class ImageBuilderEventArgs : EventArgs
  {
    public ObservableCollection<LogEntry> logEntries;
    public int ExitCode;
    public string CommandLine;

    public ImageBuilderEventArgs()
    {
    }

    public ImageBuilderEventArgs(ObservableCollection<LogEntry> logEntries) => this.logEntries = logEntries;

    public ImageBuilderEventArgs(int exitCode) => this.ExitCode = exitCode;
  }
}
