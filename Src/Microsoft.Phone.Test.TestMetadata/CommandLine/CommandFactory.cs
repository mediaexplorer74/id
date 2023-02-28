// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.CommandFactory
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  public class CommandFactory
  {
    private readonly OptionParser _argumentParser;

    public CommandFactory(Assembly commandAssembly, OptionParser parser)
      : this(new Assembly[1]{ commandAssembly }, parser)
    {
    }

    public CommandFactory(Assembly[] commandAssemblies, OptionParser parser)
    {
      this._argumentParser = parser;
      this.LoadCommandSpecification(commandAssemblies);
    }

    public CommandSpecificationCollection Commands { get; private set; }

    public Command Create(string[] arguments)
    {
      Command command1 = (Command) null;
      string commandName = this._argumentParser.ParseCommandName(arguments);
      CommandSpecification command2 = this.Commands[commandName];
      if (command2 == null)
        throw new UsageException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Unknown command '{0}'", new object[1]
        {
          (object) commandName
        }));
      try
      {
        command1 = Activator.CreateInstance(command2.CommandType) as Command;
        command1.Specification = command2;
        CommandOptionCollection options = this._argumentParser.Parse(arguments, command2.OptionSpecifications);
        foreach (OptionSpecification optionSpecification in (CollectionBase) command2.OptionSpecifications)
        {
          if (optionSpecification.DefaultValue != null && !options.Contains(optionSpecification.OptionName))
          {
            CommandOption option = new CommandOption(optionSpecification.OptionName);
            option.Add(optionSpecification.DefaultValue);
            options.Add(option);
          }
        }
        if (!command1.Specification.AllowNoNameOptions && options[""] != null)
          throw new UsageException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Invalid option '{0}'", new object[1]
          {
            (object) options[""].Value
          }));
        foreach (OptionSpecification optionSpecification in (CollectionBase) command2.OptionSpecifications)
        {
          if (optionSpecification.RelatedProperty != (PropertyInfo) null && options.Contains(optionSpecification.OptionName))
          {
            this._argumentParser.SetOptionProperty((object) command1, optionSpecification, options[optionSpecification.OptionName]);
            options.Remove(optionSpecification.OptionName);
          }
        }
        command1.Load(options);
        if (command1.Output == null)
          command1.Output = Console.Out;
        if (command1.Error == null)
          command1.Error = Console.Error;
      }
      catch (CommandException ex)
      {
        if (ex.Command == null)
          ex.Command = command1;
        throw;
      }
      return command1;
    }

    private void LoadCommandSpecification(Assembly[] commandAssemblies)
    {
      this.Commands = new CommandSpecificationCollection();
      foreach (Assembly commandAssembly in commandAssemblies)
      {
        foreach (Type type in commandAssembly.GetTypes())
        {
          if (type.GetCustomAttributes(typeof (CommandAttribute), false).Length != 0)
            this.Commands.Add(new CommandSpecification(type));
        }
      }
    }
  }
}
