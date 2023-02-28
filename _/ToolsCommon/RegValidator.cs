// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RegValidator
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class RegValidator
  {
    [DllImport("RegistryAPI", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ValidateRegFiles(
      string[] rgRegFiles,
      int cRegFiles,
      string[] rgaFiles,
      int cRgaFiles);

    public static void Validate(IEnumerable<string> regFiles, IEnumerable<string> rgaFiles)
    {
      string[] rgRegFiles = regFiles != null ? regFiles.ToArray<string>() : new string[0];
      string[] rgaFiles1 = rgaFiles != null ? rgaFiles.ToArray<string>() : new string[0];
      if (rgRegFiles.Length == 0 && rgaFiles1.Length == 0)
        return;
      int num = RegValidator.ValidateRegFiles(rgRegFiles, rgRegFiles.Length, rgaFiles1, rgaFiles1.Length);
      if (num != 0)
        throw new IUException("Registry validation failed, check output log for detailed failure information, err '0x{0:X8}'", new object[1]
        {
          (object) num
        });
    }
  }
}
