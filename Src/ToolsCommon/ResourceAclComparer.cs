// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.ResourceAclComparer
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class ResourceAclComparer : IEqualityComparer<ResourceAcl>
  {
    public bool Equals(ResourceAcl x, ResourceAcl y)
    {
      bool flag = false;
      if (!string.IsNullOrEmpty(x.Path) && !string.IsNullOrEmpty(y.Path))
        flag = x.Path.Equals(y.Path, StringComparison.OrdinalIgnoreCase);
      return flag;
    }

    public int GetHashCode(ResourceAcl obj)
    {
      int hashCode = 0;
      if (!string.IsNullOrEmpty(obj.Path))
        hashCode = obj.Path.GetHashCode();
      return hashCode;
    }
  }
}
