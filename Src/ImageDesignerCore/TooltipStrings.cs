// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.TooltipStrings
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class TooltipStrings
  {
    private static TooltipStrings _instance;
    private static ResourceManager _resourceManager;
    private Dictionary<string, string> _map = new Dictionary<string, string>();

    private TooltipStrings()
    {
    }

    public static TooltipStrings Instance
    {
      get
      {
        if (TooltipStrings._instance == null)
          TooltipStrings._instance = new TooltipStrings();
        return TooltipStrings._instance;
      }
    }

    public string this[string name]
    {
      get
      {
        string str = string.Empty;
        if (!this._map.TryGetValue(name, out str))
          str = this.LoadTooltipFromResource(name);
        return str;
      }
    }

    public static ResourceManager ResourceManager
    {
      get
      {
        if (TooltipStrings._resourceManager == null)
        {
          Assembly assembly = typeof (TooltipBehavior).Assembly;
          TooltipStrings._resourceManager = new ResourceManager(Constants.TOOLTIPS_RESOURCE_NAMESPACE, assembly);
        }
        return TooltipStrings._resourceManager;
      }
    }

    public string LoadTooltipFromResource(string name)
    {
      string str = string.Empty;
      if (!string.IsNullOrWhiteSpace(name))
      {
        str = TooltipStrings.GetTooltipFromResource(name);
        if (!string.IsNullOrWhiteSpace(str))
          this._map[name] = str;
      }
      return str;
    }

    public static string GetTooltipFromResource(string name)
    {
      string empty = string.Empty;
      ResourceManager resourceManager = TooltipStrings.ResourceManager;
      if (resourceManager != null)
        empty = resourceManager.GetString(name);
      return empty;
    }
  }
}
