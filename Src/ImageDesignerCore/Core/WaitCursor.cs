// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.WaitCursor
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Windows.Input;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class WaitCursor : IDisposable
  {
    private Cursor _previousCursor;

    public WaitCursor()
    {
      this._previousCursor = Mouse.OverrideCursor;
      if (!Tools.InUIMode())
        return;
      Mouse.OverrideCursor = Cursors.Wait;
    }

    public void Dispose() => Mouse.OverrideCursor = this._previousCursor;
  }
}
