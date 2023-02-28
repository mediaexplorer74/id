// Decompiled with JetBrains decompiler
// Type: Microsoft.Tools.IO.LongPathFile
// Assembly: Microsoft.Tools.IO, Version=1.1.11.0, Culture=neutral, PublicKeyToken=1a5b963c6f0fbeab
// MVID: 222C54A4-0FE1-469A-8627-EF94B226BBFA
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Tools.IO.dll

using Microsoft.Tools.IO.Interop;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;

namespace Microsoft.Tools.IO
{
  public static class LongPathFile
  {
    public static bool Exists(string path)
    {
      bool isDirectory;
      return LongPathCommon.Exists(path, out isDirectory) && !isDirectory;
    }

    public static void Delete(string path)
    {
      if (!NativeMethods.DeleteFile(LongPathCommon.NormalizeLongPath(path)))
        throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    public static void Move(string sourcePath, string destinationPath)
    {
      if (!NativeMethods.MoveFile(LongPathCommon.NormalizeLongPath(sourcePath, nameof (sourcePath)), LongPathCommon.NormalizeLongPath(destinationPath, nameof (destinationPath))))
        throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    public static void Copy(string sourcePath, string destinationPath, bool overwrite)
    {
      if (!NativeMethods.CopyFile(LongPathCommon.NormalizeLongPath(sourcePath, nameof (sourcePath)), LongPathCommon.NormalizeLongPath(destinationPath, nameof (destinationPath)), !overwrite))
        throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    public static FileStream Open(string path, FileMode mode, FileAccess access) => LongPathFile.Open(path, mode, access, FileShare.None);

    public static FileStream Open(
      string path,
      FileMode mode,
      FileAccess access,
      FileShare share)
    {
      return LongPathFile.Open(path, mode, access, share, 0, FileOptions.None);
    }

    public static FileStream Open(
      string path,
      FileMode mode,
      FileAccess access,
      FileShare share,
      int bufferSize,
      FileOptions options)
    {
      if (bufferSize == 0)
        bufferSize = 1024;
      return new FileStream(LongPathFile.GetFileHandle(LongPathCommon.NormalizeLongPath(path), mode, access, share, options), access, bufferSize, (options & FileOptions.Asynchronous) == FileOptions.Asynchronous);
    }

    public static DateTime GetCreationTime(string path) => LongPathFile.GetCreationTimeUtc(path).ToLocalTime();

    public static void SetCreationTime(string path, DateTime creationTime) => LongPathFile.SetCreationTimeUtc(path, creationTime.ToUniversalTime());

    public static DateTime GetCreationTimeUtc(string path)
    {
      NativeMethods.WIN32_FILE_ATTRIBUTE_DATA fileAttributeData = LongPathCommon.GetWin32FileAttributeData(LongPathCommon.NormalizeLongPath(path));
      return DateTime.FromFileTimeUtc((long) fileAttributeData.ftCreationTimeHigh << 32 | (long) fileAttributeData.ftCreationTimeLow);
    }

    public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
    {
      using (SafeFileHandle hFile = LongPathFile.OpenHandle(LongPathCommon.NormalizeLongPath(path)))
        LongPathCommon.SetFileTimes(hFile, creationTimeUtc.ToFileTimeUtc(), 0L, 0L);
    }

    public static DateTime GetLastAccessTime(string path) => LongPathFile.GetLastAccessTimeUtc(path).ToLocalTime();

    public static void SetLastAccessTime(string path, DateTime lastAccessTime) => LongPathFile.SetLastAccessTimeUtc(path, lastAccessTime.ToUniversalTime());

    public static DateTime GetLastAccessTimeUtc(string path)
    {
      NativeMethods.WIN32_FILE_ATTRIBUTE_DATA fileAttributeData = LongPathCommon.GetWin32FileAttributeData(LongPathCommon.NormalizeLongPath(path));
      return DateTime.FromFileTimeUtc((long) fileAttributeData.ftLastAccessTimeHigh << 32 | (long) fileAttributeData.ftLastAccessTimeLow);
    }

    public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
    {
      using (SafeFileHandle hFile = LongPathFile.OpenHandle(LongPathCommon.NormalizeLongPath(path)))
        LongPathCommon.SetFileTimes(hFile, 0L, lastAccessTimeUtc.ToFileTimeUtc(), 0L);
    }

    public static DateTime GetLastWriteTime(string path) => LongPathFile.GetLastWriteTimeUtc(path).ToLocalTime();

    public static void SetLastWriteTime(string path, DateTime lastWriteTime) => LongPathFile.SetLastWriteTimeUtc(path, lastWriteTime.ToUniversalTime());

    public static DateTime GetLastWriteTimeUtc(string path)
    {
      NativeMethods.WIN32_FILE_ATTRIBUTE_DATA fileAttributeData = LongPathCommon.GetWin32FileAttributeData(LongPathCommon.NormalizeLongPath(path));
      return DateTime.FromFileTimeUtc((long) fileAttributeData.ftLastWriteTimeHigh << 32 | (long) fileAttributeData.ftLastWriteTimeLow);
    }

    public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
    {
      using (SafeFileHandle hFile = LongPathFile.OpenHandle(LongPathCommon.NormalizeLongPath(path)))
        LongPathCommon.SetFileTimes(hFile, 0L, 0L, lastWriteTimeUtc.ToFileTimeUtc());
    }

    public static FileAttributes GetAttributes(string path) => LongPathCommon.GetAttributes(path);

    public static void SetAttributes(string path, FileAttributes fileAttributes) => LongPathCommon.SetAttributes(path, fileAttributes);

    public static long GetFileLengthBytes(string path)
    {
      NativeMethods.WIN32_FILE_ATTRIBUTE_DATA fileAttributeData = LongPathCommon.GetWin32FileAttributeData(LongPathCommon.NormalizeLongPath(path));
      if ((fileAttributeData.fileAttributes & 16) != 0)
        throw new FileNotFoundException(string.Format("Could not find file '{0}'", (object) path), path);
      return (long) fileAttributeData.fileSizeHigh << 32 | (long) fileAttributeData.fileSizeLow & (long) uint.MaxValue;
    }

    internal static bool InternalExists(string normalizedPath)
    {
      NativeMethods.WIN32_FILE_ATTRIBUTE_DATA data = new NativeMethods.WIN32_FILE_ATTRIBUTE_DATA();
      return LongPathCommon.FillAttributeInfo(normalizedPath, ref data, false, false) == 0 && data.fileAttributes != -1 && (data.fileAttributes & 16) == 0;
    }

    private static SafeFileHandle GetFileHandle(
      string normalizedPath,
      FileMode mode,
      FileAccess access,
      FileShare share,
      FileOptions options)
    {
      NativeMethods.EFileAccess underlyingAccess = LongPathCommon.GetUnderlyingAccess(access);
      SafeFileHandle file = NativeMethods.CreateFile(normalizedPath, underlyingAccess, (uint) share, IntPtr.Zero, (uint) mode, (uint) options, IntPtr.Zero);
      return !file.IsInvalid ? file : throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    private static SafeFileHandle OpenHandle(string normalizedPath) => LongPathFile.GetFileHandle(normalizedPath, FileMode.Open, FileAccess.Write, FileShare.None, FileOptions.None);
  }
}
