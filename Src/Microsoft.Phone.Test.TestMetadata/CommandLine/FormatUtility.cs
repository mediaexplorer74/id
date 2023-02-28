// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.FormatUtility
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  internal sealed class FormatUtility
  {
    private FormatUtility()
    {
    }

    public static string FormatStringForWidth(
      string toFormat,
      int offset,
      int hangingIndent,
      int width)
    {
      IList<string> stringList = FormatUtility.MakeWords(toFormat);
      StringBuilder stringBuilder = new StringBuilder();
      string str = new string(' ', offset + hangingIndent);
      int num1 = 0;
      int index1 = 0;
      int index2;
      for (; index1 < stringList.Count; index1 = index2 + 1)
      {
        index2 = index1;
        int num2 = offset;
        if (num1 > 0)
          num2 += hangingIndent;
        int num3 = num2 + stringList[index1].Length;
        int num4;
        for (int index3 = !stringList[index1].EndsWith(".", StringComparison.OrdinalIgnoreCase) ? num3 + 1 : num3 + 2; index2 + 1 < stringList.Count && index3 + stringList[index2 + 1].Length < width; index3 = !stringList[index2].EndsWith(".", StringComparison.OrdinalIgnoreCase) ? num4 + 1 : num4 + 2)
        {
          ++index2;
          num4 = index3 + stringList[index2].Length;
        }
        if (num1 > 0)
          stringBuilder.Append(str);
        else
          stringBuilder.Append(new string(' ', offset));
        for (int index4 = index1; index4 <= index2; ++index4)
        {
          if (index4 > index1)
          {
            if (stringList[index4 - 1].EndsWith(".", StringComparison.OrdinalIgnoreCase))
              stringBuilder.Append("  ");
            else
              stringBuilder.Append(" ");
          }
          stringBuilder.Append(stringList[index4]);
        }
        stringBuilder.Append(Environment.NewLine);
        ++num1;
      }
      return stringBuilder.ToString();
    }

    public static IList<string> MakeWords(string toParse)
    {
      char[] charArray = toParse.ToCharArray();
      StringBuilder stringBuilder = new StringBuilder();
      List<string> stringList = new List<string>();
      for (int index = 0; index < charArray.Length; ++index)
      {
        if (char.IsWhiteSpace(charArray[index]))
        {
          if (stringBuilder.Length > 0)
          {
            stringList.Add(stringBuilder.ToString());
            stringBuilder.Length = 0;
          }
        }
        else
          stringBuilder.Append(charArray[index]);
      }
      if (stringBuilder.Length > 0)
        stringList.Add(stringBuilder.ToString());
      return (IList<string>) stringList;
    }

    private enum ParseState
    {
      Start,
      StartOfLine,
      ReadNext,
      EndOfLine,
      EndOfString,
    }
  }
}
