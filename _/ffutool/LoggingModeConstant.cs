// Decompiled with JetBrains decompiler
// Type: FFUTool.LoggingModeConstant
// Assembly: FFUTool, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E16401E-1D4B-42FF-8522-F3B0C09CB0D5
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ffutool.exe

using System;

namespace FFUTool
{
  [Flags]
  internal enum LoggingModeConstant : uint
  {
    PrivateLoggerMode = 2048, // 0x00000800
    PrivateInProc = 131072, // 0x00020000
  }
}
