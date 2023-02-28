// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.UserConfig
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class UserConfig
  {
    public const string REGKEY_USER_CONFIG_ROOT = "Software\\Microsoft\\Windows Phone Image Designer\\8.1";
    public const string CONFIG_LAST_SAVED_PROJECT = "LastSavedProject";
    public const string CONFIG_CONFIRM_STARTOVER = "ShowConfirmStartOverDialog";
    public const string CONFIG_SHOW_SAVE_SUCCESS_DIALOG = "ShowSaveSuccessDialog";
    public const string CONFIG_LAST_BSP_CONFIG_XML = "LastUsedBspConfig";
    public const string CONFIG_USER_LOCALE = "Locale";

    public static RegistryKey OpenUserSubKey()
    {
      using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
        return registryKey.CreateSubKey("Software\\Microsoft\\Windows Phone Image Designer\\8.1");
    }

    public static void SaveUserConfig(string valuename, string value) => UserConfig.SaveUserConfig(new List<Tuple<string, string>>()
    {
      Tuple.Create<string, string>(valuename, value)
    });

    public static void SaveUserConfig(List<Tuple<string, string>> config)
    {
      using (RegistryKey key = UserConfig.OpenUserSubKey())
      {
        if (key == null)
          return;
        config.ForEach((Action<Tuple<string, string>>) (t => key.SetValue(t.Item1, (object) t.Item2)));
      }
    }

    public static string GetUserConfig(string valuename)
    {
      string userConfig = (string) null;
      using (RegistryKey registryKey = UserConfig.OpenUserSubKey())
      {
        if (registryKey != null)
          userConfig = registryKey.GetValue(valuename, (object) string.Empty) as string;
      }
      return userConfig;
    }

    public static Dictionary<string, string> GetRegistryValues(params string[] config)
    {
      Dictionary<string, string> map = new Dictionary<string, string>();
      using (RegistryKey key = UserConfig.OpenUserSubKey())
      {
        if (key != null)
          ((IEnumerable<string>) config).ToList<string>().ForEach((Action<string>) (s => map[s] = key.GetValue(s, (object) string.Empty) as string));
      }
      return map;
    }
  }
}
