// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathCommon
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  internal static class LongPathCommon
  {
    private static int MAX_LONG_PATH = 32000;

    internal static Exception GetExceptionFromLastWin32Error() => LongPathCommon.GetExceptionFromLastWin32Error("path");

    internal static Exception GetExceptionFromLastWin32Error(string parameterName) => LongPathCommon.GetExceptionFromWin32Error(Marshal.GetLastWin32Error(), parameterName);

    internal static Exception GetExceptionFromWin32Error(int errorCode) => LongPathCommon.GetExceptionFromWin32Error(errorCode, "path");

    internal static Exception GetExceptionFromWin32Error(
      int errorCode,
      string parameterName)
    {
      string messageFromErrorCode = LongPathCommon.GetMessageFromErrorCode(errorCode);
      switch (errorCode)
      {
        case 2:
          return (Exception) new FileNotFoundException(messageFromErrorCode);
        case 3:
          return (Exception) new DirectoryNotFoundException(messageFromErrorCode);
        case 5:
          return (Exception) new UnauthorizedAccessException(messageFromErrorCode);
        case 15:
          return (Exception) new DriveNotFoundException(messageFromErrorCode);
        case 123:
          return (Exception) new ArgumentException(messageFromErrorCode, parameterName);
        case 206:
          return (Exception) new PathTooLongException(messageFromErrorCode);
        case 995:
          return (Exception) new OperationCanceledException(messageFromErrorCode);
        default:
          return (Exception) new IOException(messageFromErrorCode, NativeMethods.MakeHRFromErrorCode(errorCode));
      }
    }

    private static string GetMessageFromErrorCode(int errorCode)
    {
      StringBuilder lpBuffer = new StringBuilder(512);
      NativeMethods.FormatMessage(12800, IntPtr.Zero, errorCode, 0, lpBuffer, lpBuffer.Capacity, IntPtr.Zero);
      return lpBuffer.ToString();
    }

    internal static string[] ConvertPtrArrayToStringArray(IntPtr strPtrArray, int cStrings)
    {
      IntPtr[] destination = new IntPtr[cStrings];
      if (strPtrArray != IntPtr.Zero)
        Marshal.Copy(strPtrArray, destination, 0, cStrings);
      List<string> stringList = new List<string>(cStrings);
      for (int index = 0; index < cStrings; ++index)
        stringList.Add(Marshal.PtrToStringUni(destination[index]));
      return stringList.ToArray();
    }

    public static string NormalizeLongPath(string path)
    {
      StringBuilder pathBuffer = new StringBuilder(LongPathCommon.MAX_LONG_PATH);
      int canonicalUncPath = NativeMethods.IU_GetCanonicalUNCPath(path, pathBuffer, pathBuffer.Capacity);
      if (canonicalUncPath != 0)
        throw LongPathCommon.GetExceptionFromWin32Error(canonicalUncPath);
      return pathBuffer.ToString();
    }

    public static FileAttributes GetAttributes(string path)
    {
      FileAttributes fileAttributes = NativeMethods.GetFileAttributes(LongPathCommon.NormalizeLongPath(path));
      return fileAttributes != (FileAttributes) -1 ? fileAttributes : throw LongPathCommon.GetExceptionFromLastWin32Error();
    }
  }
}
