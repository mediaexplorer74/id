// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.IDCommandLine
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageDesigner
{
  internal class IDCommandLine
  {
    private string _commandLine;
    private static readonly Regex _regex_extractAppName = new Regex("((?<appname>(^\")([^\"]*)(\"))(?<args>.*))|((?<appname>([^\\s]*))(?<args>.*))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public IDCommandLine(string commandLine) => this._commandLine = commandLine;

    public int Start()
    {
      int num = -1;
      MultiCmdHandler multiCmdHandler = new MultiCmdHandler();
      multiCmdHandler.AddCmdHandler((CmdHandler) new BuildImageCmdHandler());
      multiCmdHandler.AddCmdHandler((CmdHandler) new FlashImageCmdHandler());
      try
      {
        string empty = string.Empty;
        string str1 = string.Empty;
        string commandLine = Environment.CommandLine;
        Match match = IDCommandLine._regex_extractAppName.Match(commandLine);
        if (match.Success)
          commandLine = match.Groups["args"].Value;
        IEnumerable<string> strings = (IEnumerable<string>) commandLine.Split(new char[1]
        {
          ' '
        }, StringSplitOptions.RemoveEmptyEntries);
        string str2;
        if (strings.Count<string>() > 1)
        {
          str1 = strings.ElementAt<string>(0);
          str2 = string.Join(" ", strings.Skip<string>(1));
        }
        else
          str2 = string.Join(" ", strings);
        num = multiCmdHandler.Run(new string[2]
        {
          str1,
          str2
        });
      }
      catch (Exception ex)
      {
        LogUtil.Message("Internal exception:{0}", (object) ex.Message);
      }
      return num;
    }
  }
}
