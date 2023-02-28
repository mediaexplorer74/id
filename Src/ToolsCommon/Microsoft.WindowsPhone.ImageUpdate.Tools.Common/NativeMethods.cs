// NativeMethods.cs
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.NativeMethods
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  internal static class NativeMethods
  {
    private const string STRING_IUCOMMON_DLL = "IUCommon.dll";
    private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;
    private const CharSet CHAR_SET = CharSet.Unicode;
    internal const int ERROR_FILE_NOT_FOUND = 2;
    internal const int ERROR_PATH_NOT_FOUND = 3;
    internal const int ERROR_ACCESS_DENIED = 5;
    internal const int ERROR_INVALID_DRIVE = 15;
    internal const int ERROR_NO_MORE_FILES = 18;
    internal const int ERROR_INVALID_NAME = 123;
    internal const int ERROR_ALREADY_EXISTS = 183;
    internal const int ERROR_FILENAME_EXCED_RANGE = 206;
    internal const int ERROR_DIRECTORY = 267;
    internal const int ERROR_OPERATION_ABORTED = 995;
    internal const int INVALID_FILE_ATTRIBUTES = -1;
    internal const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;
    internal const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;
    internal const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;
    private const string STRING_KERNEL32_DLL = "kernel32.dll";

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int IU_GetCanonicalUNCPath(
      string strPath,
      StringBuilder pathBuffer,
      int cchPathBuffer);

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int IU_FreeStringList(IntPtr rgFiles, int cFiles);

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IU_FileExists(string strFile);

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IU_DirectoryExists(string strDir);

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void IU_EnsureDirectoryExists(string path);

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void IU_CleanDirectory(string strPath, [MarshalAs(UnmanagedType.Bool)] bool bRemoveDirectory);

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int IU_GetAllFiles(
      string strFolder,
      string strSearchPattern,
      [MarshalAs(UnmanagedType.Bool)] bool fRecursive,
      out IntPtr rgFiles,
      out int cFiles);

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int IU_GetAllDirectories(
      string strFolder,
      string strSearchPattern,
      [MarshalAs(UnmanagedType.Bool)] bool fRecursive,
      out IntPtr rgDirectories,
      out int cDirectories);

    internal static int MakeHRFromErrorCode(int errorCode) => -2147024896 | errorCode;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int FormatMessage(
      int dwFlags,
      IntPtr lpSource,
      int dwMessageId,
      int dwLanguageId,
      StringBuilder lpBuffer,
      int nSize,
      IntPtr va_list_arguments);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteFile(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool RemoveDirectory(string lpPathName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool MoveFile(string lpPathNameFrom, string lpPathNameTo);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CopyFile(string src, string dst, [MarshalAs(UnmanagedType.Bool)] bool failIfExists);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern SafeFileHandle CreateFile(
      string lpFileName,
      NativeMethods.EFileAccess dwDesiredAccess,
      uint dwShareMode,
      IntPtr lpSecurityAttributes,
      uint dwCreationDisposition,
      uint dwFlagsAndAttributes,
      IntPtr hTemplateFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern FileAttributes GetFileAttributes(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool SetFileAttributes(string lpFileName, FileAttributes attributes);

    [Flags]
    internal enum EFileAccess : uint
    {
      GenericRead = 2147483648, // 0x80000000
      GenericWrite = 1073741824, // 0x40000000
      GenericExecute = 536870912, // 0x20000000
      GenericAll = 268435456, // 0x10000000
    }
  }
}
