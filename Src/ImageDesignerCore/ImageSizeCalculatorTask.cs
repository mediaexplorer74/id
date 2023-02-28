// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ImageSizeCalculatorTask
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.FeatureAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  internal class ImageSizeCalculatorTask : AsyncWPIDTask
  {
    private Dictionary<ImageType, OEMInput> _oemInputs;
    private Dictionary<ImageType, ulong> _imageSizes = new Dictionary<ImageType, ulong>();

    public ImageSizeCalculatorTask()
      : base(BackgroundTasks.SizeCalculatorTask)
    {
      this.Task += new TaskWorkEventHandler(this.CalculateImageSizes);
    }

    public Dictionary<ImageType, OEMInput> OEMInputs
    {
      get => this._oemInputs;
      set => this._oemInputs = value;
    }

    public Dictionary<ImageType, ulong> ImageSizes => this._imageSizes;

    public string BSPRoot { get; set; }

    public string AKRoot { get; set; }

    public string MicrosoftPhoneFMFile { get; set; }

    public string MMOSFMFile { get; set; }

    public void CalculateImageSizes(object sender, TaskWorkEventArgs e)
    {
      foreach (ImageType key in Enum.GetValues(typeof (ImageType)))
        this.ImageSizes[key] = this.GetImageSize(this._oemInputs[key]);
    }

    private ulong GetImageSize(OEMInput oemInput)
    {
      if (oemInput == null)
        return 0;
      IDContext instance = IDContext.Instance;
      string implicitFMFile = oemInput.IsMMOS ? this.MMOSFMFile : this.MicrosoftPhoneFMFile;
      List<FeatureManifest.FMPkgInfo> list = Tools.GetOEMInputPackages(oemInput, this.BSPRoot, this.AKRoot, implicitFMFile).Where<FeatureManifest.FMPkgInfo>((Func<FeatureManifest.FMPkgInfo, bool>) (pkg => string.IsNullOrEmpty(pkg.Language) && pkg.FMGroup != FeatureManifest.PackageGroups.KEYBOARD && pkg.FMGroup != FeatureManifest.PackageGroups.SPEECH)).ToList<FeatureManifest.FMPkgInfo>();
      Tools.RetrieveForMetadataPackages(list, instance.PackageMetadataList);
      return Tools.GetPackageListSize(list.Select<FeatureManifest.FMPkgInfo, string>((Func<FeatureManifest.FMPkgInfo, string>) (pkg => pkg.PackagePath)).ToList<string>(), instance.PackageMetadataList);
    }
  }
}
