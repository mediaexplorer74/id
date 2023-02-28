// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.StandardOptionParser
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  public class StandardOptionParser : OptionParser
  {
    private const string NameGroup = "name";
    private const string PlusMinusGroup = "plusminus";
    private static readonly Regex s_optionRegex = new Regex("^(\\/|\\-|–)(?<name>[^\\+\\-]+)(?<plusminus>\\+|\\-)?$");
    private readonly string _commandName;

    public StandardOptionParser()
      : this((string) null)
    {
    }

    public StandardOptionParser(string commandName) => this._commandName = commandName;

    public override string ParseCommandName(string[] arguments)
    {
      if (this._commandName != null)
        return this._commandName;
      return arguments != null && arguments.Length != 0 ? arguments[0] : throw new UsageException("No command is specified");
    }

    public override CommandOptionCollection Parse(
      string[] arguments,
      OptionSpecificationCollection optionSpecifications)
    {
      if (arguments == null)
        throw new ArgumentNullException(nameof (arguments));
      if (optionSpecifications == null)
        throw new ArgumentNullException(nameof (arguments));
      CommandOptionCollection optionCollection = new CommandOptionCollection();
      for (int index = this._commandName == null ? 1 : 0; index < arguments.Length; ++index)
      {
        OptionSpecification optionSpecification = (OptionSpecification) null;
        Match match = StandardOptionParser.s_optionRegex.Match(arguments[index]);
        string optionValue;
        string name;
        if (match.Success)
        {
          string partialOptionName = match.Groups["name"].Value;
          string str = match.Groups["plusminus"].Success ? match.Groups["plusminus"].Value : (string) null;
          optionValue = (string) null;
          IList<OptionSpecification> partial = optionSpecifications.GetPartial(partialOptionName);
          if (partial.Count == 0)
            throw new UsageException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "{0} is not a valid option.", new object[1]
            {
              (object) arguments[index]
            }));
          optionSpecification = partial.Count <= 1 ? partial[0] : throw new UsageException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "{0} is an ambiguous option.", new object[1]
          {
            (object) arguments[index]
          }));
          name = optionSpecification.OptionName;
          if (optionSpecification.IsFinalOption)
          {
            CommandOption option = new CommandOption(optionSpecification.OptionName);
            for (++index; index < arguments.Length; ++index)
              option.Add(arguments[index]);
            optionCollection.Add(option);
            continue;
          }
          if (optionSpecification.ValueType == OptionValueType.NoValue)
          {
            if (str != null)
              optionValue = str;
          }
          else if (index + 1 < arguments.Length)
          {
            if (arguments[index + 1].StartsWith("-", StringComparison.OrdinalIgnoreCase) || arguments[index + 1].StartsWith("/", StringComparison.OrdinalIgnoreCase) || arguments[index + 1].StartsWith("–", StringComparison.OrdinalIgnoreCase))
            {
              if (optionSpecification.ValueType != OptionValueType.ValueOptional)
                throw new UsageException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "No value specified for option {0}", new object[1]
                {
                  (object) arguments[index]
                }));
            }
            else
            {
              optionValue = arguments[index + 1];
              ++index;
            }
          }
          else if (optionSpecification.ValueType == OptionValueType.ValueRequired)
            throw new UsageException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "No value specified for option {0}", new object[1]
            {
              (object) arguments[index]
            }));
        }
        else
        {
          name = "";
          optionValue = arguments[index];
        }
        CommandOption option1 = optionCollection[name];
        if (option1 == null)
        {
          option1 = new CommandOption(name);
          optionCollection.Add(option1);
        }
        else if (optionSpecification != null && !optionSpecification.IsMultipleValue)
          throw new UsageException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "-{0} is specified more than once.", new object[1]
          {
            (object) name
          }));
        if (optionValue != null)
          option1.Add(optionValue);
      }
      return optionCollection;
    }

    public override void SetOptionProperty(
      object command,
      OptionSpecification optionSpecification,
      CommandOption commandOption)
    {
      if (command == null)
        throw new ArgumentNullException(nameof (command));
      if (optionSpecification == null)
        throw new ArgumentNullException(nameof (optionSpecification));
      if (commandOption == null)
        throw new ArgumentNullException(nameof (commandOption));
      PropertyInfo relatedProperty = optionSpecification.RelatedProperty;
      Type propertyType = optionSpecification.RelatedProperty.PropertyType;
      if (typeof (bool).IsAssignableFrom(propertyType))
      {
        if (string.IsNullOrEmpty(commandOption.Value) || commandOption.Value == "+")
        {
          relatedProperty.SetValue(command, (object) true, (object[]) null);
        }
        else
        {
          if (!(commandOption.Value == "-"))
            return;
          relatedProperty.SetValue(command, (object) false, (object[]) null);
        }
      }
      else if (typeof (IList).IsAssignableFrom(propertyType))
      {
        if (!(optionSpecification.RelatedProperty.GetValue(command, (object[]) null) is IList list))
          throw new CommandException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "The collection property {0} needs to be initialized in the class constructor.", new object[1]
          {
            (object) optionSpecification.RelatedProperty.Name
          }));
        Type collectionType = optionSpecification.CollectionType;
        foreach (object obj1 in commandOption.Values)
        {
          object obj2 = Convert.ChangeType(obj1, collectionType, (IFormatProvider) CultureInfo.CurrentCulture);
          list.Add(obj2);
        }
      }
      else
      {
        object obj = Convert.ChangeType((object) commandOption.Value, propertyType, (IFormatProvider) CultureInfo.CurrentCulture);
        relatedProperty.SetValue(command, obj, (object[]) null);
      }
    }
  }
}
