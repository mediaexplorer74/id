﻿// Decompiled with JetBrains decompiler
// Type: FFUComponents.PropertyChangeParams
// Assembly: FFUComponents, Version=8.0.0.0, Culture=neutral, PublicKeyToken=5d653a1a5ba069fd
// MVID: 079409EC-FC99-4988-8EB4-20A87B1EBA8C
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\FFUComponents.dll

using System.Runtime.InteropServices;

namespace FFUComponents
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal struct PropertyChangeParams
  {
    public ClassInstallHeader Header;
    public uint StateChange;
    public uint Scope;
    public uint HwProfile;
  }
}