// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.OptionSpecification
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Reflection;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  public class OptionSpecification
  {
    private readonly OptionAttribute _attribute;

    public OptionSpecification(OptionAttribute attribute, PropertyInfo relatedProperty)
    {
      this._attribute = attribute;
      this.RelatedProperty = relatedProperty;
    }

    public string OptionName => this._attribute.Name;

    public OptionValueType ValueType => this._attribute.ValueType;

    public bool IsFinalOption => this._attribute.IsFinalOption;

    public bool IsMultipleValue => this._attribute.IsMultipleValue;

    public PropertyInfo RelatedProperty { get; }

    public string DefaultValue => this._attribute.DefaultValue;

    public string Description => this._attribute.Description;

    public Type CollectionType => this._attribute.CollectionType;
  }
}
