// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.PidConsoleWriter
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.IO;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class PidConsoleWriter : StreamWriter
  {
    private TextWriter _console;
    private PidConsoleWriter.LogMode _mode;

    public PidConsoleWriter(string filepath, TextWriter console, PidConsoleWriter.LogMode mode)
      : base((Stream) new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite))
    {
      this._console = console;
      this._mode = mode;
      this.AutoFlush = true;
    }

    public override void Write(char value)
    {
      if (this.LogToFile)
        base.Write(value);
      if (!this.LogToConsole)
        return;
      this._console.Write(value);
    }

    public override void Write(string value)
    {
      if (this.LogToFile)
        base.Write(value);
      if (!this.LogToConsole)
        return;
      this._console.Write(value);
    }

    public override void WriteLine()
    {
      if (this.LogToFile)
        base.WriteLine();
      if (!this.LogToConsole)
        return;
      this._console.WriteLine();
    }

    public override void WriteLine(char value)
    {
      if (this.LogToFile)
        base.WriteLine(value);
      if (!this.LogToConsole)
        return;
      this._console.WriteLine(value);
    }

    public override void WriteLine(string value)
    {
      if (this.LogToFile)
        base.WriteLine(value);
      if (!this.LogToConsole)
        return;
      this._console.WriteLine(value);
    }

    private bool LogToFile => true;

    private bool LogToConsole => (this._mode & PidConsoleWriter.LogMode.Console) == PidConsoleWriter.LogMode.Console;

    [Flags]
    public enum LogMode
    {
      File = 0,
      Console = 1,
    }
  }
}
