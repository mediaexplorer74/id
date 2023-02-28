// PackageDependency.cs
// Type: Microsoft.Phone.Test.TestMetadata.ObjectModel.PackageDependency
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

//using Microsoft.Tools.IO;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Microsoft.Phone.Test.TestMetadata.ObjectModel
{
  [Serializable]
  public sealed class PackageDependency : Dependency
  {
    public PackageDependency()
    {
    }

    public PackageDependency(string name)
    {
      if (!name.EndsWith(".cab", StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".spkg", StringComparison.OrdinalIgnoreCase))
        return;

            //this.Name = Path.Combine(LongPathPath.GetDirectoryName(name), LongPathPath.GetFileNameWithoutExtension(name));
            this.Name = Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name));
        }

    [XmlAttribute]
    public string Name { get; set; }

    public override bool Equals(object obj) => obj is PackageDependency packageDependency && this.Name.Equals(packageDependency.Name, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() => this.Name.GetHashCode();
  }
}
