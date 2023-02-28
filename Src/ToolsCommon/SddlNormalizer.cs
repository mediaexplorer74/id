// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.SddlNormalizer
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.Collections.Generic;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  internal static class SddlNormalizer
  {
    private static readonly HashSet<string> s_knownSids = new HashSet<string>()
    {
      "AN",
      "AO",
      "AU",
      "BA",
      "BG",
      "BO",
      "BU",
      "CA",
      "CD",
      "CG",
      "CO",
      "CY",
      "DA",
      "DC",
      "DD",
      "DG",
      "DU",
      "EA",
      "ED",
      "ER",
      "IS",
      "IU",
      "LA",
      "LG",
      "LS",
      "LU",
      "MU",
      "NO",
      "NS",
      "NU",
      "OW",
      "PA",
      "PO",
      "PS",
      "PU",
      "RC",
      "RD",
      "RE",
      "RO",
      "RS",
      "RU",
      "SA",
      "SO",
      "SU",
      "SY",
      "WD",
      "WR"
    };
    private static Dictionary<string, string> s_map = new Dictionary<string, string>();

    private static string ToFullSddl(string sid)
    {
      if (string.IsNullOrEmpty(sid) || sid.StartsWith("S-") || SddlNormalizer.s_knownSids.Contains(sid))
        return sid;
      string fullSddl = (string) null;
      if (!SddlNormalizer.s_map.TryGetValue(sid, out fullSddl))
      {
        fullSddl = new SecurityIdentifier(sid).ToString();
        SddlNormalizer.s_map.Add(sid, fullSddl);
      }
      return fullSddl;
    }

    public static string FixAceSddl(string sddl) => string.IsNullOrEmpty(sddl) ? sddl : Regex.Replace(sddl, ";(?<sid>[^;]*?)\\)", (MatchEvaluator) (x => string.Format(";{0})", (object) SddlNormalizer.ToFullSddl(x.Groups["sid"].Value))));

    public static string FixOwnerSddl(string sddl) => string.IsNullOrEmpty(sddl) ? sddl : Regex.Replace(sddl, "O:(?<oid>.*?)G:(?<gid>.*?)", (MatchEvaluator) (x => string.Format("O:{0}G:{1}", (object) SddlNormalizer.ToFullSddl(x.Groups["oid"].Value), (object) SddlNormalizer.ToFullSddl(x.Groups["gid"].Value))));
  }
}
