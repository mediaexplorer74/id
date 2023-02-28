// ConsoleLogSettings.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ConsoleLogSettings
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class ConsoleLogSettings
  {
    private static ConsoleLogSettings _instance;
    private string _logFile;

    private ConsoleLogSettings()
    {
    }

    public static ConsoleLogSettings Instance
    {
      get
      {
        if (ConsoleLogSettings._instance == null)
          ConsoleLogSettings._instance = new ConsoleLogSettings();
        return ConsoleLogSettings._instance;
      }
    }

    public string LogFile
    {
      get
      {
        if (this._logFile == null)
        {
          string str = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
          if (!Tools.CanWriteToFolder(str))
            str = Constants.DEFAULT_OUTPUT_FOLDER;
          FileStream fileStream = (FileStream) null;
          int num = 1;
          bool flag = true;
          string path = Path.Combine(str, "PhoneImageDesigner.log");
          do
          {
            try
            {
              fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
              flag = false;
              this._logFile = path;
            }
            catch (IOException ex)
            {
              path = Path.Combine(str, "PhoneImageDesigner" + (object) num + ".log");
              ++num;
            }
            finally
            {
              fileStream?.Close();
            }
          }
          while (flag);
        }
        return this._logFile;
      }
    }

    public void LogToConsole()
    {
      Console.SetOut(Console.Out);
      Console.SetError(Console.Error);
    }

    public void LogToFile()
    {
      Console.WriteLine("Phone Image Designer log in {0}", (object) this.LogFile);
      PidConsoleWriter pidConsoleWriter = new PidConsoleWriter(this.LogFile, Console.Out, PidConsoleWriter.LogMode.File);
      Console.SetOut((TextWriter) pidConsoleWriter);
      Console.SetError((TextWriter) pidConsoleWriter);
    }

    public void LogToFileAndConsole()
    {
      Console.WriteLine("Phone Image Designer log in {0}", (object) this.LogFile);
      PidConsoleWriter pidConsoleWriter = new PidConsoleWriter(this.LogFile, Console.Out, PidConsoleWriter.LogMode.Console);
      Console.SetOut((TextWriter) pidConsoleWriter);
      Console.SetError((TextWriter) pidConsoleWriter);
    }

    public void LogToNull()
    {
      Console.SetOut((TextWriter) null);
      Console.SetError((TextWriter) null);
    }
  }
}
