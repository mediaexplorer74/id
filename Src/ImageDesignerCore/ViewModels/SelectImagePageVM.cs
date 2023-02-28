// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.SelectImagePageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class SelectImagePageVM : IDViewModelBase
  {
    private BSPConfig _lastUsedBspConfig;
    private ImageType _selectedImageType;
    private Dictionary<ImageType, OEMInput> _oemInputs = new Dictionary<ImageType, OEMInput>();
    private Dictionary<ImageType, ulong> _imageSizes = new Dictionary<ImageType, ulong>();
    private bool _recalculateImageSizes = true;
    private readonly string _fmPackageRelativePath = "Merged\\arm\\fre";
    private readonly string _windowsPhone8FMPackage = "Microsoft.PhoneFM.spkg";
    private readonly string _msPhoneFMDevicePath = "\\Windows\\ImageUpdate\\FeatureManifest\\Microsoft\\MicrosoftPhoneFM.xml";
    private string _msFMFile;
    private readonly string _manufacturingOSFMPackage = "Microsoft.MMOSFM.spkg";
    private readonly string _mmosFMDevicePath = "\\Windows\\ImageUpdate\\FeatureManifest\\Microsoft\\MMOSFM.xml";
    private string _mmosFMFile;
    public static readonly DependencyProperty TestLabelWithSizeProperty = DependencyProperty.Register(nameof (TestLabelWithSize), typeof (string), typeof (SelectImagePageVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty RetailLabelWithSizeProperty = DependencyProperty.Register(nameof (RetailLabelWithSize), typeof (string), typeof (SelectImagePageVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty RetailManufacturingLabelWithSizeProperty = DependencyProperty.Register(nameof (RetailManufacturingLabelWithSize), typeof (string), typeof (SelectImagePageVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty ProductionLabelWithSizeProperty = DependencyProperty.Register(nameof (ProductionLabelWithSize), typeof (string), typeof (SelectImagePageVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty MMOSLabelWithSizeProperty = DependencyProperty.Register(nameof (MMOSLabelWithSize), typeof (string), typeof (SelectImagePageVM), new PropertyMetadata((object) string.Empty));

    internal SelectImagePageVM(IDStates mystate)
      : base(mystate)
    {
      if (string.IsNullOrEmpty(this.TestLabelWithSize))
        this.TestLabelWithSize = this.GetImageTypeLabel(ImageType.Test);
      if (string.IsNullOrEmpty(this.RetailLabelWithSize))
        this.RetailLabelWithSize = this.GetImageTypeLabel(ImageType.Retail);
      if (string.IsNullOrEmpty(this.RetailManufacturingLabelWithSize))
        this.RetailManufacturingLabelWithSize = this.GetImageTypeLabel(ImageType.RetailManufacturing);
      if (string.IsNullOrEmpty(this.ProductionLabelWithSize))
        this.ProductionLabelWithSize = this.GetImageTypeLabel(ImageType.Production);
      if (!string.IsNullOrEmpty(this.MMOSLabelWithSize))
        return;
      this.MMOSLabelWithSize = this.GetImageTypeLabel(ImageType.MMOS);
    }

    protected override bool SaveSupported => true;

    protected override void Validate()
    {
      if (this.SelectedOEMInput == null)
        this.IsValid = false;
      else
        this.IsValid = true;
    }

    protected override void ComputeNextState() => this._nextState = new IDStates?(IDStates.DescribeImage);

    internal override bool OnStateExit()
    {
      if (this.SelectedOEMInput != null)
        this.SelectedOEMInput.WriteToFile(this.Context.OEMInputFile);
      return true;
    }

    internal override bool OnStateEntry()
    {
      OEMInput saved = (OEMInput) null;
      int imageType = (int) this.Context.ImageType;
      if (this.Context.Workflow == Workflow.ModifyImage)
        saved = this.SelectedOEMInput;
      this.GenerateImageTypeOEMInputs();
      this.MergeSavedOEMInput(saved, this.SelectedOEMInput);
      this.SetImageTypeLabels();
      this.CalculateImageTypeSizes();
      this.Validate();
      return true;
    }

    private void MergeSavedOEMInput(OEMInput saved, OEMInput current)
    {
      if (!(saved != null & current != null))
        return;
      current.Description = saved.Description;
      current.SupportedLanguages = saved.SupportedLanguages;
      current.BootLocale = saved.BootLocale;
      current.BootUILanguage = saved.BootUILanguage;
      this.Context.SelectedOEMInput = current;
    }

    public void SelectionChanged() => this.Validate();

    public ImageType SelectedImageType
    {
      get => this._selectedImageType;
      set
      {
        if (value == this._selectedImageType)
          return;
        this._selectedImageType = value;
        this.SelectionChanged();
        this.Validate();
        this.OnPropertyChanged(nameof (SelectedImageType));
      }
    }

    public OEMInput SelectedOEMInput
    {
      get
      {
        OEMInput selectedOemInput = (OEMInput) null;
        if (this._oemInputs.Keys.Contains<ImageType>(this.SelectedImageType))
          selectedOemInput = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.DispatcherExec<OEMInput>((Func<OEMInput>) (() => this._oemInputs[this.SelectedImageType]));
        return selectedOemInput;
      }
      internal set => this._oemInputs[this.SelectedImageType] = value;
    }

    public string MicrosoftPhoneFMFile
    {
      get
      {
        if (string.IsNullOrEmpty(this._msFMFile))
        {
          string str1 = Path.Combine(this.Context.MSPackageRoot, this._fmPackageRelativePath, this._windowsPhone8FMPackage);
          string str2 = Path.Combine(this.Context.TemporaryDirectory, "MICROSOFTPHONEFM.XML");
          IPkgInfo ipkgInfo;
          try
          {
            ipkgInfo = Package.LoadFromCab(str1);
          }
          catch (IUException ex)
          {
            throw new WPIDException((Exception) ex, "Failed to load CAB file ", new object[1]
            {
              (object) str1
            });
          }
          IFileEntry file = ipkgInfo.FindFile(this._msPhoneFMDevicePath);
          if (file != null)
          {
            ipkgInfo.ExtractFile(file.DevicePath, str2, true);
            this._msFMFile = str2;
          }
        }
        return this._msFMFile;
      }
    }

    public string MMOSFMFile
    {
      get
      {
        if (string.IsNullOrEmpty(this._mmosFMFile))
        {
          string str1 = Path.Combine(this.Context.MSPackageRoot, this._fmPackageRelativePath, this._manufacturingOSFMPackage);
          string str2 = Path.Combine(this.Context.TemporaryDirectory, "MMOSFM.xml");
          IPkgInfo ipkgInfo;
          try
          {
            ipkgInfo = Package.LoadFromCab(str1);
          }
          catch (IUException ex)
          {
            throw new WPIDException((Exception) ex, "Failed to load CAB file ", new object[1]
            {
              (object) str1
            });
          }
          IFileEntry file = ipkgInfo.FindFile(this._mmosFMDevicePath);
          if (file != null)
          {
            ipkgInfo.ExtractFile(file.DevicePath, str2, true);
            this._mmosFMFile = str2;
          }
        }
        return this._mmosFMFile;
      }
    }

    private void GenerateImageTypeOEMInputs()
    {
      if (this._lastUsedBspConfig != null && this._lastUsedBspConfig.Equals((object) this.Context.BSPConfig))
        return;
      foreach (ImageType key in Enum.GetValues(typeof (ImageType)))
      {
        this._oemInputs[key] = (OEMInput) null;
        this._imageSizes[key] = 0UL;
        this._recalculateImageSizes = true;
      }
      foreach (ImageType key in Enum.GetValues(typeof (ImageType)))
      {
        if (this.Context.BSPConfig.OEMInputFiles.ContainsKey(key))
        {
          string oemInputFile = this.Context.BSPConfig.OEMInputFiles[key];
          if (File.Exists(oemInputFile))
          {
            OEMInput xmlInput = new OEMInput();
            OEMInput.ValidateInput(ref xmlInput, oemInputFile, new IULogger(), this.Context.MSPackageRoot, FeatureManifest.CPUType_ARM);
            this._oemInputs[key] = xmlInput;
          }
        }
      }
      this._lastUsedBspConfig = new BSPConfig(this.Context.BSPConfig);
    }

    private void CalculateImageTypeSizes()
    {
      ImageSizeCalculatorTask task = BackgroundTasks.GetTask(BackgroundTasks.SizeCalculatorTask) as ImageSizeCalculatorTask;
      task.OEMInputs = this._oemInputs;
      task.TaskCompletedEvent += new TaskEventHandler(this.ImageSizeCalculationsCompleted);
      task.BSPRoot = this.Context.BSPRoot;
      task.AKRoot = this.Context.AKRoot;
      task.MicrosoftPhoneFMFile = this.Context.MicrosoftPhoneFMFile;
      task.MMOSFMFile = this.Context.MMOSFMFile;
      task.RunTaskAsync();
    }

    private void ImageSizeCalculationsCompleted(object sender, TaskEventArgs e)
    {
      if (Application.Current == null)
        return;
      ImageSizeCalculatorTask sizeCalculatorTask = sender as ImageSizeCalculatorTask;
      foreach (ImageType key in Enum.GetValues(typeof (ImageType)))
        this._imageSizes[key] = sizeCalculatorTask.ImageSizes[key];
      this.SetImageTypeLabels();
    }

    private void SetImageTypeLabels()
    {
      this.TestLabelWithSize = this.GetImageTypeLabel(ImageType.Test);
      this.RetailLabelWithSize = this.GetImageTypeLabel(ImageType.Retail);
      this.RetailManufacturingLabelWithSize = this.GetImageTypeLabel(ImageType.RetailManufacturing);
      this.ProductionLabelWithSize = this.GetImageTypeLabel(ImageType.Production);
      this.MMOSLabelWithSize = this.GetImageTypeLabel(ImageType.MMOS);
    }

    private void bw_DoWork(object sender, DoWorkEventArgs e)
    {
      if (!this._recalculateImageSizes)
        return;
      foreach (ImageType key in Enum.GetValues(typeof (ImageType)))
        this._imageSizes[key] = this.GetImageSize(this._oemInputs[key]);
      this._recalculateImageSizes = false;
    }

    public ulong GetCalculatedImageSize(ImageType imageType)
    {
      ulong calculatedImageSize = 0;
      if (BackgroundTasks.GetTask(BackgroundTasks.SizeCalculatorTask).Status == TaskStatus.CompleteSucceeded && this._imageSizes.ContainsKey(imageType))
        calculatedImageSize = this._imageSizes[imageType];
      return calculatedImageSize;
    }

    public string TestLabelWithSize
    {
      get => (string) this.GetValue(SelectImagePageVM.TestLabelWithSizeProperty);
      set => this.SetValue(SelectImagePageVM.TestLabelWithSizeProperty, (object) value);
    }

    public string RetailLabelWithSize
    {
      get => (string) this.GetValue(SelectImagePageVM.RetailLabelWithSizeProperty);
      set => this.SetValue(SelectImagePageVM.RetailLabelWithSizeProperty, (object) value);
    }

    public string RetailManufacturingLabelWithSize
    {
      get => (string) this.GetValue(SelectImagePageVM.RetailManufacturingLabelWithSizeProperty);
      set => this.SetValue(SelectImagePageVM.RetailManufacturingLabelWithSizeProperty, (object) value);
    }

    public string ProductionLabelWithSize
    {
      get => (string) this.GetValue(SelectImagePageVM.ProductionLabelWithSizeProperty);
      set => this.SetValue(SelectImagePageVM.ProductionLabelWithSizeProperty, (object) value);
    }

    public string MMOSLabelWithSize
    {
      get => (string) this.GetValue(SelectImagePageVM.MMOSLabelWithSizeProperty);
      set => this.SetValue(SelectImagePageVM.MMOSLabelWithSizeProperty, (object) value);
    }

    public bool TestImageEnabled => this.IsImageTypeEnabled(ImageType.Test);

    public bool ProductionImageEnabled => this.IsImageTypeEnabled(ImageType.Production);

    public bool RetailImageEnabled => this.IsImageTypeEnabled(ImageType.Retail);

    public bool RetailManufacturingImageEnabled => this.IsImageTypeEnabled(ImageType.RetailManufacturing);

    public bool MMOSImageEnabled => this.IsImageTypeEnabled(ImageType.MMOS);

    private bool IsImageTypeEnabled(ImageType imgType)
    {
      bool flag = false;
      if (this._oemInputs.ContainsKey(imgType) && this._oemInputs[imgType] != null)
        flag = true;
      return flag;
    }

    private string GetImageTypeLabel(ImageType imgType)
    {
      string imageTypeLabel = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetImageName(imgType);
      if (this._oemInputs.ContainsKey(imgType) && this._oemInputs[imgType] != null)
      {
        string str = imageTypeLabel + " ";
        imageTypeLabel = !this._imageSizes.ContainsKey(imgType) || this._imageSizes[imgType] == 0UL ? str + Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtImageTypeSizeCalculating") : str + string.Format(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtImageTypeSize"), (object) Microsoft.WindowsPhone.ImageDesigner.Core.Tools.FormatBytes(this._imageSizes[imgType]));
      }
      return imageTypeLabel;
    }

    private ulong GetImageSize(OEMInput oemInput)
    {
      if (oemInput == null)
        return 0;
      string implicitFMFile = oemInput.IsMMOS ? this.Context.MMOSFMFile : this.Context.MicrosoftPhoneFMFile;
      List<FeatureManifest.FMPkgInfo> list = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetOEMInputPackages(oemInput, this.Context.BSPRoot, this.Context.AKRoot, implicitFMFile).Where<FeatureManifest.FMPkgInfo>((Func<FeatureManifest.FMPkgInfo, bool>) (pkg => string.IsNullOrEmpty(pkg.Language) && pkg.FMGroup != FeatureManifest.PackageGroups.KEYBOARD && pkg.FMGroup != FeatureManifest.PackageGroups.SPEECH)).ToList<FeatureManifest.FMPkgInfo>();
      Microsoft.WindowsPhone.ImageDesigner.Core.Tools.RetrieveForMetadataPackages(list, this.Context.PackageMetadataList);
      return Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetPackageListSize(list.Select<FeatureManifest.FMPkgInfo, string>((Func<FeatureManifest.FMPkgInfo, string>) (pkg => pkg.PackagePath)).ToList<string>(), this.Context.PackageMetadataList);
    }
  }
}
