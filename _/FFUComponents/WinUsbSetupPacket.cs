﻿// Decompiled with JetBrains decompiler
// Type: FFUComponents.WinUsbSetupPacket
// Assembly: FFUComponents, Version=8.0.0.0, Culture=neutral, PublicKeyToken=5d653a1a5ba069fd
// MVID: 079409EC-FC99-4988-8EB4-20A87B1EBA8C
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\FFUComponents.dll

using System.Runtime.InteropServices;

namespace FFUComponents
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal struct WinUsbSetupPacket
  {
    public byte RequestType;
    public byte Request;
    public ushort Value;
    public ushort Index;
    public ushort Length;
  }
}
