// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.CommonUtils
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class CommonUtils
  {
    private const int S_OK = 0;
    private const int WimNoCommit = 0;
    private const int WimCommit = 1;
    private static readonly HashAlgorithm Sha256Algorithm = HashAlgorithm.Create("SHA256");

    public static IntPtr MountVHD(string vhdPath, bool fReadOnly)
    {
      VIRTUAL_DISK_ACCESS_MASK accessMask = VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL;
      if (fReadOnly)
        accessMask = VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_READ;
      OPEN_VIRTUAL_DISK_FLAG openFlags = OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE;
      ATTACH_VIRTUAL_DISK_FLAG attachFlags = ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_NONE;
      if (fReadOnly)
        attachFlags = ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY;
      return CommonUtils.MountVHD(vhdPath, accessMask, openFlags, attachFlags);
    }

    public static IntPtr MountVHD(
      string vhdPath,
      VIRTUAL_DISK_ACCESS_MASK accessMask,
      OPEN_VIRTUAL_DISK_FLAG openFlags,
      ATTACH_VIRTUAL_DISK_FLAG attachFlags)
    {
      IntPtr zero = IntPtr.Zero;

       //RnD
       VIRTUAL_STORAGE_TYPE VST = new VIRTUAL_STORAGE_TYPE()
        {
            DeviceId = VHD_STORAGE_TYPE_DEVICE.VIRTUAL_STORAGE_TYPE_DEVICE_VHD,
            VendorId = VIRTUAL_STORAGE_TYPE_VENDOR.VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT
        };

      OPEN_VIRTUAL_DISK_PARAMETERS OVDP = new OPEN_VIRTUAL_DISK_PARAMETERS()
        {
            Version = OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1,
            RWDepth = 1U
        };

      int error1 = VirtualDiskLib.OpenVirtualDisk(
          ref VST, 
          vhdPath, 
          accessMask, 
          openFlags, 
          ref OVDP, 
          ref zero);

      if (0 < error1)
        throw new Win32Exception(error1);

     ATTACH_VIRTUAL_DISK_PARAMETERS AVDP = new ATTACH_VIRTUAL_DISK_PARAMETERS()
        {
            Version = ATTACH_VIRTUAL_DISK_VERSION.ATTACH_VIRTUAL_DISK_VERSION_1
        };

      int error2 = VirtualDiskLib.AttachVirtualDisk(
          zero, 
          IntPtr.Zero, 
          attachFlags, 
          0U, 
          ref AVDP, IntPtr.Zero);
      if (0 < error2)
        throw new Win32Exception(error2);
      return zero;
    }

    public static void DismountVHD(IntPtr hndlVirtDisk)
    {
      if (hndlVirtDisk == IntPtr.Zero)
        return;
      if (0 < VirtualDiskLib.DetachVirtualDisk(hndlVirtDisk, DETACH_VIRTUAL_DISK_FLAG.DETACH_VIRTUAL_DISK_FLAG_NONE, 0U))
        throw new Win32Exception();
      VirtualDiskLib.CloseHandle(hndlVirtDisk);
    }

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern int IU_MountWim(string WimPath, string MountPath, string TemporaryPath);

    [DllImport("IUCommon.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern int IU_DismountWim(string WimPath, string MountPath, int CommitMode);

    public static bool MountWIM(string wimPath, string mountPoint, string tmpDir) => 0 == CommonUtils.IU_MountWim(wimPath, mountPoint, tmpDir);

    public static bool DismountWIM(string wimPath, string mountPoint, bool commit) => 0 == CommonUtils.IU_DismountWim(wimPath, mountPoint, commit ? 1 : 0);

    public static string FindInPath(string filename)
    {
      string path1;
      if (LongPathFile.Exists(Path.Combine(Environment.CurrentDirectory, filename)))
        path1 = Environment.CurrentDirectory;
      else
        path1 = ((IEnumerable<string>) Environment.GetEnvironmentVariable("PATH").Split(';')).FirstOrDefault<string>((Func<string, bool>) (x => LongPathFile.Exists(Path.Combine(x, filename))));
      return !string.IsNullOrEmpty(path1) ? Path.Combine(path1, filename) : throw new FileNotFoundException(string.Format("Can't find file '{0}' anywhere in the %PATH%", (object) filename));
    }

    public static int RunProcess(
      string workingDir,
      string command,
      string args,
      bool hiddenWindow)
    {
      string processOutput = (string) null;
      return CommonUtils.RunProcess(workingDir, command, args, hiddenWindow, false, out processOutput);
    }

    // Run Process Verbose
    public static int RunProcessVerbose(string command, string args)
    {
        string processOutput = null;
        int result = RunProcess(null, command, args, true, true, out processOutput);
        Console.WriteLine(processOutput);
        return result;
    }

        public static int RunProcess(string command, string args)
    {
      string processOutput = (string) null;
      int num = CommonUtils.RunProcess((string) null, command, args, true, true, out processOutput);
      if (num != 0)
        Console.WriteLine(processOutput);
      return num;
    }

    private static int RunProcess(
      string workingDir,
      string command,
      string args,
      bool hiddenWindow,
      bool captureOutput,
      out string processOutput)
    {
      int num = 0;
      processOutput = string.Empty;
      command = Environment.ExpandEnvironmentVariables(command);
      args = Environment.ExpandEnvironmentVariables(args);
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.CreateNoWindow = true;
      if (hiddenWindow)
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
      if (workingDir != null)
        startInfo.WorkingDirectory = workingDir;
      startInfo.RedirectStandardInput = false;
      startInfo.RedirectStandardOutput = captureOutput;
      startInfo.UseShellExecute = !captureOutput;
      if (!string.IsNullOrEmpty(command) && !LongPathFile.Exists(command))
        CommonUtils.FindInPath(command);
      startInfo.FileName = command;
      startInfo.Arguments = args;
      using (Process process = Process.Start(startInfo))
      {
        if (process != null)
        {
          if (captureOutput)
            processOutput = process.StandardOutput.ReadToEnd();
          process.WaitForExit();
          num = process.HasExited ? process.ExitCode : throw new IUException("Process <{0}> didn't exit correctly", new object[1]
          {
            (object) command
          });
        }
      }
      return num;
    }

    public static string BytesToHexicString(byte[] bytes)
    {
      if (bytes == null)
        return string.Empty;
      StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
      for (int index = 0; index < bytes.Length; ++index)
        stringBuilder.Append(bytes[index].ToString("X2", (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat));
      return stringBuilder.ToString();
    }

    public static byte[] HexicStringToBytes(string text)
    {
      if (text == null)
        return new byte[0];
      List<byte> byteList = text.Length % 2 == 0 ? new List<byte>(text.Length / 2) : throw new IUException("Incorrect length of a hexic string:\"{0}\"", new object[1]
      {
        (object) text
      });
      for (int startIndex = 0; startIndex < text.Length; startIndex += 2)
      {
        string s = text.Substring(startIndex, 2);
        byte result;
        if (!byte.TryParse(s, NumberStyles.HexNumber, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat, out result))
          throw new IUException("Failed to parse hexic string: \"{0}\"", new object[1]
          {
            (object) s
          });
        byteList.Add(result);
      }
      return byteList.ToArray();
    }

    public static bool ByteArrayCompare(byte[] array1, byte[] array2)
    {
      if (array1 == array2)
        return true;
      if (array1 == null || array2 == null || array1.Length != array2.Length)
        return false;
      for (int index = 0; index < array1.Length; ++index)
      {
        if ((int) array1[index] != (int) array2[index])
          return false;
      }
      return true;
    }

    public static string GetCopyrightString()
    {
      string format = "Microsoft (C) {0} {1}";
      string processName = Process.GetCurrentProcess().ProcessName;
      string assemblyFileVersion = FileUtils.GetCurrentAssemblyFileVersion();
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat(format, (object) processName, (object) assemblyFileVersion);
      stringBuilder.AppendLine();
      return stringBuilder.ToString();
    }

    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
    public static bool IsCurrentUserAdmin() => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole("BUILTIN\\\\Administrators");

    public static string GetSha256Hash(byte[] buffer) => BitConverter.ToString(CommonUtils.Sha256Algorithm.ComputeHash(buffer)).Replace("-", string.Empty);
  }
}
