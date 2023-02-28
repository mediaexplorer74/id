// BinaryFile.cs
// Type: Microsoft.Phone.Test.TestMetadata.Helper.BinaryFile
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using Microsoft.MetadataReader;
using Microsoft.Tools.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Phone.Test.TestMetadata.Helper
{
  public static class BinaryFile
  {
    private const string DllExtension = ".dll";

    public static List<PortableExecutableDependency> GetDependency(
      string fileName)
    {
      List<PortableExecutableDependency> dependencyList = new List<PortableExecutableDependency>();
      try
      {
        using (PortableExecutable peFile = new PortableExecutable(fileName))
        {
          if (!peFile.IsPortableExecutableBinary)
            return dependencyList;
          BinaryFile.GetNativeDependency(dependencyList, peFile);
          if (!peFile.IsManaged)
            return dependencyList;
          BinaryFile.GetManangedDependency(peFile, dependencyList);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Unable to loading PE binary {0}", (object) fileName);
        Log.Error(ex.Message);
        if (ex.InnerException != null)
          Log.Error(ex.InnerException.Message);
        Log.Error("{0}", (object) ex);
      }
      return dependencyList;
    }

    private static void GetNativeDependency(
      List<PortableExecutableDependency> dependencyList,
      PortableExecutable peFile)
    {
      dependencyList.AddRange(peFile.Imports.Select<string, PortableExecutableDependency>((Func<string, PortableExecutableDependency>) (import => new PortableExecutableDependency()
      {
        Name = import.ToLowerInvariant(),
        Type = BinaryDependencyType.Native
      })));
      dependencyList.AddRange(peFile.DelayLoadImports.Select<string, PortableExecutableDependency>((Func<string, PortableExecutableDependency>) (delayLoadImport => new PortableExecutableDependency()
      {
        Name = delayLoadImport.ToLowerInvariant(),
        Type = BinaryDependencyType.DelayLoad
      })));
    }

    private static void GetManangedDependency(
      PortableExecutable peFile,
      List<PortableExecutableDependency> dependencyList)
    {
      using (DefaultUniverse defaultUniverse = new DefaultUniverse())
      {
        AssemblyName[] referencedAssemblies = defaultUniverse.LoadAssemblyFromFile(peFile.FullFileName).GetReferencedAssemblies();
        dependencyList.AddRange(((IEnumerable<AssemblyName>) referencedAssemblies).Select<AssemblyName, PortableExecutableDependency>((Func<AssemblyName, PortableExecutableDependency>) (assemblyName => new PortableExecutableDependency()
        {
          Name = assemblyName.Name.ToLowerInvariant() + ".dll",
          Type = BinaryDependencyType.Managed
        })));
        BinaryFile.GetPlatformInvokeDependency(peFile, dependencyList);
      }
    }

    private static void GetPlatformInvokeDependency(
      PortableExecutable peFile,
      List<PortableExecutableDependency> dependencyList)
    {
      using (MetadataFile metadataFile = new MetadataDispenser().OpenFile(peFile.FullFileName))
      {
        IMetadataImport objectForIunknown = (IMetadataImport) Marshal.GetUniqueObjectForIUnknown(metadataFile.RawPtr);
        HCORENUM hcorenum = new HCORENUM();
        int num1;
        uint num2;
        objectForIunknown.EnumModuleRefs(ref hcorenum, out num1, 1, out num2);
        while (num2 > 0U)
        {
          StringBuilder stringBuilder = new StringBuilder(1024);
          int num3 = 0;
          objectForIunknown.GetModuleRefProps(num1, stringBuilder, 
              stringBuilder.Capacity, out num3);
          string str = stringBuilder.ToString();
          if (string.IsNullOrEmpty(LongPathPath.GetExtension(str)))
            str += ".dll";
          PortableExecutableDependency executableDependency = new PortableExecutableDependency()
          {
            Name = str.ToLowerInvariant(),
            Type = BinaryDependencyType.PlatfomInvoke
          };
          dependencyList.Add(executableDependency);
          objectForIunknown.EnumModuleRefs(ref hcorenum, out num1, 1, out num2);
        }
      }
    }
  }
}
