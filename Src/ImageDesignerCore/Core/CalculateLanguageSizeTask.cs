// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.CalculateLanguageSizeTask
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  internal class CalculateLanguageSizeTask : AsyncWPIDTask
  {
    public CalculateLanguageSizeTask()
      : base(BackgroundTasks.CalculateLanguageSizeTask)
    {
      this.Task += new TaskWorkEventHandler(this.PopulateLanguagesAsync);
    }

    public ObservableNotifyObjectCollection<WPLanguage> UILanguages { get; internal set; }

    public ObservableNotifyObjectCollection<WPLanguage> KeyboardLanguages { get; internal set; }

    public ObservableNotifyObjectCollection<WPLanguage> SpeechLanguages { get; internal set; }

    public OEMInput SelectedOEMInput { get; set; }

    private void PopulateLanguagesAsync(object sender, TaskWorkEventArgs args)
    {
      DescribeImagePageVM describeImagePageVm = args.Argument as DescribeImagePageVM;
      BackgroundTasks.WaitForCompletion(BackgroundTasks.SizeCalculatorTask);
      Tools.RetrieveForMetadataPackages(describeImagePageVm.AllLangFMPkgInfo, IDContext.Instance.PackageMetadataList);
      foreach (WPLanguage uiLanguage in (Collection<WPLanguage>) this.UILanguages)
      {
        WPLanguage lang = uiLanguage;
        List<string> list = describeImagePageVm.AllLangFMPkgInfo.Where<FeatureManifest.FMPkgInfo>((Func<FeatureManifest.FMPkgInfo, bool>) (pkg => lang.Language.Equals(pkg.Language, StringComparison.OrdinalIgnoreCase))).Select<FeatureManifest.FMPkgInfo, string>((Func<FeatureManifest.FMPkgInfo, string>) (pkg => pkg.PackagePath)).ToList<string>();
        if (!lang.HasMissingPackages)
          lang.Size = Tools.GetPackageListSize(list, IDContext.Instance.PackageMetadataList);
      }
      foreach (WPLanguage keyboardLanguage in (Collection<WPLanguage>) this.KeyboardLanguages)
      {
        WPLanguage lang = keyboardLanguage;
        List<string> list = describeImagePageVm.AllLangFMPkgInfo.Where<FeatureManifest.FMPkgInfo>((Func<FeatureManifest.FMPkgInfo, bool>) (pkg => pkg.FMGroup == FeatureManifest.PackageGroups.KEYBOARD && lang.Language.Equals(pkg.GroupValue, StringComparison.OrdinalIgnoreCase))).Select<FeatureManifest.FMPkgInfo, string>((Func<FeatureManifest.FMPkgInfo, string>) (pkg => pkg.PackagePath)).ToList<string>();
        if (!lang.HasMissingPackages)
          lang.Size = Tools.GetPackageListSize(list, IDContext.Instance.PackageMetadataList);
      }
      foreach (WPLanguage speechLanguage in (Collection<WPLanguage>) this.SpeechLanguages)
      {
        WPLanguage lang = speechLanguage;
        List<string> list = describeImagePageVm.AllLangFMPkgInfo.Where<FeatureManifest.FMPkgInfo>((Func<FeatureManifest.FMPkgInfo, bool>) (pkg => pkg.FMGroup == FeatureManifest.PackageGroups.SPEECH && lang.Language.Equals(pkg.GroupValue, StringComparison.OrdinalIgnoreCase))).Select<FeatureManifest.FMPkgInfo, string>((Func<FeatureManifest.FMPkgInfo, string>) (pkg => pkg.PackagePath)).ToList<string>();
        if (!lang.HasMissingPackages)
          lang.Size = Tools.GetPackageListSize(list, IDContext.Instance.PackageMetadataList);
      }
    }
  }
}
