// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.LogEntry
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class LogEntry
  {
    private LogEntry()
    {
    }

    public LogEntry(string msg) => this.Message = msg;

    public string Message { get; set; }

    public override string ToString() => this.Message;
  }
}
