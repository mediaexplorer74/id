// Decompiled with JetBrains decompiler
// Type: BuildImageCmdHandler
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

internal class BuildImageCmdHandler : CmdHandler
{
  private ImageBuilder _imageBuilder;

  public BuildImageCmdHandler()
  {
    this._cmdLineParser.SetRequiredParameterString("OutputFile", "Path to the OEM Input XML file");
    this._cmdLineParser.SetRequiredParameterString("OemInputXml", "Path to the OEM Input XML file");
    this._cmdLineParser.SetOptionalSwitchString("CustomizationFile", "Path to the customization file");
    this._cmdLineParser.SetOptionalSwitchString("MSPackageRoot", "Path to the folder containing Microsoft base packages");
    this._cmdLineParser.SetOptionalSwitchString("BSPConfigFile", "Path to the BSPConfig.xml file under BSP root");
  }

  public override string Command => "GenerateImage";

  public override string Description => "Build a Windows Phone image";

  protected override int DoExecution()
  {
    ConsoleLogSettings.Instance.LogToFileAndConsole();
    string parameterAsString1 = this._cmdLineParser.GetParameterAsString("OutputFile");
    string parameterAsString2 = this._cmdLineParser.GetParameterAsString("OemInputXml");
    if (!LongPathFile.Exists(parameterAsString2))
    {
      LogUtil.Error("File '{0}' does not exist", (object) parameterAsString2);
      return -1;
    }
    this._imageBuilder = new ImageBuilder(parameterAsString1, parameterAsString2);
    if (this._cmdLineParser.IsAssignedSwitch("CustomizationFile"))
    {
      string switchAsString = this._cmdLineParser.GetSwitchAsString("CustomizationFile");
      if (!LongPathFile.Exists(switchAsString))
      {
        LogUtil.Error("File '{0}' does not exist", (object) switchAsString);
        return -1;
      }
      this._imageBuilder.CustomizationFile = switchAsString;
    }
    if (this._cmdLineParser.IsAssignedSwitch("MSPackageRoot"))
    {
      string switchAsString = this._cmdLineParser.GetSwitchAsString("MSPackageRoot");
      if (!LongPathDirectory.Exists(switchAsString))
      {
        LogUtil.Error("Folder '{0}' does not exist", (object) switchAsString);
        return -1;
      }
      this._imageBuilder.MSPackageRoot = switchAsString;
    }
    if (this._cmdLineParser.IsAssignedSwitch("BSPConfigFile"))
    {
      string switchAsString = this._cmdLineParser.GetSwitchAsString("BSPConfigFile");
      if (!LongPathFile.Exists(switchAsString))
      {
        LogUtil.Error("File '{0}' does not exist", (object) switchAsString);
        return -1;
      }
      this._imageBuilder.BSPRoot = BSPConfig.LoadFromXml(switchAsString, this._imageBuilder.MSPackageRoot).BSPRoot;
    }
    this._imageBuilder.ShowConsoleOutput = true;
    this._imageBuilder.OEMVersion = IDContext.Instance.CustomizationPkgVersion;
    this._imageBuilder.BuildAsync();
    this._imageBuilder.WaitForCompletion();
    return this._imageBuilder.ExitCode;
  }
}
