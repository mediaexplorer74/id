// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.ImagePartition
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class ImagePartition
  {
    private FileInfo[] _files;

    protected ImagePartition()
    {
    }

    public ImagePartition(string name, string root)
    {
      this.Name = name;
      this.Root = root;
    }

    public string PhysicalDeviceId { get; protected set; }

    public string Name { get; protected set; }

    public string Root { get; protected set; }

    public DriveInfo MountedDriveInfo { get; protected set; }

    public FileInfo[] Files
    {
      get
      {
        if (this._files == null && !string.IsNullOrEmpty(this.Root))
          this._files = new DirectoryInfo(this.Root).GetFiles("*.*", SearchOption.AllDirectories);
        return this._files;
      }
    }
  }
}
