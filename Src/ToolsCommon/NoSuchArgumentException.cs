// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.NoSuchArgumentException
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [Serializable]
  public class NoSuchArgumentException : ParseException
  {
    public NoSuchArgumentException(string type, string id)
      : base(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "The {0} '{1}' was not defined", new object[2]
      {
        (object) type,
        (object) id
      }))
    {
    }

    public NoSuchArgumentException(string id)
      : base(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "The '{0}' was not defined", new object[1]
      {
        (object) id
      }))
    {
    }

    public NoSuchArgumentException()
    {
    }

    public NoSuchArgumentException(string message, Exception except)
      : base("Program error:" + message, except)
    {
    }

    protected NoSuchArgumentException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
