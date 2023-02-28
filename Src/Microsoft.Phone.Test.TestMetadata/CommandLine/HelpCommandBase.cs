// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.HelpCommandBase
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Reflection;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  public class HelpCommandBase : Command
  {
    private readonly CommandFactory _factory;

    public HelpCommandBase()
      : this((Assembly) null)
    {
    }

    public HelpCommandBase(Assembly commandAssembly)
    {
      if (commandAssembly != (Assembly) null)
        this._factory = new CommandFactory(commandAssembly, (OptionParser) new StandardOptionParser());
      else
        this._factory = new CommandFactory(this.GetType().Assembly, (OptionParser) new StandardOptionParser());
    }

    [Option("", OptionValueType.NoValue)]
    public string CommandName { get; set; }

    protected override void RunImplementation()
    {
      this.Output.WriteLine();
      if (this.CommandName != null && this._factory.Commands[this.CommandName] != null)
      {
        this._factory.Commands[this.CommandName].PrintFullUsage(this.Output);
      }
      else
      {
        if (this.CommandName != null && this._factory.Commands[this.CommandName] == null)
        {
          this.Output.WriteLine("Unknown command '{0}'", (object) this.CommandName);
          this.Output.WriteLine();
        }
        this.Output.WriteLine("The following commands are available");
        this.Output.WriteLine();
        string[] array = new string[this._factory.Commands.Count];
        for (int index = 0; index < array.Length; ++index)
          array[index] = this._factory.Commands[index].Name;
        Array.Sort<string>(array);
        foreach (string name in array)
        {
          CommandSpecification command = this._factory.Commands[name];
          this.Output.WriteLine("  {0} - {1}", (object) command.Name, (object) command.BriefDescription);
        }
      }
    }
  }
}
