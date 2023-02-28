// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.VIRTUAL_DISK_ACCESS_MASK
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public enum VIRTUAL_DISK_ACCESS_MASK
  {
    VIRTUAL_DISK_ACCESS_ATTACH_RO = 65536, // 0x00010000
    VIRTUAL_DISK_ACCESS_ATTACH_RW = 131072, // 0x00020000
    VIRTUAL_DISK_ACCESS_DETACH = 262144, // 0x00040000
    VIRTUAL_DISK_ACCESS_GET_INFO = 524288, // 0x00080000
    VIRTUAL_DISK_ACCESS_READ = 851968, // 0x000D0000
    VIRTUAL_DISK_ACCESS_CREATE = 1048576, // 0x00100000
    VIRTUAL_DISK_ACCESS_METAOPS = 2097152, // 0x00200000
    VIRTUAL_DISK_ACCESS_WRITABLE = 3276800, // 0x00320000
    VIRTUAL_DISK_ACCESS_ALL = 4128768, // 0x003F0000
  }
}
