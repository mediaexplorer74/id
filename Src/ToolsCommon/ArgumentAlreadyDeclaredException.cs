// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.ArgumentAlreadyDeclaredException
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [Serializable]
  public class ArgumentAlreadyDeclaredException : ParseException
  {
    public ArgumentAlreadyDeclaredException(string id)
      : base(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Argument '{0}' was already defined", new object[1]
      {
        (object) id
      }))
    {
    }

    public ArgumentAlreadyDeclaredException()
    {
    }

    public ArgumentAlreadyDeclaredException(string message, Exception except)
      : base("Program error:" + message, except)
    {
    }

    protected ArgumentAlreadyDeclaredException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
