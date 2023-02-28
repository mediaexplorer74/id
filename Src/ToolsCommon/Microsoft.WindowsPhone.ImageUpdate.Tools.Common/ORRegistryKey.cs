// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.ORRegistryKey
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class ORRegistryKey : IDisposable
  {
    private const string STR_ROOT = "\\";
    private const string STR_NULLCHAR = "\0";
    private IntPtr m_handle = IntPtr.Zero;
    private string m_name = string.Empty;
    private bool m_isRoot;
    private ORRegistryKey m_parent;
    private readonly char[] BSLASH_DELIMITER = new char[1]
    {
      '\\'
    };
    private Dictionary<ORRegistryKey, bool> m_children = new Dictionary<ORRegistryKey, bool>();

    private ORRegistryKey(string name, IntPtr handle, bool isRoot, ORRegistryKey parent)
    {
      this.m_name = name;
      this.m_handle = handle;
      this.m_isRoot = isRoot;
      this.m_parent = parent;
      if (this.m_parent == null)
        return;
      this.m_parent.m_children[this] = true;
    }

    ~ORRegistryKey() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      foreach (ORRegistryKey key in this.m_children.Keys)
        key.Close();
      this.m_children.Clear();
      if (this.m_parent == null)
        return;
      this.m_parent.m_children.Remove(this);
    }

    public static ORRegistryKey OpenHive(string hivefile, string prefix = null)
    {
      if (prefix == null)
        prefix = "\\";
      return new ORRegistryKey(prefix, OfflineRegUtils.OpenHive(hivefile), true, (ORRegistryKey) null);
    }

    public static ORRegistryKey CreateEmptyHive(string prefix = null) => new ORRegistryKey(string.IsNullOrEmpty(prefix) ? "\\" : prefix, OfflineRegUtils.CreateHive(), true, (ORRegistryKey) null);

    public ORRegistryKey Parent => this.m_parent;

    public string[] SubKeys => OfflineRegUtils.GetSubKeys(this.m_handle);

    public string FullName => this.m_name;

    public string Class => OfflineRegUtils.GetClass(this.m_handle);

    public string[] ValueNames => OfflineRegUtils.GetValueNames(this.m_handle);

    public List<KeyValuePair<string, RegistryValueType>> ValueNameAndTypes => OfflineRegUtils.GetValueNamesAndTypes(this.m_handle);

    public RegistrySecurity RegistrySecurity => OfflineRegUtils.GetRegistrySecurity(this.m_handle);

    public ORRegistryKey OpenSubKey(string subkeyname)
    {
      ORRegistryKey orRegistryKey1;
      if (-1 < subkeyname.IndexOf("\\"))
      {
        string[] strArray = subkeyname.Split(this.BSLASH_DELIMITER);
        ORRegistryKey orRegistryKey2 = this;
        ORRegistryKey orRegistryKey3 = (ORRegistryKey) null;
        foreach (string subkeyname1 in strArray)
        {
          orRegistryKey3 = orRegistryKey2.OpenSubKey(subkeyname1);
          orRegistryKey2 = orRegistryKey3;
        }
        orRegistryKey1 = orRegistryKey3;
      }
      else
      {
        IntPtr handle = OfflineRegUtils.OpenKey(this.m_handle, subkeyname);
        orRegistryKey1 = new ORRegistryKey(this.CombineSubKeys(this.m_name, subkeyname), handle, false, this);
      }
      return orRegistryKey1;
    }

    public RegistryValueType GetValueKind(string valueName) => OfflineRegUtils.GetValueType(this.m_handle, valueName);

    public byte[] GetByteValue(string valueName) => OfflineRegUtils.GetValue(this.m_handle, valueName);

    [CLSCompliant(false)]
    public uint GetDwordValue(string valueName)
    {
      byte[] byteValue = this.GetByteValue(valueName);
      return byteValue.Length != 0 ? BitConverter.ToUInt32(byteValue, 0) : 0U;
    }

    [CLSCompliant(false)]
    public ulong GetQwordValue(string valueName)
    {
      byte[] byteValue = this.GetByteValue(valueName);
      return byteValue.Length != 0 ? BitConverter.ToUInt64(byteValue, 0) : 0UL;
    }

    public string GetStringValue(string valueName)
    {
      byte[] byteValue = this.GetByteValue(valueName);
      string empty = string.Empty;
      return byteValue.Length <= 1 || byteValue[byteValue.Length - 1] != (byte) 0 || byteValue[byteValue.Length - 2] != (byte) 0 ? Encoding.Unicode.GetString(byteValue) : Encoding.Unicode.GetString(byteValue, 0, byteValue.Length - 2);
    }

    public string[] GetMultiStringValue(string valueName) => Encoding.Unicode.GetString(this.GetByteValue(valueName)).Split(new char[1], StringSplitOptions.RemoveEmptyEntries);

    public object GetValue(string valueName)
    {
      RegistryValueType valueKind = this.GetValueKind(valueName);
      object obj = (object) null;
      switch (valueKind)
      {
        case RegistryValueType.None:
          obj = (object) this.GetByteValue(valueName);
          break;
        case RegistryValueType.String:
          obj = (object) this.GetStringValue(valueName);
          break;
        case RegistryValueType.ExpandString:
          obj = (object) this.GetStringValue(valueName);
          break;
        case RegistryValueType.Binary:
          obj = (object) this.GetByteValue(valueName);
          break;
        case RegistryValueType.DWord:
          obj = (object) this.GetDwordValue(valueName);
          break;
        case RegistryValueType.DWordBigEndian:
          obj = (object) this.GetByteValue(valueName);
          break;
        case RegistryValueType.Link:
          obj = (object) this.GetByteValue(valueName);
          break;
        case RegistryValueType.MultiString:
          obj = (object) this.GetMultiStringValue(valueName);
          break;
        case RegistryValueType.RegResourceList:
          obj = (object) this.GetByteValue(valueName);
          break;
        case RegistryValueType.RegFullResourceDescriptor:
          obj = (object) this.GetByteValue(valueName);
          break;
        case RegistryValueType.RegResourceRequirementsList:
          obj = (object) this.GetByteValue(valueName);
          break;
        case RegistryValueType.QWord:
          obj = (object) this.GetQwordValue(valueName);
          break;
      }
      return obj;
    }

    public void SaveHive(string path)
    {
      if (!this.m_isRoot)
        throw new IUException("Invalid operation - This registry key does not represent hive root");
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException(nameof (path));
      OfflineRegUtils.SaveHive(this.m_handle, path, 6, 3);
    }

    public ORRegistryKey CreateSubKey(string subkeyName)
    {
      ORRegistryKey subKey;
      if (-1 != subkeyName.IndexOf("\\"))
      {
        string[] strArray = subkeyName.Split(this.BSLASH_DELIMITER, StringSplitOptions.RemoveEmptyEntries);
        ORRegistryKey orRegistryKey1 = this;
        ORRegistryKey orRegistryKey2 = (ORRegistryKey) null;
        foreach (string subkeyName1 in strArray)
        {
          orRegistryKey2 = orRegistryKey1.CreateSubKey(subkeyName1);
          orRegistryKey1 = orRegistryKey2;
        }
        subKey = orRegistryKey2;
      }
      else
      {
        IntPtr key = OfflineRegUtils.CreateKey(this.m_handle, subkeyName);
        subKey = new ORRegistryKey(this.CombineSubKeys(this.m_name, subkeyName), key, false, this);
      }
      return subKey;
    }

    public void SetValue(string valueName, byte[] value) => OfflineRegUtils.SetValue(this.m_handle, valueName, RegistryValueType.Binary, value);

    public void SetValue(string valueName, string value)
    {
      byte[] bytes = Encoding.Unicode.GetBytes(value);
      OfflineRegUtils.SetValue(this.m_handle, valueName, RegistryValueType.String, bytes);
    }

    public void SetValue(string valueName, string[] values)
    {
      if (values == null)
        throw new ArgumentNullException(nameof (values));
      StringBuilder stringBuilder = new StringBuilder(1024);
      foreach (string str in values)
        stringBuilder.AppendFormat("{0}{1}", (object) str, (object) "\0");
      stringBuilder.Append("\0");
      byte[] bytes = Encoding.Unicode.GetBytes(stringBuilder.ToString());
      OfflineRegUtils.SetValue(this.m_handle, valueName, RegistryValueType.MultiString, bytes);
    }

    public void SetValue(string valueName, int value)
    {
      byte[] bytes = BitConverter.GetBytes(value);
      OfflineRegUtils.SetValue(this.m_handle, valueName, RegistryValueType.DWord, bytes);
    }

    public void SetValue(string valueName, long value)
    {
      byte[] bytes = BitConverter.GetBytes(value);
      OfflineRegUtils.SetValue(this.m_handle, valueName, RegistryValueType.QWord, bytes);
    }

    public void DeleteValue(string valueName) => OfflineRegUtils.DeleteValue(this.m_handle, valueName);

    public void DeleteKey(string keyName) => OfflineRegUtils.DeleteKey(this.m_handle, keyName);

    private string CombineSubKeys(string path1, string path2)
    {
      if (path1 == null)
        throw new ArgumentNullException("first");
      if (path2 == null)
        throw new ArgumentNullException("second");
      if (-1 < path2.IndexOf("\\") || path1.Length == 0)
        return path2;
      if (path2.Length == 0)
        return path1;
      return path1.Length == path1.LastIndexOfAny(this.BSLASH_DELIMITER) + 1 ? path1 + path2 : path1 + (object) this.BSLASH_DELIMITER[0] + path2;
    }

    private void Close()
    {
      if (!(this.m_handle != IntPtr.Zero))
        return;
      if (this.m_isRoot)
        OfflineRegUtils.CloseHive(this.m_handle);
      else
        OfflineRegUtils.CloseKey(this.m_handle);
      this.m_handle = IntPtr.Zero;
    }
  }
}
