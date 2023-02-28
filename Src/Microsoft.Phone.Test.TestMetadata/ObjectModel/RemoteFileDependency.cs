// RemoteFileDependency.cs
// Type: Microsoft.Phone.Test.TestMetadata.ObjectModel.RemoteFileDependency
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Xml.Serialization;

namespace Microsoft.Phone.Test.TestMetadata.ObjectModel
{
  [Serializable]
  public sealed class RemoteFileDependency : Dependency
  {
    public RemoteFileDependency()
    {
    }

    public RemoteFileDependency(
      string fileShare,
      string source,
      string destinationPath,
      string destination)
    {
      this.SourcePath = fileShare;
      this.Source = source;
      this.DestinationPath = destinationPath;
      this.Destination = destination;
    }

    [XmlAttribute]
    public string SourcePath { get; set; }

    [XmlAttribute]
    public string Source { get; set; }

    [XmlAttribute]
    public string DestinationPath { get; set; }

    [XmlAttribute]
    public string Destination { get; set; }

    [XmlAttribute]
    public string Tags { get; set; }

    [XmlAttribute]
    public bool IsWow { get; set; }

    [XmlAttribute]
    public string PackageArchitecture { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is RemoteFileDependency remoteFileDependency))
        return false;
      bool flag1 = false;
      if (!string.IsNullOrEmpty(this.Tags))
        flag1 = this.Tags.Equals(remoteFileDependency.Tags, StringComparison.OrdinalIgnoreCase);
      else if (string.IsNullOrEmpty(remoteFileDependency.Tags))
        flag1 = true;
      bool flag2 = false;
      if (!string.IsNullOrEmpty(this.PackageArchitecture))
        flag2 = this.PackageArchitecture.Equals(remoteFileDependency.PackageArchitecture, StringComparison.OrdinalIgnoreCase);
      else if (string.IsNullOrEmpty(remoteFileDependency.PackageArchitecture))
        flag2 = true;
      return ((((!this.SourcePath.Equals(remoteFileDependency.SourcePath, StringComparison.OrdinalIgnoreCase) || !this.Source.Equals(remoteFileDependency.Source, StringComparison.OrdinalIgnoreCase) || !this.DestinationPath.Equals(remoteFileDependency.DestinationPath, StringComparison.OrdinalIgnoreCase) ? 0 : (this.Destination.Equals(remoteFileDependency.Destination, StringComparison.OrdinalIgnoreCase) ? 1 : 0)) & (flag2 ? 1 : 0)) == 0 ? 0 : (this.IsWow == remoteFileDependency.IsWow ? 1 : 0)) & (flag1 ? 1 : 0)) != 0;
    }

    public override int GetHashCode()
    {
      int hashCode = string.Empty.GetHashCode();
      if (!string.IsNullOrEmpty(this.Tags))
        hashCode = this.Tags.GetHashCode();
      return this.SourcePath.GetHashCode() ^ this.Source.GetHashCode() ^ this.DestinationPath.GetHashCode() ^ this.Destination.GetHashCode() ^ hashCode;
    }
  }
}
