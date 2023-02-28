// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.PickOutputLocationPageVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.IO;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class PickOutputLocationPageVM : IDViewModelBase
  {
    public static readonly DependencyProperty OutputLocationProperty = DependencyProperty.Register(nameof (OutputLocation), typeof (string), typeof (PickOutputLocationPageVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty SelectedOptionProperty = DependencyProperty.Register(nameof (SelectedOption), typeof (OutputLocationOptions), typeof (PickOutputLocationPageVM), new PropertyMetadata((object) OutputLocationOptions.ReplaceExisting));

    internal PickOutputLocationPageVM(IDStates mystate)
      : base(mystate)
    {
    }

    protected override void Validate() => this.IsValid = true;

    protected override bool SaveSupported => true;

    protected override void ComputeNextState() => this._nextState = new IDStates?(IDStates.SelectImageType);

    internal override bool OnStateEntry()
    {
      this.OutputLocation = IDContext.Instance.OutputDir;
      return true;
    }

    protected override bool SavePage()
    {
      if (this.SelectedOption == OutputLocationOptions.NewLocation)
      {
        IDContext instance = IDContext.Instance;
        string outputDir = instance.OutputDir;
        string outputLocation = this.OutputLocation;
        if (!string.Equals(outputDir, outputLocation, StringComparison.OrdinalIgnoreCase))
        {
          Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.CreateDirectory(outputLocation);
          Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Copy(Path.Combine(outputDir, "OemInput.xml"), Path.Combine(outputLocation, "OemInput.xml"), true);
          instance.OutputDir = outputLocation;
          instance.Controller.SaveProject();
        }
      }
      return true;
    }

    public string OutputLocation
    {
      get => (string) this.GetValue(PickOutputLocationPageVM.OutputLocationProperty);
      set => this.SetValue(PickOutputLocationPageVM.OutputLocationProperty, (object) value);
    }

    public OutputLocationOptions SelectedOption
    {
      get => (OutputLocationOptions) this.GetValue(PickOutputLocationPageVM.SelectedOptionProperty);
      set => this.SetValue(PickOutputLocationPageVM.SelectedOptionProperty, (object) value);
    }
  }
}
