// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.DotNetOptionParser
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  public class DotNetOptionParser : OptionParser
  {
    private readonly string _commandName;

    public DotNetOptionParser(string commandName) => this._commandName = commandName;

    public override CommandOptionCollection Parse(
      string[] arguments,
      OptionSpecificationCollection optionSpecifications)
    {
      if (arguments == null)
        throw new ArgumentNullException(nameof (arguments));
      if (optionSpecifications == null)
        throw new ArgumentNullException(nameof (arguments));
      CommandOptionCollection optionCollection = new CommandOptionCollection();
      for (int index = 0; index < arguments.Length; ++index)
      {
        if (arguments[index][0] == '/' || arguments[index][0] == '-')
        {
          int num = arguments[index].IndexOf(':');
          string name;
          string optionValue;
          if (num >= 0)
          {
            name = arguments[index].Substring(1, num - 1);
            optionValue = arguments[index].Substring(num + 1);
          }
          else
          {
            name = arguments[index].Substring(1);
            optionValue = string.Empty;
          }
          CommandOption option = optionCollection[name];
          if (option == null)
          {
            option = new CommandOption(name);
            optionCollection.Add(option);
          }
          option.Add(optionValue);
        }
      }
      return optionCollection;
    }

    public override string ParseCommandName(string[] arguments) => this._commandName;

    public override void SetOptionProperty(
      object command,
      OptionSpecification optionSpecification,
      CommandOption commandOption)
    {
    }
  }
}
