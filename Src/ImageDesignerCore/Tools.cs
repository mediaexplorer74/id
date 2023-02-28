// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.Tools
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using FFUComponents;
using Microsoft.Win32;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using Application = Microsoft.WindowsPhone.ImageUpdate.Customization.XML.Application;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class Tools
  {
    public const string REGEX_GROUP_PRODUCTID = "productIdValue";
    public const string REGEX_GROUP_APPNAME = "appName";
    public static ResourceManager _resourceManager = (ResourceManager) null;
    private static Dictionary<string, string> _filterExtLookup = (Dictionary<string, string>) null;
    public static readonly Regex Regex_ProductId1 = new Regex(".*\"productid\"[\\s]*value[\\s]*=[\\s]*\"[\\s]*(?<productIdValue>[{a-f0-9\\-}]+)[\\s]*\".*", RegexOptions.Compiled);
    public static readonly Regex Regex_ProductId2 = new Regex("(.*<characteristic[\\s]*type[\\s]*=[\\s]*\"[\\s]*)(?<productIdValue>{[a-f0-9\\-]*})(.*)", RegexOptions.Compiled);
    public static readonly Regex Regex_XAPName1 = new Regex(".*\"xappath\"[\\s]*value[\\s]*=[\\s]*\"[\\s]*(?<appName>[a-z0-9_\\-\\.]*)[\\s]*\".*", RegexOptions.Compiled);
    public static readonly Regex Regex_XAPName2 = new Regex("(.*\"installinfo\")(.*)\\\\(?<appName>[a-z0-9_\\-\\.]*\\.xap)(.*)", RegexOptions.Compiled);
    public static readonly Regex Regex_APPXName = new Regex(".*\"appxpath\"[\\s]*value[\\s]*=[\\s]*\"[\\s]*(?<appName>[a-z0-9_\\-\\.]*)[\\s]*\".*", RegexOptions.Compiled);
    public static readonly LexicographicComparer LexicoGraphicComparer = new LexicographicComparer();

    public static ResourceManager ResourceManager
    {
      get
      {
        if (Microsoft.WindowsPhone.ImageDesigner.Core.Tools._resourceManager == null)
        {
          Assembly assembly = typeof (IDController).Assembly;
          Microsoft.WindowsPhone.ImageDesigner.Core.Tools._resourceManager = new ResourceManager(Constants.STRING_RESOURCE_NAMESPACE, assembly);
        }
        return Microsoft.WindowsPhone.ImageDesigner.Core.Tools._resourceManager;
      }
    }

    public static string GetString(string key)
    {
      string empty = string.Empty;
      ResourceManager resourceManager = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.ResourceManager;
      if (resourceManager != null)
        empty = resourceManager.GetString(key);
      return empty;
    }

    public static string GetEnumLocalizedString(Enum value) => Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("e" + value.GetType().Name + Enum.GetName(value.GetType(), (object) value));

    public static void DispatcherExec(Action a)
    {
      if (a == null)
        return;
            if (Application.Current != null)
            {
                //RnD
                //Application.Current.Dispatcher.Invoke((Delegate)a);
            }
            else
                a();
    }

    public static T DispatcherExec<T>(Func<T> f)
    {
      T obj = default (T);

    if (f != null)
        obj = Application.Current == null
                    ? f()
                    : default;//(T) Application.Current.Dispatcher.Invoke((Delegate) f);
      return obj;
    }

    public static string FormatBytes(ulong bytes)
    {
      string[] strArray = new string[5]
      {
        "B",
        "KB",
        "MB",
        "GB",
        "TB"
      };
      int index = 0;
      double num = (double) bytes;
      if (bytes > 1024UL)
      {
        index = 0;
        for (; bytes / 1024UL > 0UL; bytes /= 1024UL)
        {
          num = (double) bytes / 1024.0;
          ++index;
        }
      }
      return string.Format("{0:0.## }{1}", (object) num, (object) strArray[index]);
    }

    public static List<FeatureManifest.FMPkgInfo> GetOEMInputPackages(
      OEMInput oemInput,
      string bspRootDir,
      string akRootDir,
      string implicitFMFile)
    {
      List<FeatureManifest.FMPkgInfo> oemInputPackages = new List<FeatureManifest.FMPkgInfo>();
      Environment.SetEnvironmentVariable("BSPROOT", bspRootDir);
      Environment.SetEnvironmentVariable("WPDKCONTENTROOT", akRootDir);
      List<string> stringList = new List<string>();
      stringList.Add(implicitFMFile);
      if (oemInput.AdditionalFMs != null)
        stringList.AddRange((IEnumerable<string>) oemInput.AdditionalFMs);
      foreach (string name in stringList)
      {
        string xmlFile = Environment.ExpandEnvironmentVariables(name);
        FeatureManifest fm = new FeatureManifest();
        FeatureManifest.ValidateAndLoad(ref fm, xmlFile, new IULogger());
        fm.OemInput = oemInput;
        oemInputPackages.AddRange((IEnumerable<FeatureManifest.FMPkgInfo>) fm.GetFilteredPackagesByGroups());
      }
      return oemInputPackages;
    }

    public static void RetrieveForMetadataPackages(
      List<FeatureManifest.FMPkgInfo> packages,
      Dictionary<string, PackageMetadata> packageMetadataList)
    {
      foreach (FeatureManifest.FMPkgInfo package in packages)
      {
        ulong num = 0;
        string packagePath = package.PackagePath;
        PackageMetadata packageMetadata;
        if (packageMetadataList.ContainsKey(packagePath))
        {
          packageMetadata = packageMetadataList[packagePath];
        }
        else
        {
          IPkgInfo ipkgInfo = Package.LoadFromCab(packagePath);
          foreach (IFileEntry file in ipkgInfo.Files)
            num += file.Size;
          packageMetadata = new PackageMetadata();
          packageMetadata.size = num;
          packageMetadata.pkgInfo = ipkgInfo;
          packageMetadata.fmPkgInfo = package;
          packageMetadataList[packagePath] = packageMetadata;
        }
      }
    }

    public static ulong GetPackageListSize(
      List<string> packages,
      Dictionary<string, PackageMetadata> packageMetadataList)
    {
      return (ulong) packageMetadataList.Where<KeyValuePair<string, PackageMetadata>>((Func<KeyValuePair<string, PackageMetadata>, bool>) (pkg => packages.Contains<string>(pkg.Key, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))).Sum<KeyValuePair<string, PackageMetadata>>((Func<KeyValuePair<string, PackageMetadata>, long>) (pkg => (long) pkg.Value.size));
    }

    public static bool CheckAllFilesExist(List<string> files)
    {
      bool flag = true;
      foreach (string file in files)
      {
        if (!File.Exists(file))
        {
          flag = false;
          break;
        }
      }
      return flag;
    }

    public static string GetPathWithMakeCatDir()
    {
      string pathWithMakeCatDir = (string) null;
      string str = (string) null;
      string wdkRoot = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetWDKRoot();
      if (!string.IsNullOrWhiteSpace(wdkRoot))
      {
        str = Path.Combine(wdkRoot, "bin\\x86\\");
      }
      else
      {
        string environmentVariable1 = Environment.GetEnvironmentVariable("_BUILDARCH");
        if (!string.IsNullOrEmpty(environmentVariable1) && environmentVariable1.Equals(FeatureManifest.CPUType_ARM, StringComparison.OrdinalIgnoreCase))
        {
          string environmentVariable2 = Environment.GetEnvironmentVariable("SDXROOT");
          if (Directory.Exists(environmentVariable2))
            str = Path.Combine(environmentVariable2, "nttools\\x86\\");
        }
      }
      if (!string.IsNullOrWhiteSpace(str))
      {
        StringBuilder stringBuilder = new StringBuilder(str);
        stringBuilder.AppendFormat(";{0}", (object) Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine));
        pathWithMakeCatDir = stringBuilder.ToString();
      }
      return pathWithMakeCatDir;
    }

    public static string GetWDKRoot()
    {
      string environmentVariable = Environment.GetEnvironmentVariable("WDKCONTENTROOT");
      if (string.IsNullOrWhiteSpace(environmentVariable))
      {
        using (RegistryKey registryKey1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
        {
          using (RegistryKey registryKey2 = registryKey1.OpenSubKey("Software\\Microsoft\\Windows Kits\\Installed Roots", false))
          {
            if (registryKey2 != null)
              environmentVariable = registryKey2.GetValue("KitsRoot81") as string;
          }
        }
      }
      return environmentVariable;
    }

    public static string GetAKRoot()
    {
      string environmentVariable1 = Environment.GetEnvironmentVariable("WPDKCONTENTROOT");
      if (!string.IsNullOrWhiteSpace(environmentVariable1) && Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(environmentVariable1))
        return environmentVariable1;
      string environmentVariable2 = Environment.GetEnvironmentVariable("BINARY_ROOT");
      if (!string.IsNullOrWhiteSpace(environmentVariable2) && Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(environmentVariable2))
      {
        string environmentVariable3 = Environment.GetEnvironmentVariable("_BUILDARCH");
        if (!string.IsNullOrEmpty(environmentVariable3) && environmentVariable3.Equals(FeatureManifest.CPUType_ARM, StringComparison.OrdinalIgnoreCase))
        {
          Microsoft.WindowsPhone.ImageDesigner.Core.Tools.SetupPrivateEnvironment(environmentVariable2);
          return environmentVariable2;
        }
      }
      using (RegistryKey registryKey1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
      {
        using (RegistryKey registryKey2 = registryKey1.OpenSubKey("Software\\Microsoft\\Windows Phone SDKs\\V8.1\\WPAK", false))
        {
          if (registryKey2 != null)
            environmentVariable2 = registryKey2.GetValue("InstallDir") as string;
        }
      }
      return !string.IsNullOrWhiteSpace(environmentVariable2) && Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(environmentVariable2) ? environmentVariable2 : (string) null;
    }

    public static void SetupPrivateEnvironment(string binaryRoot)
    {
      string environmentVariable = Environment.GetEnvironmentVariable("_BUILDARCH");
      if (string.IsNullOrEmpty(environmentVariable) || !environmentVariable.Equals(FeatureManifest.CPUType_ARM, StringComparison.OrdinalIgnoreCase) || !Directory.Exists(binaryRoot))
        return;
      string str1 = Path.Combine(binaryRoot, "DeviceImaging\\MSNonProductionPartnerShareFM.xml");
      if (File.Exists(str1))
      {
        string str2 = Path.Combine(binaryRoot, "FMFiles");
        Directory.CreateDirectory(str2);
        string destFileName = Path.Combine(str2, "MSOptionalFeatures.xml");
        bool overwrite = true;
        File.Copy(str1, destFileName, overwrite);
      }
      string str3 = Path.Combine(binaryRoot, "DeviceImaging\\MMOSNonProductionPartnerShareFM.xml");
      if (File.Exists(str3))
      {
        string str4 = Path.Combine(binaryRoot, "FMFiles");
        Directory.CreateDirectory(str4);
        string destFileName = Path.Combine(str4, "MMOSOptionalFeatures.xml");
        bool overwrite = true;
        File.Copy(str3, destFileName, overwrite);
      }
      string str5 = Path.Combine(binaryRoot, "DeviceImaging\\HoverFM.xml");
      if (File.Exists(str5))
      {
        string destFileName = Path.Combine(Path.Combine(binaryRoot, "FMFiles"), "Hover.xml");
        bool overwrite = true;
        File.Copy(str5, destFileName, overwrite);
      }
      string path = Path.Combine(Environment.GetEnvironmentVariable("_WINPHONEROOT"), "src\\baseos\\prod\\SecurityModel\\OEMCertificates");
      string str6 = Path.Combine(binaryRoot, "tools\\Certificates");
      Directory.CreateDirectory(str6);
      foreach (string file in Directory.GetFiles(path))
      {
        string str7 = Path.Combine(str6, Path.GetFileName(file));
        if (!File.Exists(str7))
          File.Copy(file, str7);
      }
    }

    public static string GetBSPRoot()
    {
      string bspRoot = Environment.GetEnvironmentVariable("BSPROOT");
      if (!Microsoft.WindowsPhone.ImageDesigner.Core.Tools.IsValidBSP(bspRoot))
      {
        string userConfig = UserConfig.GetUserConfig("LastUsedBspConfig");
        if (!string.IsNullOrWhiteSpace(userConfig))
        {
          bspRoot = Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPath.GetDirectoryName(userConfig);
          if (!Microsoft.WindowsPhone.ImageDesigner.Core.Tools.IsValidBSP(bspRoot))
            throw new WPIDException("BSP Root not found");
        }
      }
      return bspRoot;
    }

    public static string GetBSPConfig(string bspRoot)
    {
      string path = string.Empty;
      if (Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(bspRoot))
      {
        path = Path.Combine(bspRoot, "Bsp.Config.xml");
        if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Exists(path))
        {
          string userConfig = UserConfig.GetUserConfig("LastUsedBspConfig");
          if (!string.IsNullOrWhiteSpace(userConfig) && Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPath.GetDirectoryName(userConfig).Equals(bspRoot, StringComparison.OrdinalIgnoreCase))
            path = userConfig;
        }
      }
      return path;
    }

    public static bool IsValidBSP(string bspRoot)
    {
      bool flag = false;
      if (Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(bspRoot))
        flag = true;
      return flag;
    }

    public static string GetMSPackageRoot()
    {
      string path = Path.Combine(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKRoot(), "MSPackages");
      if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(path))
        path = Path.Combine(Environment.GetEnvironmentVariable("BINARY_ROOT"), "prebuilt");
      return path;
    }

    public static string GetMSPackageRoot(string AKRoot) => Path.Combine(AKRoot, "MSPackages");

    public static string GetAKToolsRoot(string AKRoot) => Path.Combine(AKRoot, "tools\\bin\\i386");

    public static string GetAKToolsRoot()
    {
      string path = Path.Combine(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKRoot(), "tools\\bin\\i386");
      if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(path))
        path = Path.Combine(Environment.GetEnvironmentVariable("_WINPHONEROOT"), "tools\\bin\\i386");
      return path;
    }

    public static string GetHelpDocPath()
    {
      string path = Path.Combine(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAKRoot(), "Docs", "WP_Blue_Documentation.chm");
      if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Exists(path))
      {
        path = Path.Combine(Environment.GetEnvironmentVariable("_WINPHONEROOT"), "src\\baseos\\prod\\ImgUpd\\Managed\\ImageDesigner\\Docs", "WP_Blue_Documentation.chm");
        if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(path))
        {
          path = Path.Combine(Environment.CurrentDirectory, "WP_Blue_Documentation.chm");
          if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(path))
            path = (string) null;
        }
      }
      return path;
    }

    public static IEnumerable<IFFUDevice> GetFlashableDevices()
    {
      ICollection<IFFUDevice> devices = (ICollection<IFFUDevice>) new List<IFFUDevice>();
      try
      {
        FFUManager.GetFlashableDevices(ref devices);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      return (IEnumerable<IFFUDevice>) devices;
    }

    public static void CopyTextToClipboard(string text)
    {
      Clipboard.Clear();
      Clipboard.SetText(text);
    }

    public static string GetFileFiterByType(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.FileFilterType type)
    {
      string fileFiterByType = "";
      switch (type)
      {
        case Microsoft.WindowsPhone.ImageDesigner.Core.Tools.FileFilterType.Application:
          fileFiterByType = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtApplicationFilter");
          break;
        case Microsoft.WindowsPhone.ImageDesigner.Core.Tools.FileFilterType.License:
          fileFiterByType = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtLicenseFilter");
          break;
        case Microsoft.WindowsPhone.ImageDesigner.Core.Tools.FileFilterType.ProvXML:
          fileFiterByType = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtProvXMLFilter");
          break;
        case Microsoft.WindowsPhone.ImageDesigner.Core.Tools.FileFilterType.ApplicationAndProvXML:
          fileFiterByType = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtApplicationAndProvXMLFilter");
          break;
      }
      return fileFiterByType;
    }

    public static string GetFileFiterFromExtension(List<string> extensions)
    {
      string str = "";
      if (Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup == null)
      {
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.Add(".*", Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtAllFilesFilter"));
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.Add(".jpg", Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtJPGFilter"));
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.Add(".jpeg", Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtJPEGFilter"));
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.Add(".png", Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtPNGFilter"));
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.Add(".bmp", Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtBMPFilter"));
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.Add(".gif", Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtGIFFilter"));
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.Add(".wma", Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtWMAFilter"));
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.Add(".xml", Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtXMLFilter"));
        Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.Add(".map", Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtMapDataFilter"));
      }
      foreach (string extension in extensions)
      {
        if (Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup.ContainsKey(extension))
          str = str + Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup[extension] + "|";
        else
          str += string.Format("{0} ({1})|{1};", (object) extension.TrimStart('.').ToUpper(), (object) ("*" + extension));
      }
      string fiterFromExtension = str.Trim('|');
      if (string.IsNullOrEmpty(fiterFromExtension))
        fiterFromExtension = Microsoft.WindowsPhone.ImageDesigner.Core.Tools._filterExtLookup[".*"];
      return fiterFromExtension;
    }

    public static void ViewInBrowser(string file)
    {
      if (!Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathFile.Exists(file))
        return;
      string path2 = "Internet Explorer\\IExplore.exe";
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), path2);
      if (!File.Exists(path))
      {
        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), path2);
        if (!File.Exists(path))
          path = "notepad.exe";
      }
      using (Process process = new Process())
      {
        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = file;
        process.StartInfo.UseShellExecute = false;
        process.Start();
      }
    }

    public static string GetImageName(ImageType imgType)
    {
      string imageName;
      switch (imgType)
      {
        case ImageType.Test:
          imageName = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtTestImageLabel");
          break;
        case ImageType.Production:
          imageName = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtProductionImageLabel");
          break;
        case ImageType.RetailManufacturing:
          imageName = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtRetailManufacturingImageLabel");
          break;
        case ImageType.Retail:
          imageName = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtRetailImageLabel");
          break;
        case ImageType.MMOS:
          imageName = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetString("txtMMOSImageLabel");
          break;
        default:
          imageName = imgType.ToString();
          break;
      }
      return imageName;
    }

    public static T GetAttribute<T>(XElement entry, string attribute) where T : class
    {
      object obj1 = (object) (entry.Attribute((XName) attribute).Value as T);
      T obj2 = default (T);
      return !(typeof (T) == typeof (Enum)) ? obj1 as T : Enum.Parse(typeof (T), obj1 as string) as T;
    }

    public static T GetEnumAttribute<T>(XElement entry, string attribute)
    {
      XAttribute xattribute = entry.Attribute((XName) attribute);
      T enumAttribute = default (T);
      if (xattribute != null)
        enumAttribute = (T) Enum.Parse(typeof (T), xattribute.Value);
      return enumAttribute;
    }

    public static XAttribute CreateAttribute<T>(string name, T obj)
    {
      XAttribute attribute = (XAttribute) null;
      if ((object) obj != null)
        attribute = !(obj.GetType() == typeof (Enum)) ? new XAttribute((XName) name, (object) obj) : new XAttribute((XName) name, (object) Enum.GetName(obj.GetType(), (object) obj));
      return attribute;
    }

    public static string GetApplicationProductID(Application app)
    {
      string empty = string.Empty;
      if (app != null && File.Exists(app.ProvXML))
      {
        string lower = File.ReadAllText(app.ProvXML).ToLower();
        foreach (Regex regex in new List<Regex>()
        {
          Microsoft.WindowsPhone.ImageDesigner.Core.Tools.Regex_ProductId1,
          Microsoft.WindowsPhone.ImageDesigner.Core.Tools.Regex_ProductId2
        })
        {
          Match match = regex.Match(lower);
          if (match.Success)
          {
            empty = match.Groups["productIdValue"].Value;
            break;
          }
        }
      }
      return empty;
    }

    public static string GetApplicationName(Application app)
    {
      string empty = string.Empty;
      if (app != null && File.Exists(app.ProvXML))
      {
        List<Regex> regexList = new List<Regex>();
        if (Path.GetExtension(app.Source).EndsWith("xap", StringComparison.OrdinalIgnoreCase))
        {
          regexList.Add(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.Regex_XAPName1);
          regexList.Add(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.Regex_XAPName2);
        }
        else if (Path.GetExtension(app.Source).EndsWith("appx", StringComparison.OrdinalIgnoreCase))
          regexList.Add(Microsoft.WindowsPhone.ImageDesigner.Core.Tools.Regex_APPXName);
        string lower = File.ReadAllText(app.ProvXML).ToLower();
        foreach (Regex regex in regexList)
        {
          Match match = regex.Match(lower);
          if (match.Success)
          {
            empty = match.Groups["appName"].Value;
            break;
          }
        }
      }
      return empty;
    }

    public static bool CanWriteToFolder(string folder)
    {
      bool folder1 = false;
      try
      {
        if (Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.Exists(folder))
        {
          Directory.GetAccessControl(folder);
          folder1 = true;
        }
      }
      catch (UnauthorizedAccessException ex)
      {
        folder1 = false;
      }
      return folder1;
    }

    public static bool InUIMode() => Application.Current != null;

    public enum FileFilterType
    {
      Application,
      License,
      ProvXML,
      ApplicationAndProvXML,
    }
  }
}
