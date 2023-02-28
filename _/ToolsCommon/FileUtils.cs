// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.FileUtils
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class FileUtils
  {
    public const int MAX_PATH = 260;

    public static string RerootPath(string path, string oldRoot, string newRoot)
    {
      if (oldRoot.Last<char>() != '\\')
        oldRoot += (string) (object) '\\';
      if (newRoot.Last<char>() != '\\')
        newRoot += (string) (object) '\\';
      return path.Replace(oldRoot, newRoot);
    }

    public static string GetTempFile() => FileUtils.GetTempFile(Path.GetTempPath());

    public static string GetTempFile(string dir) => Path.Combine(dir, Path.GetRandomFileName());

    public static void DeleteTree(string dirPath)
    {
      if (string.IsNullOrEmpty(dirPath))
        throw new ArgumentException("Empty directory path");
      if (LongPathFile.Exists(dirPath))
        throw new IOException(string.Format("Cannot delete directory {0}, it's a file", (object) dirPath));
      if (!LongPathDirectory.Exists(dirPath))
        return;
      LongPathDirectory.Delete(dirPath, true);
    }

    public static void DeleteFile(string filePath)
    {
      if (!LongPathFile.Exists(filePath))
        return;
      LongPathFile.SetAttributes(filePath, FileAttributes.Normal);
      LongPathFile.Delete(filePath);
    }

    public static void CleanDirectory(string dirPath)
    {
      if (string.IsNullOrEmpty(dirPath))
        throw new ArgumentException("Empty directory path");
      if (LongPathFile.Exists(dirPath))
        throw new IOException(string.Format("Cannot create directory {0}, a file with same name exists", (object) dirPath));
      NativeMethods.IU_CleanDirectory(dirPath, false);
    }

    public static string GetTempDirectory()
    {
      string path;
      do
      {
        path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
      }
      while (LongPathDirectory.Exists(path));
      LongPathDirectory.CreateDirectory(path);
      return path;
    }

    public static string GetFileVersion(string filepath)
    {
      string fileVersion = string.Empty;
      if (LongPathFile.Exists(filepath))
        fileVersion = FileVersionInfo.GetVersionInfo(filepath).FileVersion;
      return fileVersion;
    }

    public static string GetCurrentAssemblyFileVersion() => FileUtils.GetFileVersion(Process.GetCurrentProcess().MainModule.FileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint GetShortPathName(
      [MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath,
      [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath,
      uint cchBuffer);

    public static string GetShortPathName(string dirPath)
    {
      StringBuilder lpszShortPath = new StringBuilder(260);
      int shortPathName = (int) FileUtils.GetShortPathName(dirPath, lpszShortPath, 260U);
      return lpszShortPath.Length == 0 ? dirPath : lpszShortPath.ToString();
    }

    public static void CopyDirectory(string source, string destination)
    {
      LongPathDirectory.CreateDirectory(destination);
      foreach (string file in LongPathDirectory.GetFiles(source))
        LongPathFile.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
      foreach (string directory in LongPathDirectory.GetDirectories(source))
        FileUtils.CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
    }
  }
}
