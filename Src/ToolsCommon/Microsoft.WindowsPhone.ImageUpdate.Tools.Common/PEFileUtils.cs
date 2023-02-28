// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.PEFileUtils
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class PEFileUtils
  {
    private static uint c_iPESignature = 4660;

    private static T ReadStruct<T>(BinaryReader br)
    {
      GCHandle gcHandle = GCHandle.Alloc((object) br.ReadBytes(Marshal.SizeOf(typeof (T))), GCHandleType.Pinned);
      T structure = (T) Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof (T));
      gcHandle.Free();
      return structure;
    }

    public static bool IsPE(string path)
    {
      using (BinaryReader br = new BinaryReader((Stream) File.OpenRead(path)))
      {
        if (br.BaseStream.Length < (long) Marshal.SizeOf(typeof (PEFileUtils.IMAGE_DOS_HEADER)))
          return false;
        PEFileUtils.IMAGE_DOS_HEADER imageDosHeader = PEFileUtils.ReadStruct<PEFileUtils.IMAGE_DOS_HEADER>(br);
        if (imageDosHeader.e_lfanew < Marshal.SizeOf(typeof (PEFileUtils.IMAGE_DOS_HEADER)) || imageDosHeader.e_lfanew > int.MaxValue - Marshal.SizeOf(typeof (PEFileUtils.IMAGE_NT_HEADERS32)) || br.BaseStream.Length < (long) (imageDosHeader.e_lfanew + Marshal.SizeOf(typeof (PEFileUtils.IMAGE_NT_HEADERS32))))
          return false;
        br.BaseStream.Seek((long) imageDosHeader.e_lfanew, SeekOrigin.Begin);
        if ((int) br.ReadUInt32() != (int) PEFileUtils.c_iPESignature)
          return false;
        PEFileUtils.ReadStruct<PEFileUtils.IMAGE_FILE_HEADER>(br);
        return true;
      }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_DOS_HEADER
    {
      [FieldOffset(60)]
      public int e_lfanew;
    }

    public struct IMAGE_FILE_HEADER
    {
      public ushort Machine;
      public ushort NumberOfSections;
      public ulong TimeDateStamp;
      public ulong PointerToSymbolTable;
      public ulong NumberOfSymbols;
      public ushort SizeOfOptionalHeader;
      public ushort Characteristics;
    }

    public struct IMAGE_DATA_DIRECTORY
    {
      public uint VirtualAddress;
      public uint Size;
    }

    public struct IMAGE_OPTIONAL_HEADER32
    {
      public ushort Magic;
      public byte MajorLinkerVersion;
      public byte MinorLinkerVersion;
      public uint SizeOfCode;
      public uint SizeOfInitializedData;
      public uint SizeOfUninitializedData;
      public uint AddressOfEntryPoint;
      public uint BaseOfCode;
      public uint BaseOfData;
      public uint ImageBase;
      public uint SectionAlignment;
      public uint FileAlignment;
      public ushort MajorOperatingSystemVersion;
      public ushort MinorOperatingSystemVersion;
      public ushort MajorImageVersion;
      public ushort MinorImageVersion;
      public ushort MajorSubsystemVersion;
      public ushort MinorSubsystemVersion;
      public uint Win32VersionValue;
      public uint SizeOfImage;
      public uint SizeOfHeaders;
      public uint CheckSum;
      public ushort Subsystem;
      public ushort DllCharacteristics;
      public uint SizeOfStackReserve;
      public uint SizeOfStackCommit;
      public uint SizeOfHeapReserve;
      public uint SizeOfHeapCommit;
      public uint LoaderFlags;
      public uint NumberOfRvaAndSizes;
    }

    public struct IMAGE_OPTIONAL_HEADER64
    {
      public ushort Magic;
      public byte MajorLinkerVersion;
      public byte MinorLinkerVersion;
      public uint SizeOfCode;
      public uint SizeOfInitializedData;
      public uint SizeOfUninitializedData;
      public uint AddressOfEntryPoint;
      public uint BaseOfCode;
      public ulong ImageBase;
      public uint SectionAlignment;
      public uint FileAlignment;
      public ushort MajorOperatingSystemVersion;
      public ushort MinorOperatingSystemVersion;
      public ushort MajorImageVersion;
      public ushort MinorImageVersion;
      public ushort MajorSubsystemVersion;
      public ushort MinorSubsystemVersion;
      public uint Win32VersionValue;
      public uint SizeOfImage;
      public uint SizeOfHeaders;
      public uint CheckSum;
      public ushort Subsystem;
      public ushort DllCharacteristics;
      public ulong SizeOfStackReserve;
      public ulong SizeOfStackCommit;
      public ulong SizeOfHeapReserve;
      public ulong SizeOfHeapCommit;
      public uint LoaderFlags;
      public uint NumberOfRvaAndSizes;
    }

    public struct IMAGE_NT_HEADERS32
    {
      public uint Signature;
      public PEFileUtils.IMAGE_FILE_HEADER FileHeader;
      public PEFileUtils.IMAGE_OPTIONAL_HEADER32 OptionalHeader;
    }

    public struct IMAGE_NT_HEADERS64
    {
      public uint Signature;
      public PEFileUtils.IMAGE_FILE_HEADER FileHeader;
      public PEFileUtils.IMAGE_OPTIONAL_HEADER64 OptionalHeader;
    }
  }
}
