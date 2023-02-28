// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.MobileCoreImage
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public abstract class MobileCoreImage
  {
    private const string EXTENSION_VHD = ".VHD";
    private const string EXTENSION_WIM = ".WIM";
    private const string ERROR_IMAGENOTFOUND = "The specified file ({0}) either does not exist or cannot be read.";
    private const string ERROR_INVALIDIMAGE = "The specified file ({0}) is not a valid VHD image.";
    private const string STR_HIVE_PATH = "Windows\\System32\\Config";
    private const string ERROR_NO_SUCH_PARTITION = "Request partition {0} cannot be found in the image";
    private const string STR_SYSTEM32_DIR = "Windows\\System32";
    protected string m_mobileCoreImagePath;
    protected ImagePartitionCollection m_partitions = new ImagePartitionCollection();

    protected MobileCoreImage(string path) => this.m_mobileCoreImagePath = path;

    public static MobileCoreImage Create(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException(path);
      FileInfo fileInfo = LongPathFile.Exists(path) ? new FileInfo(path) : throw new FileNotFoundException(string.Format("The specified file ({0}) is not a valid VHD image.", (object) path));
      if (fileInfo.Extension.Equals(".VHD", StringComparison.OrdinalIgnoreCase))
        return (MobileCoreImage) new MobileCoreVHD(path);
      if (fileInfo.Extension.Equals(".WIM", StringComparison.OrdinalIgnoreCase))
        return (MobileCoreImage) new MobileCoreWIM(path);
      throw new ArgumentException(string.Format("The specified file ({0}) is not a valid VHD image.", (object) path));
    }

    public string ImagePath => this.m_mobileCoreImagePath;

    public bool IsMounted { get; protected set; }

    public ReadOnlyCollection<ImagePartition> Partitions
    {
      get
      {
        ReadOnlyCollection<ImagePartition> partitions = (ReadOnlyCollection<ImagePartition>) null;
        if (this.IsMounted)
          partitions = new ReadOnlyCollection<ImagePartition>((IList<ImagePartition>) this.m_partitions);
        return partitions;
      }
    }

    public abstract void Mount();

    public abstract void MountReadOnly();

    public abstract void Unmount();

    public ImagePartition GetPartition(MobileCorePartitionType type)
    {
      ImagePartition imagePartition = (ImagePartition) null;
      if (!this.IsMounted)
        return (ImagePartition) null;
      foreach (ImagePartition partition in this.Partitions)
      {
        if (partition.Root != null && type == MobileCorePartitionType.System && LongPathDirectory.Exists(Path.Combine(partition.Root, "Windows\\System32")))
          imagePartition = partition;
      }
      return imagePartition != null ? imagePartition : throw new IUException("Request partition {0} cannot be found in the image", new object[1]
      {
        (object) Enum.GetName(typeof (MobileCorePartitionType), (object) type)
      });
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (this.IsMounted)
      {
        foreach (ImagePartition partition in this.Partitions)
          stringBuilder.AppendFormat("{0}, Root = {1}", (object) partition.Name, (object) partition.Root);
      }
      else
        stringBuilder.AppendLine("This image is not mounted");
      return stringBuilder.ToString();
    }

    public AclCollection GetFileSystemACLs()
    {
      ImagePartition partition = this.GetPartition(MobileCorePartitionType.System);
      bool flag = false;
      if (!this.IsMounted)
      {
        this.Mount();
        flag = true;
      }
      try
      {
        return SecurityUtils.GetFileSystemACLs(partition.Root);
      }
      finally
      {
        if (flag)
          this.Unmount();
      }
    }

    public AclCollection GetRegistryACLs()
    {
      ImagePartition partition = this.GetPartition(MobileCorePartitionType.System);
      bool flag = false;
      if (!this.IsMounted)
      {
        this.Mount();
        flag = true;
      }
      AclCollection registryAcLs = (AclCollection) null;
      try
      {
        registryAcLs = new AclCollection();
        string hiveRoot = Path.Combine(partition.Root, "Windows\\System32\\Config");
        registryAcLs.UnionWith((IEnumerable<ResourceAcl>) SecurityUtils.GetRegistryACLs(hiveRoot));
      }
      finally
      {
        if (flag)
          this.Unmount();
      }
      return registryAcLs;
    }
  }
}
