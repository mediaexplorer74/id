// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.LexicographicComparer
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class LexicographicComparer : IComparer<string>
  {
    private readonly Comparison<string> comparison;

    public LexicographicComparer() => this.comparison = new Comparison<string>(SafeNativeMethods.StrCmpLogicalW);

    public int Compare(string x, string y) => this.comparison(x, y);
  }
}
