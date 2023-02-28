// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.WPIDException
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class WPIDException : IUException
  {
    public WPIDException()
    {
    }

    public WPIDException(string message)
      : base(message)
    {
    }

    public WPIDException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public WPIDException(string message, params object[] args)
      : this(string.Format(message, args))
    {
    }

    public WPIDException(Exception innerException, string message)
      : base(message, innerException)
    {
    }

    public WPIDException(Exception innerException, string message, params object[] args)
      : this(innerException, string.Format(message, args))
    {
    }
  }
}
