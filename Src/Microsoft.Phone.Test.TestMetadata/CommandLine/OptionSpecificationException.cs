// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.OptionSpecificationException
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Runtime.Serialization;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  [Serializable]
  public class OptionSpecificationException : CommandException
  {
    public OptionSpecificationException()
    {
    }

    public OptionSpecificationException(string message)
      : base(message)
    {
    }

    public OptionSpecificationException(string message, Exception exception)
      : base(message, exception)
    {
    }

    protected OptionSpecificationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
