// IDContext.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.IDContext
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Application = Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Application;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class IDContext
  {
    internal const string ATTR_OUTPUTDIR = "OutputDir";
    internal const string ATTR_BSPCONFIG = "BSPConfig";
    public const string ATTR_OEMINPUTFILE = "OEMInput";
    internal const string ATTR_IMAGETYPE = "ImageType";
    internal const string ATTR_AKROOT = "AKRoot";
    internal const string ATTR_AKVERSION = "AKVersion";
    private static IDContext _instance;
    private string _msPackageRoot;
    private string _akRoot;
    private string _akVersion = string.Empty;
    private string _tempDir;
    private ProjectXml _projectXml;
    public Dictionary<string, PackageMetadata> PackageMetadataList = new Dictionary<string, PackageMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private Version _customizationPkgVersion = new Version(8, 1, 0, 0);
    private IDController _controller;
    private Dictionary<IDStates, IDViewModelBase> _stateMap;

    private IDContext()
    {
    }

    public static IDContext Instance
    {
      get
      {
        if (IDContext._instance == null)
          IDContext._instance = new IDContext();
        return IDContext._instance;
      }
    }

    internal static IDContext NewInstance => new IDContext()
    {
      Controller = IDContext._instance.Controller
    };

    internal static void SwitchInstance(IDContext newContext) => IDContext._instance = newContext;

    public IDController Controller
    {
      set
      {
        this._controller = value;
        this._stateMap = this._controller.StateMap;
      }
      get
      {
        if (this._controller == null)
        {
          this._controller = new IDController();
          this._stateMap = this._controller.StateMap;
        }
        return this._controller;
      }
    }

    public string OutputDir
    {
      get
      {
        SettingUpPageVM vm = this._stateMap[IDStates.SettingUp] as SettingUpPageVM;
        string empty = string.Empty;

                //RnD ; fix it
                string outputDir = ""; // Application.Current == null
                           // ? vm.OutputPath
                          //  : Application.Current.Dispatcher.Invoke((Action) (() => vm.OutputPath)) as string;
      
         if (string.IsNullOrWhiteSpace(outputDir))
          outputDir = Constants.DEFAULT_OUTPUT_FOLDER;
        return outputDir;
      }
      set => (this._stateMap[IDStates.SettingUp] as SettingUpPageVM).OutputPath = value;
    }

    public string FFUPath
    {
      get
      {
        BuildImagePageVM vm = this._stateMap[IDStates.BuildImage] as BuildImagePageVM;
        string empty = string.Empty;

                //RnD ; fix it
                return /*Application.Current == */default; 
            //? vm.FFUPath 
            //: default;//Application.Current.Dispatcher.Invoke((Delegate) (() => vm.FFUPath)) as string;
      }
    }

    public string BSPRoot => this.BSPConfig == null ? (string) null : this.BSPConfig.BSPRoot;

    public BSPConfig BSPConfig { get; set; }

    public string MSPackageRoot
    {
      get
      {
        if (string.IsNullOrEmpty(this._msPackageRoot))
        {
          string path = Path.Combine(this.AKRoot, "MSPackages");
          if (!Directory.Exists(path))
            path = Path.Combine(this.AKRoot, "Prebuilt");
          this._msPackageRoot = path;
        }
        return this._msPackageRoot;
      }
    }

    public string OEMInputFile => Path.Combine(this.OutputDir, "OemInput.xml");

    public OEMInput SelectedOEMInput
    {
      get
      {
                //RnD
        SelectImagePageVM vm = this._stateMap[IDStates.SelectImageType] as SelectImagePageVM;
                return default; //Application.Current == null
                       //     ? vm.SelectedOEMInput
                       //     : default;//Application.Current.Dispatcher.Invoke((Delegate) (() => vm.SelectedOEMInput)) as OEMInput;
      }
      internal set => (this._stateMap[IDStates.SelectImageType] as SelectImagePageVM).SelectedOEMInput = value;
    }

    public ImageType ImageType
    {
      get => (this._stateMap[IDStates.SelectImageType] as SelectImagePageVM).SelectedImageType;
      internal set => (this._stateMap[IDStates.SelectImageType] as SelectImagePageVM).SelectedImageType = value;
    }

    public string MicrosoftPhoneFMFile
    {
      get
      {
        SelectImagePageVM vm = this._stateMap[IDStates.SelectImageType] as SelectImagePageVM;
        string empty = string.Empty;

                //RnD ; fix it
        return default;//Application.Current == null ? vm.MicrosoftPhoneFMFile : Microsoft.WindowsPhone.ImageDesigner.Core.Tools.DispatcherExec<string>((Func<string>) (() => vm.MicrosoftPhoneFMFile));
      }
    }

    public string MMOSFMFile
    {
      get
      {
        SelectImagePageVM vm = this._stateMap[IDStates.SelectImageType] as SelectImagePageVM;
        string empty = string.Empty;

        // RnD ; fix it
        return default; //Application.Current == null ? vm.MMOSFMFile : Microsoft.WindowsPhone.ImageDesigner.Core.Tools.DispatcherExec<string>((Func<string>) (() => vm.MMOSFMFile));
      }
    }

    public string AKCustomizationTemplatesDir => (this._stateMap[IDStates.SelectTemplates] as SelectTemplatesPageVM).AKCustomizationTemplatesDir;

    public ImageCustomizations CurrentImageCustomization
    {
      get => (this._stateMap[IDStates.SelectTemplates] as SelectTemplatesPageVM).CurrentOEMCustomization;
      set => (this._stateMap[IDStates.SelectTemplates] as SelectTemplatesPageVM).CurrentOEMCustomization = value;
    }

    public string CurrentOEMCustomizationFile => (this._stateMap[IDStates.SelectTemplates] as SelectTemplatesPageVM).CurrentOEMCustomizationFile;

    public string AKRoot
    {
      get
      {
        if (string.IsNullOrEmpty(this._akRoot))
          this._akRoot = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKRoot();
        return this._akRoot;
      }
      internal set
      {
        this._akRoot = value;
        Environment.SetEnvironmentVariable("WPDKCONTENTROOT", this._akRoot);
      }
    }

    public string AKVersion
    {
      get => this._akVersion;
      internal set => this._akVersion = value;
    }

    public string TemporaryDirectory
    {
      get
      {
        if (string.IsNullOrEmpty(this._tempDir))
          this._tempDir = FileUtils.GetTempDirectory();
        if (!Directory.Exists(this._tempDir))
          Directory.CreateDirectory(this._tempDir);
        return this._tempDir;
      }
    }

    public ProjectXml ProjectXml
    {
      get
      {
        if (this._projectXml == null)
          this._projectXml = new ProjectXml();
        return this._projectXml;
      }
      set => this._projectXml = value;
    }

    public Workflow Workflow
    {
      get => (this._stateMap[IDStates.GettingStarted] as GettingStartedPageVM).SelectedStartOption;
      set => (this._stateMap[IDStates.GettingStarted] as GettingStartedPageVM).SelectedStartOption = value;
    }

    public CustomizationChoice SelectedCustomizationChoice => (this._stateMap[IDStates.CustomizationChoice] as CustomizationChoicePageVM).SelectedCustomizationChoice;

    public ulong GetCalculatedImageSize(ImageType imageType) => (this._stateMap[IDStates.SelectImageType] as SelectImagePageVM).GetCalculatedImageSize(imageType);

    public Version CustomizationPkgVersion
    {
      get => this._customizationPkgVersion;
      internal set => this._customizationPkgVersion = value;
    }
  }
}
