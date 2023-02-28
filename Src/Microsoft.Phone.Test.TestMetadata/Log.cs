// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.Log
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Microsoft.Phone.Test.TestMetadata
{
  public static class Log
  {
    private static bool s_verbose;
    private static readonly Dictionary<Log.MessageLevel, string> s_msgPrefix = new Dictionary<Log.MessageLevel, string>();
    private static readonly Dictionary<Log.MessageLevel, ConsoleColor> s_msgColor = new Dictionary<Log.MessageLevel, ConsoleColor>();
    private static readonly object s_logConsoleLock = new object();
    private static readonly object s_logFileLock = new object();

    public static bool LogErrorAsWarning { get; set; }

    public static string LogFile { get; set; }

    static Log()
    {
      Log.s_msgPrefix.Add(Log.MessageLevel.Debug, "diagnostic");
      Log.s_msgPrefix.Add(Log.MessageLevel.Message, "info");
      Log.s_msgPrefix.Add(Log.MessageLevel.Warning, "warning ");
      Log.s_msgPrefix.Add(Log.MessageLevel.Error, "fatal error ");
      Log.s_msgColor.Add(Log.MessageLevel.Debug, ConsoleColor.DarkGray);
      Log.s_msgColor.Add(Log.MessageLevel.Message, ConsoleColor.Gray);
      Log.s_msgColor.Add(Log.MessageLevel.Warning, ConsoleColor.Yellow);
      Log.s_msgColor.Add(Log.MessageLevel.Error, ConsoleColor.Red);
    }

    private static void LogMessage(Log.MessageLevel level, string message)
    {
      if (level == Log.MessageLevel.Debug && !Log.s_verbose)
        return;
      string[] strArray = message.Split(new char[2]
      {
        '\r',
        '\n'
      }, StringSplitOptions.RemoveEmptyEntries);
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string str in strArray)
        stringBuilder.AppendLine(string.Format("{0}: {1}", (object) Log.s_msgPrefix[level], (object) str));
      string contents = stringBuilder.ToString();
      lock (Log.s_logConsoleLock)
      {
        ConsoleColor foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = Log.s_msgColor[level];
        Console.Write(contents);
        Console.ForegroundColor = foregroundColor;
      }
      if (Log.LogFile != null && (level == Log.MessageLevel.Error || level == Log.MessageLevel.Warning))
      {
        lock (Log.s_logFileLock)
          File.AppendAllText(Log.LogFile, contents);
      }
    }

    private static void LogMessage(Log.MessageLevel level, string format, params object[] args) => Log.LogMessage(level, string.Format((IFormatProvider) CultureInfo.InvariantCulture, format, args));

    public static void SetVerbosity(bool enabled) => Log.s_verbose = enabled;

    public static void Error(string format, params object[] args)
    {
      Log.MessageLevel level = Log.MessageLevel.Error;
      if (Log.LogErrorAsWarning)
        level = Log.MessageLevel.Warning;
      Log.LogMessage(level, format, args);
    }

    public static void Warning(string format, params object[] args) => Log.LogMessage(Log.MessageLevel.Warning, format, args);

    public static void Message(string format, params object[] args) => Log.LogMessage(Log.MessageLevel.Message, format, args);

    public static void Diagnostic(string format, params object[] args) => Log.LogMessage(Log.MessageLevel.Debug, format, args);

    private enum MessageLevel
    {
      Error,
      Warning,
      Message,
      Debug,
    }
  }
}
