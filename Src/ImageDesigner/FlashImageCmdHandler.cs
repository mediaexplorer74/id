// Decompiled with JetBrains decompiler
// Type: FlashImageCmdHandler
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Diagnostics;
using System.IO;

internal class FlashImageCmdHandler : CmdHandler
{
  private const string PARAM_IMAGEFILE = "ImageFile";
  private const string PARAM_IMAGEFILE_DESCR = "Path to the image file (.FFU)";

  public FlashImageCmdHandler() => this._cmdLineParser.SetRequiredParameterString("ImageFile", "Path to the image file (.FFU)");

  public override string Command => "Flash";

  public override string Description => "Flash a Windows Phone image to your device";

  protected override int DoExecution()
  {
    ConsoleLogSettings.Instance.LogToFileAndConsole();
    string parameterAsString = this._cmdLineParser.GetParameterAsString("ImageFile");
    DeviceFlasher deviceFlasher = Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Exists(parameterAsString) ? new DeviceFlasher(parameterAsString) : throw new WPIDException((Exception) new FileNotFoundException(string.Format("FFUImage {0} not found", (object) parameterAsString), parameterAsString), "Specified FFU file does not exist");
    deviceFlasher.ShowConsoleOutput = true;
    deviceFlasher.BeginFlashAsync();
    deviceFlasher.WaitForCompletion();
    return 0;
  }

  private void FlashOutputDataReceived(object sender, DataReceivedEventArgs e) => Console.WriteLine(e.Data);
}
