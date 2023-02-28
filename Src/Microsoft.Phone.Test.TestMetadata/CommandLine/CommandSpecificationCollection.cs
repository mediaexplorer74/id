// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.CommandSpecificationCollection
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  [SuppressMessage("Microsoft.Design", "CA1058:TypesShouldNotExtendCertainBaseTypes")]
  [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
  public class CommandSpecificationCollection : CollectionBase
  {
    public CommandSpecification this[string name]
    {
      get
      {
        foreach (CommandSpecification inner in this.InnerList)
        {
          if (string.Compare(name, inner.Name, true, CultureInfo.CurrentCulture) == 0)
            return inner;
          if (inner.CommandAliases.Count > 0)
          {
            foreach (string commandAlias in inner.CommandAliases)
            {
              if (string.Compare(name, commandAlias, true, CultureInfo.CurrentCulture) == 0)
                return inner;
            }
          }
        }
        return (CommandSpecification) null;
      }
    }

    public CommandSpecification this[int index] => this.InnerList[index] as CommandSpecification;

    public void CopyTo(CommandSpecification[] array, int index) => ((ICollection) this).CopyTo((Array) array, index);

    public int Add(CommandSpecification specification) => ((IList) this).Add((object) specification);

    public bool Contains(CommandSpecification specification) => ((IList) this).Contains((object) specification);

    public void Insert(int index, CommandSpecification specification) => ((IList) this).Insert(index, (object) specification);

    public void Remove(CommandSpecification specification) => ((IList) this).Remove((object) specification);

    public int IndexOf(CommandSpecification specification) => ((IList) this).IndexOf((object) specification);
  }
}
