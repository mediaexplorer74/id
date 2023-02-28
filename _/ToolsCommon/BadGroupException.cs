// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.BadGroupException
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [Serializable]
  public class BadGroupException : ParseException
  {
    public BadGroupException(string message)
      : base(message)
    {
    }

    public BadGroupException()
    {
    }

    public BadGroupException(string message, Exception except)
      : base("Program error:" + message, except)
    {
    }

    protected BadGroupException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
