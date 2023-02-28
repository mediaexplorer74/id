// ImageDesigner.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.ImageDesigner
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using Microsoft.WindowsPhone.ImageDesigner.UI.Common;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace Microsoft.WindowsPhone.ImageDesigner.UI
{
    public partial class ImageDesigner : Window
    {

        public ImageDesigner()
        {
            this.DataContext = (object)IDContext.Instance.Controller;
            this.InitializeComponent();
            HelpProvider.InitializeHelp();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
                => IDContext.Instance.Controller.OnApplicationExit();

    }
}
