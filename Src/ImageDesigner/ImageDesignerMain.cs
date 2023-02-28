// ImageDesignerMain.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.ImageDesignerMain
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.UI;
using Microsoft.WindowsPhone.ImageDesigner.UI.Views;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.WindowsPhone.ImageDesigner
{
    internal class ImageDesignerMain
    {
        private const string SPLASH_IMAGE = "Resources\\Images\\ImageDesignerSplash.png";
        private const string RES_NAMESPACE = "Microsoft.WindowsPhone.ImageDesigner.UI.Resources";

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 0 || args[0].Equals("-Environment", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleLogSettings.Instance.LogToFile();

                new App().InitializeComponent();
                
                if (args.Length == 1)
                {
                    CustomMessageBox.ShowMessage(
                        Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtBadArgumentTitle"), string.Format((IFormatProvider)CultureInfo.CurrentCulture, Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtInvalidArgument")));
                    Application.Current.Shutdown(-1);
                }
                else
                {
                    if (args.Length == 2)
                    {
                        string str1 = args[1];
                        char[] chArray = new char[1] { ';' };
                        foreach (string str2 in str1.Split(chArray))
                        {
                            string[] strArray = str2.Split('=');
                            if (strArray.Length == 2)
                            {
                                Environment.SetEnvironmentVariable(strArray[0], strArray[1]);
                            }
                            else
                            {
                                CustomMessageBox.ShowMessage(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtBadArgumentTitle"), string.Format((IFormatProvider)CultureInfo.CurrentCulture, Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtUnexpectedArgument"), new object[1]
                                {
                  (object) str2
                                }));
                                Application.Current.Shutdown(-1);
                                return;
                            }
                        }
                    }
                    else if (args.Length > 2)
                    {
                        CustomMessageBox.ShowMessage(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtBadArgumentTitle"), string.Format((IFormatProvider)CultureInfo.CurrentCulture, Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtUnexpectedArgument"), new object[1]
                        {
              (object) args[2]
                        }));
                        Application.Current.Shutdown(-1);
                        return;
                    }

                    // check if run as not root (admin) mode
                    if (!ImageDesignerMain.SetAKRoot())
                    {
                        // show warning msg ( RnD & fix locale ! )
                        CustomMessageBox.ShowMessage
                        (
                            "Missing Environment Variable Title (No Software\\Microsoft\\Windows Phone SDKs\\V8.1\\WPAK registry key!)",//Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtMissingEnvironmentVariableTitle"),
                            " WPDK Content Root Not Set"//string.Format((IFormatProvider)CultureInfo.CurrentCulture, Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtWpdkContentRootNotSet"))
                        );
                        
                        Application.Current.Shutdown(-1);
                    }
                    else
                    {
                        string userConfig = UserConfig.GetUserConfig("Locale");
                        if (!string.IsNullOrWhiteSpace(userConfig))
                            LocalizationVM.Instance.SetLocale(userConfig);
                        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                        {
                            CustomMessageBox.ShowMessage(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtNotAdminTitle"), Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtNotAdminMessage"));
                            Application.Current.Shutdown(-1);
                        }
                        else
                        {
                            string str = Process.GetCurrentProcess().ProcessName;
                            int num = 1;
                            if (str.EndsWith("vshost", StringComparison.OrdinalIgnoreCase))
                            {
                                str = Path.GetFileNameWithoutExtension(str);
                                num = 0;
                            }
                            if (Process.GetProcessesByName(str).Length > num)
                            {
                                CustomMessageBox.ShowMessage(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtMultipleInstancesTitle"), Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtMultipleInstancesMessage"));
                                Application.Current.Shutdown(-1);
                            }
                            else
                            {
                                ResourceManager resourceManager = new ResourceManager("Microsoft.WindowsPhone.ImageDesigner.UI.Resources", typeof(IDContainer).Assembly);
                                SplashScreen splashScreen = new SplashScreen("Resources\\Images\\ImageDesignerSplash.png");
                                splashScreen.Show(false, true);
                                Stopwatch stopwatch = new Stopwatch();
                                stopwatch.Start();
                                Microsoft.WindowsPhone.ImageDesigner.UI.ImageDesigner imageDesigner = new Microsoft.WindowsPhone.ImageDesigner.UI.ImageDesigner();
                                imageDesigner.Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(ImageDesignerMain.Dispatcher_UnhandledException);
                                stopwatch.Stop();
                                int millisecondsTimeout = 1500 - (int)stopwatch.ElapsedMilliseconds;
                                if (millisecondsTimeout > 0)
                                    Thread.Sleep(millisecondsTimeout);
                                splashScreen.Close(TimeSpan.FromMilliseconds(1500.0));
                                imageDesigner.ShowDialog();
                            }
                        }
                    }
                }
            }
            else
            {
                int lpdwProcessId;
                int windowThreadProcessId = (int)ImageDesignerMain.GetWindowThreadProcessId(ImageDesignerMain.GetForegroundWindow(), out lpdwProcessId);
                Process processById = Process.GetProcessById(lpdwProcessId);
                if (processById.ProcessName == "cmd")
                    ImageDesignerMain.AttachConsole(processById.Id);
                else
                    ImageDesignerMain.AllocConsole();
                if (!ImageDesignerMain.SetAKRoot())
                {
                    Debug.WriteLine("Error: Could not find an installed phone kit. Set WPDKCONTENTROOT to point to a valid phone kit root");
                    Console.WriteLine("Error: Could not find an installed phone kit. Set WPDKCONTENTROOT to point to a valid phone kit root");
                    Environment.Exit(1);
                }
                LogUtil.LogCopyright();
                new IDCommandLine(Environment.CommandLine).Start();
                ImageDesignerMain.FreeConsole();
            }
        }

        private static void Dispatcher_UnhandledException(
          object sender,
          DispatcherUnhandledExceptionEventArgs e)
        {
            CustomMessageBox.ShowError(e.Exception);
            e.Handled = true;
        }

        private static bool SetAKRoot()
        {
            bool flag = false;
            string akRoot = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKRoot();
            if (akRoot != null)
            {
                Environment.SetEnvironmentVariable("WPDKCONTENTROOT", akRoot);
                flag = true;
            }
            return flag;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
    }
}
