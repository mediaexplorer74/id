// Decompiled with JetBrains decompiler
// Type: FFUTool.FFUTool
// Assembly: FFUTool, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E16401E-1D4B-42FF-8522-F3B0C09CB0D5
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ffutool.exe

using FFUComponents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FFUTool
{
  public class FFUTool
  {
    private static ProgressReporter flashProgress = (ProgressReporter) null;
    private static Regex flashParam = new Regex("[-/]flash$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex uefiFlashParam = new Regex("[-/]uefiflash$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex skipParam = new Regex("[-/]skip$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex listParam = new Regex("[-/]list$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex forceParam = new Regex("[-/]force$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex massParam = new Regex("[-/]massStorage$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex clearIdParam = new Regex("[-/]clearId$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex serParam = new Regex("[-/]serial$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex wimParam = new Regex("[-/]wim$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static void Main(string[] args)
    {
      if (args.Length > 2 && FFUTool.FFUTool.forceParam.IsMatch(args[2]))
        Console.WriteLine("WARNING: Use of -force no longer has any effect.");
      string str = (string) null;
      if (args.Length > 1)
        str = args[1];
      if (args.Length >= 1 && (FFUTool.FFUTool.flashParam.IsMatch(args[0]) || FFUTool.FFUTool.uefiFlashParam.IsMatch(args[0]) || FFUTool.FFUTool.wimParam.IsMatch(args[0]) || FFUTool.FFUTool.skipParam.IsMatch(args[0]) || FFUTool.FFUTool.listParam.IsMatch(args[0]) || FFUTool.FFUTool.massParam.IsMatch(args[0]) || FFUTool.FFUTool.serParam.IsMatch(args[0]) || FFUTool.FFUTool.clearIdParam.IsMatch(args[0])))
      {
        if (!string.IsNullOrEmpty(str))
        {
          if (!File.Exists(str))
            goto label_7;
        }
        try
        {
          FFUManager.Start();
          ICollection<IFFUDevice> devices = (ICollection<IFFUDevice>) new List<IFFUDevice>();
          FFUManager.GetFlashableDevices(ref devices);
          if (FFUTool.FFUTool.listParam.IsMatch(args[0]))
          {
            if (devices.Count == 0)
            {
              Console.WriteLine("\nThere are no connected devices.");
            }
            else
            {
              Console.WriteLine("\nDevices Found: {0}", (object) devices.Count);
              string format = "{0,-10} : {1,3}";
              foreach (IFFUDevice ffuDevice in (IEnumerable<IFFUDevice>) devices)
              {
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine(format, (object) "Name", (object) ffuDevice.DeviceFriendlyName);
                Console.WriteLine(format, (object) "ID", (object) ffuDevice.DeviceUniqueID);
              }
            }
            Environment.ExitCode = 0;
          }
          else if (devices.Count != 1)
          {
            Console.WriteLine("One connected device is expected in order to flash, {0} found.", (object) devices.Count);
            Environment.ExitCode = -1;
          }
          else
          {
            IFFUDevice device = devices.First<IFFUDevice>();
            if (FFUTool.FFUTool.skipParam.IsMatch(args[0]))
            {
              if (!FFUTool.FFUTool.DoSkip(device))
                Environment.ExitCode = -1;
            }
            else if (FFUTool.FFUTool.massParam.IsMatch(args[0]))
            {
              if (!FFUTool.FFUTool.DoMassStorage(device))
                Environment.ExitCode = -1;
            }
            else if (FFUTool.FFUTool.clearIdParam.IsMatch(args[0]))
            {
              if (!FFUTool.FFUTool.DoClearId(device))
                Environment.ExitCode = -1;
            }
            else if (FFUTool.FFUTool.serParam.IsMatch(args[0]))
              Console.WriteLine("{0,-10} : {1,3}", (object) "Serial No.", (object) BitConverter.ToString(device.SerialNumber.ToByteArray()).Replace("-", string.Empty));
            else if (FFUTool.FFUTool.wimParam.IsMatch(args[0]))
              FFUTool.FFUTool.DoWim(device, str);
            else if (FFUTool.FFUTool.flashParam.IsMatch(args[0]))
              FFUTool.FFUTool.DoWimFlash(device, str);
            else if (FFUTool.FFUTool.uefiFlashParam.IsMatch(args[0]))
              FFUTool.FFUTool.DoFlash(device, str);
          }
        }
        catch (FFUException ex)
        {
          Console.WriteLine();
          Console.WriteLine("An FFU error occurred: " + ex.Message);
          Environment.ExitCode = -1;
        }
        catch (TimeoutException ex)
        {
          Console.WriteLine();
          Console.WriteLine("A wait timed out: " + ex.Message);
          Environment.ExitCode = -1;
        }
        FFUManager.Stop();
        return;
      }
label_7:
      Console.WriteLine("Usage: FFUTool -flash <path to FFU file to apply to disk>");
      Console.WriteLine("       FFUTool -uefiflash <path to FFU, flashed from UEFI directly>");
      Console.WriteLine("       FFUTool -wim <path to WIM to boot from RAM>");
      if (!string.IsNullOrEmpty(str))
      {
        Console.WriteLine("       Could not locate {0}, please verify that the file specified is accessible.", (object) str);
      }
      else
      {
        Console.WriteLine("       FFUTool -skip");
        Console.WriteLine("       FFUTool -list");
        Console.WriteLine("       FFUTool -massStorage");
        Console.WriteLine("       FFUTool -clearId");
        Console.WriteLine("       FFUTool -serial");
      }
      Environment.ExitCode = -1;
    }

    private static bool DoSkip(IFFUDevice device)
    {
      bool flag = device.SkipTransfer();
      if (flag)
        Console.WriteLine("Success, transfer skipped.");
      else
        Console.WriteLine("Failed to skip transfer.");
      return flag;
    }

    private static bool DoMassStorage(IFFUDevice device)
    {
      bool flag = device.EnterMassStorage();
      if (flag)
        Console.WriteLine("Success, device resetting to mass storage mode.");
      else
        Console.WriteLine("Failed to reset to mass storage mode.");
      return flag;
    }

    private static bool DoClearId(IFFUDevice device)
    {
      Console.WriteLine("Device ID currently: {0}", (object) device.DeviceFriendlyName);
      bool flag = device.ClearIdOverride();
      if (flag)
        Console.WriteLine("Success, device removed platform ID info. ID now: {0}", (object) device.DeviceFriendlyName);
      else
        Console.WriteLine("No platform ID override info cleared.");
      return flag;
    }

    private static void DoWim(IFFUDevice device, string wimPath)
    {
      Console.WriteLine("Found device:\nName:\t{0}\nID:\t{1}", (object) device.DeviceFriendlyName, (object) device.DeviceUniqueID);
      Console.WriteLine("Booting WIM: {0:s}", (object) Path.GetFileName(wimPath));
      Stopwatch stopwatch = Stopwatch.StartNew();
      device.EndTransfer();
      bool flag = device.WriteWim(wimPath);
      stopwatch.Stop();
      if (flag)
        Console.WriteLine("WIM transfer completed in {0} seconds.", (object) stopwatch.Elapsed.TotalSeconds);
      else
        Console.WriteLine("Failed to boot specified WIM.  Please ensure the device supports this operation.");
    }

    private static void PrepareFlash(IFFUDevice device, EtwSession session)
    {
      Console.CancelKeyPress += (ConsoleCancelEventHandler) ((param0, param1) => session.Dispose());
      Console.WriteLine("Logging to ETL file: {0:s}", (object) session.EtlPath);
      Console.WriteLine("Found device:\nName:\t{0}\nID:\t{1}", (object) device.DeviceFriendlyName, (object) device.DeviceUniqueID);
      device.EndTransfer();
    }

    private static void TransferWimIfPresent(ref IFFUDevice device, string ffuFilePath)
    {
      IFFUDevice wimDevice = (IFFUDevice) null;
      Guid id = device.DeviceUniqueID;
      ManualResetEvent deviceConnected = new ManualResetEvent(false);
      EventHandler<ConnectEventArgs> eventHandler = (EventHandler<ConnectEventArgs>) ((sender, e) =>
      {
        if (!(e.Device.DeviceUniqueID == id))
          return;
        wimDevice = e.Device;
        deviceConnected.Set();
      });
      string str = Path.Combine(Path.GetDirectoryName(ffuFilePath), "flashwim.wim");
      if (!File.Exists(str))
        return;
      FFUManager.DeviceConnectEvent += eventHandler;
      Console.WriteLine("Attempting to boot flashing WIM: {0:s}", (object) Path.GetFileName(str));
      bool flag1 = false;
      try
      {
        flag1 = device.WriteWim(str);
      }
      catch (FFUException ex)
      {
      }
      if (!flag1)
        return;
      bool flag2 = deviceConnected.WaitOne(TimeSpan.FromSeconds(30.0));
      FFUManager.DeviceConnectEvent -= eventHandler;
      if (!flag2)
        throw new FFUException(device.DeviceFriendlyName, device.DeviceUniqueID, "WIM boot failed.  Please reset your device and use \"ffutool -uefiFlash\" to flash your image.");
      device = wimDevice;
    }

    private static void FlashFile(IFFUDevice device, string ffuFilePath)
    {
      Console.WriteLine("Flashing: {0:s}", (object) Path.GetFileName(ffuFilePath));
      device.ProgressEvent += new EventHandler<ProgressEventArgs>(FFUTool.FFUTool.Device_ProgressEvent);
      device.EndTransfer();
      device.FlashFFUFile(ffuFilePath);
    }

    private static void DoWimFlash(IFFUDevice device, string ffuFilePath)
    {
      using (EtwSession session = new EtwSession())
      {
        FFUTool.FFUTool.PrepareFlash(device, session);
        FFUTool.FFUTool.TransferWimIfPresent(ref device, ffuFilePath);
        FFUTool.FFUTool.FlashFile(device, ffuFilePath);
      }
    }

    private static void DoFlash(IFFUDevice device, string ffuFilePath)
    {
      using (EtwSession session = new EtwSession())
      {
        FFUTool.FFUTool.PrepareFlash(device, session);
        FFUTool.FFUTool.FlashFile(device, ffuFilePath);
      }
    }

    private static void Device_ProgressEvent(object sender, ProgressEventArgs e) => Console.Write(FFUTool.FFUTool.GetProgress().CreateProgressDisplay(e.Position, e.Length));

    private static ProgressReporter GetProgress()
    {
      if (FFUTool.FFUTool.flashProgress == null)
      {
        ProgressReporter progressReporter = new ProgressReporter(Console.WindowWidth);
        Interlocked.CompareExchange<ProgressReporter>(ref FFUTool.FFUTool.flashProgress, progressReporter, (ProgressReporter) null);
      }
      return FFUTool.FFUTool.flashProgress;
    }
  }
}
