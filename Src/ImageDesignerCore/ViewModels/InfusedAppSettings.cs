// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.InfusedAppSettings
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Application = Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Application;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class InfusedAppSettings : PinnedAppSettings
  {
    public static readonly DependencyProperty AvailableAppsProperty = DependencyProperty.Register(nameof (AvailableApps), typeof (ObservableCollection<string>), typeof (PinnedAppSettingsVM), new PropertyMetadata((PropertyChangedCallback) null));

    public InfusedAppSettings(
      PinnedAppSettings oldSettings,
      AppType type,
      IEnumerable<Application> availableOEMApps,
      IEnumerable<EnumWrapper> availableMSApps)
      : base(type)
    {
      if (oldSettings != null)
      {
        this.CoordinateX = oldSettings.CoordinateX;
        this.CoordinateY = oldSettings.CoordinateY;
        this.TileSize = oldSettings.TileSize;
      }
      switch (type)
      {
        case AppType.OEMApplication:
          this.AvailableApps = new ObservableCollection<string>();
          using (IEnumerator<Application> enumerator = availableOEMApps.GetEnumerator())
          {
            while (((IEnumerator) enumerator).MoveNext())
              this.AvailableApps.Add(Tools.GetApplicationName(enumerator.Current));
            break;
          }
        case AppType.MSApplication:
          this.AvailableApps = new ObservableCollection<string>(availableMSApps.Select<EnumWrapper, string>((Func<EnumWrapper, string>) (app => app.DisplayText)));
          break;
      }
    }

    public ObservableCollection<string> AvailableApps
    {
      get => (ObservableCollection<string>) this.GetValue(InfusedAppSettings.AvailableAppsProperty);
      set => this.SetValue(InfusedAppSettings.AvailableAppsProperty, (object) value);
    }
  }
}
