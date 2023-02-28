// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPCustomization
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System.ComponentModel;
using System.IO;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPCustomization : ISelectable, INotifyPropertyChanged
  {
    public ImageCustomizations Customization;
    public string CustomizationFile;
    private bool _isSelected;

    public WPCustomization(string File, ImageCustomizations customization)
    {
      this.CustomizationFile = File;
      this.Customization = customization;
      if (customization != null && !string.IsNullOrEmpty(customization.Name))
        this.DisplayText = customization.Name;
      else
        this.DisplayText = Path.GetFileNameWithoutExtension(File);
    }

    public string DisplayText { get; set; }

    public bool IsSelected
    {
      get => this._isSelected;
      set
      {
        bool flag = value != this._isSelected;
        this._isSelected = value;
        if (!flag || this.PropertyChanged == null)
          return;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (IsSelected)));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
