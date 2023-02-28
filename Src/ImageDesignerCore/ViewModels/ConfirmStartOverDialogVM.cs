// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.ConfirmStartOverDialogVM
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class ConfirmStartOverDialogVM : CustomMessageBoxVM
  {
    public ConfirmStartOverDialogVM()
      : base(CustomDialogType.YesNoDialog)
    {
      this.ShowDialogNextTimeVisible = true;
    }

    public override bool OnLoad()
    {
      string userConfig = UserConfig.GetUserConfig("ShowConfirmStartOverDialog");
      if (!string.IsNullOrWhiteSpace(userConfig))
        this.ShowDialogNextTime = Convert.ToBoolean(userConfig);
      return true;
    }

    public override bool OnExit()
    {
      UserConfig.SaveUserConfig("ShowConfirmStartOverDialog", Convert.ToString(this.ShowDialogNextTime));
      return true;
    }
  }
}
