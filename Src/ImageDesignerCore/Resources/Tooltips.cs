// Tooltips.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.Resources.Tooltips
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.Resources
{
  [DebuggerNonUserCode]
  [CompilerGenerated]
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  internal class Tooltips
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Tooltips()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (object.ReferenceEquals((object) Tooltips.resourceMan, (object) null))
          Tooltips.resourceMan = new ResourceManager("Microsoft.WindowsPhone.ImageDesigner.Core.Resources.Tooltips", typeof (Tooltips).Assembly);
        return Tooltips.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => Tooltips.resourceCulture;
      set => Tooltips.resourceCulture = value;
    }
  }
}
