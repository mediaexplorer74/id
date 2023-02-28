// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPApplicationsGroups
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPApplicationsGroups : WPListItemBase
  {
    public List<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications> Applications;
    public static readonly DependencyProperty EnableAppPinningProperty = DependencyProperty.Register(nameof (EnableAppPinning), typeof (bool), typeof (WPApplicationsGroups), new PropertyMetadata((object) false));

    public WPApplicationsGroups(
      List<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications> applications,
      bool isIncludedInOutput,
      WPListItemBase parent)
      : base(parent)
    {
      this.Applications = applications;
      this.IsIncludedInOutput = isIncludedInOutput;
      this.ToolTip = Tools.GetString("ttCOSApplications");
      ObservableCollection<WPListItemBase> collection = new ObservableCollection<WPListItemBase>();
      foreach (Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications application1 in applications)
      {
        foreach (Application application2 in application1.Items)
          collection.Add((WPListItemBase) new WPApplication(application2, isIncludedInOutput, (WPListItemBase) this));
      }
      this.Children = new ObservableCollection<WPListItemBase>((IEnumerable<WPListItemBase>) collection);
      this.EnableAppPinning = this.CurrentVariant.IsStaticVariant;
      this.InitializationComplete();
    }

    public bool EnableAppPinning
    {
      get => (bool) this.GetValue(WPApplicationsGroups.EnableAppPinningProperty);
      set => this.SetValue(WPApplicationsGroups.EnableAppPinningProperty, (object) value);
    }

    public override string TreeViewIcon => "/ImageDesigner;component/Resources/Images/folder_closed_32xMD.png";

    public override string DisplayText => Tools.GetString("txtCOSApplications");

    public List<string> InUseSourceFilenames
    {
      get
      {
        List<string> useSourceFilenames = new List<string>();
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          WPApplication wpApplication = child as WPApplication;
          useSourceFilenames.Add(Path.GetFileName(wpApplication.Source));
        }
        return useSourceFilenames;
      }
    }

    public override object GetPreviewItem(bool includeAllLevels, int level = 0)
    {
      List<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications> previewItem1 = new List<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications>();
      Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications applications = new Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications();
      applications.Items = new List<Application>();
      if (this.Children != null && (includeAllLevels || !includeAllLevels && level <= 1))
      {
        ++level;
        foreach (WPListItemBase child in (Collection<WPListItemBase>) this.Children)
        {
          Application previewItem2 = child.GetPreviewItem(includeAllLevels, level) as Application;
          applications.Items.Add(previewItem2);
        }
      }
      previewItem1.Add(applications);
      return (object) previewItem1;
    }

    protected override void ValidateItem()
    {
      if (this.IsIncludedInOutput || this.IsDirty || this.IsNewItem)
      {
        bool includChildren = true;
        bool flag = false;
        WPVariant parent = this.Parent as WPVariant;
        List<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications> previewItem = this.GetPreviewItem(includChildren, 0) as List<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications>;
        this._validationIssues = new WPErrors(CustomContentGenerator.VerifyApplicationsGroup((IEnumerable<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications>) previewItem, parent.VariantItem, flag).ToList<CustomizationError>(), (WPListItemBase) this);
        foreach (Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications applications in previewItem)
          this._validationIssues.AddRange(CustomContentGenerator.VerifyApplications(applications, parent.VariantItem, flag).ToList<CustomizationError>());
      }
      base.ValidateItem();
    }

    public static WPListItemBase AddNewItem(WPListItemBase parent)
    {
      WPApplicationsGroups newItem = new WPApplicationsGroups(new List<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications>(), false, parent);
      WPListItemBase.AddNewItem(parent, (WPListItemBase) newItem);
      return (WPListItemBase) newItem;
    }

    protected override bool SaveItem()
    {
      this.Applications.Clear();
      if (this.Children != null)
        this.Applications.AddRange((IEnumerable<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications>) (this.GetPreviewItem(true, 0) as List<Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Applications>));
      return base.SaveItem();
    }
  }
}
