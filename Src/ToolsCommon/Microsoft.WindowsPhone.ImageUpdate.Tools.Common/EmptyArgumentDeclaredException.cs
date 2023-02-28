// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.EmptyArgumentDeclaredException
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [Serializable]
  public class EmptyArgumentDeclaredException : ParseException
  {
    public EmptyArgumentDeclaredException()
      : base("You cannot define an argument with ID: \"\"")
    {
    }

    public EmptyArgumentDeclaredException(string message)
      : base("You cannot define an argument with ID: " + message)
    {
    }

    public EmptyArgumentDeclaredException(string message, Exception except)
      : base("Program error:" + message, except)
    {
    }

    protected EmptyArgumentDeclaredException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
