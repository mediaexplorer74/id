// CommandAliasAttribute.cs
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.CommandAliasAttribute
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public sealed class CommandAliasAttribute : Attribute
  {
    public CommandAliasAttribute(string alias) => this.Alias = alias;

    public string Alias { get; }
  }
}
