// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.WPLanguage
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.ComponentModel;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class WPLanguage : INotifyPropertyChanged, ISelectable
  {
    private bool _isSelected;
    public bool IsEnabled;
    private ulong _size;
    private bool _hasMissingPackages;

    public WPLanguage(string language) => this.Language = language;

    public bool IsSelected
    {
      get => this._isSelected;
      set
      {
        bool flag = value != this._isSelected;
        this._isSelected = !this.HasMissingPackages && value;
        if (!flag || this.PropertyChanged == null)
          return;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (IsSelected)));
      }
    }

    public string Language { get; set; }

    public ulong Size
    {
      get => this._size;
      set
      {
        if ((long) this._size == (long) value)
          return;
        this._size = value;
        if (this.PropertyChanged == null)
          return;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(nameof (Size)));
        this.PropertyChanged((object) this, new PropertyChangedEventArgs("SizeText"));
      }
    }

    public bool HasMissingPackages
    {
      get => this._hasMissingPackages;
      set
      {
        if (value)
        {
          this.IsSelected = false;
          this.IsEnabled = false;
        }
        this._hasMissingPackages = value;
      }
    }

    public string DisplayText
    {
      get
      {
        string language = this.Language;
        if (this.HasMissingPackages)
          language += string.Format(" ({0})", (object) Tools.GetString("txtMissingPackages"));
        return language;
      }
    }

    public string SizeText
    {
      get
      {
        string empty = string.Empty;
        return this.Size != 0UL ? "(" + Tools.FormatBytes(this.Size) + ")" : string.Format("({0})", (object) Tools.GetString("txtCalculatingPackages"));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
