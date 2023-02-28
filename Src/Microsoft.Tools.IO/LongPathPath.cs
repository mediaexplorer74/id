// Decompiled with JetBrains decompiler
// Type: Microsoft.Tools.IO.LongPathPath
// Assembly: Microsoft.Tools.IO, Version=1.1.11.0, Culture=neutral, PublicKeyToken=1a5b963c6f0fbeab
// MVID: 222C54A4-0FE1-469A-8627-EF94B226BBFA
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Tools.IO.dll

using System;
using System.IO;

namespace Microsoft.Tools.IO
{
  public static class LongPathPath
  {
    public static readonly char DirectorySeparatorChar = Path.DirectorySeparatorChar;
    public static readonly char AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar;
    public static readonly char VolumeSeparatorChar = Path.VolumeSeparatorChar;
    [Obsolete("Please use GetInvalidPathChars or GetInvalidFileNameChars instead.")]
    public static readonly char[] InvalidPathChars = Path.InvalidPathChars;
    public static readonly char PathSeparator = Path.PathSeparator;
    internal static readonly char[] TrimEndChars = new char[8]
    {
      '\t',
      '\n',
      '\v',
      '\f',
      '\r',
      ' ',
      '\u0085',
      ' '
    };

    public static string GetFullPath(string path)
    {
      path = LongPathPath.NormalizePath(path, true);
      path = LongPathCommon.NormalizeLongPath(path);
      return LongPathCommon.RemoveLongPathPrefix(path);
    }

    public static string GetPathRoot(string path)
    {
      if (path == null)
        return (string) null;
      path = LongPathPath.NormalizePath(path, false);
      return path?.Substring(0, LongPathPath.GetRootLength(path));
    }

    public static string GetDirectoryName(string path)
    {
      if (path != null)
      {
        LongPathPath.CheckInvalidPathChars(path);
        path = LongPathPath.NormalizePath(path, false);
        int rootLength = LongPathPath.GetRootLength(path);
        if (path.Length > rootLength)
        {
          int length = path.Length;
          if (length == rootLength)
            return (string) null;
          do
            ;
          while (length > rootLength && (int) path[--length] != (int) LongPathPath.DirectorySeparatorChar && (int) path[length] != (int) LongPathPath.AltDirectorySeparatorChar);
          string directoryName = path.Substring(0, length);
          if (length <= rootLength)
            return directoryName;
          return directoryName.TrimEnd(LongPathPath.DirectorySeparatorChar, LongPathPath.AltDirectorySeparatorChar);
        }
      }
      return (string) null;
    }

    public static string Combine(string path1, string path2) => Path.Combine(path1, path2);

    public static string Combine(string path1, string path2, string path3) => Path.Combine(path1, path2, path3);

    public static string Combine(string path1, string path2, string path3, string path4) => Path.Combine(path1, path2, path3, path4);

    public static string Combine(params string[] paths) => Path.Combine(paths);

    public static string GetFileName(string path) => Path.GetFileName(path);

    public static string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

    public static bool IsPathRooted(string path) => Path.IsPathRooted(path);

    public static string GetRandomFileName() => Path.GetRandomFileName();

    public static string ChangeExtension(string path, string extension) => Path.ChangeExtension(path, extension);

    public static string GetExtension(string path) => Path.GetExtension(path);

    public static char[] GetInvalidPathChars() => Path.GetInvalidPathChars();

    public static char[] GetInvalidFileNameChars() => Path.GetInvalidFileNameChars();

    public static string GetTempPath() => Path.GetTempPath();

    public static string GetTempFileName() => Path.GetTempFileName();

    public static bool HasExtension(string path) => Path.HasExtension(path);

    internal static void CheckInvalidPathChars(string path)
    {
      for (int index = 0; index < path.Length; ++index)
      {
        int num = (int) path[index];
        switch (num)
        {
          case 34:
          case 60:
          case 62:
          case 124:
            throw new ArgumentException("Illegal characters in path.");
          default:
            if (num >= 32)
              continue;
            goto case 34;
        }
      }
    }

    internal static int GetRootLength(string path)
    {
      LongPathPath.CheckInvalidPathChars(path);
      int index = 0;
      int length = path.Length;
      if (length >= 1 && LongPathPath.IsDirectorySeparator(path[0]))
      {
        index = 1;
        if (length >= 2 && LongPathPath.IsDirectorySeparator(path[1]))
        {
          index = 2;
          int num = 2;
          while (index < length && ((int) path[index] != (int) LongPathPath.DirectorySeparatorChar && (int) path[index] != (int) LongPathPath.AltDirectorySeparatorChar || --num > 0))
            ++index;
        }
      }
      else if (length >= 2 && (int) path[1] == (int) LongPathPath.VolumeSeparatorChar)
      {
        index = 2;
        if (length >= 3 && LongPathPath.IsDirectorySeparator(path[2]))
          ++index;
      }
      return index;
    }

    internal static bool IsDirectorySeparator(char c) => (int) c == (int) LongPathPath.DirectorySeparatorChar || (int) c == (int) LongPathPath.AltDirectorySeparatorChar;

    private static string NormalizePath(string path, bool fullCheck)
    {
      if (fullCheck)
      {
        path = path.TrimEnd(LongPathPath.TrimEndChars);
        LongPathPath.CheckInvalidPathChars(path);
      }
      string str = path.Substring(0, LongPathPath.GetRootLength(path));
      path = path.Remove(0, str.Length);
      path = path.Replace(LongPathPath.AltDirectorySeparatorChar, LongPathPath.DirectorySeparatorChar);
      string oldValue = new string(new char[2]
      {
        LongPathPath.DirectorySeparatorChar,
        LongPathPath.DirectorySeparatorChar
      });
      do
      {
        path = path.Replace(oldValue, LongPathPath.DirectorySeparatorChar.ToString());
      }
      while (path.Contains(oldValue));
      path = path.Insert(0, str);
      return path;
    }
  }
}
