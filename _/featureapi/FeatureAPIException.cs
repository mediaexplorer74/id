// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.FeatureAPI.FeatureAPIException
// Assembly: FeatureAPI, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 4F41EFE3-23B1-4967-A014-E2DD7F414640
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\featureapi.dll

using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
  [Serializable]
  public class FeatureAPIException : Exception
  {
    public FeatureAPIException()
    {
    }

    public FeatureAPIException(string message)
      : base(message)
    {
    }

    public FeatureAPIException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected FeatureAPIException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override string ToString()
    {
      string message = this.Message;
      if (this.InnerException != null)
        message += this.InnerException.ToString();
      return message;
    }
  }
}
