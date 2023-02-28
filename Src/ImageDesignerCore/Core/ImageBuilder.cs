// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ImageBuilder
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class ImageBuilder : IDisposable
  {
    public const string IMAGEAPP_OEMVERSION_SWITCH = "/OEMVersion:";
    public const string IMAGEAPP_CUSTOMIZATION_XML_SWITCH = "/OEMCustomizationXML:";
    private object _lock = new object();
    private AutoResetEvent arevent = new AutoResetEvent(false);
    private AutoResetEvent waitHandle = new AutoResetEvent(false);
    private Process _process;
    private LogMonitor _logmon;
    private bool _cancel;
    private static readonly char[] _newLineDelim = new char[1]
    {
      '\n'
    };

    public ImageBuilder(string ffuImagePath, string oemInputFile)
    {
      this.FFUImagePath = ffuImagePath;
      this.OEMInputFile = oemInputFile;
      this.Initialize();
    }

    ~ImageBuilder() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool Disposing)
    {
      if (!Disposing || this._logmon == null)
        return;
      this._logmon.Close();
    }

    protected void Initialize()
    {
      this.OutputDir = Path.GetDirectoryName(this.FFUImagePath);
      Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.CreateDirectory(this.OutputDir);
      if (string.IsNullOrEmpty(this.AKRoot))
        this.AKRoot = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKRoot();
      if (string.IsNullOrEmpty(this.BSPRoot))
        this.BSPRoot = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetBSPRoot();
      if (string.IsNullOrEmpty(this.MSPackageRoot))
        this.MSPackageRoot = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetMSPackageRoot();
      this.AKToolsRoot = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKToolsRoot();
      this.ValidateParameters();
    }

    public string FFUImagePath { get; private set; }

    public string OEMInputFile { get; private set; }

    public string BSPRoot { get; set; }

    public string AKRoot { get; set; }

    public string MSPackageRoot { get; set; }

    public string CustomizationFile { get; set; }

    public Version OEMVersion { get; set; }

    public bool ReadLogOutput { get; set; }

    public bool ShowConsoleOutput { get; set; }

    public string OutputDir { get; private set; }

    public string LogFilePath { get; private set; }

    public string ImageAppFilePath { get; private set; }

    public int ExitCode { get; private set; }

    public string ErrorOutput { get; private set; }

    public bool BuildInProgress { get; private set; }

    public bool BuildSucceeded { get; private set; }

    public string AKToolsRoot { get; private set; }

    public string CommandLine { get; private set; }

    public event ImageBuilderEventHandler LogOutputChanged;

    public event ImageBuilderEventHandler BuildCompleted;

    public event ImageBuilderEventHandler BuildStarted;

    public event ImageBuilderEventHandler CancelStarted;

    public event ImageBuilderEventHandler CancelCompleted;

    public void BuildAsync()
    {
      if (string.IsNullOrEmpty(this.FFUImagePath))
        this.FFUImagePath = Path.Combine(this.OutputDir, "Flash.FFU");
      this.LogFilePath = Path.Combine(this.OutputDir, "Flash.ImageApp.log");
      if (File.Exists(this.LogFilePath))
        File.Delete(this.LogFilePath);
      this._cancel = false;
      this.ImageAppFilePath = Path.Combine(this.AKToolsRoot, "ImageApp.exe");
      if (this.ReadLogOutput)
      {
        this._logmon = new LogMonitor(this.LogFilePath);
        this._logmon.FileContentChanged += new LogMonitorEventHandler(this.FileContentChanged);
      }
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.FileName = this.ImageAppFilePath;
      startInfo.UseShellExecute = false;
      startInfo.CreateNoWindow = true;
      startInfo.WorkingDirectory = this.AKToolsRoot;
      startInfo.EnvironmentVariables["BSPROOT"] = this.BSPRoot;
      startInfo.EnvironmentVariables["WPDKCONTENTROOT"] = this.AKRoot;
      startInfo.EnvironmentVariables["PATH"] = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetPathWithMakeCatDir();
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat("\"{0}\" ", (object) this.FFUImagePath);
      stringBuilder.AppendFormat("\"{0}\" ", (object) this.OEMInputFile);
      stringBuilder.AppendFormat("\"{0}\" ", (object) this.MSPackageRoot);
      if (!string.IsNullOrWhiteSpace(this.CustomizationFile))
      {
        if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Exists(this.CustomizationFile))
          throw new FileNotFoundException(string.Format("Customization input file {0} not found", (object) this.CustomizationFile));
        stringBuilder.AppendFormat("{0}\"{1}\" ", (object) "/OEMVersion:", (object) this.OEMVersion.ToString());
        stringBuilder.AppendFormat("{0}\"{1}\" ", (object) "/OEMCustomizationXML:", (object) this.CustomizationFile);
      }
      startInfo.Arguments = stringBuilder.ToString();
      this.CommandLine = string.Format("{0} {1}", (object) startInfo.FileName, (object) startInfo.Arguments);
      Console.WriteLine("ImageApp Command Line: {0}", (object) this.CommandLine);
      this.ErrorOutput = string.Empty;
      ThreadPool.QueueUserWorkItem((WaitCallback) (param0 =>
      {
        this._process = new Process();
        this._process.StartInfo = startInfo;
        this._process.EnableRaisingEvents = true;
        this._process.Exited += new EventHandler(this.BuildProcessExited);
        if (this.ShowConsoleOutput)
        {
          this._process.StartInfo.UseShellExecute = false;
          this._process.StartInfo.RedirectStandardOutput = true;
          this._process.OutputDataReceived += new DataReceivedEventHandler(this.BuildOutputDataReceived);
        }
        this._process.Start();
        if (!this.ShowConsoleOutput)
          return;
        this._process.BeginOutputReadLine();
      }));
      this.ExitCode = -1;
      this.BuildInProgress = true;
      this.BuildSucceeded = false;
      if (this.BuildStarted == null)
        return;
      this.BuildStarted((object) this, new ImageBuilderEventArgs()
      {
        CommandLine = this.CommandLine
      });
    }

    public bool CancelBuild()
    {
      bool flag = false;
      if (this.BuildInProgress && this._process != null)
      {
        this._process.Kill();
        if (this._logmon != null)
        {
          this._logmon.Close();
          this._logmon = (LogMonitor) null;
        }
        if (this.CancelStarted != null)
        {
          this._cancel = true;
          this.CancelStarted((object) this, new ImageBuilderEventArgs());
        }
      }
      return flag;
    }

    public void WaitForCompletion() => this.waitHandle.WaitOne();

    private void FileContentChanged(object sender, LogMonitorEventArgs e)
    {
      if (this.LogOutputChanged == null || e.logEntries.Count <= 0)
        return;
      this.LogOutputChanged((object) this, new ImageBuilderEventArgs(e.logEntries));
    }

    private void BuildOutputDataReceived(object sender, DataReceivedEventArgs e) => Console.WriteLine(e.Data);

    private void SignOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      if (this.ShowConsoleOutput)
      {
        Console.WriteLine(e.Data);
      }
      else
      {
        if (this.LogOutputChanged == null)
          return;
        ObservableCollection<LogEntry> logEntries = new ObservableCollection<LogEntry>();
        if (e.Data != null)
          logEntries.Add(new LogEntry(e.Data));
        this.LogOutputChanged((object) this, new ImageBuilderEventArgs(logEntries));
      }
    }

    private void BuildProcessExited(object sender, EventArgs e)
    {
      try
      {
        if (this._process == null)
          return;
        this.ExitCode = this._process.ExitCode;
        this.BuildInProgress = false;
        if (this._logmon != null)
        {
          this._logmon.Close();
          this._logmon = (LogMonitor) null;
        }
        if (this._cancel)
        {
          if (this.CancelCompleted != null)
            this.CancelCompleted((object) this, new ImageBuilderEventArgs(this.ExitCode));
          this.ExitCode = -1;
        }
        else
        {
          if (this.ExitCode == 0)
            this.SignImage();
          this.DeviceNodeCleanup();
          if (this.BuildCompleted != null)
            this.BuildCompleted((object) this, new ImageBuilderEventArgs(this.ExitCode));
          this.BuildSucceeded = this.ExitCode == 0;
        }
      }
      finally
      {
        this.waitHandle.Set();
      }
    }

    protected void ValidateParameters()
    {
      if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(this.AKRoot))
        throw new WPIDException((Exception) new DirectoryNotFoundException(string.Format("WPAK Root {0} not found", (object) this.AKRoot)), "Windows Phone Adaptation Kit is not installed");
      if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(this.BSPRoot))
        throw new WPIDException((Exception) new DirectoryNotFoundException(string.Format("BSPRoot {0} not found", (object) this.BSPRoot)), "BSP Kit is not installed");
      if (string.IsNullOrEmpty(this.OEMInputFile))
        throw new WPIDException((Exception) new ArgumentNullException("OEMInputFile"), "OEMInputFile parameter cannot be null");
      if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Exists(this.OEMInputFile))
        throw new WPIDException((Exception) new FileNotFoundException(string.Format("OEMInputFile {0} not found", (object) this.OEMInputFile)), "OEM Input file not found");
      if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(this.MSPackageRoot))
        throw new WPIDException((Exception) new DirectoryNotFoundException(string.Format("MSPackageRoot {0} not found", (object) this.MSPackageRoot)), "Microsoft Packages cannot be found in the installed WPAK.");
      if (!string.IsNullOrEmpty(this.CustomizationFile) && !Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Exists(this.CustomizationFile))
        throw new WPIDException((Exception) new FileNotFoundException(string.Format("CustomizationFile {0} not found", (object) this.CustomizationFile)), "Customization Input file not found");
    }

    private void SignImage()
    {
      string catFilePath = Path.Combine(Path.GetDirectoryName(this.FFUImagePath), "Flash.cat");
      if (!this.InstallOEMCertificates() || !this.SignCatalog(catFilePath))
        return;
      this.InsertSignedCatalog(catFilePath, this.FFUImagePath);
    }

    private void DeviceNodeCleanup()
    {
      string str1 = Path.Combine(this.AKToolsRoot, string.Format("devicenodecleanup.{0}.exe", Environment.Is64BitOperatingSystem ? (object) "x64" : (object) "x86"));
      string str2 = string.Empty;
      ProcessStartInfo processStartInfo = new ProcessStartInfo();
      processStartInfo.FileName = str1;
      processStartInfo.UseShellExecute = false;
      processStartInfo.CreateNoWindow = true;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.WorkingDirectory = this.AKToolsRoot;
      Process process = new Process();
      process.StartInfo = processStartInfo;
      try
      {
        process.Start();
        str2 = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
      }
      catch (Exception ex)
      {
        str2 = str2 + "Exception: \n" + ex.Message;
      }
      finally
      {
        if (this.LogOutputChanged != null)
        {
          ObservableCollection<LogEntry> le = new ObservableCollection<LogEntry>();
          ((IEnumerable<string>) str2.Split(ImageBuilder._newLineDelim, StringSplitOptions.RemoveEmptyEntries)).ToList<string>().ForEach((Action<string>) (l => le.Add(new LogEntry(l))));
          this.LogOutputChanged((object) this, new ImageBuilderEventArgs(le));
        }
      }
    }

    private bool InstallOEMCertificates()
    {
      string str1 = Path.Combine(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKToolsRoot(), "InstallOEMCerts.cmd");
      string str2 = string.Empty;
      ProcessStartInfo processStartInfo = new ProcessStartInfo();
      processStartInfo.FileName = Environment.GetEnvironmentVariable("COMSPEC");
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("/C");
      stringBuilder.Append(" ");
      stringBuilder.Append("\"" + str1 + "\"");
      processStartInfo.Arguments = stringBuilder.ToString();
      processStartInfo.EnvironmentVariables["WPDKCONTENTROOT"] = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKRoot();
      processStartInfo.UseShellExecute = false;
      processStartInfo.CreateNoWindow = true;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.WorkingDirectory = this.AKToolsRoot;
      Process process = new Process();
      process.StartInfo = processStartInfo;
      try
      {
        process.Start();
        str2 = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
      }
      catch (Exception ex)
      {
        str2 = str2 + "Exception: \n" + ex.Message;
      }
      finally
      {
        this.ExitCode = process.ExitCode;
        if (this.ExitCode != 0 && this.LogOutputChanged != null)
        {
          ObservableCollection<LogEntry> le = new ObservableCollection<LogEntry>();
          le.Add(new LogEntry("Failed to Install OEM Certificates."));
          ((IEnumerable<string>) str2.Split(ImageBuilder._newLineDelim, StringSplitOptions.RemoveEmptyEntries)).ToList<string>().ForEach((Action<string>) (l => le.Add(new LogEntry(l))));
          this.LogOutputChanged((object) this, new ImageBuilderEventArgs(le));
        }
      }
      return this.ExitCode == 0;
    }

    private bool InsertSignedCatalog(string catFilePath, string ffuImage)
    {
      string str1 = Path.Combine(this.AKToolsRoot, "ImageSigner.exe");
      string str2 = string.Empty;
      ProcessStartInfo processStartInfo = new ProcessStartInfo();
      processStartInfo.FileName = str1;
      processStartInfo.UseShellExecute = false;
      processStartInfo.CreateNoWindow = true;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("\"sign\"");
      stringBuilder.Append(" ");
      stringBuilder.Append("\"" + this.FFUImagePath + "\"");
      stringBuilder.Append(" ");
      stringBuilder.Append("\"" + catFilePath + "\"");
      processStartInfo.Arguments = stringBuilder.ToString();
      processStartInfo.UseShellExecute = false;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.CreateNoWindow = true;
      processStartInfo.WorkingDirectory = this.AKToolsRoot;
      Process process = new Process();
      process.StartInfo = processStartInfo;
      process.EnableRaisingEvents = true;
      process.OutputDataReceived += new DataReceivedEventHandler(this.SignOutputDataReceived);
      try
      {
        process.Start();
        str2 = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
      }
      catch (Exception ex)
      {
        str2 = str2 + "Exception: \n" + ex.Message;
      }
      finally
      {
        this.ExitCode = process.ExitCode;
        if (this.ExitCode != 0 && this.LogOutputChanged != null)
        {
          ObservableCollection<LogEntry> le = new ObservableCollection<LogEntry>();
          le.Add(new LogEntry("Failed to sign FFU catalog."));
          ((IEnumerable<string>) str2.Split(ImageBuilder._newLineDelim, StringSplitOptions.RemoveEmptyEntries)).ToList<string>().ForEach((Action<string>) (l => le.Add(new LogEntry(l))));
          this.LogOutputChanged((object) this, new ImageBuilderEventArgs(le));
        }
      }
      return this.ExitCode == 0;
    }

    private bool SignCatalog(string catFilePath)
    {
      string str1 = Path.Combine(this.AKToolsRoot, "sign.cmd");
      string str2 = string.Empty;
      ProcessStartInfo processStartInfo = new ProcessStartInfo();
      processStartInfo.FileName = str1;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("-pk");
      stringBuilder.Append(" ");
      stringBuilder.Append("\"" + catFilePath + "\"");
      processStartInfo.Arguments = stringBuilder.ToString();
      processStartInfo.UseShellExecute = false;
      processStartInfo.CreateNoWindow = true;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.WorkingDirectory = this.AKToolsRoot;
      processStartInfo.EnvironmentVariables["SIGN_OEM"] = "1";
      processStartInfo.EnvironmentVariables["SIGN_WITH_TIMESTAMP"] = "0";
      Process process = new Process();
      process.OutputDataReceived += new DataReceivedEventHandler(this.SignOutputDataReceived);
      process.StartInfo = processStartInfo;
      try
      {
        process.Start();
        str2 = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
      }
      catch (Exception ex)
      {
        str2 = str2 + "Exception: \n" + ex.Message;
      }
      finally
      {
        this.ExitCode = process.ExitCode;
        if (this.ExitCode != 0 && this.LogOutputChanged != null)
        {
          ObservableCollection<LogEntry> le = new ObservableCollection<LogEntry>();
          le.Add(new LogEntry("Failed to sign catalog file " + catFilePath));
          ((IEnumerable<string>) str2.Split(ImageBuilder._newLineDelim, StringSplitOptions.RemoveEmptyEntries)).ToList<string>().ForEach((Action<string>) (l => le.Add(new LogEntry(l))));
          this.LogOutputChanged((object) this, new ImageBuilderEventArgs(le));
        }
      }
      return this.ExitCode == 0;
    }
  }
}
