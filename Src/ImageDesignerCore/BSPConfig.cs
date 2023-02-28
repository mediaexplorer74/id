// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.BSPConfig
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class BSPConfig
  {
    private const string ELEMENT_ROOT = "BSP";
    private const string ATTRIBUTE_BSPVERSION = "Version";
    private const string ELEMENT_OEMINPUTFILES = "OEMInputFiles";
    private const string ELEMENT_OEMCUSTOMIZATIONDIR = "OEMCustomizationTemplateDir";
    private const string ELEMENT_ENVIRONMENTVAR = "EnvironmentVariables";
    private Dictionary<ImageType, OEMInput> _oemInputs = new Dictionary<ImageType, OEMInput>();

    public string OEMCustomizationTemplateDir { get; private set; }

    public Dictionary<string, string> EnvironmentVariables { get; private set; }

    public Dictionary<ImageType, string> OEMInputFiles { get; private set; }

    public Dictionary<ImageType, OEMInput> OEMInputs => this._oemInputs;

    public string Version { get; private set; }

    public string BSPRoot { get; private set; }

    public string BSPConfigFile { get; set; }

    public BSPConfig()
    {
    }

    public BSPConfig(BSPConfig bspConfig)
    {
      this.BSPRoot = bspConfig.BSPRoot;
      this.EnvironmentVariables = new Dictionary<string, string>((IDictionary<string, string>) bspConfig.EnvironmentVariables, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      this.OEMCustomizationTemplateDir = bspConfig.OEMCustomizationTemplateDir;
      this.OEMInputFiles = new Dictionary<ImageType, string>((IDictionary<ImageType, string>) bspConfig.OEMInputFiles);
      this.Version = bspConfig.Version;
      this.BSPConfigFile = this.BSPConfigFile;
    }

    public bool IsValidBSP(string msPackageRoot)
    {
      try
      {
        this.ValidateBSP(msPackageRoot);
      }
      catch
      {
        return false;
      }
      return true;
    }

    public void ValidateBSP(string msPackageRoot)
    {
      if (this.OEMInputFiles == null || this.OEMInputFiles.Count<KeyValuePair<ImageType, string>>() == 0)
        throw new WPIDException("The BSP Config '{0}' does not contain any OEMInput file definitions.", new object[1]
        {
          (object) this.BSPConfigFile
        });
      foreach (ImageType key in this.OEMInputFiles.Keys)
      {
        string oemInputFile = this.OEMInputFiles[key];
        if (!File.Exists(oemInputFile))
          throw new WPIDException("The {0} OEMInput file '{1}' does not exist in BSP Config '{2}'", new object[3]
          {
            (object) key.ToString(),
            (object) oemInputFile,
            (object) this.BSPConfigFile
          });
      }
      foreach (ImageType key in this.OEMInputFiles.Keys)
      {
        string oemInputFile = this.OEMInputFiles[key];
        OEMInput xmlInput;
        if (this._oemInputs.ContainsKey(key))
        {
          xmlInput = this._oemInputs[key];
        }
        else
        {
          xmlInput = new OEMInput();
          OEMInput.ValidateInput(ref xmlInput, oemInputFile, new IULogger(), msPackageRoot, FeatureManifest.CPUType_ARM.ToString());
          this._oemInputs[key] = xmlInput;
        }
        foreach (string additionalFm in xmlInput.AdditionalFMs)
        {
          string str = Environment.ExpandEnvironmentVariables(additionalFm);
          if (!File.Exists(str))
            throw new WPIDException("The FM file '{0}' defined in the AdditionalFMs section of the OEMInput file '{1}' does not exist in BSP Config '{2}'", new object[3]
            {
              (object) str,
              (object) oemInputFile,
              (object) this.BSPConfigFile
            });
          FeatureManifest fm = new FeatureManifest();
          FeatureManifest.ValidateAndLoad(ref fm, str, new IULogger());
          fm.OemInput = xmlInput;
          StringBuilder stringBuilder = new StringBuilder();
          bool flag = false;
          foreach (string packageFile in fm.GetPackageFileList())
          {
            if (!File.Exists(packageFile))
            {
              flag = true;
              stringBuilder.AppendLine("\t" + packageFile);
            }
          }
          if (flag)
            throw new WPIDException("The FM file '{0}' has missing package files for image '{1}': \n{2}", new object[3]
            {
              (object) additionalFm,
              (object) key.ToString(),
              (object) stringBuilder.ToString()
            });
        }
      }
    }

    public static BSPConfig LoadFromXml(string configFile, string msPackageRoot)
    {
      BSPConfig bspConfig = BSPConfig.LoadFromXmlInternal(configFile);
      bspConfig.BSPRoot = Environment.ExpandEnvironmentVariables(Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPath.GetDirectoryName(configFile));
      bspConfig.SetEnvironment();
      bspConfig.UpdatePaths();
      bspConfig.ValidateBSP(msPackageRoot);
      bspConfig.BSPConfigFile = configFile;
      return bspConfig;
    }

    private void SetEnvironment()
    {
      Environment.SetEnvironmentVariable("BSPROOT", this.BSPRoot, EnvironmentVariableTarget.Process);
      foreach (KeyValuePair<string, string> environmentVariable in this.EnvironmentVariables)
      {
        string str = Environment.ExpandEnvironmentVariables(environmentVariable.Value);
        Environment.SetEnvironmentVariable(environmentVariable.Key, str);
      }
    }

    private void UpdatePaths()
    {
      if (!string.IsNullOrEmpty(this.OEMCustomizationTemplateDir))
        this.OEMCustomizationTemplateDir = Environment.ExpandEnvironmentVariables(this.OEMCustomizationTemplateDir);
      Dictionary<ImageType, string> dictionary = new Dictionary<ImageType, string>();
      foreach (ImageType key in this.OEMInputFiles.Keys)
        dictionary.Add(key, Environment.ExpandEnvironmentVariables(this.OEMInputFiles[key]));
      this.OEMInputFiles = dictionary;
    }

    private static BSPConfig LoadFromXmlInternal(string configFile)
    {
      XDocument xml = BSPConfig.ParseXml(configFile);
      BSPConfig bspConfig = new BSPConfig();
      try
      {
        XElement entry = xml.Descendants((XName) "BSP").First<XElement>();
        bspConfig.Version = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAttribute<string>(entry, "Version");
        bspConfig.EnvironmentVariables = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        foreach (XElement element in entry.Descendants((XName) "EnvironmentVariables").Elements<XElement>())
        {
          string key = element.Attribute((XName) "Name").Value;
          if (bspConfig.EnvironmentVariables.ContainsKey(key))
            throw new WPIDException("Environment variable '{0}' found in config file is defined twice.", new object[1]
            {
              (object) key
            });
          bspConfig.EnvironmentVariables.Add(key, element.Attribute((XName) "Value").Value);
        }
        foreach (XElement descendant in entry.Descendants((XName) "OEMCustomizationTemplateDir"))
          bspConfig.OEMCustomizationTemplateDir = descendant.Value;
        IEnumerable<XElement> xelements = entry.Descendants((XName) "OEMInputFiles").Elements<XElement>();
        bspConfig.OEMInputFiles = new Dictionary<ImageType, string>();
        foreach (XElement xelement in xelements)
        {
          string localName = xelement.Name.LocalName;
          string str = xelement.Value;
          ImageType result;
          if (!Enum.TryParse<ImageType>(xelement.Name.LocalName, out result))
            throw new WPIDException("Invalid Image Type '{0}' found in OEMInputFiles section of config file.", new object[1]
            {
              (object) xelement.Name.LocalName
            });
          if (bspConfig.OEMInputFiles.ContainsKey(result))
            throw new WPIDException("Invalid Image Type '{0}' found in OEMInputFiles section of config file is defined twice.", new object[1]
            {
              (object) xelement.Name.LocalName
            });
          bspConfig.OEMInputFiles.Add(result, xelement.Value);
        }
      }
      catch (Exception ex)
      {
        throw new WPIDException(ex, "Invalid config file {0}", new object[1]
        {
          (object) configFile
        });
      }
      return bspConfig;
    }

    private static XDocument ParseXml(string configFile)
    {
      if (string.IsNullOrEmpty(configFile))
        throw new ArgumentNullException(nameof (configFile));
      if (!File.Exists(configFile))
        throw new FileNotFoundException("Cannot find or open config file", configFile);
      try
      {
        return XDocument.Load(configFile);
      }
      catch (Exception ex)
      {
        throw new WPIDException(ex, "Invalid config file {0}", new object[1]
        {
          (object) configFile
        });
      }
    }
  }
}
