// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.ExtensionMethods
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public static class ExtensionMethods
  {
    public static T GetCustomAttribute<T>(this PropertyInfo pi, bool inherit = false) where T : Attribute => ((IEnumerable<object>) pi.GetCustomAttributes(typeof (T), inherit)).FirstOrDefault<object>() as T;

    public static void SaveToSettings<T>(this PinVMBase vm, WPSettings settings) where T : CustomPinAttribute
    {
      if (vm == null)
        throw new ArgumentNullException(nameof (vm));
      if (settings == null)
        throw new ArgumentNullException(nameof (settings));
      foreach (PropertyInfo pi in ((IEnumerable<PropertyInfo>) vm.GetType().GetProperties()).Where<PropertyInfo>((Func<PropertyInfo, bool>) (pi => pi.GetCustomAttributes(typeof (T), false).Length > 0)))
      {
        CustomPinAttribute customAttribute = pi.GetCustomAttribute<CustomPinAttribute>();
        if (customAttribute != null && customAttribute.GetType() == typeof (T))
        {
          string empty = string.Empty;
          string str;
          if (pi.PropertyType.IsEnum)
          {
            Enum e = (Enum) pi.GetValue((object) vm, (object[]) null);
            str = !e.HasMemberAttribute<EnumStringValueAttribute>() ? ((int) pi.GetValue((object) vm, (object[]) null)).ToString() : e.GetStringValue();
          }
          else
            str = pi.GetValue((object) vm, (object[]) null).ToString();
          WPSetting byName = settings.FindByName(customAttribute.Name);
          if (byName != null)
          {
            if (!string.IsNullOrWhiteSpace(str))
              byName.Value = str;
            else
              byName.ResetToPolicySettingValues();
          }
        }
      }
    }

    private static string GetStringValue(this Enum e)
    {
      string stringValue = (string) null;
      MemberInfo[] member = e.GetType().GetMember(e.ToString());
      if (member != null && member.Length > 0)
      {
        object obj = ((IEnumerable<object>) member[0].GetCustomAttributes(typeof (EnumStringValueAttribute), false)).FirstOrDefault<object>();
        if (obj != null)
          stringValue = ((EnumStringValueAttribute) obj).Value;
      }
      return stringValue;
    }

    private static bool HasMemberAttribute<T>(this Enum e) where T : Attribute
    {
      bool flag = false;
      List<MemberInfo> list = ((IEnumerable<MemberInfo>) e.GetType().GetMember(e.ToString())).ToList<MemberInfo>();
      if (list != null)
        flag = list.FirstOrDefault<MemberInfo>((Func<MemberInfo, bool>) (m => Attribute.GetCustomAttribute(m, typeof (T)) != null)) != (MemberInfo) null;
      return flag;
    }

    private static FieldInfo GetFieldWithStringValue(this Enum e, string value)
    {
      FieldInfo fieldWithStringValue = (FieldInfo) null;
      List<FieldInfo> list = ((IEnumerable<FieldInfo>) e.GetType().GetFields()).ToList<FieldInfo>();
      if (list != null)
      {
        foreach (FieldInfo element in list)
        {
          if (Attribute.GetCustomAttribute((MemberInfo) element, typeof (EnumStringValueAttribute)) is EnumStringValueAttribute customAttribute && value.Equals(customAttribute.Value, StringComparison.OrdinalIgnoreCase))
          {
            fieldWithStringValue = element;
            break;
          }
        }
      }
      return fieldWithStringValue;
    }

    public static void LoadFromSettings<T>(this PinVMBase vm, WPSettings settings) where T : CustomPinAttribute
    {
      if (vm == null)
        throw new ArgumentNullException(nameof (vm));
      if (settings == null)
        throw new ArgumentNullException(nameof (settings));
      foreach (PropertyInfo pi in ((IEnumerable<PropertyInfo>) vm.GetType().GetProperties()).Where<PropertyInfo>((Func<PropertyInfo, bool>) (pi => pi.GetCustomAttributes(typeof (T), false).Length > 0)))
      {
        CustomPinAttribute customAttribute = pi.GetCustomAttribute<CustomPinAttribute>();
        if (customAttribute != null && customAttribute.GetType() == typeof (T))
        {
          WPSetting byName = settings.FindByName(customAttribute.Name);
          if (byName != null && byName.Value != null)
          {
            string s = byName.Value;
            if (pi.PropertyType.IsEnum)
            {
              bool flag = false;
              Enum e = (Enum) pi.GetValue((object) vm, (object[]) null);
              if (e.HasMemberAttribute<EnumStringValueAttribute>())
              {
                FieldInfo fieldWithStringValue = e.GetFieldWithStringValue(s);
                if (fieldWithStringValue != (FieldInfo) null)
                {
                  object obj = fieldWithStringValue.GetValue((object) e);
                  pi.SetValue((object) vm, obj, (object[]) null);
                  flag = true;
                }
              }
              if (!flag)
              {
                int result = 0;
                if (int.TryParse(s, out result))
                  pi.SetValue((object) vm, (object) result, (object[]) null);
              }
            }
            else
              pi.SetValue((object) vm, Convert.ChangeType((object) s, pi.PropertyType), (object[]) null);
          }
        }
      }
    }

    public static void MoveTo<T>(
      this ObservableCollection<T> from,
      ObservableCollection<T> to,
      Func<T, bool> condition)
    {
      foreach (T obj in from.Where<T>(condition).ToList<T>())
      {
        from.Remove(obj);
        to.Add(obj);
      }
    }

    public static void MoveBy<T>(
      this ObservableCollection<T> list,
      int increment,
      Func<T, bool> condition)
    {
      T obj = list.First<T>(condition);
      int index1 = list.IndexOf(obj);
      int index2 = index1 + increment;
      if (index2 < 0 || index2 > list.Count)
        return;
      list.RemoveAt(index1);
      list.Insert(index2, obj);
    }
  }
}
