// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.Helper.Contract
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;

namespace Microsoft.Phone.Test.TestMetadata.Helper
{
  public static class Contract
  {
    public static void Requires(bool precondition)
    {
      if (!precondition)
        throw new ArgumentException("Method precondition violated");
    }

    public static void Requires(bool precondition, string parameter)
    {
      if (!precondition)
        throw new ArgumentException("Invalid argument value", parameter);
    }

    public static void Requires(bool precondition, string parameter, string message)
    {
      if (!precondition)
        throw new ArgumentException(message, parameter);
    }

    public static void RequiresNotNull(object value, string parameter)
    {
      if (value == null)
        throw new ArgumentNullException(parameter);
    }

    public static void RequiresNotEmpty(string variable, string parameter)
    {
      if (variable != null && variable.Length == 0)
        throw new ArgumentException("Non-empty string required", parameter);
    }
  }
}
