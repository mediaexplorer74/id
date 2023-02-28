// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WebLinkSettings
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.IO;
using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WebLinkSettings : PinnedAppSettings
  {
    public static readonly DependencyProperty WeblinkURLProperty = DependencyProperty.Register(nameof (WeblinkURL), typeof (string), typeof (WebLinkSettings), new PropertyMetadata((object) string.Empty, new PropertyChangedCallback(WebLinkSettings.OnURLChanged)));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof (Title), typeof (string), typeof (PinnedAppSettingsVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(nameof (BackgroundColor), typeof (string), typeof (PinnedAppSettingsVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty WebLinkSmallIconDisplayProperty = DependencyProperty.Register(nameof (WebLinkSmallIconDisplay), typeof (string), typeof (PinnedAppSettingsVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty WebLinkSmallIconProperty = DependencyProperty.Register(nameof (WebLinkSmallIcon), typeof (string), typeof (PinnedAppSettingsVM), new PropertyMetadata((object) string.Empty, new PropertyChangedCallback(WebLinkSettings.OnSmallIconChanged)));
    public static readonly DependencyProperty WebLinkMediumLargeIconDisplayProperty = DependencyProperty.Register(nameof (WebLinkMediumLargeIconDisplay), typeof (string), typeof (PinnedAppSettingsVM), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty WebLinkMediumLargeIconProperty = DependencyProperty.Register(nameof (WebLinkMediumLargeIcon), typeof (string), typeof (PinnedAppSettingsVM), new PropertyMetadata((object) string.Empty, new PropertyChangedCallback(WebLinkSettings.OnLargeIconChanged)));

    public WebLinkSettings(PinnedAppSettings oldSettings, AppType type)
      : base(type)
    {
      if (oldSettings == null)
        return;
      this.CoordinateX = oldSettings.CoordinateX;
      this.CoordinateY = oldSettings.CoordinateY;
      this.TileSize = oldSettings.TileSize;
    }

    [Setting("StartPrepinnedWebLinkURL")]
    public string WeblinkURL
    {
      get => (string) this.GetValue(WebLinkSettings.WeblinkURLProperty);
      set => this.SetValue(WebLinkSettings.WeblinkURLProperty, (object) value);
    }

    protected static void OnURLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((PinnedAppSettings) d).Name = e.NewValue as string;

    [Setting("StartPrepinnedWebLinkTileTitle")]
    public string Title
    {
      get => (string) this.GetValue(WebLinkSettings.TitleProperty);
      set => this.SetValue(WebLinkSettings.TitleProperty, (object) value);
    }

    [Setting("StartPrepinnedWebLinkTileBackgroundColor")]
    public string BackgroundColor
    {
      get => (string) this.GetValue(WebLinkSettings.BackgroundColorProperty);
      set => this.SetValue(WebLinkSettings.BackgroundColorProperty, (object) value);
    }

    public string WebLinkSmallIconDisplay
    {
      get => (string) this.GetValue(WebLinkSettings.WebLinkSmallIconDisplayProperty);
      set => this.SetValue(WebLinkSettings.WebLinkSmallIconDisplayProperty, (object) value);
    }

    [Setting("StartPrepinnedWebLinkTileSmallIcon")]
    public string WebLinkSmallIcon
    {
      get => (string) this.GetValue(WebLinkSettings.WebLinkSmallIconProperty);
      set => this.SetValue(WebLinkSettings.WebLinkSmallIconProperty, (object) value);
    }

    private static void OnSmallIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as WebLinkSettings).WebLinkSmallIconDisplay = Path.GetFileName(e.NewValue as string);

    public string WebLinkMediumLargeIconDisplay
    {
      get => (string) this.GetValue(WebLinkSettings.WebLinkMediumLargeIconDisplayProperty);
      set => this.SetValue(WebLinkSettings.WebLinkMediumLargeIconDisplayProperty, (object) value);
    }

    [Setting("StartPrepinnedWebLinkTileMediumLargeIcon")]
    public string WebLinkMediumLargeIcon
    {
      get => (string) this.GetValue(WebLinkSettings.WebLinkMediumLargeIconProperty);
      set => this.SetValue(WebLinkSettings.WebLinkMediumLargeIconProperty, (object) value);
    }

    private static void OnLargeIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as WebLinkSettings).WebLinkMediumLargeIconDisplay = Path.GetFileName(e.NewValue as string);
  }
}
