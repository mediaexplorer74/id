// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.CommandOptionCollection
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
  [Serializable]
  public class CommandOptionCollection : NameObjectCollectionBase
  {
    public CommandOptionCollection()
    {
    }

    protected CommandOptionCollection(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public void Add(CommandOption option)
    {
      if (option == null)
        throw new ArgumentNullException(nameof (option));
      this.BaseAdd(option.Name.ToLower(CultureInfo.CurrentUICulture), (object) option);
    }

    public void Remove(CommandOption option)
    {
      if (option == null)
        throw new ArgumentNullException(nameof (option));
      this.Remove(option.Name);
    }

    public void Remove(string optionName)
    {
      if (optionName == null)
        throw new ArgumentNullException(nameof (optionName));
      this.BaseRemove(optionName.ToLower(CultureInfo.CurrentUICulture));
    }

    public bool Contains(string name)
    {
      if (name == null)
        throw new ArgumentNullException(nameof (name));
      return this.BaseGet(name.ToLower(CultureInfo.CurrentUICulture)) != null;
    }

    public CommandOption this[string name] => name != null ? (CommandOption) this.BaseGet(name.ToLower(CultureInfo.CurrentUICulture)) : throw new ArgumentNullException(nameof (name));

    public void CopyTo(CommandOption[] array, int index) => ((ICollection) this).CopyTo((Array) array, index);

    public void Insert(int index, CommandOption option) => ((IList) this).Insert(index, (object) option);

    public int IndexOf(CommandOption option) => ((IList) this).IndexOf((object) option);
  }
}
