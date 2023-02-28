// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPDataAsset
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPDataAsset : WPListItemBase
  {
    public DataAsset DataAsset;
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof (Source), typeof (string), typeof (WPDataAsset), new PropertyMetadata((object) "", new PropertyChangedCallback(WPListItemBase.OnPropertyChanged)));

    public WPDataAsset(DataAsset dataAsset, bool isIncludedInOutput, WPListItemBase parent)
      : base(parent)
    {
      this.DataAsset = dataAsset;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.Source = this.DataAsset.Source;
      this.InitializationComplete();
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/resource_32xMD.png";

    public override string DisplayText => Path.GetFileName(this.Source);

    public override object GetPreviewItem(bool includeAllLevels, int level = 0) => (object) new DataAsset()
    {
      DefinedInFile = this.DataAsset.DefinedInFile,
      Source = this.Source
    };

    protected override void ValidateItem()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifyDataAsset(this.GetPreviewItem(false, 0) as DataAsset).ToList<CustomizationError>(), (WPListItemBase) this);
      base.ValidateItem();
    }

    protected override void UpdateIsDirty()
    {
      if (this.Source.Equals(this.DataAsset.Source) && !this.IsNewItem)
        this.IsDirty = false;
      else
        this.IsDirty = true;
    }

    public static WPListItemBase AddNewItem(WPListItemBase parent, string source)
    {
      WPDataAsset newItem = new WPDataAsset(new DataAsset()
      {
        Source = source
      }, false, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      return (WPListItemBase) newItem;
    }

    protected override bool SaveItem()
    {
      if (this.IsDirty)
        this.DataAsset.Source = this.Source;
      return base.SaveItem();
    }

    public string Source
    {
      get => (string) this.GetValue(WPDataAsset.SourceProperty);
      set => this.SetValue(WPDataAsset.SourceProperty, (object) value);
    }
  }
}
