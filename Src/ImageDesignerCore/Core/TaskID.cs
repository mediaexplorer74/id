﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.TaskID
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class TaskID
  {
    internal TaskID(string id) => this.ID = id;

    public string ID { get; internal set; }
  }
}