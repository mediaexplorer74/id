// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.HiveToRegConverter
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class HiveToRegConverter
  {
    private HashSet<string> m_exclusions = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private string m_keyPrefix;
    private string m_hiveFile;
    private TextWriter m_writer;

    public HiveToRegConverter(string hiveFile, string keyPrefix = null)
    {
      if (string.IsNullOrEmpty(hiveFile))
        throw new ArgumentNullException(nameof (hiveFile));
      this.m_hiveFile = LongPathFile.Exists(hiveFile) ? hiveFile : throw new FileNotFoundException(string.Format("Hive file {0} does not exist or cannot be read", (object) hiveFile));
      this.m_keyPrefix = keyPrefix;
    }

    public void ConvertToReg(string outputFile, HashSet<string> exclusions = null, bool append = false)
    {
      if (string.IsNullOrEmpty(outputFile))
        throw new ArgumentNullException(nameof (outputFile));
      if (exclusions != null)
        this.m_exclusions.UnionWith((IEnumerable<string>) exclusions);
      FileMode mode = append ? FileMode.Append : FileMode.Create;
      using (this.m_writer = (TextWriter) new StreamWriter((Stream) LongPathFile.Open(outputFile, mode, FileAccess.Write), Encoding.Unicode))
        this.ConvertToStream(!append, (string) null);
    }

    public void ConvertToReg(ref StringBuilder outputStr, HashSet<string> exclusions = null) => this.ConvertToReg(ref outputStr, (string) null, true, exclusions);

    public void ConvertToReg(
      ref StringBuilder outputStr,
      string subKey,
      bool outputHeader,
      HashSet<string> exclusions = null)
    {
      if (outputStr == null)
        throw new ArgumentNullException(nameof (outputStr));
      if (exclusions != null)
        this.m_exclusions.UnionWith((IEnumerable<string>) exclusions);
      using (this.m_writer = (TextWriter) new StringWriter(outputStr))
        this.ConvertToStream(outputHeader, subKey);
    }

    private void ConvertToStream(bool outputHeader, string subKey)
    {
      if (outputHeader)
        this.m_writer.WriteLine("Windows Registry Editor Version 5.00");
      using (ORRegistryKey orRegistryKey1 = ORRegistryKey.OpenHive(this.m_hiveFile, this.m_keyPrefix))
      {
        ORRegistryKey orRegistryKey2 = orRegistryKey1;
        if (!string.IsNullOrEmpty(subKey))
          orRegistryKey2 = orRegistryKey1.OpenSubKey(subKey);
        this.WriteKeyContents(orRegistryKey2);
        this.WalkHive(orRegistryKey2);
      }
    }

    private void WalkHive(ORRegistryKey root)
    {
      foreach (string subkeyname in (IEnumerable<string>) ((IEnumerable<string>) root.SubKeys).OrderBy<string, string>((Func<string, string>) (x => x), (IComparer<string>) StringComparer.OrdinalIgnoreCase))
      {
        using (ORRegistryKey orRegistryKey = root.OpenSubKey(subkeyname))
        {
          try
          {
            bool flag1 = this.m_exclusions.Contains(orRegistryKey.FullName + "\\*");
            bool flag2 = this.m_exclusions.Contains(orRegistryKey.FullName);
            if (!flag1)
            {
              if (!flag2)
                this.WriteKeyContents(orRegistryKey);
              this.WalkHive(orRegistryKey);
            }
          }
          catch (Exception ex)
          {
            throw new IUException("Failed to iterate through hive", ex);
          }
        }
      }
    }

    private void WriteKeyName(string keyname)
    {
      this.m_writer.WriteLine();
      this.m_writer.WriteLine("[{0}]", (object) keyname);
    }

    private string FormatValueName(string valueName)
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      if (valueName.Equals(""))
      {
        stringBuilder1.Append("@=");
      }
      else
      {
        StringBuilder stringBuilder2 = new StringBuilder(valueName);
        stringBuilder2.Replace("\\", "\\\\").Replace("\"", "\\\"");
        stringBuilder1.AppendFormat("\"{0}\"=", (object) stringBuilder2.ToString());
      }
      return stringBuilder1.ToString();
    }

    private string FormatValue(ORRegistryKey key, string valueName)
    {
      RegistryValueType valueKind = key.GetValueKind(valueName);
      StringBuilder stringBuilder1 = new StringBuilder();
      switch (valueKind)
      {
        case RegistryValueType.String:
          StringBuilder stringBuilder2 = new StringBuilder();
          stringBuilder2.Append(key.GetStringValue(valueName));
          stringBuilder2.Replace("\\", "\\\\").Replace("\"", "\\\"");
          stringBuilder1.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "\"{0}\"", new object[1]
          {
            (object) stringBuilder2.ToString()
          });
          break;
        case RegistryValueType.DWord:
          uint dwordValue = key.GetDwordValue(valueName);
          stringBuilder1.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "dword:{0:X8}", new object[1]
          {
            (object) dwordValue
          });
          break;
        case RegistryValueType.MultiString:
          byte[] byteValue1 = key.GetByteValue(valueName);
          stringBuilder1.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "hex(7):{0}", new object[1]
          {
            (object) OfflineRegUtils.ConvertByteArrayToRegStrings(byteValue1)
          });
          string[] multiStringValue = key.GetMultiStringValue(valueName);
          stringBuilder1.AppendLine();
          stringBuilder1.AppendLine(this.GetMultiStringValuesAsComments(multiStringValue));
          break;
        default:
          byte[] byteValue2 = key.GetByteValue(valueName);
          stringBuilder1.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "hex({0,1:X}):{1}", new object[2]
          {
            (object) valueKind,
            (object) OfflineRegUtils.ConvertByteArrayToRegStrings(byteValue2)
          });
          if (valueKind == RegistryValueType.ExpandString)
          {
            stringBuilder1.AppendLine();
            stringBuilder1.AppendLine(this.GetExpandStringValueAsComments(key.GetStringValue(valueName)));
            break;
          }
          break;
      }
      return stringBuilder1.ToString();
    }

    private string GetMultiStringValuesAsComments(string[] values)
    {
      StringBuilder stringBuilder = new StringBuilder(500);
      int num1 = 80;
      if (values != null && values.Length > 0)
      {
        stringBuilder.Append(";Values=");
        int num2 = stringBuilder.Length;
        foreach (string str in values)
        {
          stringBuilder.AppendFormat("{0},", (object) str);
          num2 += str.Length + 1;
          if (num2 > num1)
          {
            stringBuilder.AppendLine();
            stringBuilder.Append(";");
            num2 = 1;
          }
        }
        stringBuilder.Replace(",", string.Empty, stringBuilder.Length - 1, 1);
      }
      return stringBuilder.ToString();
    }

    private string GetExpandStringValueAsComments(string value) => string.Format(";Value={0}", (object) value);

    private void WriteKeyContents(ORRegistryKey key)
    {
      this.WriteKeyName(key.FullName);
      string str = key.Class;
      if (!string.IsNullOrEmpty(str))
        this.m_writer.WriteLine(string.Format(";Class=\"{0}\"", (object) str));
      foreach (string valueName in (IEnumerable<string>) ((IEnumerable<string>) key.ValueNames).OrderBy<string, string>((Func<string, string>) (x => x), (IComparer<string>) StringComparer.OrdinalIgnoreCase))
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(this.FormatValueName(valueName));
        stringBuilder.Append(this.FormatValue(key, valueName));
        this.m_writer.WriteLine(stringBuilder.ToString());
      }
    }
  }
}
