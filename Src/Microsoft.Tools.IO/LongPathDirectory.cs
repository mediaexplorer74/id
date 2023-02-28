// Decompiled with JetBrains decompiler
// Type: Microsoft.Tools.IO.LongPathDirectory
// Assembly: Microsoft.Tools.IO, Version=1.1.11.0, Culture=neutral, PublicKeyToken=1a5b963c6f0fbeab
// MVID: 222C54A4-0FE1-469A-8627-EF94B226BBFA
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Tools.IO.dll

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Tools.IO
{
  public static class LongPathDirectory
  {
    private const int FILE_FLAG_BACKUP_SEMANTICS = 33554432;

    public static void Create(string path) => LongPathDirectory.InternalCreateDirectory(LongPathCommon.NormalizeLongPath(path), path);

    public static void Delete(string path) => LongPathDirectory.InternalDelete(LongPathCommon.NormalizeLongPath(path), path, false);

    public static void Delete(string path, bool recursive) => LongPathDirectory.InternalDelete(LongPathCommon.NormalizeLongPath(path), path, recursive);

    public static bool Exists(string path)
    {
      bool isDirectory;
      return LongPathCommon.Exists(path, out isDirectory) && isDirectory;
    }

    public static IEnumerable<string> EnumerateDirectories(string path) => path != null ? LongPathDirectory.InternalEnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly) : throw new ArgumentNullException(nameof (path));

    public static IEnumerable<string> EnumerateDirectories(
      string path,
      string searchPattern)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      return searchPattern != null ? LongPathDirectory.InternalEnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly) : throw new ArgumentNullException(nameof (searchPattern));
    }

    public static IEnumerable<string> EnumerateDirectories(
      string path,
      string searchPattern,
      SearchOption searchOption)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (searchPattern == null)
        throw new ArgumentNullException(nameof (searchPattern));
      if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
        throw new ArgumentOutOfRangeException(nameof (searchOption), "Enum value was out of legal range");
      return LongPathDirectory.InternalEnumerateDirectories(path, searchPattern, searchOption);
    }

    private static IEnumerable<string> InternalEnumerateDirectories(
      string path,
      string searchPattern,
      SearchOption searchOption)
    {
      return LongPathDirectory.EnumerateFileSystemNames(path, searchPattern, searchOption, false, true);
    }

    public static IEnumerable<string> EnumerateFiles(string path) => path != null ? LongPathDirectory.InternalEnumerateFiles(path, "*", SearchOption.TopDirectoryOnly) : throw new ArgumentNullException(nameof (path));

    public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      return searchPattern != null ? LongPathDirectory.InternalEnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly) : throw new ArgumentNullException(nameof (searchPattern));
    }

    public static IEnumerable<string> EnumerateFiles(
      string path,
      string searchPattern,
      SearchOption searchOption)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (searchPattern == null)
        throw new ArgumentNullException(nameof (searchPattern));
      if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
        throw new ArgumentOutOfRangeException(nameof (searchOption), "Enum value was out of legal range");
      return LongPathDirectory.InternalEnumerateFiles(path, searchPattern, searchOption);
    }

    private static IEnumerable<string> InternalEnumerateFiles(
      string path,
      string searchPattern,
      SearchOption searchOption)
    {
      return LongPathDirectory.EnumerateFileSystemNames(path, searchPattern, searchOption, true, false);
    }

    public static IEnumerable<string> EnumerateFileSystemEntries(string path) => path != null ? LongPathDirectory.InternalEnumerateFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly) : throw new ArgumentNullException(nameof (path));

    public static IEnumerable<string> EnumerateFileSystemEntries(
      string path,
      string searchPattern)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      return searchPattern != null ? LongPathDirectory.InternalEnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly) : throw new ArgumentNullException(nameof (searchPattern));
    }

    public static IEnumerable<string> EnumerateFileSystemEntries(
      string path,
      string searchPattern,
      SearchOption searchOption)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (searchPattern == null)
        throw new ArgumentNullException(nameof (searchPattern));
      if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
        throw new ArgumentOutOfRangeException(nameof (searchOption), "Enum value was out of legal range");
      return LongPathDirectory.InternalEnumerateFileSystemEntries(path, searchPattern, searchOption);
    }

    private static IEnumerable<string> InternalEnumerateFileSystemEntries(
      string path,
      string searchPattern,
      SearchOption searchOption)
    {
      return LongPathDirectory.EnumerateFileSystemNames(path, searchPattern, searchOption, true, true);
    }

    private static IEnumerable<string> EnumerateFileSystemNames(
      string path,
      string searchPattern,
      SearchOption searchOption,
      bool includeFiles,
      bool includeDirs)
    {
      return LongPathDirectory.EnumerateFileSystemEntries(path, searchPattern, includeFiles, includeDirs, searchOption);
    }

    public static DateTime GetCreationTime(string path) => LongPathFile.GetCreationTime(path);

    public static void SetCreationTime(string path, DateTime creationTime) => LongPathDirectory.SetCreationTimeUtc(path, creationTime.ToUniversalTime());

    public static DateTime GetCreationTimeUtc(string path) => LongPathFile.GetCreationTimeUtc(path);

    public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
    {
      using (SafeFileHandle hFile = LongPathDirectory.OpenHandle(path))
        LongPathCommon.SetFileTimes(hFile, creationTimeUtc.ToFileTimeUtc(), 0L, 0L);
    }

    public static DateTime GetLastAccessTime(string path) => LongPathFile.GetLastAccessTime(path);

    public static void SetLastAccessTime(string path, DateTime lastAccessTime) => LongPathDirectory.SetLastAccessTimeUtc(path, lastAccessTime.ToUniversalTime());

    public static DateTime GetLastAccessTimeUtc(string path) => LongPathFile.GetLastAccessTimeUtc(path);

    public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
    {
      using (SafeFileHandle hFile = LongPathDirectory.OpenHandle(path))
        LongPathCommon.SetFileTimes(hFile, 0L, lastAccessTimeUtc.ToFileTimeUtc(), 0L);
    }

    public static DateTime GetLastWriteTime(string path) => LongPathFile.GetLastWriteTime(path);

    public static void SetLastWriteTime(string path, DateTime lastWriteTime) => LongPathDirectory.SetLastWriteTimeUtc(path, lastWriteTime.ToUniversalTime());

    public static DateTime GetLastWriteTimeUtc(string path) => LongPathFile.GetLastWriteTimeUtc(path);

    public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
    {
      using (SafeFileHandle hFile = LongPathDirectory.OpenHandle(path))
        LongPathCommon.SetFileTimes(hFile, 0L, 0L, lastWriteTimeUtc.ToFileTimeUtc());
    }

    public static FileAttributes GetAttributes(string path) => LongPathCommon.GetAttributes(path);

    public static void SetAttributes(string path, FileAttributes directoryAttributes) => LongPathCommon.SetAttributes(path, directoryAttributes);

    private static IEnumerable<string> EnumerateFileSystemEntries(
      string path,
      string searchPattern,
      bool includeFiles,
      bool includeDirectories,
      SearchOption searchOption)
    {
      string normalizedSearchPattern = LongPathDirectory.NormalizeSearchPatternForIterator(searchPattern);
      string normalizedPath = LongPathCommon.NormalizeLongPath(path);
      int directoryAttributes = LongPathCommon.TryGetDirectoryAttributes(normalizedPath, out FileAttributes _);
      if (directoryAttributes != 0)
        throw LongPathCommon.GetExceptionFromWin32Error(directoryAttributes);
      return LongPathDirectory.EnumerateFileSystemIterator(normalizedPath, normalizedSearchPattern, includeFiles, includeDirectories, searchOption);
    }

    private static string NormalizeSearchPatternForIterator(string searchPattern)
    {
      string searchPattern1 = searchPattern.TrimEnd(LongPathPath.TrimEndChars);
      if (searchPattern1.Equals("."))
        searchPattern1 = "*";
      LongPathDirectory.CheckSearchPattern(searchPattern1);
      return searchPattern1;
    }

    [SecuritySafeCritical]
    internal static void CheckSearchPattern(string searchPattern)
    {
      int num;
      for (; (num = searchPattern.IndexOf("..", StringComparison.Ordinal)) != -1; searchPattern = searchPattern.Substring(num + 2))
      {
        if (num + 2 == searchPattern.Length)
          throw new ArgumentException("Search pattern cannot contain '..' to move up directories and can be contained only internally in file/directory names, as in 'a..b'.");
        if ((int) searchPattern[num + 2] == (int) LongPathPath.DirectorySeparatorChar || (int) searchPattern[num + 2] == (int) LongPathPath.AltDirectorySeparatorChar)
          throw new ArgumentException("Search pattern cannot contain '..' to move up directories and can be contained only internally in file/directory names, as in 'a..b'.");
      }
    }

    private static IEnumerable<string> EnumerateFileSystemIterator(
      string normalizedPath,
      string normalizedSearchPattern,
      bool includeFiles,
      bool includeDirectories,
      SearchOption searchOption)
    {
      if (normalizedSearchPattern.Length != 0)
      {
        Queue<string> directoryQueue = new Queue<string>();
        directoryQueue.Enqueue(normalizedPath);
        while (directoryQueue.Count > 0)
        {
          normalizedPath = directoryQueue.Dequeue();
          string path = LongPathCommon.RemoveLongPathPrefix(normalizedPath);
          if (searchOption == SearchOption.AllDirectories)
          {
            foreach (string enumerateDirectory in LongPathDirectory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
              directoryQueue.Enqueue(LongPathCommon.NormalizeLongPath(enumerateDirectory));
          }
          Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FIND_DATA wiN32FindData;
          using (Microsoft.Tools.IO.Interop.SafeFindHandle handle = LongPathDirectory.BeginFind(Path.Combine(normalizedPath, normalizedSearchPattern), out wiN32FindData))
          {
            if (handle == null)
            {
              if (searchOption == SearchOption.TopDirectoryOnly)
                break;
              continue;
            }
            do
            {
              string currentFileName = wiN32FindData.cFileName;
              if (LongPathDirectory.IsDirectory(wiN32FindData.dwFileAttributes))
              {
                if (!LongPathDirectory.IsCurrentOrParentDirectory(currentFileName))
                {
                  if (searchOption == SearchOption.AllDirectories)
                  {
                    string str = Path.Combine(normalizedPath, currentFileName);
                    if (!directoryQueue.Contains(str))
                      directoryQueue.Enqueue(str);
                  }
                  if (includeDirectories)
                    yield return Path.Combine(path, currentFileName);
                }
              }
              else if (includeFiles)
                yield return Path.Combine(path, currentFileName);
              currentFileName = (string) null;
            }
            while (Microsoft.Tools.IO.Interop.NativeMethods.FindNextFile(handle, out wiN32FindData));
            int lastWin32Error = Marshal.GetLastWin32Error();
            if (lastWin32Error != 18)
              throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
          }
          path = (string) null;
        }
      }
    }

    private static Microsoft.Tools.IO.Interop.SafeFindHandle BeginFind(
      string normalizedPathWithSearchPattern,
      out Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FIND_DATA findData)
    {
      Microsoft.Tools.IO.Interop.SafeFindHandle firstFile = Microsoft.Tools.IO.Interop.NativeMethods.FindFirstFile(normalizedPathWithSearchPattern, out findData);
      if (!firstFile.IsInvalid)
        return firstFile;
      int lastWin32Error = Marshal.GetLastWin32Error();
      if (lastWin32Error != 2)
        throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
      return (Microsoft.Tools.IO.Interop.SafeFindHandle) null;
    }

    internal static bool IsDirectory(FileAttributes attributes) => (attributes & FileAttributes.Directory) == FileAttributes.Directory;

    internal static void InternalCreateDirectory(string normalizedPath, string path)
    {
      int length = normalizedPath.Length;
      if (length >= 2 && LongPathPath.IsDirectorySeparator(normalizedPath[length - 1]))
        --length;
      int rootLength = LongPathPath.GetRootLength(normalizedPath);
      if (length == 2 && LongPathPath.IsDirectorySeparator(normalizedPath[1]))
        throw new IOException(string.Format("The specified directory '{0}' cannot be created.", (object) path));
      List<string> stringList1 = new List<string>();
      bool flag1 = false;
      if (length > rootLength)
      {
        for (int index = length - 1; index >= rootLength && !flag1; --index)
        {
          string normalizedPath1 = normalizedPath.Substring(0, index + 1);
          if (!LongPathDirectory.InternalExists(normalizedPath1))
            stringList1.Add(normalizedPath1);
          else
            flag1 = true;
          while (index > rootLength && (int) normalizedPath[index] != (int) Path.DirectorySeparatorChar && (int) normalizedPath[index] != (int) Path.AltDirectorySeparatorChar)
            --index;
        }
      }
      int count = stringList1.Count;
      if (stringList1.Count != 0)
      {
        string[] array = new string[stringList1.Count];
        stringList1.CopyTo(array, 0);
        for (int index = 0; index < array.Length; ++index)
        {
          // ISSUE: explicit reference operation
          ^ref array[index] += "\\.";
        }
      }
      bool flag2 = true;
      int errorCode = 0;
      while (stringList1.Count > 0)
      {
        List<string> stringList2 = stringList1;
        string str = stringList2[stringList2.Count - 1];
        List<string> stringList3 = stringList1;
        stringList3.RemoveAt(stringList3.Count - 1);
        flag2 = str.Length < 32000 ? Microsoft.Tools.IO.Interop.NativeMethods.CreateDirectory(str, IntPtr.Zero) : throw new PathTooLongException("The specified file name or path is too long, or a component of the specified path is too long.");
        if (!flag2 && errorCode == 0)
        {
          int lastError = Marshal.GetLastWin32Error();
          if (lastError != 183)
            errorCode = lastError;
          else if (LongPathFile.InternalExists(str) || !LongPathDirectory.InternalExists(str, out lastError) && lastError == 5)
            errorCode = lastError;
        }
      }
      if (count == 0 && !flag1)
      {
        if (!LongPathDirectory.InternalExists(LongPathDirectory.InternalGetDirectoryRoot(normalizedPath)))
          throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", (object) LongPathDirectory.InternalGetDirectoryRoot(path)));
      }
      else if (!flag2 && errorCode != 0)
        throw LongPathCommon.GetExceptionFromWin32Error(errorCode);
    }

    internal static bool InternalExists(string normalizedPath) => LongPathDirectory.InternalExists(normalizedPath, out int _);

    internal static bool InternalExists(string normalizedPath, out int lastError)
    {
      Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FILE_ATTRIBUTE_DATA data = new Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FILE_ATTRIBUTE_DATA();
      lastError = LongPathCommon.FillAttributeInfo(normalizedPath, ref data, false, false);
      return lastError == 0 && data.fileAttributes != -1 && (data.fileAttributes & 16) != 0;
    }

    internal static string InternalGetDirectoryRoot(string path) => path?.Substring(0, LongPathPath.GetRootLength(path));

    internal static void InternalDelete(string normalizedPath, string userPath, bool recursive)
    {
      Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FILE_ATTRIBUTE_DATA data = new Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FILE_ATTRIBUTE_DATA();
      LongPathCommon.FillAttributeInfo(normalizedPath, ref data, false, true);
      if ((data.fileAttributes & 1024) != 0)
        recursive = false;
      LongPathDirectory.DeleteHelper(normalizedPath, userPath, recursive);
    }

    private static void DeleteHelper(string fullPath, string userPath, bool recursive)
    {
      Exception exception = (Exception) null;
      if (recursive)
      {
        Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FIND_DATA lpFindFileData = new Microsoft.Tools.IO.Interop.NativeMethods.WIN32_FIND_DATA();
        string str1 = fullPath;
        char directorySeparatorChar = Path.DirectorySeparatorChar;
        string str2 = directorySeparatorChar.ToString();
        int lastWin32Error;
        using (Microsoft.Tools.IO.Interop.SafeFindHandle firstFile = Microsoft.Tools.IO.Interop.NativeMethods.FindFirstFile(str1 + str2 + "*", out lpFindFileData))
        {
          if (firstFile.IsInvalid)
            throw LongPathCommon.GetExceptionFromLastWin32Error();
          do
          {
            if ((lpFindFileData.dwFileAttributes & FileAttributes.Directory) != 0)
            {
              if (!lpFindFileData.cFileName.Equals(".") && !lpFindFileData.cFileName.Equals(".."))
              {
                if ((lpFindFileData.dwFileAttributes & FileAttributes.ReparsePoint) == (FileAttributes) 0)
                {
                  string fullPath1 = Path.Combine(fullPath, lpFindFileData.cFileName);
                  string userPath1 = Path.Combine(userPath, lpFindFileData.cFileName);
                  try
                  {
                    LongPathDirectory.DeleteHelper(fullPath1, userPath1, recursive);
                  }
                  catch (Exception ex)
                  {
                    if (exception == null)
                      exception = ex;
                  }
                }
                else
                {
                  if (lpFindFileData.dwReserved0 == -1610612733)
                  {
                    string path1 = fullPath;
                    string cFileName = lpFindFileData.cFileName;
                    directorySeparatorChar = Path.DirectorySeparatorChar;
                    string str3 = directorySeparatorChar.ToString();
                    string path2 = cFileName + str3;
                    if (!Microsoft.Tools.IO.Interop.NativeMethods.DeleteVolumeMountPoint(Path.Combine(path1, path2)))
                    {
                      lastWin32Error = Marshal.GetLastWin32Error();
                      try
                      {
                        throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
                      }
                      catch (Exception ex)
                      {
                        if (exception == null)
                          exception = ex;
                      }
                    }
                  }
                  if (!Microsoft.Tools.IO.Interop.NativeMethods.RemoveDirectory(Path.Combine(fullPath, lpFindFileData.cFileName)))
                  {
                    lastWin32Error = Marshal.GetLastWin32Error();
                    try
                    {
                      throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
                    }
                    catch (Exception ex)
                    {
                      if (exception == null)
                        exception = ex;
                    }
                  }
                }
              }
            }
            else if (!Microsoft.Tools.IO.Interop.NativeMethods.DeleteFile(Path.Combine(fullPath, lpFindFileData.cFileName)))
            {
              lastWin32Error = Marshal.GetLastWin32Error();
              if (lastWin32Error != 2)
              {
                try
                {
                  throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
                }
                catch (Exception ex)
                {
                  if (exception == null)
                    exception = ex;
                }
              }
            }
          }
          while (Microsoft.Tools.IO.Interop.NativeMethods.FindNextFile(firstFile, out lpFindFileData));
          lastWin32Error = Marshal.GetLastWin32Error();
        }
        if (exception != null)
          throw exception;
        if (lastWin32Error != 0 && lastWin32Error != 18)
          throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
      }
      if (Microsoft.Tools.IO.Interop.NativeMethods.RemoveDirectory(fullPath))
        return;
      int errorCode = Marshal.GetLastWin32Error();
      if (errorCode == 2)
        errorCode = 3;
      if (errorCode == 5)
        throw new IOException(string.Format("Access to the path '{0}' is denied.", (object) userPath));
      throw LongPathCommon.GetExceptionFromWin32Error(errorCode);
    }

    private static SafeFileHandle OpenHandle(string path)
    {
      string str = LongPathCommon.NormalizeLongPath(path);
      string pathRoot = LongPathPath.GetPathRoot(str);
      if (pathRoot == str && (int) pathRoot[1] == (int) Path.VolumeSeparatorChar)
        throw new ArgumentException("Path must not be a drive.", nameof (path));
      SafeFileHandle file = Microsoft.Tools.IO.Interop.NativeMethods.SafeCreateFile(str, Microsoft.Tools.IO.Interop.NativeMethods.EFileAccess.GenericWrite, 6U, IntPtr.Zero, 3U, 33554432U, IntPtr.Zero);
      return !file.IsInvalid ? file : throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    private static bool IsCurrentOrParentDirectory(string directoryName) => directoryName.Equals(".", StringComparison.OrdinalIgnoreCase) || directoryName.Equals("..", StringComparison.OrdinalIgnoreCase);
  }
}
