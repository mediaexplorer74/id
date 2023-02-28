// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.OfflineRegUtils
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class OfflineRegUtils
  {
    private static readonly char[] BSLASH_DELIMITER = new char[1]
    {
      '\\'
    };

    public static IntPtr CreateHive()
    {
      IntPtr zero = IntPtr.Zero;
      int hive = OffRegNativeMethods.ORCreateHive(ref zero);
      if (hive != 0)
        throw new Win32Exception(hive);
      return zero;
    }

    public static IntPtr CreateKey(IntPtr handle, string keyName)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      if (string.IsNullOrEmpty(nameof (keyName)))
        throw new ArgumentNullException(nameof (keyName));
      IntPtr zero = IntPtr.Zero;
      uint dwDisposition = 0;
      foreach (string str in keyName.Split(OfflineRegUtils.BSLASH_DELIMITER))
      {
        int key = OffRegNativeMethods.ORCreateKey(handle, keyName, (string) null, 0U, (byte[]) null, ref zero, ref dwDisposition);
        if (key != 0)
          throw new Win32Exception(key);
      }
      return zero;
    }

    public static void SetValue(
      IntPtr handle,
      string valueName,
      RegistryValueType type,
      byte[] value)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      if (valueName == null)
        valueName = string.Empty;
      int error = OffRegNativeMethods.ORSetValue(handle, valueName, (uint) type, value, (uint) value.Length);
      if (error != 0)
        throw new Win32Exception(error);
    }

    public static void DeleteValue(IntPtr handle, string valueName)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      if (valueName == null)
        valueName = string.Empty;
      int error = OffRegNativeMethods.ORDeleteValue(handle, valueName);
      if (error != 0)
        throw new Win32Exception(error);
    }

    public static void DeleteKey(IntPtr handle, string keyName)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      int error = keyName != null ? OffRegNativeMethods.ORDeleteKey(handle, keyName) : throw new ArgumentNullException(nameof (keyName));
      if (error != 0)
        throw new Win32Exception(error);
    }

    public static IntPtr OpenHive(string hivefile)
    {
      if (string.IsNullOrEmpty(hivefile))
        throw new ArgumentNullException(nameof (hivefile));
      IntPtr zero = IntPtr.Zero;
      int error = OffRegNativeMethods.OROpenHive(hivefile, ref zero);
      if (error != 0)
        throw new Win32Exception(error);
      return zero;
    }

    public static void SaveHive(IntPtr handle, string path, int osMajor, int osMinor)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException(nameof (path));
      if (File.Exists(path))
        FileUtils.DeleteFile(path);
      int error = OffRegNativeMethods.ORSaveHive(handle, path, osMajor, osMinor);
      if (error != 0)
        throw new Win32Exception(error);
    }

    public static void CloseHive(IntPtr handle)
    {
      int error = !(handle == IntPtr.Zero) ? OffRegNativeMethods.ORCloseHive(handle) : throw new ArgumentNullException(nameof (handle));
      if (error != 0)
        throw new Win32Exception(error);
    }

    public static IntPtr OpenKey(IntPtr handle, string subKeyName)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      if (string.IsNullOrEmpty(nameof (subKeyName)))
        throw new ArgumentNullException(nameof (subKeyName));
      IntPtr zero = IntPtr.Zero;
      int error = OffRegNativeMethods.OROpenKey(handle, subKeyName, ref zero);
      if (error != 0)
        throw new Win32Exception(error);
      return zero;
    }

    public static void CloseKey(IntPtr handle)
    {
      int error = !(handle == IntPtr.Zero) ? OffRegNativeMethods.ORCloseKey(handle) : throw new ArgumentNullException(nameof (handle));
      if (error != 0)
        throw new Win32Exception(error);
    }

    public static void ConvertHiveToReg(string inputHiveFile, string outputRegFile) => new HiveToRegConverter(inputHiveFile).ConvertToReg(outputRegFile);

    public static void ConvertHiveToReg(
      string inputHiveFile,
      string outputRegFile,
      string keyPrefix)
    {
      new HiveToRegConverter(inputHiveFile, keyPrefix).ConvertToReg(outputRegFile);
    }

    public static void ConvertHiveToReg(
      string inputHiveFile,
      string outputRegFile,
      string keyPrefix,
      bool appendExisting)
    {
      new HiveToRegConverter(inputHiveFile, keyPrefix).ConvertToReg(outputRegFile, append: appendExisting);
    }

    public static string ConvertByteArrayToRegStrings(byte[] data) => OfflineRegUtils.ConvertByteArrayToRegStrings(data, 40);

    public static string ConvertByteArrayToRegStrings(byte[] data, int maxOnALine)
    {
      string empty = string.Empty;
      string regStrings;
      if (-1 == maxOnALine)
      {
        regStrings = BitConverter.ToString(data).Replace('-', ',');
      }
      else
      {
        int startIndex = 0;
        int length1 = data.Length;
        StringBuilder stringBuilder = new StringBuilder();
        while (length1 > 0)
        {
          int length2 = length1 > maxOnALine ? maxOnALine : length1;
          string str1 = BitConverter.ToString(data, startIndex, length2);
          startIndex += length2;
          length1 -= length2;
          string str2 = str1.Replace('-', ',');
          stringBuilder.Append(str2);
          if (length1 > 0)
          {
            stringBuilder.Append(",\\");
            stringBuilder.AppendLine();
          }
        }
        regStrings = stringBuilder.ToString();
      }
      return regStrings;
    }

    public static RegistryValueType GetValueType(IntPtr handle, string valueName)
    {
      uint pdwType = 0;
      uint pcbData = 0;
      int error = OffRegNativeMethods.ORGetValue(handle, (string) null, valueName, out pdwType, (byte[]) null, ref pcbData);
      if (error != 0)
        throw new Win32Exception(error);
      return (RegistryValueType) pdwType;
    }

    public static List<KeyValuePair<string, RegistryValueType>> GetValueNamesAndTypes(
      IntPtr handle)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      uint index = 0;
      StringBuilder lpValueName = new StringBuilder(1024);
      List<string> stringList = new List<string>();
      List<KeyValuePair<string, RegistryValueType>> valueNamesAndTypes = new List<KeyValuePair<string, RegistryValueType>>();
      int error;
      do
      {
        uint capacity = (uint) lpValueName.Capacity;
        uint lpType = 0;
        error = OffRegNativeMethods.OREnumValue(handle, index, lpValueName, ref capacity, out lpType, IntPtr.Zero, IntPtr.Zero);
        switch (error)
        {
          case 0:
            string key = lpValueName.ToString();
            RegistryValueType registryValueType = (RegistryValueType) lpType;
            valueNamesAndTypes.Add(new KeyValuePair<string, RegistryValueType>(key, registryValueType));
            ++index;
            goto case 259;
          case 259:
            continue;
          default:
            throw new Win32Exception(error);
        }
      }
      while (error != 259);
      return valueNamesAndTypes;
    }

    public static string[] GetValueNames(IntPtr handle)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      return OfflineRegUtils.GetValueNamesAndTypes(handle).Select<KeyValuePair<string, RegistryValueType>, string>((Func<KeyValuePair<string, RegistryValueType>, string>) (a => a.Key)).ToArray<string>();
    }

    public static string GetClass(IntPtr handle)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      StringBuilder classname = new StringBuilder(128);
      uint capacity = (uint) classname.Capacity;
      uint[] numArray = new uint[8];
      IntPtr zero = IntPtr.Zero;
      int error = OffRegNativeMethods.ORQueryInfoKey(handle, classname, ref capacity, out numArray[0], out numArray[1], out numArray[3], out numArray[4], out numArray[5], out numArray[6], out numArray[7], zero);
      if (error == 234)
      {
        uint lpcClass = capacity + 1U;
        classname.Capacity = (int) lpcClass;
        error = OffRegNativeMethods.ORQueryInfoKey(handle, classname, ref lpcClass, out numArray[0], out numArray[1], out numArray[3], out numArray[4], out numArray[5], out numArray[6], out numArray[7], zero);
      }
      if (error != 0)
        throw new Win32Exception(error);
      return classname.ToString();
    }

    public static byte[] GetValue(IntPtr handle, string valueName)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      uint pdwType = 0;
      uint pcbData = 0;
      int error1 = OffRegNativeMethods.ORGetValue(handle, (string) null, valueName, out pdwType, (byte[]) null, ref pcbData);
      if (error1 != 0)
        throw new Win32Exception(error1);
      byte[] pvData = new byte[(int)(IntPtr) pcbData];
      int error2 = OffRegNativeMethods.ORGetValue(handle, (string) null, valueName, out pdwType, pvData, ref pcbData);
      if (error2 != 0)
        throw new Win32Exception(error2);
      return pvData;
    }

    public static string[] GetSubKeys(IntPtr registryKey)
    {
      if (registryKey == IntPtr.Zero)
        throw new ArgumentNullException("handle");
      uint dwIndex = 0;
      StringBuilder name = new StringBuilder(1024);
      List<string> stringList = new List<string>();
      int error;
      do
      {
        uint classnamecount = 0;
        IntPtr zero = IntPtr.Zero;
        uint capacity = (uint) name.Capacity;
        error = OffRegNativeMethods.OREnumKey(registryKey, dwIndex, name, ref capacity, (StringBuilder) null, ref classnamecount, ref zero);
        switch (error)
        {
          case 0:
            stringList.Add(name.ToString());
            ++dwIndex;
            goto case 259;
          case 259:
            continue;
          default:
            throw new Win32Exception(error);
        }
      }
      while (error != 259);
      return stringList.ToArray();
    }

    public static byte[] GetRawRegistrySecurity(IntPtr handle)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      uint size = 0;
      int num = 234;
      int keySecurity1 = OffRegNativeMethods.ORGetKeySecurity(handle, SecurityInformationFlags.DACL_SECURITY_INFORMATION | SecurityInformationFlags.SACL_SECURITY_INFORMATION | SecurityInformationFlags.MANDATORY_ACCESS_LABEL, (byte[]) null, ref size);
      if (num != keySecurity1)
        throw new Win32Exception(keySecurity1);
      byte[] lpSecBuf = new byte[(IntPtr) size];
      int keySecurity2 = OffRegNativeMethods.ORGetKeySecurity(handle, SecurityInformationFlags.DACL_SECURITY_INFORMATION | SecurityInformationFlags.SACL_SECURITY_INFORMATION | SecurityInformationFlags.MANDATORY_ACCESS_LABEL, lpSecBuf, ref size);
      if (keySecurity2 != 0)
        throw new Win32Exception(keySecurity2);
      return lpSecBuf;
    }

    public static void SetRawRegistrySecurity(IntPtr handle, byte[] buf)
    {
      int error = !(handle == IntPtr.Zero) ? OffRegNativeMethods.ORSetKeySecurity(handle, SecurityInformationFlags.DACL_SECURITY_INFORMATION | SecurityInformationFlags.SACL_SECURITY_INFORMATION | SecurityInformationFlags.MANDATORY_ACCESS_LABEL, buf) : throw new ArgumentNullException(nameof (handle));
      if (error != 0)
        throw new Win32Exception(error);
    }

    public static RegistrySecurity GetRegistrySecurity(IntPtr handle)
    {
      byte[] numArray = !(handle == IntPtr.Zero) ? OfflineRegUtils.GetRawRegistrySecurity(handle) : throw new ArgumentNullException(nameof (handle));
      SecurityUtils.ConvertSDToStringSD(numArray, SecurityInformationFlags.SACL_SECURITY_INFORMATION | SecurityInformationFlags.MANDATORY_ACCESS_LABEL);
      RegistrySecurity registrySecurity = new RegistrySecurity();
      registrySecurity.SetSecurityDescriptorBinaryForm(numArray);
      return registrySecurity;
    }

    public static int GetVirtualFlags(IntPtr handle)
    {
      if (handle == IntPtr.Zero)
        throw new ArgumentNullException(nameof (handle));
      int pbFlags = 0;
      OffRegNativeMethods.ORGetVirtualFlags(handle, ref pbFlags);
      return pbFlags;
    }

    public static int ExtractFromHive(string hivePath, RegistryValueType type, string targetPath)
    {
      if (string.IsNullOrEmpty(nameof (hivePath)))
        throw new ArgumentNullException(nameof (hivePath));
      if (string.IsNullOrEmpty(nameof (targetPath)))
        throw new ArgumentNullException(nameof (targetPath));
      if (!File.Exists(hivePath))
        throw new FileNotFoundException("Hive file {0} does not exist", hivePath);
      int fromHive = 0;
      bool flag = false;
      using (ORRegistryKey srcHiveRoot = ORRegistryKey.OpenHive(hivePath))
      {
        using (ORRegistryKey emptyHive = ORRegistryKey.CreateEmptyHive())
        {
          flag = 0 < (fromHive = OfflineRegUtils.ExtractFromHiveRecursive(srcHiveRoot, type, emptyHive));
          if (flag)
            emptyHive.SaveHive(targetPath);
        }
        if (flag)
          srcHiveRoot.SaveHive(hivePath);
      }
      return fromHive;
    }

    private static int ExtractFromHiveRecursive(
      ORRegistryKey srcHiveRoot,
      RegistryValueType type,
      ORRegistryKey dstHiveRoot)
    {
      int fromHiveRecursive = 0;
      string fullName = srcHiveRoot.FullName;
      foreach (string str in srcHiveRoot.ValueNameAndTypes.Where<KeyValuePair<string, RegistryValueType>>((Func<KeyValuePair<string, RegistryValueType>, bool>) (p => p.Value == RegistryValueType.MultiString)).Select<KeyValuePair<string, RegistryValueType>, string>((Func<KeyValuePair<string, RegistryValueType>, string>) (q => q.Key)))
      {
        string valueName = string.IsNullOrEmpty(str) ? (string) null : str;
        string[] multiStringValue = srcHiveRoot.GetMultiStringValue(valueName);
        using (ORRegistryKey subKey = dstHiveRoot.CreateSubKey(fullName))
        {
          subKey.SetValue(valueName, multiStringValue);
          ++fromHiveRecursive;
        }
        srcHiveRoot.DeleteValue(valueName);
      }
      foreach (string subKey in srcHiveRoot.SubKeys)
      {
        using (ORRegistryKey srcHiveRoot1 = srcHiveRoot.OpenSubKey(subKey))
          fromHiveRecursive += OfflineRegUtils.ExtractFromHiveRecursive(srcHiveRoot1, type, dstHiveRoot);
      }
      return fromHiveRecursive;
    }
  }
}
