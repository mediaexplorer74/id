// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.CommandOption
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System.Collections.Specialized;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  public class CommandOption
  {
    public const string NoName = "";

    public CommandOption(string name) => this.Name = name;

    public string Name { get; }

    public void Add(string optionValue) => this.Values.Add(optionValue);

    public string Value => this.Values.Count > 0 ? this.Values[0] : string.Empty;

    public StringCollection Values { get; } = new StringCollection();

    public string this[int index] => this.Values[index];
  }
}
