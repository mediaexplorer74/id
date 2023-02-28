// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.MultiCmdHandler
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class MultiCmdHandler
  {
    private string appName = new FileInfo(Environment.GetCommandLineArgs()[0]).Name;
    private Dictionary<string, CmdHandler> _allHandlers = new Dictionary<string, CmdHandler>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);

    private void ShowUsage()
    {
      LogUtil.Message("Usage: {0} <command> <parameters>", (object) this.appName);
      LogUtil.Message("\t available command:");
      foreach (KeyValuePair<string, CmdHandler> allHandler in this._allHandlers)
        LogUtil.Message("\t\t{0}:{1}", (object) allHandler.Value.Command, (object) allHandler.Value.Description);
      LogUtil.Message("\t Run {0} <command> /? to check command line parameters for each command", (object) this.appName);
    }

    public void AddCmdHandler(CmdHandler cmdHandler) => this._allHandlers.Add(cmdHandler.Command, cmdHandler);

    public int Run(string[] args)
    {
      if (args.Length < 1)
      {
        this.ShowUsage();
        return -1;
      }
      int num = -1;
      string cmdParams = args.Length > 1 ? string.Join(" ", ((IEnumerable<string>) args).Skip<string>(1)) : string.Empty;
      CmdHandler cmdHandler = (CmdHandler) null;
      if (!this._allHandlers.TryGetValue(args[0], out cmdHandler))
        this.ShowUsage();
      else
        num = cmdHandler.Execute(cmdParams, this.appName);
      return num;
    }
  }
}
