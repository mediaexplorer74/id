// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.IUException
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class IUException : Exception
  {
    public IUException()
    {
    }

    public IUException(string message)
      : base(message)
    {
    }

    public IUException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public IUException(string message, params object[] args)
      : this(string.Format(message, args))
    {
    }

    public IUException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public IUException(Exception innerException, string message)
      : base(message, innerException)
    {
    }

    public IUException(Exception innerException, string message, params object[] args)
      : this(innerException, string.Format(message, args))
    {
    }

    public string MessageTrace
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder();
        for (Exception exception = (Exception) this; exception != null; exception = exception.InnerException)
        {
          if (!string.IsNullOrEmpty(exception.Message))
            stringBuilder.AppendLine(exception.Message);
        }
        return stringBuilder.ToString();
      }
    }
  }
}
