// Decompiled with JetBrains decompiler
// Type: Microsoft.Tools.IO.LongPathCommon
// Assembly: Microsoft.Tools.IO, Version=1.1.11.0, Culture=neutral, PublicKeyToken=1a5b963c6f0fbeab
// MVID: 222C54A4-0FE1-469A-8627-EF94B226BBFA
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Tools.IO.dll

using Microsoft.Win32.SafeHandles;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Tools.IO
{
  internal static class LongPathCommon
  {
    internal static string NormalizeSearchPattern(string searchPattern) => string.IsNullOrEmpty(searchPattern) || searchPattern == "." ? "*" : searchPattern;

    internal static string NormalizeLongPath(string path) => LongPathCommon.NormalizeLongPath(path, nameof (path));

    internal static string NormalizeLongPath(string path, string parameterName)
    {
      switch (path)
      {
        case "":
          throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "'{0}' cannot be an empty string.", new object[1]
          {
            (object) parameterName
          }), parameterName);
        case null:
          throw new ArgumentNullException(parameterName);
        default:
          StringBuilder lpBuffer = new StringBuilder(path.Length + 1);
          uint fullPathName = Microsoft.Tools.IO.Interop.NativeMethods.GetFullPathName(path, (uint) lpBuffer.Capacity, lpBuffer, IntPtr.Zero);
          if ((long) fullPathName > (long) lpBuffer.Capacity)
          {
            lpBuffer.Capacity = (int) fullPathName;
            fullPathName = Microsoft.Tools.IO.Interop.NativeMethods.GetFullPathName(path, fullPathName, lpBuffer, IntPtr.Zero);
          }
          if (fullPathName == 0U)
            throw LongPathCommon.GetExceptionFromLastWin32Error(parameterName);
          if (fullPathName > 32000U)
            throw LongPathCommon.GetExceptionFromWin32Error(206, parameterName);
          return LongPathCommon.AddLongPathPrefix(lpBuffer.ToString());
      }
    }

    private static bool TryNormalizeLongPath(string path, out string result)
    {
      try
      {
        result = LongPathCommon.NormalizeLongPath(path);
        return true;
      }
      catch (ArgumentException ex)
      {
      }
      catch (PathTooLongException ex)
      {
      }
      result = (string) null;
      return false;
    }

    private static string AddLongPathPrefix(string path) => !path.StartsWith("\\\\") ? "\\\\?\\" + path : "\\\\?\\UNC\\" + path.Substring(2);

    internal static string RemoveLongPathPrefix(string normalizedPath) => !normalizedPath.StartsWith("\\\\?\\UNC\\") ? normalizedPath.Substring("\\\\?\\".Length) : "\\\\" + normalizedPath.Substring("\\\\?\\UNC\\".Length);

    internal static bool Exists(string path, out bool isDirectory)
    {
      string result;
      FileAttributes attributes;
      if (LongPathCommon.TryNormalizeLongPath(path, out result) && LongPathCommon.TryGetFileAttributes(result, out attributes) == 0)
      {
        isDirectory = LongPathDirectory.IsDirectory(attributes);
        return true;
      }
      isDirectory = false;
      return false;
    }

    internal static int TryGetDirectoryAttributes(
      string normalizedPath,
      out FileAttributes attributes)
    {
      int directoryAttributes = LongPathCommon.TryGetFileAttributes(normalizedPath, out attributes);
      if (!LongPathDirectory.IsDirectory(attributes))
        directoryAttributes = 267;
      return directoryAttributes;
    }

    internal static int TryGetFileAttributes(string normalizedPath, out FileAttributes attributes)
    {
      attributes = Microsoft.Tools.IO.Interop.NativeMethods.GetFileAttributes(normalizedPath);
      return attributes == (FileAttributes) -1 ? Marshal.GetLastWin32Error() : 0;
    }

    internal static FileAttributes GetAttributes(string path) => (FileAttributes) LongPathCommon.GetWin32FileAttributeData(LongPathCommon.NormalizeLongPath(path)).fileAttributes;

    internal static void SetAttributes(string path, FileAttributes attributes)
    {
      if (Microsoft.Tools.IO.Interop.NativeMethods.SetFileAttributes(LongPathCommon.NormalizeLongPath(path), (int) attributes))
        return;
      int lastWin32Error = Marshal.GetLastWin32Error();
      switch (lastWin32Error)
      {
        case 5:
          throw new ArgumentException("Access to the path is denied.");
        case 87:
          throw new ArgumentException("Invalid File or Directory attributes value.");
        default:
          throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
      }
    }

    internal static void SetFileTimes(
      SafeFileHandle hFile,
      long creationTime,
      long accessTime,
      long writeTime)
    {
      if (!Microsoft.Tools.IO.Interop.NativeMethods.SetFileTime(hFile, ref creationTime, ref accessTime, ref writeTime))
        throw LongPathCommon.GetExceptionFromWin32Error(Marshal.GetLastWin32Error());
    }

    internal static Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FILE_ATTRIBUTE_DATA GetWin32FileAttributeData(
      string normalizedPath)
    {
      Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FILE_ATTRIBUTE_DATA data = new Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FILE_ATTRIBUTE_DATA();
      int errorCode = LongPathCommon.FillAttributeInfo(normalizedPath, ref data, false, false);
      if (errorCode != 0)
        throw LongPathCommon.GetExceptionFromWin32Error(errorCode);
      return data;
    }

    internal static int FillAttributeInfo(
      string normalizedLongPath,
      ref Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FILE_ATTRIBUTE_DATA data,
      bool tryagain,
      bool returnErrorOnNotFound)
    {
      int num = 0;
      if (tryagain)
      {
        string lpFileName = normalizedLongPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        int newMode = Microsoft.Tools.IO.Interop.NativeMethods.SetErrorMode(1);
        Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FIND_DATA lpFindFileData;
        try
        {
          bool flag = false;
          Microsoft.Tools.IO.Interop.SafeFindHandle firstFile = Microsoft.Tools.IO.Interop.NativeMethods.FindFirstFile(lpFileName, out lpFindFileData);
          try
          {
            if (firstFile.IsInvalid)
            {
              flag = true;
              num = Marshal.GetLastWin32Error();
              switch (num)
              {
                case 2:
                case 3:
                case 21:
                  if (!returnErrorOnNotFound)
                  {
                    num = 0;
                    data.fileAttributes = -1;
                    break;
                  }
                  break;
              }
              return num;
            }
          }
          finally
          {
            try
            {
              firstFile.Close();
            }
            catch
            {
              if (!flag)
                throw LongPathCommon.GetExceptionFromLastWin32Error("handle");
            }
          }
        }
        finally
        {
          Microsoft.Tools.IO.Interop.NativeMethods.SetErrorMode(newMode);
        }
        data.PopulateFrom(lpFindFileData);
      }
      else
      {
        int newMode = Microsoft.Tools.IO.Interop.NativeMethods.SetErrorMode(1);
        bool fileAttributesEx;
        try
        {
          fileAttributesEx = Microsoft.Tools.IO.Interop.NativeMethods.GetFileAttributesEx(normalizedLongPath, 0, ref data);
        }
        finally
        {
          Microsoft.Tools.IO.Interop.NativeMethods.SetErrorMode(newMode);
        }
        if (!fileAttributesEx)
        {
          num = Marshal.GetLastWin32Error();
          switch (num)
          {
            case 2:
            case 3:
            case 21:
              if (!returnErrorOnNotFound)
              {
                num = 0;
                data.fileAttributes = -1;
                break;
              }
              break;
            default:
              return LongPathCommon.FillAttributeInfo(normalizedLongPath, ref data, true, returnErrorOnNotFound);
          }
        }
      }
      return num;
    }

    internal static Microsoft.Tools.IO.Interop.NativeMethods.EFileAccess GetUnderlyingAccess(
      FileAccess access)
    {
      switch (access)
      {
        case FileAccess.Read:
          return Microsoft.Tools.IO.Interop.NativeMethods.EFileAccess.GenericRead;
        case FileAccess.Write:
          return Microsoft.Tools.IO.Interop.NativeMethods.EFileAccess.GenericWrite;
        case FileAccess.ReadWrite:
          return Microsoft.Tools.IO.Interop.NativeMethods.EFileAccess.GenericRead | Microsoft.Tools.IO.Interop.NativeMethods.EFileAccess.GenericWrite;
        default:
          throw new ArgumentOutOfRangeException(nameof (access));
      }
    }

    internal static Exception GetExceptionFromLastWin32Error(string parameterName = "path") => LongPathCommon.GetExceptionFromWin32Error(Marshal.GetLastWin32Error(), parameterName);

    internal static Exception GetExceptionFromWin32Error(
      int errorCode,
      string parameterName = "path")
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
          return (Exception) new IOException(messageFromErrorCode, Microsoft.Tools.IO.Interop.NativeMethods.MakeHRFromErrorCode(errorCode));
      }
    }

    private static string GetMessageFromErrorCode(int errorCode)
    {
      StringBuilder stringBuilder = new StringBuilder(512);
      IntPtr zero1 = IntPtr.Zero;
      int dwMessageId = errorCode;
      StringBuilder lpBuffer = stringBuilder;
      int capacity = lpBuffer.Capacity;
      IntPtr zero2 = IntPtr.Zero;
      Microsoft.Tools.IO.Interop.NativeMethods.FormatMessage(12800, zero1, dwMessageId, 0, lpBuffer, capacity, zero2);
      return stringBuilder.ToString();
    }
  }
}
