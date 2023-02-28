// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ProjectXml
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class ProjectXml
  {
    private const string ELEMENT_ROOT = "WindowsPhoneImageDesigner";
    private const string ATTR_LASTSAVEDSTATE = "LastSavedState";
    private const string ATTR_DATESAVED = "DateSaved";

    public string ProjectPath { get; private set; }

    public static void Validate(string projFile)
    {
      IDContext idContext = File.Exists(projFile) ? ProjectXml.Load(projFile) : throw new WPIDException("The project file {0} does not exist.  Use a different project or start over and create a new one.", new object[1]
      {
        (object) projFile
      });
      if (idContext.BSPConfig == null || string.IsNullOrEmpty(idContext.BSPConfig.BSPConfigFile))
        throw new WPIDException("The project file {0} does not contain a refrenced to BSP Config file.  Use a different project or start over and create a new one.", new object[1]
        {
          (object) projFile
        });
      if (!File.Exists(idContext.BSPConfig.BSPConfigFile))
        throw new WPIDException("The project file {0} contains a refrenced to BSP Config '{1}' which does not exist.  Use a different project or start over and create a new one.", new object[2]
        {
          (object) projFile,
          (object) idContext.BSPConfig.BSPConfigFile
        });
      BSPConfig.LoadFromXml(idContext.BSPConfig.BSPConfigFile, IDContext.Instance.MSPackageRoot);
    }

    public static IDContext Load(string projFile)
    {
      XDocument xml = ProjectXml.ParseXml(projFile);
      ProjectXml projectXml = new ProjectXml();
      IDContext newInstance = IDContext.NewInstance;
      try
      {
        XElement entry = xml.Descendants((XName) "WindowsPhoneImageDesigner").First<XElement>();
        newInstance.AKRoot = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAttribute<string>(entry, "AKRoot");
        string attribute = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAttribute<string>(entry, "BSPConfig");
        try
        {
          BSPConfig bspConfig = BSPConfig.LoadFromXml(attribute, newInstance.MSPackageRoot);
          newInstance.BSPConfig = bspConfig;
        }
        catch
        {
          newInstance.BSPConfig = (BSPConfig) null;
          throw new WPIDException("The project file {0} does not contain valid BSP Configuration.  Use a different project or create a new one.", new object[1]
          {
            (object) projFile
          });
        }
        newInstance.ImageType = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetEnumAttribute<ImageType>(entry, "ImageType");
        newInstance.AKVersion = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAttribute<string>(entry, "AKVersion");
        newInstance.OutputDir = Microsoft.WindowsPhone.ImageDesigner.Core.Tools.GetAttribute<string>(entry, "OutputDir");
        if (string.IsNullOrEmpty(newInstance.AKVersion))
          Microsoft.WindowsPhone.ImageDesigner.Core.Tools.SetupPrivateEnvironment(newInstance.AKRoot);
        OEMInput xmlInput = (OEMInput) null;
        OEMInput.ValidateInput(ref xmlInput, newInstance.OEMInputFile, new IULogger(), newInstance.MSPackageRoot, FeatureManifest.CPUType_ARM);
        newInstance.SelectedOEMInput = xmlInput;
        projectXml.ProjectPath = projFile;
        newInstance.ProjectXml = projectXml;
      }
      catch (InvalidOperationException ex)
      {
        throw new WPIDException((Exception) ex, "Invalid project file {0}", new object[1]
        {
          (object) projFile
        });
      }
      return newInstance;
    }

    public bool Save(string outputFile)
    {
      IDContext instance = IDContext.Instance;
      string directoryName = Path.GetDirectoryName(outputFile);
      Microsoft.WindowsPhone.ImageUpdate.Tools.Common.LongPathDirectory.CreateDirectory(directoryName);
      if (!Directory.Exists(directoryName))
        throw new WPIDException("Could not create folder {0} to save project file", new object[1]
        {
          (object) directoryName
        });
      string str = instance.BSPConfig == null ? "" : instance.BSPConfig.BSPConfigFile;
      using (XmlWriter writer = XmlWriter.Create(outputFile, new XmlWriterSettings()
      {
        Indent = true,
        NewLineOnAttributes = true
      }))
        new XDocument(new XDeclaration("1.0", "utf-8", "true"), new object[1]
        {
          (object) new XElement((XName) "WindowsPhoneImageDesigner", new object[8]
          {
            (object) Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CreateAttribute<string>("OEMInput", instance.OEMInputFile),
            (object) Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CreateAttribute<string>("BSPConfig", str),
            (object) Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CreateAttribute<string>("AKRoot", instance.AKRoot),
            (object) Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CreateAttribute<string>("AKVersion", instance.AKVersion),
            (object) Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CreateAttribute<ImageType>("ImageType", instance.ImageType),
            (object) Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CreateAttribute<string>("OutputDir", instance.OutputDir),
            (object) Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CreateAttribute<string>("DateSaved", string.Format("{0}", (object) DateTime.Now.ToString("F"))),
            (object) Microsoft.WindowsPhone.ImageDesigner.Core.Tools.CreateAttribute<IDStates>("LastSavedState", instance.Controller.CurrentState)
          })
        }).Save(writer);
      this.ProjectPath = outputFile;
      this.UpdateRegistry();
      return true;
    }

    private static XDocument ParseXml(string projectFile)
    {
      if (string.IsNullOrEmpty(projectFile))
        throw new ArgumentNullException("projFile");
      if (!File.Exists(projectFile))
        throw new FileNotFoundException("Cannot find or open project file", projectFile);
      try
      {
        return XDocument.Load(projectFile);
      }
      catch (Exception ex)
      {
        throw new WPIDException(ex, "Invalid project file {0}", new object[1]
        {
          (object) projectFile
        });
      }
    }

    private void UpdateRegistry() => UserConfig.SaveUserConfig(new List<Tuple<string, string>>()
    {
      new Tuple<string, string>("LastSavedProject", this.ProjectPath)
    });
  }
}
