// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.LocalizationVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class LocalizationVM : DependencyObject, INotifyPropertyChanged
  {
    private static LocalizationVM _instance = (LocalizationVM) null;
    private ObservableCollection<WPIDLanguage> _localizedLanguages = new ObservableCollection<WPIDLanguage>();
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof (SelectedIndex), typeof (int), typeof (LocalizationVM), new PropertyMetadata((object) 0, new PropertyChangedCallback(LocalizationVM.OnLanguageSelectionChanged)));

    private LocalizationVM() => this.Initialize();

    public static LocalizationVM Instance
    {
      get
      {
        if (LocalizationVM._instance == null)
          LocalizationVM._instance = new LocalizationVM();
        return LocalizationVM._instance;
      }
    }

    private void Initialize()
    {
      foreach (WPIDLocale locale in Enum.GetValues(typeof (WPIDLocale)))
        this._localizedLanguages.Add(new WPIDLanguage(locale));
    }

    public ObservableCollection<WPIDLanguage> LocalizedLanguages => this._localizedLanguages;

    public int SelectedIndex
    {
      get => (int) this.GetValue(LocalizationVM.SelectedIndexProperty);
      set => this.SetValue(LocalizationVM.SelectedIndexProperty, (object) value);
    }

    public void SetLocale(WPIDLocale locale) => this.SelectedIndex = this.LocalizedLanguages.IndexOf(this.LocalizedLanguages.First<WPIDLanguage>((Func<WPIDLanguage, bool>) (l => l.Locale == locale)));

    public void SetLocale(string localeString)
    {
      WPIDLocale result = WPIDLocale.EN_US;
      if (!Enum.TryParse<WPIDLocale>(localeString, out result))
        return;
      this.SetLocale(result);
    }

    private static void OnLanguageSelectionChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      LocalizationVM sender = d as LocalizationVM;
      WPIDLanguage localizedLanguage = sender.LocalizedLanguages[sender.SelectedIndex];
      localizedLanguage.SetActive();
      UserConfig.SaveUserConfig("Locale", localizedLanguage.LocaleString);
      if (sender.PropertyChanged == null)
        return;
      sender.PropertyChanged((object) sender, new PropertyChangedEventArgs("Language"));
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
