// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.CmdHandler
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public abstract class CmdHandler
  {
    protected CommandLineParser _cmdLineParser = new CommandLineParser();

    protected abstract int DoExecution();

    public abstract string Command { get; }

    public abstract string Description { get; }

    public int Execute(string cmdParams, string applicationName)
    {
      if (this._cmdLineParser.ParseString("appName " + cmdParams, true))
        return this.DoExecution();
      LogUtil.Message(this._cmdLineParser.UsageString(applicationName + " " + this.Command));
      return -1;
    }
  }
}
