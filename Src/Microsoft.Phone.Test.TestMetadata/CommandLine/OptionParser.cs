// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.OptionParser
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  public abstract class OptionParser
  {
    public abstract string ParseCommandName(string[] arguments);

    public abstract CommandOptionCollection Parse(
      string[] arguments,
      OptionSpecificationCollection optionSpecifications);

    public abstract void SetOptionProperty(
      object command,
      OptionSpecification optionSpecification,
      CommandOption commandOption);
  }
}
