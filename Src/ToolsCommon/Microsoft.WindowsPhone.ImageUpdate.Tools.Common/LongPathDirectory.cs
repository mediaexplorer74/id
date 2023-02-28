// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class LongPathDirectory
  {
    public const string ALL_FILE_PATTERN = "*.*";

    public static void CreateDirectory(string path)
    {
      try
      {
        NativeMethods.IU_EnsureDirectoryExists(path);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static void Delete(string path)
    {
      string str = LongPathCommon.NormalizeLongPath(path);
      if (LongPathDirectory.Exists(str) && !NativeMethods.RemoveDirectory(str))
        throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    public static void Delete(string path, bool recursive)
    {
      if (recursive)
        NativeMethods.IU_CleanDirectory(path, true);
      else
        LongPathDirectory.Delete(path);
    }

    public static bool Exists(string path)
    {
        //RnD
        //return NativeMethods.IU_DirectoryExists(path);
        return Directory.Exists(path);
    }

    public static FileAttributes GetAttributes(string path)
    {
      FileAttributes attributes = LongPathCommon.GetAttributes(path);
      return attributes.HasFlag((Enum) FileAttributes.Directory) ? attributes : throw LongPathCommon.GetExceptionFromWin32Error(267);
    }

    public static IEnumerable<string> EnumerateDirectories(
      string path,
      string searchPattern,
      SearchOption searchOptions)
    {
      return (IEnumerable<string>) LongPathDirectory.GetDirectories(path, searchPattern, searchOptions);
    }

    public static IEnumerable<string> EnumerateDirectories(
      string path,
      string searchPattern)
    {
      return LongPathDirectory.EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
    }

    public static IEnumerable<string> EnumerateDirectories(string path) => LongPathDirectory.EnumerateDirectories(path, "*.*", SearchOption.TopDirectoryOnly);

    public static string[] GetDirectories(
      string path,
      string searchPattern,
      SearchOption searchOptions)
    {
      if (searchOptions != SearchOption.AllDirectories && searchOptions != SearchOption.TopDirectoryOnly)
        throw new NotImplementedException("Unknown search option: " + (object) searchOptions);
      bool fRecursive = searchOptions == SearchOption.AllDirectories;
      IntPtr rgDirectories = IntPtr.Zero;
      int cDirectories = 0;
      int allDirectories = NativeMethods.IU_GetAllDirectories(Path.Combine(path, Path.GetDirectoryName(searchPattern)), Path.GetFileName(searchPattern), fRecursive, out rgDirectories, out cDirectories);
      if (allDirectories != 0)
        throw LongPathCommon.GetExceptionFromWin32Error(allDirectories);
      try
      {
        return LongPathCommon.ConvertPtrArrayToStringArray(rgDirectories, cDirectories);
      }
      finally
      {
        NativeMethods.IU_FreeStringList(rgDirectories, cDirectories);
      }
    }

    public static string[] GetDirectories(string path, string searchPattern) => LongPathDirectory.GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);

    public static string[] GetDirectories(string path) => LongPathDirectory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);

    public static IEnumerable<string> EnumerateFiles(
      string path,
      string searchPattern,
      SearchOption searchOptions)
    {
      return (IEnumerable<string>) LongPathDirectory.GetFiles(path, searchPattern, searchOptions);
    }

    public static IEnumerable<string> EnumerateFiles(string path, string searchPattern) => LongPathDirectory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

    public static IEnumerable<string> EnumerateFiles(string path) => LongPathDirectory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly);

    public static string[] GetFiles(string path, string searchPattern, SearchOption searchOptions)
    {
      if (searchOptions != SearchOption.AllDirectories && searchOptions != SearchOption.TopDirectoryOnly)
        throw new NotImplementedException("Unknown search option: " + (object) searchOptions);
      bool fRecursive = searchOptions == SearchOption.AllDirectories;
      IntPtr rgFiles = IntPtr.Zero;
      int cFiles = 0;
      int allFiles = NativeMethods.IU_GetAllFiles(Path.Combine(path, Path.GetDirectoryName(searchPattern)), Path.GetFileName(searchPattern), fRecursive, out rgFiles, out cFiles);
      if (allFiles != 0)
        throw LongPathCommon.GetExceptionFromWin32Error(allFiles);
      try
      {
        return LongPathCommon.ConvertPtrArrayToStringArray(rgFiles, cFiles);
      }
      finally
      {
        NativeMethods.IU_FreeStringList(rgFiles, cFiles);
      }
    }

    public static string[] GetFiles(string path, string searchPattern) => LongPathDirectory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

    public static string[] GetFiles(string path) => LongPathDirectory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
  }
}
