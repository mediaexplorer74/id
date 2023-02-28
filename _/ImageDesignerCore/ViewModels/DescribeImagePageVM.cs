// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.DescribeImagePageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class DescribeImagePageVM : IDViewModelBase
  {
    private static string LocaleInfoTable = "LocaleInfoTable.xml";
    private static string LocaleInfoTableAKPath = Path.Combine(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKToolsRoot(), DescribeImagePageVM.LocaleInfoTable);
    private static string LocaleInfoTableDevPath = Path.Combine("DeviceImaging\\pid\\", DescribeImagePageVM.LocaleInfoTable);
    public static readonly DependencyProperty ImageDescriptionProperty = DependencyProperty.Register(nameof (ImageDescription), typeof (string), typeof (DescribeImagePageVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register(nameof (SelectedImage), typeof (string), typeof (DescribeImagePageVM), new PropertyMetadata((object) string.Empty));
    private ulong _baseImageSize;
    private ulong _totalLanguageSize;
    private ulong _totalImageSize;
    public static readonly DependencyProperty BaseImageSizeStringProperty = DependencyProperty.Register(nameof (BaseImageSizeString), typeof (string), typeof (DescribeImagePageVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty TotalImageSizeStringProperty = DependencyProperty.Register(nameof (TotalImageSizeString), typeof (string), typeof (DescribeImagePageVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty TotalLanguageSizeStringProperty = DependencyProperty.Register(nameof (TotalLanguageSizeString), typeof (string), typeof (DescribeImagePageVM), new PropertyMetadata((object) string.Empty));
    public OEMInput FinalOEMInput;
    private SupportedLangs _allSupportedLangs;
    private ObservableNotifyObjectCollection<WPLanguage> _uiLanguages;
    private List<string> _availableBootLanguages;
    private string _bootLanguage = "";
    public static readonly DependencyProperty SupportedBootLocalesProperty = DependencyProperty.Register(nameof (SupportedBootLocales), typeof (List<string>), typeof (DescribeImagePageVM), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty BootLocaleProperty = DependencyProperty.Register(nameof (BootLocale), typeof (string), typeof (DescribeImagePageVM), new PropertyMetadata((object) "En-US"));
    private ObservableNotifyObjectCollection<WPLanguage> _speechLanguages;
    private ObservableNotifyObjectCollection<WPLanguage> _keyboardLanguages;

    internal DescribeImagePageVM(IDStates mystate)
      : base(mystate)
    {
    }

    private void task_TaskCompletedEvent(object sender, TaskEventArgs e)
    {
    }

    protected override bool SaveSupported => true;

    internal override bool OnStateEntry()
    {
      if (string.IsNullOrEmpty(this.ImageDescription))
        this.ImageDescription = string.Format(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtDefaultImageDescription"), (object) this.Context.SelectedOEMInput.SOC, (object) Enum.GetName(typeof (ImageType), (object) this.Context.ImageType), (object) DateTime.Now.ToLongDateString(), (object) DateTime.Now.ToShortTimeString());
      this.PopulateLanguagesWithSizes();
      this.UpdateBootLanguageList();
      this.GetSupportedBootLocales();
      this.SelectedImage = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetImageName(this.Context.ImageType);
      this.BaseImageSize = this.Context.GetCalculatedImageSize(this.Context.ImageType);
      this.TotalLanguageSize = this.CalculateTotalLanguageSize();
      this.BootLanguage = this.Context.SelectedOEMInput.BootUILanguage.ToLower();
      this.BootLocale = this.Context.SelectedOEMInput.BootLocale.ToLower();
      this.Validate();
      return true;
    }

    protected override bool SavePage()
    {
      this.GenerateFinalOEMInput();
      this.FinalOEMInput.WriteToFile(this.Context.OEMInputFile);
      return true;
    }

    protected override void Validate()
    {
      if (this.UILanguages == null || this.UILanguages.Count<WPLanguage>() == 0 || this.KeyboardLanguages == null || this.KeyboardLanguages.Count<WPLanguage>() == 0)
        this.IsValid = false;
      else if (this.UILanguages.Where<WPLanguage>((Func<WPLanguage, bool>) (lang => lang.IsSelected)).Count<WPLanguage>() == 0)
        this.IsValid = false;
      else if (!this.Context.SelectedOEMInput.IsMMOS && this.KeyboardLanguages.Where<WPLanguage>((Func<WPLanguage, bool>) (lang => lang.IsSelected)).Count<WPLanguage>() == 0)
        this.IsValid = false;
      else
        this.IsValid = true;
    }

    protected override void ComputeNextState()
    {
      if (this.Context.ImageType == ImageType.MMOS)
        this._nextState = new IDStates?(IDStates.BuildImage);
      else
        this._nextState = new IDStates?(IDStates.CustomizationChoice);
    }

    public bool NotMMOS => !this.Context.SelectedOEMInput.IsMMOS;

    public string ImageDescription
    {
      get => (string) this.GetValue(DescribeImagePageVM.ImageDescriptionProperty);
      set => this.SetValue(DescribeImagePageVM.ImageDescriptionProperty, (object) value);
    }

    public string SelectedImage
    {
      get => (string) this.GetValue(DescribeImagePageVM.SelectedImageProperty);
      set => this.SetValue(DescribeImagePageVM.SelectedImageProperty, (object) value);
    }

    public ulong BaseImageSize
    {
      get => this._baseImageSize;
      set
      {
        this._baseImageSize = value;
        this.OnPropertyChanged(nameof (BaseImageSize));
        this.BaseImageSizeString = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.FormatBytes(value);
        this.TotalImageSize = this.BaseImageSize + this.TotalLanguageSize;
      }
    }

    public ulong TotalLanguageSize
    {
      get => this._totalLanguageSize;
      set
      {
        this._totalLanguageSize = value;
        this.OnPropertyChanged(nameof (TotalLanguageSize));
        this.TotalLanguageSizeString = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.FormatBytes(value);
        this.TotalImageSize = this.BaseImageSize + this.TotalLanguageSize;
      }
    }

    public ulong TotalImageSize
    {
      get => this._totalImageSize;
      set
      {
        this._totalImageSize = value;
        this.OnPropertyChanged(nameof (TotalImageSize));
        this.TotalImageSizeString = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.FormatBytes(value);
      }
    }

    public string BaseImageSizeString
    {
      get => (string) this.GetValue(DescribeImagePageVM.BaseImageSizeStringProperty);
      set => this.SetValue(DescribeImagePageVM.BaseImageSizeStringProperty, (object) value);
    }

    public string TotalImageSizeString
    {
      get => (string) this.GetValue(DescribeImagePageVM.TotalImageSizeStringProperty);
      set => this.SetValue(DescribeImagePageVM.TotalImageSizeStringProperty, (object) value);
    }

    public string TotalLanguageSizeString
    {
      get => (string) this.GetValue(DescribeImagePageVM.TotalLanguageSizeStringProperty);
      set => this.SetValue(DescribeImagePageVM.TotalLanguageSizeStringProperty, (object) value);
    }

    private void PopulateLanguagesWithSizes()
    {
      this.PopulateLanguages();
      CalculateLanguageSizeTask task = BackgroundTasks.GetTask(BackgroundTasks.CalculateLanguageSizeTask) as CalculateLanguageSizeTask;
      task.SelectedOEMInput = this.Context.SelectedOEMInput;
      task.UILanguages = this.UILanguages;
      task.KeyboardLanguages = this.KeyboardLanguages;
      task.SpeechLanguages = this.SpeechLanguages;
      task.TaskCompletedEvent += new TaskEventHandler(this.CalculateLanguageSizesCompleted);
      task.RunTaskAsync((object) this);
    }

    private void CalculateLanguageSizesCompleted(object sender, TaskEventArgs e)
    {
      if (Application.Current == null)
        return;
      Microsoft.WindowsPhone.ImageDesigner.Core.Tools.DispatcherExec((Action) (() =>
      {
        this.BaseImageSize = this.Context.GetCalculatedImageSize(this.Context.ImageType);
        this.TotalLanguageSize = this.CalculateTotalLanguageSize();
      }));
      this.Validate();
    }

    private ulong CalculateTotalLanguageSize()
    {
      HashSet<WPLanguage> source = new HashSet<WPLanguage>();
      source.UnionWith(this.UILanguages.Where<WPLanguage>((Func<WPLanguage, bool>) (l => l.IsSelected)));
      source.UnionWith(this.SpeechLanguages.Where<WPLanguage>((Func<WPLanguage, bool>) (l => l.IsSelected)));
      source.UnionWith(this.KeyboardLanguages.Where<WPLanguage>((Func<WPLanguage, bool>) (l => l.IsSelected)));
      return (ulong) source.Sum<WPLanguage>((Func<WPLanguage, long>) (l => (long) l.Size));
    }

    internal List<FeatureManifest.FMPkgInfo> AllLangFMPkgInfo { get; set; }

    private void PopulateLanguages()
    {
      OEMInput selectedOemInput = IDContext.Instance.SelectedOEMInput;
      OEMInput oemInput = new OEMInput(selectedOemInput);
      oemInput.SupportedLanguages = this.GetAllSupportedLanguages();
      string implicitFMFile = this.Context.SelectedOEMInput.IsMMOS ? IDContext.Instance.MMOSFMFile : IDContext.Instance.MicrosoftPhoneFMFile;
      this.AllLangFMPkgInfo = new List<FeatureManifest.FMPkgInfo>();
      this.AllLangFMPkgInfo = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetOEMInputPackages(oemInput, IDContext.Instance.BSPRoot, IDContext.Instance.AKRoot, implicitFMFile);
      List<WPLanguage> list1 = new List<WPLanguage>();
      foreach (string language in oemInput.SupportedLanguages.UserInterface)
      {
        WPLanguage wpLanguage = new WPLanguage(language);
        if (selectedOemInput.SupportedLanguages.UserInterface.Contains<string>(language, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
          wpLanguage.IsSelected = true;
        list1.Add(wpLanguage);
      }
      this.UILanguages = new ObservableNotifyObjectCollection<WPLanguage>(new string[3]
      {
        "IsSelected",
        "DisplayText",
        "SizeText"
      });
      this.UILanguages.AddItems((IEnumerable<WPLanguage>) list1);
      this.UILanguages.ObjectPropertyChanged += new ObjectPropertyChangedEventHandler(this.Languages_ObjectPropertyChanged);
      this.UILanguages.ObjectPropertyChanged += new ObjectPropertyChangedEventHandler(this.Languages_UpdateBootLanguageList);
      List<WPLanguage> list2 = new List<WPLanguage>();
      foreach (string language in oemInput.SupportedLanguages.Keyboard)
      {
        WPLanguage wpLanguage = new WPLanguage(language);
        if (selectedOemInput.SupportedLanguages.Keyboard.Contains<string>(language, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
          wpLanguage.IsSelected = true;
        list2.Add(wpLanguage);
      }
      this.KeyboardLanguages = new ObservableNotifyObjectCollection<WPLanguage>(new string[3]
      {
        "IsSelected",
        "DisplayText",
        "SizeText"
      });
      this.KeyboardLanguages.AddItems((IEnumerable<WPLanguage>) list2);
      this.KeyboardLanguages.ObjectPropertyChanged += new ObjectPropertyChangedEventHandler(this.Languages_ObjectPropertyChanged);
      List<WPLanguage> list3 = new List<WPLanguage>();
      foreach (string language in oemInput.SupportedLanguages.Speech)
      {
        WPLanguage wpLanguage = new WPLanguage(language);
        if (selectedOemInput.SupportedLanguages.Speech.Contains<string>(language, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
          wpLanguage.IsSelected = true;
        list3.Add(wpLanguage);
      }
      this.SpeechLanguages = new ObservableNotifyObjectCollection<WPLanguage>(new string[3]
      {
        "IsSelected",
        "DisplayText",
        "SizeText"
      });
      this.SpeechLanguages.AddItems((IEnumerable<WPLanguage>) list3);
      this.SpeechLanguages.ObjectPropertyChanged += new ObjectPropertyChangedEventHandler(this.Languages_ObjectPropertyChanged);
      this.TotalLanguageSize = this.CalculateTotalLanguageSize();
    }

    private void Languages_UpdateBootLanguageList(
      object sender,
      ObjectPropertyChangedEventArgs args)
    {
      if (!args.PropertyName.Equals("IsSelected"))
        return;
      this.UpdateBootLanguageList();
    }

    private void Languages_ObjectPropertyChanged(object sender, ObjectPropertyChangedEventArgs args) => Microsoft.WindowsPhone.ImageDesigner.Core.Tools.DispatcherExec((Action) (() =>
    {
      this.Validate();
      if (!args.PropertyName.Equals("IsSelected", StringComparison.OrdinalIgnoreCase) && !args.PropertyName.Equals("Size", StringComparison.OrdinalIgnoreCase))
        return;
      if (this.BaseImageSize == 0UL)
        this.BaseImageSize = this.Context.GetCalculatedImageSize(this.Context.ImageType);
      WPLanguage wpLanguage = args.Item as WPLanguage;
      if (args.PropertyName.Equals("IsSelected", StringComparison.OrdinalIgnoreCase))
      {
        if (wpLanguage.IsSelected)
          this.TotalLanguageSize += wpLanguage.Size;
        else
          this.TotalLanguageSize = this.TotalLanguageSize > wpLanguage.Size ? (this.TotalLanguageSize -= wpLanguage.Size) : this.TotalLanguageSize;
      }
      else
      {
        if (!args.PropertyName.Equals("Size", StringComparison.OrdinalIgnoreCase) || !wpLanguage.IsSelected)
          return;
        this.TotalLanguageSize += wpLanguage.Size;
      }
    }));

    internal SupportedLangs GetAllSupportedLanguages()
    {
      if (this._allSupportedLangs == null)
      {
        this._allSupportedLangs = new SupportedLangs();
        FeatureManifest fm = new FeatureManifest();
        FeatureManifest.ValidateAndLoad(ref fm, this.Context.MicrosoftPhoneFMFile, new IULogger());
        fm.OemInput = this.Context.SelectedOEMInput;
        PkgFile pkgFile = fm.BasePackages.First<PkgFile>((Func<PkgFile, bool>) (pkg => pkg.Language != null && pkg.Language.Equals("*")));
        string directoryName = Path.GetDirectoryName(pkgFile.PackagePath);
        string baseLanguageName = Path.GetFileNameWithoutExtension(pkgFile.PackagePath) + PkgFile.DefaultLanguagePattern;
        string searchPattern = baseLanguageName + "*" + PkgConstants.c_strPackageExtension;
        this._allSupportedLangs.UserInterface = Directory.EnumerateFiles(directoryName, searchPattern).ToList<string>().Select<string, string>((Func<string, string>) (pkg => Path.GetFileNameWithoutExtension(pkg).Substring(baseLanguageName.Length))).ToList<string>();
        this._allSupportedLangs.Speech = new List<string>();
        foreach (PkgFile speechPackage in fm.SpeechPackages)
          this._allSupportedLangs.Speech = this._allSupportedLangs.Speech.Union<string>((IEnumerable<string>) speechPackage.Language.Split(new char[3]
          {
            '(',
            ';',
            ')'
          }, StringSplitOptions.RemoveEmptyEntries), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase).ToList<string>();
        this._allSupportedLangs.Keyboard = new List<string>();
        foreach (PkgFile keyboardPackage in fm.KeyboardPackages)
          this._allSupportedLangs.Keyboard = this._allSupportedLangs.Keyboard.Union<string>((IEnumerable<string>) keyboardPackage.Language.Split(new char[3]
          {
            '(',
            ';',
            ')'
          }, StringSplitOptions.RemoveEmptyEntries), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase).ToList<string>();
      }
      return this._allSupportedLangs;
    }

    private void GenerateFinalOEMInput()
    {
      this.FinalOEMInput = new OEMInput(this.Context.SelectedOEMInput);
      this.FinalOEMInput.Description = this.ImageDescription;
      SupportedLangs supportedLangs = new SupportedLangs();
      if (this.UILanguages != null && this.UILanguages.Count<WPLanguage>() != 0)
        supportedLangs.UserInterface = this.UILanguages.Where<WPLanguage>((Func<WPLanguage, bool>) (lang => lang.IsSelected)).Select<WPLanguage, string>((Func<WPLanguage, string>) (lang => lang.Language)).ToList<string>();
      if (this.KeyboardLanguages != null && this.KeyboardLanguages.Count<WPLanguage>() != 0)
        supportedLangs.Keyboard = this.KeyboardLanguages.Where<WPLanguage>((Func<WPLanguage, bool>) (lang => lang.IsSelected)).Select<WPLanguage, string>((Func<WPLanguage, string>) (lang => lang.Language)).ToList<string>();
      if (this.SpeechLanguages != null && this.SpeechLanguages.Count<WPLanguage>() != 0)
        supportedLangs.Speech = this.SpeechLanguages.Where<WPLanguage>((Func<WPLanguage, bool>) (lang => lang.IsSelected)).Select<WPLanguage, string>((Func<WPLanguage, string>) (lang => lang.Language)).ToList<string>();
      this.FinalOEMInput.SupportedLanguages = supportedLangs;
      this.FinalOEMInput.BootUILanguage = this.BootLanguage;
      this.FinalOEMInput.BootLocale = this.BootLocale;
    }

    public ObservableNotifyObjectCollection<WPLanguage> UILanguages
    {
      get => this._uiLanguages;
      set
      {
        this._uiLanguages = value;
        this.OnPropertyChanged(nameof (UILanguages));
      }
    }

    private void UpdateBootLanguageList()
    {
      this.AvailableBootLanguages = this.UILanguages.Where<WPLanguage>((Func<WPLanguage, bool>) (lang => lang.IsSelected)).Select<WPLanguage, string>((Func<WPLanguage, string>) (lang2 => lang2.DisplayText)).ToList<string>();
      if (this.AvailableBootLanguages.Contains<string>(this.BootLanguage, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
        return;
      if (this.UILanguages.Any<WPLanguage>((Func<WPLanguage, bool>) (lang => lang.IsSelected)))
        this.BootLanguage = this.UILanguages.First<WPLanguage>((Func<WPLanguage, bool>) (lang => lang.IsSelected)).DisplayText;
      else
        this.BootLanguage = "";
    }

    public List<string> AvailableBootLanguages
    {
      get => this._availableBootLanguages;
      set
      {
        this._availableBootLanguages = value;
        this.OnPropertyChanged(nameof (AvailableBootLanguages));
      }
    }

    public string BootLanguage
    {
      get => this._bootLanguage;
      set
      {
        string str = value;
        if ((this._bootLanguage != null || !(this._bootLanguage != str)) && (this._bootLanguage == null || this._bootLanguage.Equals(str, StringComparison.OrdinalIgnoreCase)))
          return;
        this._bootLanguage = str;
        this.OnPropertyChanged(nameof (BootLanguage));
      }
    }

    private void GetSupportedBootLocales()
    {
      List<string> source = new List<string>();
      string str = DescribeImagePageVM.LocaleInfoTableAKPath;
      if (!File.Exists(str))
        str = Path.Combine(this.Context.AKRoot, DescribeImagePageVM.LocaleInfoTableDevPath);
      if (File.Exists(str))
        source = LocaleInfoLookup.Load(str).Locales.Where<LocaleInfo>((Func<LocaleInfo, bool>) (loc => loc.IsWPLocale)).Select<LocaleInfo, string>((Func<LocaleInfo, string>) (loc2 => loc2.name.ToLower())).ToList<string>();
      if (!source.Contains<string>(this.Context.SelectedOEMInput.BootLocale, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
        source.Add(this.Context.SelectedOEMInput.BootLocale.ToLower());
      this.SupportedBootLocales = source;
    }

    public List<string> SupportedBootLocales
    {
      get => (List<string>) this.GetValue(DescribeImagePageVM.SupportedBootLocalesProperty);
      set => this.SetValue(DescribeImagePageVM.SupportedBootLocalesProperty, (object) value);
    }

    public string BootLocale
    {
      get => (string) this.GetValue(DescribeImagePageVM.BootLocaleProperty);
      set => this.SetValue(DescribeImagePageVM.BootLocaleProperty, (object) value);
    }

    public ObservableNotifyObjectCollection<WPLanguage> SpeechLanguages
    {
      get => this._speechLanguages;
      set
      {
        this._speechLanguages = value;
        this.OnPropertyChanged(nameof (SpeechLanguages));
      }
    }

    public ObservableNotifyObjectCollection<WPLanguage> KeyboardLanguages
    {
      get => this._keyboardLanguages;
      set
      {
        this._keyboardLanguages = value;
        this.OnPropertyChanged(nameof (KeyboardLanguages));
      }
    }
  }
}
