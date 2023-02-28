// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.PinnedAppSettings
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class PinnedAppSettings : PinVMBase
  {
    public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof (Name), typeof (string), typeof (PinnedAppSettings), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty SelectedAppIndexProperty = DependencyProperty.Register(nameof (SelectedAppIndex), typeof (int), typeof (PinnedAppSettings), new PropertyMetadata((object) 0));
    public static readonly DependencyProperty AppTypeProperty = DependencyProperty.Register(nameof (AppType), typeof (AppType), typeof (PinnedAppSettings), new PropertyMetadata((object) AppType.OEMApplication, new PropertyChangedCallback(PinnedAppSettings.OnAppTypeChanged)));
    public static readonly DependencyProperty LocalizedAppTypeProperty = DependencyProperty.Register(nameof (LocalizedAppType), typeof (string), typeof (PinnedAppSettings), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty TileSizeProperty = DependencyProperty.Register(nameof (TileSize), typeof (AppTileSize), typeof (PinnedAppSettings), new PropertyMetadata((object) AppTileSize.NotConfigured, new PropertyChangedCallback(PinnedAppSettings.OnTileSizeChanged)));
    public static readonly DependencyProperty LocalizedTileSizeProperty = DependencyProperty.Register(nameof (LocalizedTileSize), typeof (string), typeof (PinnedAppSettings), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty SixColTileSizeProperty = DependencyProperty.Register(nameof (SixColTileSize), typeof (AppTileSize), typeof (PinnedAppSettings), new PropertyMetadata((object) AppTileSize.NotConfigured, new PropertyChangedCallback(PinnedAppSettings.OnSixColTileSizeChanged)));
    public static readonly DependencyProperty SixColLocalizedTileSizeProperty = DependencyProperty.Register(nameof (SixColLocalizedTileSize), typeof (string), typeof (PinnedAppSettings), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty AppTileSizeListProperty = DependencyProperty.Register(nameof (AppTileSizeList), typeof (List<EnumWrapper>), typeof (PinnedAppSettings), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty CoordinateXProperty = DependencyProperty.Register(nameof (CoordinateX), typeof (string), typeof (PinnedAppSettings), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty CoordinateYProperty = DependencyProperty.Register(nameof (CoordinateY), typeof (string), typeof (PinnedAppSettings), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty SixColCoordinateXProperty = DependencyProperty.Register(nameof (SixColCoordinateX), typeof (string), typeof (PinnedAppSettings), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty SixColCoordinateYProperty = DependencyProperty.Register(nameof (SixColCoordinateY), typeof (string), typeof (PinnedAppSettings), new PropertyMetadata((object) string.Empty));

    private PinnedAppSettings()
    {
    }

    public PinnedAppSettings(AppType type)
    {
      this.AppType = type;
      this.AppTileSizeList = EnumWrapper.GetEnumList(typeof (AppTileSize));
      this.LocalizedAppType = Tools.GetEnumLocalizedString((Enum) this.AppType);
      this.LocalizedTileSize = this.TileSize == AppTileSize.NotConfigured ? string.Empty : Tools.GetEnumLocalizedString((Enum) this.TileSize);
      this.SixColLocalizedTileSize = this.SixColTileSize == AppTileSize.NotConfigured ? string.Empty : Tools.GetEnumLocalizedString((Enum) this.SixColTileSize);
    }

    public string Name
    {
      get => (string) this.GetValue(PinnedAppSettings.NameProperty);
      set => this.SetValue(PinnedAppSettings.NameProperty, (object) value);
    }

    public int SelectedAppIndex
    {
      get => (int) this.GetValue(PinnedAppSettings.SelectedAppIndexProperty);
      set => this.SetValue(PinnedAppSettings.SelectedAppIndexProperty, (object) value);
    }

    public AppType AppType
    {
      get => (AppType) this.GetValue(PinnedAppSettings.AppTypeProperty);
      set => this.SetValue(PinnedAppSettings.AppTypeProperty, (object) value);
    }

    private static void OnAppTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as PinnedAppSettings).LocalizedAppType = Tools.GetEnumLocalizedString((Enum) (AppType) e.NewValue);

    public string LocalizedAppType
    {
      get => (string) this.GetValue(PinnedAppSettings.LocalizedAppTypeProperty);
      set => this.SetValue(PinnedAppSettings.LocalizedAppTypeProperty, (object) value);
    }

    [Setting("StartPrepinnedTileSize")]
    public AppTileSize TileSize
    {
      get => (AppTileSize) this.GetValue(PinnedAppSettings.TileSizeProperty);
      set => this.SetValue(PinnedAppSettings.TileSizeProperty, (object) value);
    }

    private static void OnTileSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      PinnedAppSettings pinnedAppSettings = d as PinnedAppSettings;
      if ((AppTileSize) e.NewValue == AppTileSize.NotConfigured)
        pinnedAppSettings.LocalizedTileSize = string.Empty;
      else
        pinnedAppSettings.LocalizedTileSize = Tools.GetEnumLocalizedString((Enum) (AppTileSize) e.NewValue);
    }

    public string LocalizedTileSize
    {
      get => (string) this.GetValue(PinnedAppSettings.LocalizedTileSizeProperty);
      set => this.SetValue(PinnedAppSettings.LocalizedTileSizeProperty, (object) value);
    }

    [Setting("6ColumnStartPrepinnedTileSize")]
    public AppTileSize SixColTileSize
    {
      get => (AppTileSize) this.GetValue(PinnedAppSettings.SixColTileSizeProperty);
      set => this.SetValue(PinnedAppSettings.SixColTileSizeProperty, (object) value);
    }

    private static void OnSixColTileSizeChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      PinnedAppSettings pinnedAppSettings = d as PinnedAppSettings;
      if ((AppTileSize) e.NewValue == AppTileSize.NotConfigured)
        pinnedAppSettings.SixColLocalizedTileSize = string.Empty;
      else
        pinnedAppSettings.SixColLocalizedTileSize = Tools.GetEnumLocalizedString((Enum) (AppTileSize) e.NewValue);
    }

    public string SixColLocalizedTileSize
    {
      get => (string) this.GetValue(PinnedAppSettings.SixColLocalizedTileSizeProperty);
      set => this.SetValue(PinnedAppSettings.SixColLocalizedTileSizeProperty, (object) value);
    }

    public List<EnumWrapper> AppTileSizeList
    {
      get => (List<EnumWrapper>) this.GetValue(PinnedAppSettings.AppTileSizeListProperty);
      set => this.SetValue(PinnedAppSettings.AppTileSizeListProperty, (object) value);
    }

    [Setting("StartPrepinnedTileXCoordinate")]
    public string CoordinateX
    {
      get => (string) this.GetValue(PinnedAppSettings.CoordinateXProperty);
      set => this.SetValue(PinnedAppSettings.CoordinateXProperty, (object) value);
    }

    [Setting("StartPrepinnedTileYCoordinate")]
    public string CoordinateY
    {
      get => (string) this.GetValue(PinnedAppSettings.CoordinateYProperty);
      set => this.SetValue(PinnedAppSettings.CoordinateYProperty, (object) value);
    }

    [Setting("6ColumnStartPrepinnedTileXCoordinate")]
    public string SixColCoordinateX
    {
      get => (string) this.GetValue(PinnedAppSettings.SixColCoordinateXProperty);
      set => this.SetValue(PinnedAppSettings.SixColCoordinateXProperty, (object) value);
    }

    [Setting("6ColumnStartPrepinnedTileYCoordinate")]
    public string SixColCoordinateY
    {
      get => (string) this.GetValue(PinnedAppSettings.SixColCoordinateYProperty);
      set => this.SetValue(PinnedAppSettings.SixColCoordinateYProperty, (object) value);
    }
  }
}
