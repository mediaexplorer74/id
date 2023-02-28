// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.LogMonitor
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.Collections.ObjectModel;
using System.IO;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  internal class LogMonitor : FileSystemWatcher
  {
    private string _fileName;
    private FileStream _fs;
    private StreamReader _sr;

    public LogMonitor(string fileName)
    {
      this._fileName = fileName;
      this.Initialize();
    }

    public void Close()
    {
      this.EnableRaisingEvents = false;
      this._sr.Close();
      this._fs.Close();
    }

    private void Initialize()
    {
        // RnD ; fix it
        this.Path = this._fileName;//Path.GetDirectoryName(this._fileName);
        this.Filter = this._fileName;//Path.GetFileName(this._fileName);

      this.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
      this.EnableRaisingEvents = true;
      this._fs = new FileStream(this._fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
      this._fs.Seek(0L, SeekOrigin.End);
      this._sr = new StreamReader((Stream) this._fs);
      this.Changed += new FileSystemEventHandler(this.OnChanged);
    }

    public void OnChanged(object o, FileSystemEventArgs e)
    {
      if (this._fs.Length < this._fs.Position)
        this._fs.Position = this._fs.Seek(0L, SeekOrigin.Begin);
      ObservableCollection<LogEntry> logEntries = new ObservableCollection<LogEntry>();
      string msg;
      while ((msg = this._sr.ReadLine()) != null)
        logEntries.Add(new LogEntry(msg));
      LogMonitorEventArgs e1 = new LogMonitorEventArgs(logEntries);
      if (this.FileContentChanged == null)
        return;
      this.FileContentChanged((object) this, e1);
    }

    public event LogMonitorEventHandler FileContentChanged;
  }
}
