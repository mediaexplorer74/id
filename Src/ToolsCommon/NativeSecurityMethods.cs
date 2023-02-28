// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.NativeSecurityMethods
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class NativeSecurityMethods
  {
    [CLSCompliant(false)]
    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ConvertSecurityDescriptorToStringSecurityDescriptor(
      [In] byte[] pBinarySecurityDescriptor,
      int RequestedStringSDRevision,
      SecurityInformationFlags SecurityInformation,
      out IntPtr StringSecurityDescriptor,
      out int StringSecurityDescriptorLen);

    [CLSCompliant(false)]
    [DllImport("AdvAPI32.DLL", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetFileSecurity(
      string lpFileName,
      SecurityInformationFlags RequestedInformation,
      IntPtr pSecurityDescriptor,
      int nLength,
      ref int lpnLengthNeeded);

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int IU_AdjustProcessPrivilege(string strPrivilegeName, bool fEnabled);
  }
}
