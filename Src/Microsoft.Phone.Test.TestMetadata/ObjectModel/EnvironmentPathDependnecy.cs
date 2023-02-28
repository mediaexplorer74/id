// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.ObjectModel.EnvironmentPathDependnecy
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Xml.Serialization;

namespace Microsoft.Phone.Test.TestMetadata.ObjectModel
{
  [Serializable]
  public sealed class EnvironmentPathDependnecy : Dependency
  {
    public EnvironmentPathDependnecy()
    {
    }

    public EnvironmentPathDependnecy(string name) => this.Name = name;

    [XmlAttribute]
    public string Name { get; set; }

    public override bool Equals(object obj) => obj is EnvironmentPathDependnecy environmentPathDependnecy && this.Name.Equals(environmentPathDependnecy.Name, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() => this.Name.GetHashCode();
  }
}
