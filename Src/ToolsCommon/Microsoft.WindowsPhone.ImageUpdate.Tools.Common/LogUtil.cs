// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LogUtil
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class LogUtil
  {
    private static bool _verbose = false;
    private static Dictionary<LogUtil.MessageLevel, string> _msgPrefix = new Dictionary<LogUtil.MessageLevel, string>();
    private static Dictionary<LogUtil.MessageLevel, ConsoleColor> _msgColor = new Dictionary<LogUtil.MessageLevel, ConsoleColor>();
    private static LogUtil.InteropLogString _iuErrorLogger = (LogUtil.InteropLogString) null;
    private static LogUtil.InteropLogString _iuWarningLogger = (LogUtil.InteropLogString) null;
    private static LogUtil.InteropLogString _iuMsgLogger = (LogUtil.InteropLogString) null;
    private static LogUtil.InteropLogString _iuDebugLogger = (LogUtil.InteropLogString) null;

    [DllImport("IUCommon.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    private static extern void IU_LogTo(
      [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString ErrorMsgHandler,
      [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString WarningMsgHandler,
      [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString InfoMsgHandler,
      [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString DebugMsgHandler);

    static LogUtil()
    {
      LogUtil._msgPrefix.Add(LogUtil.MessageLevel.Debug, "diagnostic");
      LogUtil._msgPrefix.Add(LogUtil.MessageLevel.Message, "info");
      LogUtil._msgPrefix.Add(LogUtil.MessageLevel.Warning, "warning ");
      LogUtil._msgPrefix.Add(LogUtil.MessageLevel.Error, "fatal error ");
      LogUtil._msgColor.Add(LogUtil.MessageLevel.Debug, ConsoleColor.DarkGray);
      LogUtil._msgColor.Add(LogUtil.MessageLevel.Message, ConsoleColor.Gray);
      LogUtil._msgColor.Add(LogUtil.MessageLevel.Warning, ConsoleColor.Yellow);
      LogUtil._msgColor.Add(LogUtil.MessageLevel.Error, ConsoleColor.Red);
      LogUtil.IULogTo(new LogUtil.InteropLogString(LogUtil.Error), new LogUtil.InteropLogString(LogUtil.Warning), new LogUtil.InteropLogString(LogUtil.Message), new LogUtil.InteropLogString(LogUtil.Diagnostic));
    }

    private static void LogMessage(LogUtil.MessageLevel level, string message)
    {
      if (level == LogUtil.MessageLevel.Debug && !LogUtil._verbose)
        return;
      string str1 = message;
      char[] separator = new char[2]{ '\r', '\n' };
      foreach (string str2 in str1.Split(separator, StringSplitOptions.RemoveEmptyEntries))
      {
        ConsoleColor foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = LogUtil._msgColor[level];
        Console.WriteLine("{0}: {1}", (object) LogUtil._msgPrefix[level], (object) str2);
        Console.ForegroundColor = foregroundColor;
      }
    }

    private static void LogMessage(LogUtil.MessageLevel level, string format, params object[] args) => LogUtil.LogMessage(level, string.Format(format, args));

    public static void SetVerbosity(bool enabled) => LogUtil._verbose = enabled;

    public static void Error(string message) => LogUtil.LogMessage(LogUtil.MessageLevel.Error, message);

    public static void Error(string format, params object[] args) => LogUtil.LogMessage(LogUtil.MessageLevel.Error, format, args);

    public static void Warning(string message) => LogUtil.LogMessage(LogUtil.MessageLevel.Warning, message);

    public static void Warning(string format, params object[] args) => LogUtil.LogMessage(LogUtil.MessageLevel.Warning, format, args);

    public static void Message(string message) => LogUtil.LogMessage(LogUtil.MessageLevel.Message, message);

    public static void Message(string format, params object[] args) => LogUtil.LogMessage(LogUtil.MessageLevel.Message, format, args);

    public static void Diagnostic(string message) => LogUtil.LogMessage(LogUtil.MessageLevel.Debug, message);

    public static void Diagnostic(string format, params object[] args) => LogUtil.LogMessage(LogUtil.MessageLevel.Debug, format, args);

    public static void LogCopyright() => Console.WriteLine(CommonUtils.GetCopyrightString());

    public static void IULogTo(
      LogUtil.InteropLogString errorLogger,
      LogUtil.InteropLogString warningLogger,
      LogUtil.InteropLogString msgLogger,
      LogUtil.InteropLogString debugLogger)
    {
      LogUtil._iuErrorLogger = errorLogger;
      LogUtil._iuWarningLogger = warningLogger;
      LogUtil._iuMsgLogger = msgLogger;
      LogUtil._iuDebugLogger = debugLogger;
      LogUtil.IU_LogTo(errorLogger, warningLogger, msgLogger, debugLogger);
    }

    private enum MessageLevel
    {
      Error,
      Warning,
      Message,
      Debug,
    }

    public delegate void InteropLogString([MarshalAs(UnmanagedType.LPWStr)] string outputStr);
  }
}
