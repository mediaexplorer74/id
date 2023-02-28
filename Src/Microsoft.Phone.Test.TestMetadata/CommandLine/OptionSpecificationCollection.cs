// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.OptionSpecificationCollection
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  [SuppressMessage("Microsoft.Design", "CA1058:TypesShouldNotExtendCertainBaseTypes")]
  [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
  public class OptionSpecificationCollection : CollectionBase
  {
    public void Add(OptionSpecification specification)
    {
      if (specification == null)
        throw new OptionSpecificationException("Specification passed to add is null");
      if (this.Contains(specification.OptionName))
        throw new OptionSpecificationException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Option {0} is specified twice", new object[1]
        {
          (object) specification.OptionName
        }));
      this.InnerList.Add((object) specification);
    }

    public bool Contains(string optionName)
    {
      foreach (OptionSpecification inner in this.InnerList)
      {
        if (string.Compare(inner.OptionName, optionName, true, CultureInfo.CurrentCulture) == 0)
          return true;
      }
      return false;
    }

    public bool IsDisjoint(OptionSpecificationCollection otherCollection)
    {
      if (otherCollection == null)
        throw new OptionSpecificationException("Collection specified is null");
      foreach (OptionSpecification inner in this.InnerList)
      {
        if (otherCollection.Contains(inner.OptionName))
          return false;
      }
      return true;
    }

    public OptionSpecification this[string optionName]
    {
      get
      {
        foreach (OptionSpecification inner in this.InnerList)
        {
          if (string.Compare(inner.OptionName, optionName, true, CultureInfo.CurrentUICulture) == 0)
            return inner;
        }
        return (OptionSpecification) null;
      }
    }

    public OptionSpecification this[int index]
    {
      get => (OptionSpecification) ((IList) this)[index];
      set => ((IList) this)[index] = (object) value;
    }

    public void CopyTo(OptionSpecification[] array, int index) => ((ICollection) this).CopyTo((Array) array, index);

    public void Insert(int index, OptionSpecification specification) => ((IList) this).Insert(index, (object) specification);

    public void Remove(OptionSpecification specification) => ((IList) this).Remove((object) specification);

    public int IndexOf(OptionSpecification specification) => ((IList) this).IndexOf((object) specification);

    public IList<OptionSpecification> GetPartial(string partialOptionName)
    {
      if (partialOptionName == null)
        throw new OptionSpecificationException("Option name cannot be null");
      List<OptionSpecification> optionSpecificationList = new List<OptionSpecification>();
      foreach (OptionSpecification inner in this.InnerList)
      {
        if (string.Compare(partialOptionName, 0, inner.OptionName, 0, partialOptionName.Length, true, CultureInfo.CurrentCulture) == 0)
          optionSpecificationList.Add(inner);
      }
      return (IList<OptionSpecification>) optionSpecificationList.ToArray();
    }

    public void LoadFromType(Type type)
    {
      if (type == (Type) null)
        throw new OptionSpecificationException("Type passed is null");
      foreach (OptionAttribute customAttribute in type.GetCustomAttributes(typeof (OptionAttribute), true))
        this.Add(new OptionSpecification(customAttribute, (PropertyInfo) null));
      foreach (PropertyInfo property in type.GetProperties())
      {
        object[] customAttributes = property.GetCustomAttributes(typeof (OptionAttribute), true);
        if (customAttributes.Length == 1)
        {
          if (property.GetSetMethod() == (MethodInfo) null && !typeof (IList).IsAssignableFrom(property.PropertyType))
            throw new OptionSpecificationException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "The property '{0}' has no setter so it cannot be used as an option.", new object[1]
            {
              (object) property.Name
            }));
          this.Add(new OptionSpecification(customAttributes[0] as OptionAttribute, property));
        }
        else if (customAttributes.Length != 0)
          throw new OptionSpecificationException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "The property '{0}' has been marked with two or more options", new object[1]
          {
            (object) property.Name
          }));
      }
    }
  }
}
