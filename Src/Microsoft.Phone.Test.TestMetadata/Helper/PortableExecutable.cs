// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.Helper.PortableExecutable
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using Microsoft.Tools.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.Phone.Test.TestMetadata.Helper
{
  public class PortableExecutable : IDisposable
  {
    private const ushort DosSignature = 23117;
    private const uint NtSignature = 17744;
    private readonly string _fileName;
    private IntPtr _fileHandle;
    [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
    private IntPtr _mapFileHandle;
    [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
    private IntPtr _imageBase;
    [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
    private IntPtr _imageNtHeader;

    public string FileName { get; private set; }

    public string FullFileName { get; private set; }

    internal ImageNtHeaders NtHeaders { get; private set; }

    public List<string> Imports { get; private set; }

    public List<string> DelayLoadImports { get; private set; }

    public List<string> Exports { get; private set; }

    public bool IsPortableExecutableBinary => Marshal.ReadInt16(this._imageBase) == (short) 23117 && this.NtHeaders.Signature == 17744U;

    public bool IsManaged
    {
      get
      {
        if (!this.IsPortableExecutableBinary)
          return false;
        uint size = 0;
        IntPtr zero = IntPtr.Zero;
        return NativeMethods.ImageDirectoryEntryToDataEx(this._imageBase, 0, (ushort) 14, ref size, ref zero).ToInt32() != 0;
      }
    }

    public bool IsNative => !this.IsManaged;

    public PortableExecutable(string fileName)
    {
      this._fileName = "\\\\?\\" + fileName;
      this.Load();
    }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Va")]
    public IntPtr ImageRvaToVa(uint relativeVirtualAddress) => NativeMethods.ImageRvaToVa(this._imageNtHeader, this._imageBase, relativeVirtualAddress, IntPtr.Zero);

    public void CopyData(uint relativeVirtualAddress, byte[] buffer, int startIndex, int length) => Marshal.Copy(NativeMethods.ImageRvaToVa(this._imageNtHeader, this._imageBase, relativeVirtualAddress, IntPtr.Zero), buffer, startIndex, length);

    private void Load()
    {
      this._fileHandle = PortableExecutable.OpenFile(this._fileName);
      this.FullFileName = this._fileName;
      this.FileName = LongPathPath.GetFileName(this._fileName);
      uint fileSizeHigh = 0;
      uint fileSize = PortableExecutable.GetFileSize(this._fileHandle, ref fileSizeHigh);
      this._mapFileHandle = NativeMethods.CreateFileMapping(this._fileHandle, IntPtr.Zero, 2U, fileSizeHigh, fileSize, (string) null);
      int lastWin32Error1 = Marshal.GetLastWin32Error();
      this._imageBase = this._mapFileHandle.ToInt32() != 0 && this._mapFileHandle.ToInt32() != -1 ? NativeMethods.MapViewOfFile(this._mapFileHandle, 4U, 0U, 0U, IntPtr.Zero) : throw new Win32Exception(lastWin32Error1);
      int lastWin32Error2 = Marshal.GetLastWin32Error();
      this._imageNtHeader = this._imageBase.ToInt32() != 0 ? NativeMethods.ImageNtHeader(this._imageBase) : throw new Win32Exception(lastWin32Error2);
      int lastWin32Error3 = Marshal.GetLastWin32Error();
      this.NtHeaders = this._imageNtHeader.ToInt32() != 0 ? (ImageNtHeaders) Marshal.PtrToStructure(this._imageNtHeader, typeof (ImageNtHeaders)) : throw new Win32Exception(lastWin32Error3);
      this.LoadImports();
      this.LoadDelayLoadImports();
      this.LoadExports();
    }

    private void LoadImports()
    {
      if (this.Imports == null)
        this.Imports = new List<string>();
      else
        this.Imports.Clear();
      uint size = 0;
      IntPtr zero = IntPtr.Zero;
      IntPtr ptr = NativeMethods.ImageDirectoryEntryToDataEx(this._imageBase, 0, (ushort) 1, ref size, ref zero);
      if (ptr.ToInt32() == 0)
        return;
      for (ImageImportDescriptor structure = (ImageImportDescriptor) Marshal.PtrToStructure(ptr, typeof (ImageImportDescriptor)); structure.Name != 0U; structure = (ImageImportDescriptor) Marshal.PtrToStructure(ptr, typeof (ImageImportDescriptor)))
      {
        this.Imports.Add(Marshal.PtrToStringAnsi(NativeMethods.ImageRvaToVa(this._imageNtHeader, this._imageBase, structure.Name, IntPtr.Zero)));
        ptr = (IntPtr) ((int) ptr + Marshal.SizeOf((object) structure));
      }
    }

    private void LoadDelayLoadImports()
    {
      if (this.DelayLoadImports == null)
        this.DelayLoadImports = new List<string>();
      else
        this.DelayLoadImports.Clear();
      uint size = 0;
      IntPtr zero = IntPtr.Zero;
      IntPtr ptr = NativeMethods.ImageDirectoryEntryToDataEx(this._imageBase, 0, (ushort) 13, ref size, ref zero);
      if (ptr.ToInt32() == 0)
        return;
      for (DelayLoadImportDescriptor structure = (DelayLoadImportDescriptor) Marshal.PtrToStructure(ptr, typeof (DelayLoadImportDescriptor)); structure.DllName != 0U; structure = (DelayLoadImportDescriptor) Marshal.PtrToStructure(ptr, typeof (DelayLoadImportDescriptor)))
      {
        this.DelayLoadImports.Add(Marshal.PtrToStringAnsi(NativeMethods.ImageRvaToVa(this._imageNtHeader, this._imageBase, structure.DllName, IntPtr.Zero)));
        ptr = (IntPtr) ((int) ptr + Marshal.SizeOf((object) structure));
      }
    }

    private void LoadExports()
    {
      if (this.Exports == null)
        this.Exports = new List<string>();
      else
        this.Exports.Clear();
      uint size = 0;
      IntPtr zero = IntPtr.Zero;
      IntPtr dataEx = NativeMethods.ImageDirectoryEntryToDataEx(this._imageBase, 0, (ushort) 0, ref size, ref zero);
      if (dataEx.ToInt32() == 0)
        return;
      ImageExportDirectory structure = (ImageExportDirectory) Marshal.PtrToStructure(dataEx, typeof (ImageExportDirectory));
      IntPtr ptr = NativeMethods.ImageRvaToVa(this._imageNtHeader, this._imageBase, structure.AddressOfNames, IntPtr.Zero);
      if (ptr.ToInt32() == 0)
        return;
      for (int index = 0; (long) index < (long) structure.NumberOfNames; ++index)
      {
        this.Exports.Add(Marshal.PtrToStringAnsi(NativeMethods.ImageRvaToVa(this._imageNtHeader, this._imageBase, (uint) Marshal.ReadInt32(ptr), IntPtr.Zero)));
        ptr = (IntPtr) (ptr.ToInt32() + Marshal.SizeOf(typeof (uint)));
      }
    }

    private static uint GetFileSize(IntPtr fileHandle, ref uint fileSizeHigh)
    {
      uint fileSize = NativeMethods.GetFileSize(fileHandle, ref fileSizeHigh);
      return fileSize != uint.MaxValue && fileSizeHigh <= 0U ? fileSize : throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private static IntPtr OpenFile(string fileName)
    {
      IntPtr file = NativeMethods.CreateFile(LongPathPath.GetFullPath(fileName), 2147483648U, 1U, IntPtr.Zero, 3U, 0U, IntPtr.Zero);
      int lastWin32Error = Marshal.GetLastWin32Error();
      return file.ToInt32() != -1 ? file : throw new Win32Exception(lastWin32Error);
    }

    private static void CloseHandle(IntPtr fileHandle)
    {
      if (NativeMethods.CloseHandle(fileHandle) == 1)
        ;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      if (this._imageBase.ToInt32() != 0)
      {
        if (NativeMethods.UnmapViewOfFile(this._imageBase) == 1)
          ;
        this._imageBase = IntPtr.Zero;
      }
      if (this._mapFileHandle.ToInt32() != 0 && -1 != this._mapFileHandle.ToInt32())
        PortableExecutable.CloseHandle(this._mapFileHandle);
      if (this._fileHandle.ToInt32() != 0 && -1 != this._fileHandle.ToInt32())
        PortableExecutable.CloseHandle(this._fileHandle);
      this._imageNtHeader = IntPtr.Zero;
    }

    ~PortableExecutable() => this.Dispose(false);
  }
}
