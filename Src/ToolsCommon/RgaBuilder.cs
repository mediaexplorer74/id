// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.RgaBuilder
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class RgaBuilder
  {
    private Dictionary<KeyValuePair<string, string>, List<string>> _rgaValues = new Dictionary<KeyValuePair<string, string>, List<string>>((IEqualityComparer<KeyValuePair<string, string>>) new RgaBuilder.KeyValuePairComparer());

    public bool HasContent => this._rgaValues.Count > 0;

    public void AddRgaValue(string keyName, string valueName, params string[] values)
    {
      KeyValuePair<string, string> key = new KeyValuePair<string, string>(keyName, valueName);
      List<string> stringList = (List<string>) null;
      if (!this._rgaValues.TryGetValue(key, out stringList))
      {
        stringList = new List<string>();
        this._rgaValues.Add(key, stringList);
      }
      stringList.AddRange((IEnumerable<string>) values);
    }

    public void Save(string outputFile)
    {
      StringBuilder output = new StringBuilder();
      output.AppendLine("Windows Registry Editor Version 5.00");
      foreach (IGrouping<string, KeyValuePair<KeyValuePair<string, string>, List<string>>> grouping in this._rgaValues.GroupBy<KeyValuePair<KeyValuePair<string, string>, List<string>>, string>((Func<KeyValuePair<KeyValuePair<string, string>, List<string>>, string>) (x => x.Key.Key)))
      {
        output.AppendFormat("[{0}]", (object) grouping.Key);
        output.AppendLine();
        foreach (KeyValuePair<KeyValuePair<string, string>, List<string>> keyValuePair in (IEnumerable<KeyValuePair<KeyValuePair<string, string>, List<string>>>) grouping)
          RegUtil.RegOutput(output, keyValuePair.Key.Value, (IEnumerable<string>) keyValuePair.Value);
        output.AppendLine();
      }
      LongPathFile.WriteAllText(outputFile, output.ToString(), Encoding.Unicode);
    }

    private class KeyValuePairComparer : IEqualityComparer<KeyValuePair<string, string>>
    {
      public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y) => x.Key.Equals(y.Key, StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(y.Value, StringComparison.InvariantCultureIgnoreCase);

      public int GetHashCode(KeyValuePair<string, string> obj) => obj.Key.GetHashCode() ^ obj.Value.GetHashCode();
    }
  }
}
