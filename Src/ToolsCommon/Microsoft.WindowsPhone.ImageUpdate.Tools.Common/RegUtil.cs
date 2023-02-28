// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RegUtil
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public static class RegUtil
  {
    private const string c_strDefaultValueName = "@";
    private const int c_iBinaryStringLengthPerByte = 3;
    public const string c_strRegHeader = "Windows Registry Editor Version 5.00";
    public static int BinaryLineLength = 120;

    private static string QuoteString(string input) => "\"" + input.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    private static string NormalizeValueName(string name) => name == "@" ? "@" : RegUtil.QuoteString(name);

    private static byte[] RegStringToBytes(string value) => Encoding.Unicode.GetBytes(value);

    public static RegValueType RegValueTypeForString(string strType)
    {
      foreach (FieldInfo field in typeof (RegValueType).GetFields())
      {
        object[] customAttributes = field.GetCustomAttributes(typeof (XmlEnumAttribute), false);
        if (customAttributes.Length == 1 && strType.Equals(((XmlEnumAttribute) customAttributes[0]).Name, StringComparison.OrdinalIgnoreCase))
          return (RegValueType) field.GetRawConstantValue();
      }
      throw new ArgumentException(string.Format("Unknown Registry value type: {0}", (object) strType));
    }

    public static byte[] HexStringToByteArray(string hexString)
    {
      List<byte> byteList = new List<byte>();
      if (hexString != string.Empty)
      {
        string str = hexString;
        char[] chArray = new char[1]{ ',' };
        foreach (string s in str.Split(chArray))
        {
          byte result = 0;
          if (!byte.TryParse(s, NumberStyles.AllowHexSpecifier, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat, out result))
            throw new IUException("Invalid hex string: {0}", new object[1]
            {
              (object) hexString
            });
          byteList.Add(result);
        }
      }
      return byteList.ToArray();
    }

    public static void ByteArrayToRegString(StringBuilder output, byte[] data, int maxOnALine = 2147483647)
    {
      int startIndex = 0;
      int length1 = data.Length;
      while (length1 > 0)
      {
        int length2 = Math.Min(length1, maxOnALine);
        string str = BitConverter.ToString(data, startIndex, length2).Replace('-', ',');
        output.Append(str);
        startIndex += length2;
        length1 -= length2;
        if (length1 > 0)
          output.AppendLine(",\\");
      }
    }

    public static void RegOutput(StringBuilder output, string name, IEnumerable<string> values)
    {
      string str = RegUtil.NormalizeValueName(name);
      output.AppendFormat(";Value:{0}", (object) string.Join(";", values.Select<string, string>((Func<string, string>) (x => x.Replace(";", "\\;")))));
      output.AppendLine();
      output.AppendFormat("{0}=hex(7):", (object) str);
      RegUtil.ByteArrayToRegString(output, RegUtil.RegStringToBytes(string.Join("\0", values) + "\0\0"), RegUtil.BinaryLineLength / 3);
      output.AppendLine();
    }

    public static void RegOutput(StringBuilder output, string name, string value, bool expandable)
    {
      string str = RegUtil.NormalizeValueName(name);
      if (expandable)
      {
        output.AppendFormat(";Value:{0}", (object) value);
        output.AppendLine();
        output.AppendFormat("{0}=hex(2):", (object) str);
        RegUtil.ByteArrayToRegString(output, RegUtil.RegStringToBytes(value + "\0"), RegUtil.BinaryLineLength / 3);
      }
      else
        output.AppendFormat("{0}={1}", (object) str, (object) RegUtil.QuoteString(value));
      output.AppendLine();
    }

    public static void RegOutput(StringBuilder output, string name, ulong value)
    {
      string str = RegUtil.NormalizeValueName(name);
      output.AppendFormat(";Value:0X{0:X16}", (object) value);
      output.AppendLine();
      output.AppendFormat("{0}=hex(b):", (object) str);
      RegUtil.ByteArrayToRegString(output, BitConverter.GetBytes(value));
      output.AppendLine();
    }

    public static void RegOutput(StringBuilder output, string name, uint value)
    {
      string str = RegUtil.NormalizeValueName(name);
      output.AppendFormat("{0}=dword:{1:X8}", (object) str, (object) value);
      output.AppendLine();
    }

    public static void RegOutput(StringBuilder output, string name, byte[] value)
    {
      string str = RegUtil.NormalizeValueName(name);
      output.AppendFormat("{0}=hex:", (object) str);
      RegUtil.ByteArrayToRegString(output, ((IEnumerable<byte>) value).ToArray<byte>(), RegUtil.BinaryLineLength / 3);
      output.AppendLine();
    }

    public static void RegOutput(StringBuilder output, string name, int type, byte[] value)
    {
      string str = RegUtil.NormalizeValueName(name);
      output.AppendFormat("{0}=hex({1:x}):", (object) str, (object) type);
      RegUtil.ByteArrayToRegString(output, ((IEnumerable<byte>) value).ToArray<byte>(), RegUtil.BinaryLineLength / 3);
      output.AppendLine();
    }
  }
}
