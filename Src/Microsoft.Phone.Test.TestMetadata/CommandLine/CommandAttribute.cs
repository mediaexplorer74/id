// CommandAttribute.cs
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.CommandAttribute
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  [AttributeUsage(AttributeTargets.Class)]
  public sealed class CommandAttribute : Attribute
  {
    public CommandAttribute(string name)
    {
      this.Name = name;
      this.BriefUsage = string.Empty;
      this.GeneralInformation = string.Empty;
    }

    public string Name { get; }

    public string BriefDescription { get; set; }

    public string BriefUsage { get; set; }

    public string GeneralInformation { get; set; }

    public bool AllowNoNameOptions { get; set; }
  }
}
