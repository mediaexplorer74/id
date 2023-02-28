// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.Helper.NativeMethods
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Phone.Test.TestMetadata.Helper
{
  internal static class NativeMethods
  {
    public const uint FileShareRead = 1;
    public const uint FileShareWrite = 2;
    public const uint FileShareDelete = 4;
    public const uint OpenExisting = 3;
    public const uint GenericRead = 2147483648;
    public const uint GenericWrite = 1073741824;
    public const uint FileFlagNoBuffering = 536870912;
    public const uint FileReadAttributes = 128;
    public const uint FileWriteAttributes = 256;
    public const uint ErrorInsufficientBuffer = 122;
    public const int InvalidHandleValue = -1;
    public const int ErrorAlreadyExists = 183;
    public const uint PageReadonly = 2;
    public const uint FileMapRead = 4;
    public const ushort ImageDirectoryEntryExport = 0;
    public const ushort ImageDirectoryEntryImport = 1;
    public const ushort ImageDirectoryEntryDelayLoadImport = 13;
    public const ushort ImageDirectoryEntryComDescriptor = 14;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr CreateFile(
      string lpFileName,
      uint dwDesiredAccess,
      uint dwShareMode,
      IntPtr lpSecurityAttributes,
      uint dwCreationDisposition,
      uint dwFlagsAndAttributes,
      IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr CreateFileMapping(
      IntPtr hFile,
      IntPtr pFileMappigAttributes,
      uint flProtect,
      uint dwMaximumSizeHigh,
      uint dwMaximumSizeLow,
      string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint GetFileSize(IntPtr hFile, ref uint lpFileSizeHigh);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr MapViewOfFile(
      IntPtr hFileMappingObject,
      uint dwDesiredAccess,
      uint dwFileOffsetHigh,
      uint dwFileOffsetLow,
      IntPtr dwNumberOfBytesToMap);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int UnmapViewOfFile(IntPtr lpBaseAddress);

    [DllImport("dbgHelp.dll", SetLastError = true)]
    public static extern IntPtr ImageDirectoryEntryToDataEx(
      IntPtr imageBase,
      int mappedAsImage,
      ushort directoryEntry,
      [In, Out] ref uint size,
      [In, Out] ref IntPtr foundHeader);

    [DllImport("dbgHelp.dll", SetLastError = true)]
    public static extern IntPtr ImageNtHeader(IntPtr imageBase);

    [DllImport("dbgHelp.dll", SetLastError = true)]
    public static extern IntPtr ImageRvaToVa(
      IntPtr ntHeaders,
      IntPtr imageBase,
      uint rva,
      IntPtr lastRvaSection);
  }
}
