// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.Constants
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.IO;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class Constants
  {
    public const string MS_PACKAGES_RELATIVE_DIR = "MSPackages";
    public const string AK_TOOLS_BIN_RELATIVE_DIR = "tools\\bin\\i386";
    public const string AK_HELP_DOCS_RELATIVE_DIR = "Docs";
    public const string WPID_HELP_DOC_NAME = "WP_Blue_Documentation.chm";
    public const string FFUTOOLEXE = "ffutool";
    public const string DEFAULT_PROJECT_FILENAME = "Project";
    public const string PROJECT_FILE_EXTENSION = "wpid.xml";
    public const string OEMINPUT_FILENAME = "OemInput.xml";
    public const string DEFAULT_PROJECT_SUBFOLDER = "Project";
    public const string DEFAULT_BSPCONFIG_XML = "Bsp.Config.xml";
    public const string PIDLOGFILENAME = "PhoneImageDesigner";
    public const string LOG_EXT = ".log";
    public const string ENV_BSP_ROOT = "BSPROOT";
    public const string ENV_WPDK_ROOT = "WPDKCONTENTROOT";
    public const string ENV_BINARY_ROOT = "BINARY_ROOT";
    public const string CMD_GENERATEIMAGE = "GenerateImage";
    public const string CMD_FLASH = "Flash";
    public const string PARAM_OUTPUTFILE = "OutputFile";
    public const string PARAM_OUTPUTFILE_DESCR = "Path to the output image file (.FFU)";
    public const string PARAM_OEMINPUTXML = "OemInputXml";
    public const string PARAM_OEMINPUTXML_DESCR = "Path to the OEM Input XML file";
    public const string PARAM_CUSTOMIZATIONFILE = "CustomizationFile";
    public const string PARAM_CUSTOMIZATIONFILE_DESCR = "Path to the customization file";
    public const string PARAM_MSPACKAGEROOT = "MSPackageRoot";
    public const string PARAM_MSPACKAGEROOT_DESCR = "Path to the folder containing Microsoft base packages";
    public const string PARAM_BSPCONFIG = "BSPConfigFile";
    public const string PARAM_BSPCONFIG_DESCR = "Path to the BSPConfig.xml file under BSP root";
    public static string STRING_RESOURCE_NAMESPACE = "Microsoft.WindowsPhone.ImageDesigner.Core.Resources.Strings";
    public static string TOOLTIPS_RESOURCE_NAMESPACE = "Microsoft.WindowsPhone.ImageDesigner.Core.Resources.Tooltips";
    public static readonly string DEFAULT_OUTPUT_ROOT = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Windows Phone Image Designer (WPID)");
    public static readonly string DEFAULT_OUTPUT_FOLDER = Path.Combine(Constants.DEFAULT_OUTPUT_ROOT, "Project");
  }
}
