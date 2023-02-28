// LongPath.cs
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPath
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class LongPath
  {
    private const string LONGPATH_PREFIX = "\\\\?\\";

        public static string Combine(string pkgRoot, string c_FODDirectory)
        {
            throw new NotImplementedException();
        }

        public static string GetDirectoryName(string path)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        throw new ArgumentException(nameof (path));
      int length = path.LastIndexOfAny(new char[2]
      {
        Path.DirectorySeparatorChar,
        Path.VolumeSeparatorChar
      });
      return length == -1 ? (string) null : path.Substring(0, length);
    }

        //RnD
        public static string GetFileName(string path2)
        {
            // fix it
            return LongPathCommon.NormalizeLongPath(path2).Substring("\\\\?\\".Length);
        }

        public static string GetFullPath(string path)
        {
            return LongPathCommon.NormalizeLongPath(path).Substring("\\\\?\\".Length);
        }

        //RnD
        public static string GetFullPathUNC(string path)
        {
            // fix it
            return LongPathCommon.NormalizeLongPath(path).Substring("\\\\?\\".Length);
        }

        public static string GetPathRoot(string path)
    {
      if (path == null)
        return (string) null;
      if (path == string.Empty || path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        throw new ArgumentException(nameof (path));
      if (!Path.IsPathRooted(path))
        return string.Empty;
      if (path.StartsWith("\\\\"))
      {
        int num = path.IndexOf(Path.DirectorySeparatorChar, "\\\\".Length);
        if (num == -1)
          return path;
        int length = path.IndexOf(Path.DirectorySeparatorChar, num + 1);
        return length == -1 ? path : path.Substring(0, length);
      }
      if (path.IndexOf(Path.VolumeSeparatorChar) != 1)
        return string.Empty;
      return path.Length <= 2 || (int) path[2] != (int) Path.DirectorySeparatorChar ? path.Substring(0, 2) : path.Substring(0, 3);
    }
  }
}
