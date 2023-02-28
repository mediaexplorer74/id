// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RegBuilder
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class RegBuilder
  {
    private static void CheckConflicts(IEnumerable<RegValueInfo> values)
    {
      Dictionary<string, RegValueInfo> dictionary = new Dictionary<string, RegValueInfo>();
      foreach (RegValueInfo regValueInfo1 in values)
      {
        if (regValueInfo1.ValueName != null)
        {
          RegValueInfo regValueInfo2 = (RegValueInfo) null;
          if (dictionary.TryGetValue(regValueInfo1.ValueName, out regValueInfo2))
            throw new IUException("Registry conflict discovered: keyName: {0}, valueName: {1}, oldValue: {2}, newValue: {3}", new object[4]
            {
              (object) regValueInfo1.KeyName,
              (object) regValueInfo1.ValueName,
              (object) regValueInfo2.Value,
              (object) regValueInfo1.Value
            });
          dictionary.Add(regValueInfo1.ValueName, regValueInfo1);
        }
      }
    }

    private static void ConvertRegSz(StringBuilder output, string name, string value) => RegUtil.RegOutput(output, name, value, false);

    private static void ConvertRegExpandSz(StringBuilder output, string name, string value) => RegUtil.RegOutput(output, name, value, true);

    private static void ConvertRegMultiSz(StringBuilder output, string name, string value) => RegUtil.RegOutput(output, name, (IEnumerable<string>) value.Split(';'));

    private static void ConvertRegDWord(StringBuilder output, string name, string value)
    {
      uint result = 0;
      if (!uint.TryParse(value, NumberStyles.AllowHexSpecifier, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat, out result))
        throw new IUException("Invalid dword string: {0}", new object[1]
        {
          (object) value
        });
      RegUtil.RegOutput(output, name, result);
    }

    private static void ConvertRegQWord(StringBuilder output, string name, string value)
    {
      ulong result = 0;
      if (!ulong.TryParse(value, NumberStyles.AllowHexSpecifier, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat, out result))
        throw new IUException("Invalid qword string: {0}", new object[1]
        {
          (object) value
        });
      RegUtil.RegOutput(output, name, result);
    }

    private static void ConvertRegBinary(StringBuilder output, string name, string value) => RegUtil.RegOutput(output, name, RegUtil.HexStringToByteArray(value));

    private static void ConvertRegHex(StringBuilder output, string name, string value)
    {
      List<byte> byteList = new List<byte>();
      Match match = Regex.Match(value, "^hex\\((?<type>[0-9A-Fa-f]+)\\):(?<value>.*)$");
      if (!match.Success)
        throw new IUException("Invalid value '{0}' for REG_HEX type, shoudl be 'hex(<type>):<binary_values>'", new object[1]
        {
          (object) value
        });
      int result = 0;
      if (!int.TryParse(match.Groups["type"].Value, NumberStyles.AllowHexSpecifier, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat, out result))
        throw new IUException("Invalid hex type '{0}' in REG_HEX value '{1}'", new object[2]
        {
          (object) match.Groups["type"].Value,
          (object) value
        });
      string hexString = match.Groups[nameof (value)].Value;
      RegUtil.RegOutput(output, name, result, RegUtil.HexStringToByteArray(hexString));
    }

    private static void WriteValue(RegValueInfo value, StringBuilder regContent)
    {
      switch (value.Type)
      {
        case RegValueType.String:
          RegBuilder.ConvertRegSz(regContent, value.ValueName, value.Value);
          break;
        case RegValueType.ExpandString:
          RegBuilder.ConvertRegExpandSz(regContent, value.ValueName, value.Value);
          break;
        case RegValueType.Binary:
          RegBuilder.ConvertRegBinary(regContent, value.ValueName, value.Value);
          break;
        case RegValueType.DWord:
          RegBuilder.ConvertRegDWord(regContent, value.ValueName, value.Value);
          break;
        case RegValueType.MultiString:
          RegBuilder.ConvertRegMultiSz(regContent, value.ValueName, value.Value);
          break;
        case RegValueType.QWord:
          RegBuilder.ConvertRegQWord(regContent, value.ValueName, value.Value);
          break;
        case RegValueType.Hex:
          RegBuilder.ConvertRegHex(regContent, value.ValueName, value.Value);
          break;
        default:
          throw new IUException("Unknown registry value type '{0}'", new object[1]
          {
            (object) value.Type
          });
      }
    }

    private static void WriteKey(
      string keyName,
      IEnumerable<RegValueInfo> values,
      StringBuilder regContent)
    {
      regContent.AppendFormat("[{0}]", (object) keyName);
      regContent.AppendLine();
      foreach (RegValueInfo regValueInfo in values)
      {
        if (regValueInfo.ValueName != null)
          RegBuilder.WriteValue(regValueInfo, regContent);
      }
      regContent.AppendLine();
    }

    public static void Build(IEnumerable<RegValueInfo> values, string outputFile) => RegBuilder.Build(values, outputFile, (string) null);

    public static void Build(
      IEnumerable<RegValueInfo> values,
      string outputFile,
      string headerComment = "")
    {
      StringBuilder regContent = new StringBuilder();
      regContent.AppendLine("Windows Registry Editor Version 5.00");
      if (!string.IsNullOrEmpty(headerComment))
      {
        foreach (string str1 in headerComment.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
        {
          string str2 = str1.TrimStart(' ');
          if (str2 != string.Empty && str2[0] == ';')
            regContent.AppendLine(str1);
          else
            regContent.AppendLine("; " + str1);
        }
        regContent.AppendLine("");
      }
      foreach (IGrouping<string, RegValueInfo> values1 in values.GroupBy<RegValueInfo, string>((Func<RegValueInfo, string>) (x => x.KeyName)))
      {
        RegBuilder.CheckConflicts((IEnumerable<RegValueInfo>) values1);
        RegBuilder.WriteKey(values1.Key, (IEnumerable<RegValueInfo>) values1, regContent);
      }
      LongPathFile.WriteAllText(outputFile, regContent.ToString(), Encoding.Unicode);
    }
  }
}
