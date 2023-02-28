// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ExtensionMethods
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Text;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public static class ExtensionMethods
  {
    public static string GetExceptionMessage(this Exception e)
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (e != null)
      {
        for (Exception exception = e; exception != null; exception = exception.InnerException)
        {
          if (!string.IsNullOrWhiteSpace(exception.Message))
          {
            stringBuilder.Append(exception.Message);
            stringBuilder.Append(" ");
          }
        }
      }
      return stringBuilder.ToString().Trim();
    }

    public static Version Normalize(this Version version)
    {
      Version result = version;
      if (result != (Version) null)
      {
        StringBuilder stringBuilder = new StringBuilder(version.Major.ToString(), 50);
        stringBuilder.Append(".");
        stringBuilder.Append(version.Minor < 0 ? "0" : version.Minor.ToString());
        stringBuilder.Append(".");
        stringBuilder.Append(version.Build < 0 ? "0" : version.Build.ToString());
        stringBuilder.Append(".");
        stringBuilder.Append(version.Revision < 0 ? "0" : version.Revision.ToString());
        Version.TryParse(stringBuilder.ToString(), out result);
      }
      return result;
    }
  }
}
