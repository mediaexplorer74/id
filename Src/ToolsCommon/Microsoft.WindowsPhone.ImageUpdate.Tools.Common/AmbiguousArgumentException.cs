// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.AmbiguousArgumentException
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  [Serializable]
  public class AmbiguousArgumentException : ParseException
  {
    public AmbiguousArgumentException(string id1, string id2)
      : base(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Defined arguments '{0}' and '{1}' are ambiguous", new object[2]
      {
        (object) id1,
        (object) id2
      }))
    {
    }

    public AmbiguousArgumentException(string id1)
      : base(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Defined argument '{0}' is ambiguous", new object[1]
      {
        (object) id1
      }))
    {
    }

    public AmbiguousArgumentException()
    {
    }

    public AmbiguousArgumentException(string message, Exception except)
      : base("Program error:" + message, except)
    {
    }

    protected AmbiguousArgumentException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
