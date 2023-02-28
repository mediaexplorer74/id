// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.CommandSpecification
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  public class CommandSpecification
  {
    private readonly CommandAttribute _commandAttribute;

    public CommandSpecification(Type type)
    {
      this.CommandType = !(type == (Type) null) ? type : throw new ArgumentNullException(nameof (type));
      this._commandAttribute = this.CommandType.GetCustomAttributes(typeof (CommandAttribute), false)[0] as CommandAttribute;
      this.OptionSpecifications = new OptionSpecificationCollection();
      this.OptionSpecifications.LoadFromType(this.CommandType);
      this.CommandAliases = new StringCollection();
      foreach (CommandAliasAttribute customAttribute in this.CommandType.GetCustomAttributes(typeof (CommandAliasAttribute), true))
        this.CommandAliases.Add(customAttribute.Alias);
    }

    public bool AllowNoNameOptions => this._commandAttribute.AllowNoNameOptions;

    public string Name => this._commandAttribute.Name;

    public string GeneralInformation => this._commandAttribute.GeneralInformation;

    public string BriefDescription => this._commandAttribute.BriefDescription;

    public string BriefUsage => this._commandAttribute.BriefUsage;

    public OptionSpecificationCollection OptionSpecifications { get; }

    public Type CommandType { get; }

    public StringCollection CommandAliases { get; }

    public void PrintFullUsage(TextWriter writer)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof (writer));
      writer.WriteLine(" {0} - {1}", (object) this.Name, (object) this.BriefDescription);
      writer.WriteLine();
      writer.WriteLine(FormatUtility.FormatStringForWidth(this.GeneralInformation, 2, 0, 80));
      writer.WriteLine(" Usage: {0}", (object) this.BriefUsage);
      writer.WriteLine();
      if (this.OptionSpecifications.Count <= 0)
        return;
      writer.WriteLine(" Options:");
      writer.WriteLine();
      string[] array = new string[this.OptionSpecifications.Count];
      for (int index = 0; index < array.Length; ++index)
        array[index] = this.OptionSpecifications[index].OptionName;
      Array.Sort<string>(array);
      foreach (string optionName in array)
      {
        OptionSpecification optionSpecification = this.OptionSpecifications[optionName];
        if (optionSpecification.Description != null)
        {
          string toFormat = string.Format((IFormatProvider) CultureInfo.CurrentCulture, optionSpecification.Description, new object[1]
          {
            (object) optionSpecification.OptionName
          });
          writer.WriteLine(FormatUtility.FormatStringForWidth(toFormat, 2, 2, 80));
        }
      }
    }
  }
}
