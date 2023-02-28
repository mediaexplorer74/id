// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.WPIDLanguage
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Globalization;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class WPIDLanguage
  {
    private CultureInfo _culture;
    private WPIDLocale _locale;
    private string _displayString = string.Empty;

    public WPIDLanguage(WPIDLocale locale)
    {
      this._locale = locale;
      this._culture = CultureInfo.GetCultureInfo(Enum.GetName(typeof (WPIDLocale), (object) this._locale).Replace('_', '-'));
    }

    public string DisplayString => this._culture.DisplayName;

    public WPIDLocale Locale => this._locale;

    public string LocaleString => Enum.GetName(typeof (WPIDLocale), (object) this._locale);

    public void SetActive()
    {
      Thread.CurrentThread.CurrentCulture = this._culture;
      Thread.CurrentThread.CurrentUICulture = this._culture;
    }
  }
}
