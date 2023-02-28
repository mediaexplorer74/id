// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.IULogger
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Globalization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class IULogger
  {
    private static LogString _defaultLogger = new LogString(IULogger.LogToConsole);
    private LogString _logError = IULogger.DefaultLogger;
    private LogString _logWarning = IULogger.DefaultLogger;
    private LogString _logInfo = IULogger.DefaultLogger;
    private LogString _logDebug = IULogger.DefaultLogger;

    public static LogString DefaultLogger
    {
      get
      {
        if (IULogger._defaultLogger == null)
          IULogger._defaultLogger = new LogString(IULogger.LogToConsole);
        return IULogger._defaultLogger;
      }
      set
      {
        if (value != null)
          IULogger._defaultLogger = value;
        else
          IULogger._defaultLogger = new LogString(IULogger.LogToNull);
      }
    }

    private static void LogToConsole(string format, params object[] list) => Console.WriteLine(string.Format((IFormatProvider) CultureInfo.CurrentCulture, format, list));

    private static void LogToNull(string format, params object[] list)
    {
    }

    public void LogError(string format, params object[] list) => this._logError(format, list);

    public void LogWarning(string format, params object[] list) => this._logWarning(format, list);

    public void LogInfo(string format, params object[] list) => this._logInfo(format, list);

    public void LogDebug(string format, params object[] list) => this._logDebug(format, list);

    public LogString ErrorLogger
    {
      get => this._logError;
      set
      {
        if (value == null)
          this._logError = new LogString(IULogger.LogToNull);
        else
          this._logError = value;
      }
    }

    public LogString WarningLogger
    {
      get => this._logWarning;
      set
      {
        if (value == null)
          this._logWarning = new LogString(IULogger.LogToNull);
        else
          this._logWarning = value;
      }
    }

    public LogString InformationLogger
    {
      get => this._logInfo;
      set
      {
        if (value == null)
          this._logInfo = new LogString(IULogger.LogToNull);
        else
          this._logInfo = value;
      }
    }

    public LogString DebugLogger
    {
      get => this._logDebug;
      set
      {
        if (value == null)
          this._logDebug = new LogString(IULogger.LogToNull);
        else
          this._logDebug = value;
      }
    }
  }
}
