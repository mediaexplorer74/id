// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.IIDControl
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public interface IIDControl
  {
    IDViewModelBase CurrentPage { get; }

    IDContext Context { get; }

    bool CanMoveToNextPage { get; }

    bool CanMoveToPreviousPage { get; }

    bool CanMoveToStartPage { get; }

    bool MoveToStartPage();

    bool MoveToNextPage();

    bool MoveToPreviousPage();

    bool Save();
  }
}
