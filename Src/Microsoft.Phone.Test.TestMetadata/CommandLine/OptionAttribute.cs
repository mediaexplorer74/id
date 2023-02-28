// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.OptionAttribute
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
  public sealed class OptionAttribute : Attribute
  {
    public OptionAttribute(string name, OptionValueType valueType)
    {
      this.Name = name;
      this.ValueType = valueType;
    }

    public string Name { get; }

    public OptionValueType ValueType { get; }

    public bool IsMultipleValue { get; set; }

    public string DefaultValue { get; set; }

    public bool IsFinalOption { get; set; }

    public string Description { get; set; }

    public Type CollectionType { get; set; }
  }
}
