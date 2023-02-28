// Decompiled with JetBrains decompiler
// Type: FFUTool.EventTraceProperties
// Assembly: FFUTool, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E16401E-1D4B-42FF-8522-F3B0C09CB0D5
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ffutool.exe

using System;
using System.Runtime.InteropServices;

namespace FFUTool
{
  [Serializable]
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  internal struct EventTraceProperties
  {
    public const int MaxLoggerNameLength = 260;
    public EventTraceProperties.EventTracePropertiesCore CoreProperties;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    private string loggerName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    private string logFileName;

    internal static EventTraceProperties CreateProperties(
      string sessionName,
      string logFilePath,
      LoggingModeConstant logMode)
    {
      uint num = (uint) Marshal.SizeOf(typeof (EventTraceProperties));
      EventTraceProperties properties = new EventTraceProperties()
      {
        CoreProperties = {
          Wnode = new EventTraceProperties.WNodeHeader()
        }
      };
      properties.CoreProperties.Wnode.BufferSize = num;
      properties.CoreProperties.Wnode.Flags = 131072U;
      properties.CoreProperties.Wnode.Guid = Guid.NewGuid();
      properties.CoreProperties.BufferSize = 64U;
      properties.CoreProperties.MinimumBuffers = 5U;
      properties.CoreProperties.MaximumBuffers = 200U;
      properties.CoreProperties.FlushTimer = 0U;
      properties.CoreProperties.LogFileMode = logMode;
      if (logFilePath != null && logFilePath.Length < 260)
        properties.logFileName = logFilePath;
      properties.CoreProperties.LogFileNameOffset = (uint) (int) Marshal.OffsetOf(typeof (EventTraceProperties), "logFileName");
      if (sessionName != null && sessionName.Length < 260)
        properties.loggerName = sessionName;
      properties.CoreProperties.LoggerNameOffset = (uint) (int) Marshal.OffsetOf(typeof (EventTraceProperties), "loggerName");
      return properties;
    }

    internal static EventTraceProperties CreateProperties() => EventTraceProperties.CreateProperties((string) null, (string) null, (LoggingModeConstant) 0);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WNodeHeader
    {
      public uint BufferSize;
      public uint ProviderId;
      public ulong HistoricalContext;
      public long TimeStamp;
      public Guid Guid;
      public uint ClientContext;
      public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct EventTracePropertiesCore
    {
      public EventTraceProperties.WNodeHeader Wnode;
      public uint BufferSize;
      public uint MinimumBuffers;
      public uint MaximumBuffers;
      public uint MaximumFileSize;
      public LoggingModeConstant LogFileMode;
      public uint FlushTimer;
      public uint EnableFlags;
      public int AgeLimit;
      public uint NumberOfBuffers;
      public uint FreeBuffers;
      public uint EventsLost;
      public uint BuffersWritten;
      public uint LogBuffersLost;
      public uint RealTimeBuffersLost;
      public IntPtr LoggerThreadId;
      public uint LogFileNameOffset;
      public uint LoggerNameOffset;
    }
  }
}
