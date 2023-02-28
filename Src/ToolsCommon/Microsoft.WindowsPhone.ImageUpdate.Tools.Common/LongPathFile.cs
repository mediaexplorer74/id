// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class LongPathFile
  {
    public static bool Exists(string path) => NativeMethods.IU_FileExists(path);

    public static FileAttributes GetAttributes(string path) => LongPathCommon.GetAttributes(path);

    public static void SetAttributes(string path, FileAttributes attributes)
    {
      if (!NativeMethods.SetFileAttributes(LongPathCommon.NormalizeLongPath(path), attributes))
        throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    public static void Delete(string path)
    {
      string lpFileName = LongPathCommon.NormalizeLongPath(path);
      if (!LongPathFile.Exists(path) || NativeMethods.DeleteFile(lpFileName))
        return;
      int lastWin32Error = Marshal.GetLastWin32Error();
      if (lastWin32Error != 2)
        throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
    }

    public static void Move(string sourcePath, string destinationPath)
    {
      if (!NativeMethods.MoveFile(LongPathCommon.NormalizeLongPath(sourcePath), LongPathCommon.NormalizeLongPath(destinationPath)))
        throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    public static void Copy(string sourcePath, string destinationPath, bool overwrite)
    {
      if (!NativeMethods.CopyFile(LongPathCommon.NormalizeLongPath(sourcePath), LongPathCommon.NormalizeLongPath(destinationPath), !overwrite))
        throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    public static void Copy(string sourcePath, string destinationPath) => LongPathFile.Copy(sourcePath, destinationPath, false);

    public static FileStream Open(string path, FileMode mode, FileAccess access) => LongPathFile.Open(path, mode, access, FileShare.None, 0, FileOptions.None);

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
      FileStream fileStream = new FileStream(LongPathFile.GetFileHandle(LongPathCommon.NormalizeLongPath(path), mode, access, share, options), access, bufferSize, (options & FileOptions.Asynchronous) == FileOptions.Asynchronous);
      if (mode == FileMode.Append)
        fileStream.Seek(0L, SeekOrigin.End);
      return fileStream;
    }

    public static FileStream OpenRead(string path) => LongPathFile.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    public static FileStream OpenWrite(string path) => LongPathFile.Open(path, FileMode.Create, FileAccess.ReadWrite);

    public static StreamWriter CreateText(string path) => new StreamWriter((Stream) LongPathFile.Open(path, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);

    public static byte[] ReadAllBytes(string path)
    {
      using (FileStream fileStream = LongPathFile.OpenRead(path))
      {
        byte[] buffer = new byte[fileStream.Length];
        fileStream.Read(buffer, 0, buffer.Length);
        return buffer;
      }
    }

    public static void WriteAllBytes(string path, byte[] contents)
    {
      using (FileStream fileStream = LongPathFile.OpenWrite(path))
        fileStream.Write(contents, 0, contents.Length);
    }

    public static string ReadAllText(string path, Encoding encoding)
    {
      using (StreamReader streamReader = new StreamReader((Stream) LongPathFile.OpenRead(path), encoding, true))
        return streamReader.ReadToEnd();
    }

    public static string ReadAllText(string path) => LongPathFile.ReadAllText(path, Encoding.Default);

    public static void WriteAllText(string path, string contents, Encoding encoding)
    {
      using (StreamWriter streamWriter = new StreamWriter((Stream) LongPathFile.OpenWrite(path), encoding))
        streamWriter.Write(contents);
    }

    public static void WriteAllText(string path, string contents) => LongPathFile.WriteAllText(path, contents, (Encoding) new UTF8Encoding(false));

    public static string[] ReadAllLines(string path, Encoding encoding)
    {
      using (StreamReader streamReader = new StreamReader((Stream) LongPathFile.OpenRead(path), encoding, true))
      {
        List<string> stringList = new List<string>();
        while (!streamReader.EndOfStream)
          stringList.Add(streamReader.ReadLine());
        return stringList.ToArray();
      }
    }

    public static string[] ReadAllLines(string path) => LongPathFile.ReadAllLines(path, Encoding.Default);

    public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
      using (StreamWriter streamWriter = new StreamWriter((Stream) LongPathFile.OpenWrite(path), encoding))
      {
        foreach (string content in contents)
          streamWriter.WriteLine(content);
      }
    }

    public static void WriteAllLines(string path, IEnumerable<string> contents) => LongPathFile.WriteAllLines(path, contents, (Encoding) new UTF8Encoding(false));

    public static void AppendAllText(string path, string contents, Encoding encoding)
    {
      using (StreamWriter streamWriter = new StreamWriter((Stream) LongPathFile.Open(path, FileMode.Append, FileAccess.ReadWrite), encoding))
        streamWriter.Write(contents);
    }

    public static void AppendAllText(string path, string contents) => LongPathFile.AppendAllText(path, contents, (Encoding) new UTF8Encoding(false));

    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive")]
    private static SafeFileHandle GetFileHandle(
      string normalizedPath,
      FileMode mode,
      FileAccess access,
      FileShare share,
      FileOptions options)
    {
      NativeMethods.EFileAccess underlyingAccess = LongPathFile.GetUnderlyingAccess(access);
      FileMode underlyingMode = LongPathFile.GetUnderlyingMode(mode);
      SafeFileHandle file = NativeMethods.CreateFile(normalizedPath, underlyingAccess, (uint) share, IntPtr.Zero, (uint) underlyingMode, (uint) options, IntPtr.Zero);
      return !file.IsInvalid ? file : throw LongPathCommon.GetExceptionFromLastWin32Error();
    }

    private static FileMode GetUnderlyingMode(FileMode mode) => mode == FileMode.Append ? FileMode.OpenOrCreate : mode;

    private static NativeMethods.EFileAccess GetUnderlyingAccess(FileAccess access)
    {
      switch (access)
      {
        case FileAccess.Read:
          return NativeMethods.EFileAccess.GenericRead;
        case FileAccess.Write:
          return NativeMethods.EFileAccess.GenericWrite;
        case FileAccess.ReadWrite:
          return NativeMethods.EFileAccess.GenericRead | NativeMethods.EFileAccess.GenericWrite;
        default:
          throw new ArgumentOutOfRangeException(nameof (access));
      }
    }
  }
}
