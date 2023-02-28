// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.TargetValuesDialogVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class TargetValuesDialogVM : DependencyObject
  {
    public static readonly DependencyProperty ShowHeaderProperty = DependencyProperty.Register(nameof (ShowHeader), typeof (bool), typeof (TargetValuesDialogVM), new PropertyMetadata((object) true));
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof (Header), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty InstructionsProperty = DependencyProperty.Register(nameof (Instructions), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof (Name), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) Tools.GetString("strCOSDefaultVariantName")));
    public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(nameof (Result), typeof (CustomGetValueResult), typeof (TargetValuesDialogVM), new PropertyMetadata((object) CustomGetValueResult.Cancel));
    public static readonly DependencyProperty ShowDialogNextTimeProperty = DependencyProperty.Register(nameof (ShowDialogNextTime), typeof (bool), typeof (TargetValuesDialogVM), new PropertyMetadata((object) true));
    public static readonly DependencyProperty DialogTypeProperty = DependencyProperty.Register(nameof (DialogType), typeof (CustomDialogType), typeof (TargetValuesDialogVM), new PropertyMetadata((object) CustomDialogType.Invalid));
    public static readonly DependencyProperty CommonName1Property = DependencyProperty.Register(nameof (CommonName1), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty CommonValue1Property = DependencyProperty.Register(nameof (CommonValue1), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty CommonName2Property = DependencyProperty.Register(nameof (CommonName2), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty CommonValue2Property = DependencyProperty.Register(nameof (CommonValue2), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty CommonName3Property = DependencyProperty.Register(nameof (CommonName3), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty CommonValue3Property = DependencyProperty.Register(nameof (CommonValue3), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty OtherName1Property = DependencyProperty.Register(nameof (OtherName1), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty OtherValue1Property = DependencyProperty.Register(nameof (OtherValue1), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty OtherName2Property = DependencyProperty.Register(nameof (OtherName2), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty OtherValue2Property = DependencyProperty.Register(nameof (OtherValue2), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) ""));
    private Dictionary<string, string> _results = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    public static readonly DependencyProperty OtherGroupHeaderProperty = DependencyProperty.Register(nameof (OtherGroupHeader), typeof (string), typeof (TargetValuesDialogVM), new PropertyMetadata((object) Tools.GetString("gCOSOtherConditions")));
    public List<string> ExistingVariantNames;
    public List<string> ExistingConditionNames;
    public static readonly DependencyProperty ShowCommon1Property = DependencyProperty.Register(nameof (ShowCommon1), typeof (bool), typeof (TargetValuesDialogVM), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ShowCommon2Property = DependencyProperty.Register(nameof (ShowCommon2), typeof (bool), typeof (TargetValuesDialogVM), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ShowCommon3Property = DependencyProperty.Register(nameof (ShowCommon3), typeof (bool), typeof (TargetValuesDialogVM), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ShowOther1Property = DependencyProperty.Register(nameof (ShowOther1), typeof (bool), typeof (TargetValuesDialogVM), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ShowOther2Property = DependencyProperty.Register(nameof (ShowOther2), typeof (bool), typeof (TargetValuesDialogVM), new PropertyMetadata((object) false));

    public TargetValuesDialogVM()
    {
    }

    public TargetValuesDialogVM(CustomDialogType type) => this.DialogType = type;

    public bool ShowHeader
    {
      get => (bool) this.GetValue(TargetValuesDialogVM.ShowHeaderProperty);
      set => this.SetValue(TargetValuesDialogVM.ShowHeaderProperty, (object) value);
    }

    public string Header
    {
      get => (string) this.GetValue(TargetValuesDialogVM.HeaderProperty);
      set => this.SetValue(TargetValuesDialogVM.HeaderProperty, (object) value);
    }

    public string Instructions
    {
      get => (string) this.GetValue(TargetValuesDialogVM.InstructionsProperty);
      set => this.SetValue(TargetValuesDialogVM.InstructionsProperty, (object) value);
    }

    public string Name
    {
      get => (string) this.GetValue(TargetValuesDialogVM.NameProperty);
      set => this.SetValue(TargetValuesDialogVM.NameProperty, (object) value);
    }

    public CustomGetValueResult Result
    {
      get => (CustomGetValueResult) this.GetValue(TargetValuesDialogVM.ResultProperty);
      set => this.SetValue(TargetValuesDialogVM.ResultProperty, (object) value);
    }

    public bool ShowDialogNextTime
    {
      get => (bool) this.GetValue(TargetValuesDialogVM.ShowDialogNextTimeProperty);
      set => this.SetValue(TargetValuesDialogVM.ShowDialogNextTimeProperty, (object) value);
    }

    public CustomDialogType DialogType
    {
      get => (CustomDialogType) this.GetValue(TargetValuesDialogVM.DialogTypeProperty);
      set => this.SetValue(TargetValuesDialogVM.DialogTypeProperty, (object) value);
    }

    public string CommonName1
    {
      get => (string) this.GetValue(TargetValuesDialogVM.CommonName1Property);
      set => this.SetValue(TargetValuesDialogVM.CommonName1Property, (object) value);
    }

    public string CommonValue1
    {
      get => (string) this.GetValue(TargetValuesDialogVM.CommonValue1Property);
      set => this.SetValue(TargetValuesDialogVM.CommonValue1Property, (object) value);
    }

    public string CommonName2
    {
      get => (string) this.GetValue(TargetValuesDialogVM.CommonName2Property);
      set => this.SetValue(TargetValuesDialogVM.CommonName2Property, (object) value);
    }

    public string CommonValue2
    {
      get => (string) this.GetValue(TargetValuesDialogVM.CommonValue2Property);
      set => this.SetValue(TargetValuesDialogVM.CommonValue2Property, (object) value);
    }

    public string CommonName3
    {
      get => (string) this.GetValue(TargetValuesDialogVM.CommonName3Property);
      set => this.SetValue(TargetValuesDialogVM.CommonName3Property, (object) value);
    }

    public string CommonValue3
    {
      get => (string) this.GetValue(TargetValuesDialogVM.CommonValue3Property);
      set => this.SetValue(TargetValuesDialogVM.CommonValue3Property, (object) value);
    }

    public string OtherName1
    {
      get => (string) this.GetValue(TargetValuesDialogVM.OtherName1Property);
      set => this.SetValue(TargetValuesDialogVM.OtherName1Property, (object) value);
    }

    public string OtherValue1
    {
      get => (string) this.GetValue(TargetValuesDialogVM.OtherValue1Property);
      set => this.SetValue(TargetValuesDialogVM.OtherValue1Property, (object) value);
    }

    public string OtherName2
    {
      get => (string) this.GetValue(TargetValuesDialogVM.OtherName2Property);
      set => this.SetValue(TargetValuesDialogVM.OtherName2Property, (object) value);
    }

    public string OtherValue2
    {
      get => (string) this.GetValue(TargetValuesDialogVM.OtherValue2Property);
      set => this.SetValue(TargetValuesDialogVM.OtherValue2Property, (object) value);
    }

    public Dictionary<string, string> Results => this._results;

    public string OtherGroupHeader
    {
      get => (string) this.GetValue(TargetValuesDialogVM.OtherGroupHeaderProperty);
      set => this.SetValue(TargetValuesDialogVM.OtherGroupHeaderProperty, (object) value);
    }

    public void SetCommon(Dictionary<string, string> nameValuePairs)
    {
      int num = 1;
      foreach (string key in nameValuePairs.Keys)
      {
        switch (num)
        {
          case 1:
            this.CommonName1 = key;
            this.CommonValue1 = nameValuePairs[key];
            this.ShowCommon1 = true;
            break;
          case 2:
            this.CommonName2 = key;
            this.CommonValue2 = nameValuePairs[key];
            this.ShowCommon2 = true;
            break;
          case 3:
            this.CommonName3 = key;
            this.CommonValue3 = nameValuePairs[key];
            this.ShowCommon2 = true;
            break;
        }
        ++num;
      }
    }

    public void SetOther(Dictionary<string, string> nameValuePairs)
    {
      int num = 1;
      foreach (string key in nameValuePairs.Keys)
      {
        switch (num)
        {
          case 1:
            this.OtherName1 = key;
            this.OtherValue1 = nameValuePairs[key];
            this.ShowOther1 = true;
            continue;
          case 2:
            this.OtherName2 = key;
            this.OtherValue2 = nameValuePairs[key];
            this.ShowOther2 = true;
            continue;
          default:
            continue;
        }
      }
    }

    public int OtherCount
    {
      set
      {
        switch (value)
        {
          case 0:
            this.ShowOther1 = false;
            break;
          case 1:
            this.ShowOther1 = true;
            break;
          case 2:
            this.ShowOther1 = true;
            this.ShowOther2 = true;
            break;
        }
      }
    }

    private bool AddOtherValue(string name, string value)
    {
      if (string.IsNullOrEmpty(name) == string.IsNullOrEmpty(value))
        return this.AddValue(name, value);
      int num = (int) MessageBox.Show(!string.IsNullOrEmpty(name) ? string.Format(Tools.GetString("msgCOSAddVariantConditionValueCantBeEmptyError"), (object) name) : string.Format(Tools.GetString("msgCOSAddVariantConditionNameCantBeEmptyError"), (object) value), Tools.GetString("msgCOSAddVariantConditionNameOrValueCantBeEmptyErrorMsgTitle"));
      return false;
    }

    private bool AddCommonValue(string name, string value) => this.AddValue(name, value);

    private bool AddValue(string name, string value)
    {
      if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
      {
        if (this._results.ContainsKey(name))
        {
          int num = (int) MessageBox.Show(string.Format(Tools.GetString("msgCOSAddVariantConditionNameDuplicateError"), (object) name), Tools.GetString("msgCOSAddVariantConditionNameDuplicateErrorTitle"));
          return false;
        }
        this._results.Add(name, value);
      }
      return true;
    }

    public bool ShowCommon1
    {
      get => (bool) this.GetValue(TargetValuesDialogVM.ShowCommon1Property);
      set => this.SetValue(TargetValuesDialogVM.ShowCommon1Property, (object) value);
    }

    public bool ShowCommon2
    {
      get => (bool) this.GetValue(TargetValuesDialogVM.ShowCommon2Property);
      set => this.SetValue(TargetValuesDialogVM.ShowCommon2Property, (object) value);
    }

    public bool ShowCommon3
    {
      get => (bool) this.GetValue(TargetValuesDialogVM.ShowCommon3Property);
      set => this.SetValue(TargetValuesDialogVM.ShowCommon3Property, (object) value);
    }

    public bool ShowOther1
    {
      get => (bool) this.GetValue(TargetValuesDialogVM.ShowOther1Property);
      set => this.SetValue(TargetValuesDialogVM.ShowOther1Property, (object) value);
    }

    public bool ShowOther2
    {
      get => (bool) this.GetValue(TargetValuesDialogVM.ShowOther2Property);
      set => this.SetValue(TargetValuesDialogVM.ShowOther2Property, (object) value);
    }

    public bool OnLoad() => true;

    public bool OnExit()
    {
      if (this.Result == CustomGetValueResult.Cancel)
        return true;
      this._results.Clear();
      if (this.DialogType == CustomDialogType.NewVariantDialog && (this.ExistingVariantNames != null || this.ExistingVariantNames.Count<string>() != 0) && this.ExistingVariantNames.Contains<string>(this.Name, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
      {
        int num = (int) MessageBox.Show(string.Format(Tools.GetString("msgCOSAddVariantNameDuplicateError"), (object) this.Name), Tools.GetString("msgCOSAddVariantNameDuplicateErrorTitle"));
        return false;
      }
      if (!this.AddCommonValue(this.CommonName1, this.CommonValue1) || !this.AddCommonValue(this.CommonName2, this.CommonValue2) || !this.AddCommonValue(this.CommonName3, this.CommonValue3) || !this.AddOtherValue(this.OtherName1, this.OtherValue1) || !this.AddOtherValue(this.OtherName2, this.OtherValue2))
        return false;
      if (this.DialogType == CustomDialogType.EditTargetDialog && (this.ExistingConditionNames != null || this.ExistingConditionNames.Count<string>() != 0))
      {
        foreach (string key in this._results.Keys)
        {
          if (this.ExistingConditionNames.Contains<string>(key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
          {
            int num = (int) MessageBox.Show(string.Format(Tools.GetString("msgCOSAddVariantDuplicateConditionsError"), (object) key), Tools.GetString("msgCOSAddVariantDuplicateConditionsErrorTitle"));
            return false;
          }
        }
      }
      if (this._results.Count<KeyValuePair<string, string>>() != 0)
        return true;
      if (this.DialogType == CustomDialogType.EditTargetDialog)
      {
        int num1 = (int) MessageBox.Show(Tools.GetString("msgCOSEditConditionsNoConditionsSetError"), Tools.GetString("msgCOSEditConditionsNoConditionsSetErrorTitle"));
      }
      else
      {
        int num2 = (int) MessageBox.Show(Tools.GetString("msgCOSAddVariantNoConditionsSetError"), Tools.GetString("msgCOSAddVariantNoConditionsSetErrorTitle"));
      }
      return false;
    }
  }
}
