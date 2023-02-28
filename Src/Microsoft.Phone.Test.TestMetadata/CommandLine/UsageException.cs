// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.UsageException
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Runtime.Serialization;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  [Serializable]
  public class UsageException : CommandException
  {
    public UsageException()
    {
    }

    public UsageException(string message)
      : base(message)
    {
    }

    public UsageException(string message, Exception exception)
      : base(message, exception)
    {
    }

    protected UsageException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
