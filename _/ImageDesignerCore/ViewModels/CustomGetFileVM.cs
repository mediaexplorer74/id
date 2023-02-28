// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.CustomGetFileVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class CustomGetFileVM : DependencyObject
  {
    public static readonly DependencyProperty ShowHeaderProperty = DependencyProperty.Register(nameof (ShowHeader), typeof (bool), typeof (CustomGetFileVM), new PropertyMetadata((object) true));
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof (Header), typeof (string), typeof (CustomGetFileVM), new PropertyMetadata((object) nameof (Header)));
    public static readonly DependencyProperty InstructionsProperty = DependencyProperty.Register(nameof (Instructions), typeof (string), typeof (CustomGetFileVM), new PropertyMetadata((object) nameof (Instructions)));
    public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(nameof (Result), typeof (CustomGetFileResult), typeof (CustomGetFileVM), new PropertyMetadata((object) CustomGetFileResult.Cancel));
    public static readonly DependencyProperty ShowDialogNextTimeProperty = DependencyProperty.Register(nameof (ShowDialogNextTime), typeof (bool), typeof (CustomGetFileVM), new PropertyMetadata((object) true));
    public static readonly DependencyProperty DialogTypeProperty = DependencyProperty.Register(nameof (DialogType), typeof (CustomDialogType), typeof (CustomGetFileVM), new PropertyMetadata((object) CustomDialogType.Invalid));
    public static readonly DependencyProperty ShowDisplayNameProperty = DependencyProperty.Register(nameof (ShowDisplayName), typeof (bool), typeof (CustomGetFileVM), new PropertyMetadata((object) false));
    public List<string> DisplayNamesAlreadyInUseList;
    public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(nameof (DisplayName), typeof (string), typeof (CustomGetFileVM), new PropertyMetadata((object) ""));
    public List<string> FilenamesAlreadyInUseList;
    public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register(nameof (Filename), typeof (string), typeof (CustomGetFileVM), new PropertyMetadata((object) ""));
    public string FileExtensionsFilter;
    public string FilterGroupName = "";
    public bool AllowDirectorySelection;

    public CustomGetFileVM()
    {
    }

    public CustomGetFileVM(CustomDialogType type) => this.DialogType = type;

    public bool ShowHeader
    {
      get => (bool) this.GetValue(CustomGetFileVM.ShowHeaderProperty);
      set => this.SetValue(CustomGetFileVM.ShowHeaderProperty, (object) value);
    }

    public string Header
    {
      get => (string) this.GetValue(CustomGetFileVM.HeaderProperty);
      set => this.SetValue(CustomGetFileVM.HeaderProperty, (object) value);
    }

    public string Instructions
    {
      get => (string) this.GetValue(CustomGetFileVM.InstructionsProperty);
      set => this.SetValue(CustomGetFileVM.InstructionsProperty, (object) value);
    }

    public CustomGetFileResult Result
    {
      get => (CustomGetFileResult) this.GetValue(CustomGetFileVM.ResultProperty);
      set => this.SetValue(CustomGetFileVM.ResultProperty, (object) value);
    }

    public bool ShowDialogNextTime
    {
      get => (bool) this.GetValue(CustomGetFileVM.ShowDialogNextTimeProperty);
      set => this.SetValue(CustomGetFileVM.ShowDialogNextTimeProperty, (object) value);
    }

    public CustomDialogType DialogType
    {
      get => (CustomDialogType) this.GetValue(CustomGetFileVM.DialogTypeProperty);
      set => this.SetValue(CustomGetFileVM.DialogTypeProperty, (object) value);
    }

    public bool ShowDisplayName
    {
      get => (bool) this.GetValue(CustomGetFileVM.ShowDisplayNameProperty);
      set => this.SetValue(CustomGetFileVM.ShowDisplayNameProperty, (object) value);
    }

    public string DisplayName
    {
      get => (string) this.GetValue(CustomGetFileVM.DisplayNameProperty);
      set => this.SetValue(CustomGetFileVM.DisplayNameProperty, (object) value);
    }

    public string Filename
    {
      get => (string) this.GetValue(CustomGetFileVM.FilenameProperty);
      set => this.SetValue(CustomGetFileVM.FilenameProperty, (object) value);
    }

    public bool OnLoad() => true;

    public bool OnExit()
    {
      if (this.Result == CustomGetFileResult.Cancel)
        return true;
      this.Filename = this.Filename.TrimEnd('\\');
      if (!string.IsNullOrEmpty(this.DisplayName) && this.DisplayNamesAlreadyInUseList != null && this.DisplayNamesAlreadyInUseList.Count != 0 && this.DisplayNamesAlreadyInUseList.Contains<string>(this.DisplayName, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
      {
        int num = (int) MessageBox.Show(string.Format(Tools.GetString("msgCOSAddFileDuplicateNameError"), (object) this.DisplayName), Tools.GetString("msgCOSAddFileDuplicateNameErrorTitle"));
        return false;
      }
      if (this.AllowDirectorySelection && !Directory.Exists(this.Filename) && (string.IsNullOrEmpty(this.Filename) || !File.Exists(this.Filename)))
      {
        int num = (int) MessageBox.Show(string.Format(Tools.GetString("msgCOSAddFileDuplicateFileNotFoundError"), (object) this.Filename), Tools.GetString("msgCOSAddFileDuplicateFileNotFoundErrorTitle"));
        return false;
      }
      if (this.FilenamesAlreadyInUseList != null && this.FilenamesAlreadyInUseList.Count != 0)
      {
        string str = Path.GetFileName(this.Filename);
        if (this.AllowDirectorySelection && Directory.Exists(this.Filename))
          str = this.Filename;
        if (this.FilenamesAlreadyInUseList.Contains<string>(str, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
        {
          int num = (int) MessageBox.Show(string.Format(Tools.GetString("msgCOSAddFileDuplicateNameError"), (object) str), Tools.GetString("msgCOSAddFileDuplicateNameErrorTitle"));
          return false;
        }
      }
      return true;
    }
  }
}
