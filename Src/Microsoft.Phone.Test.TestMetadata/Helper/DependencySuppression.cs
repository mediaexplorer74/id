// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.Helper.DependencySuppression
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Phone.Test.TestMetadata.Helper
{
  public class DependencySuppression
  {
    private const int SupressionType = 0;
    private const int PartitionColumn = 1;
    private const int SourceNameColumn = 2;
    private const int TargetNameColumn = 3;
    private const int PackageNameColumn = 2;
    private const string PackageSupression = "PKG";
    private const string BinarySupression = "BIN";
    private const string MatchAny = "*";
    private string _supressionFile;
    private readonly Dictionary<string, Dictionary<string, HashSet<string>>> _binarySupressionTable = new Dictionary<string, Dictionary<string, HashSet<string>>>();
    private readonly Dictionary<string, HashSet<string>> _packageSupressionTable = new Dictionary<string, HashSet<string>>();

    public bool IsFileSupressed(string targetName) => this.IsFileSupressed("*", "*", targetName);

    public bool IsFileSupressed(string partitionName, string sourceName, string targetName)
    {
      lock (this)
      {
        if (!this._binarySupressionTable.ContainsKey(partitionName))
        {
          if (!this._binarySupressionTable.ContainsKey("*"))
            return false;
          partitionName = "*";
        }
        Dictionary<string, HashSet<string>> dictionary = this._binarySupressionTable[partitionName];
        if (!dictionary.ContainsKey(targetName))
          return false;
        HashSet<string> stringSet = dictionary[targetName];
        return stringSet.Contains(sourceName) || stringSet.Contains("*");
      }
    }

    public bool IsPackageSupressed(string partitionName, string packageName)
    {
      lock (this)
      {
        if (!this._packageSupressionTable.ContainsKey(partitionName))
        {
          if (!this._packageSupressionTable.ContainsKey("*"))
            return false;
          partitionName = "*";
        }
        return this._packageSupressionTable[partitionName].Contains(packageName);
      }
    }

    public bool IsPackageSupressed(string packageName) => this.IsPackageSupressed("*", packageName);

    public DependencySuppression(string supressionFile)
    {
      this._supressionFile = supressionFile;
      lock (this)
      {
        using (StreamReader streamReader = new StreamReader(supressionFile))
        {
          string str1;
          while ((str1 = streamReader.ReadLine()) != null)
          {
            string str2 = str1.ToLowerInvariant().Trim();
            string[] source = str2.Split(',');
            if (!string.IsNullOrWhiteSpace(str2) && !str2.StartsWith("#", StringComparison.OrdinalIgnoreCase) && (((IEnumerable<string>) source).Count<string>() == 3 || ((IEnumerable<string>) source).Count<string>() == 4))
            {
              string str3 = source[0].Trim();
              string partitionName = source[1].Trim();
              if (str3.Equals("BIN", StringComparison.OrdinalIgnoreCase))
              {
                string sourceName = source[2].Trim();
                this.AddBinarySupressionEntry(source[3].Trim(), sourceName, partitionName);
              }
              else if (str3.Equals("PKG", StringComparison.OrdinalIgnoreCase))
                this.AddPackageSupressionEntry(source[2].Trim(), partitionName);
            }
          }
        }
      }
    }

    private void AddPackageSupressionEntry(string packageName, string partitionName)
    {
      lock (this)
      {
        if (!this._packageSupressionTable.ContainsKey(partitionName))
          this._packageSupressionTable.Add(partitionName, new HashSet<string>());
        HashSet<string> stringSet = this._packageSupressionTable[partitionName];
        if (stringSet.Contains(packageName))
          return;
        stringSet.Add(packageName);
      }
    }

    private void AddBinarySupressionEntry(
      string targetName,
      string sourceName,
      string partitionName)
    {
      lock (this)
      {
        if (!this._binarySupressionTable.ContainsKey(partitionName))
          this._binarySupressionTable.Add(partitionName, new Dictionary<string, HashSet<string>>());
        Dictionary<string, HashSet<string>> dictionary = this._binarySupressionTable[partitionName];
        if (!dictionary.ContainsKey(targetName))
          dictionary.Add(targetName, new HashSet<string>());
        dictionary[targetName].Add(sourceName);
      }
    }
  }
}
