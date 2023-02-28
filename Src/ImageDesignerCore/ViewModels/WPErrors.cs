// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels.WPErrors
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using Microsoft.WindowsPhone.ImageUpdate.Customization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core.ViewModels
{
  public class WPErrors
  {
    public List<CustomizationError> Issues = new List<CustomizationError>();
    public WPListItemBase SourceItem;

    public WPErrors(List<CustomizationError> issues, WPListItemBase source)
    {
      this.Issues = issues;
      this.SourceItem = source;
    }

    public void AddRange(List<CustomizationError> issues) => this.Issues.AddRange((IEnumerable<CustomizationError>) issues);

    public bool HasErrors => ((IEnumerable<CustomizationError>) this.Errors).Count<CustomizationError>() > 0;

    public bool HasWarnings => ((IEnumerable<CustomizationError>) this.Warnings).Count<CustomizationError>() > 0;

    public List<CustomizationError> Errors => ((IEnumerable<CustomizationError>) this.Issues).Where<CustomizationError>((Func<CustomizationError, bool>) (issue => issue.Severity == 1)).ToList<CustomizationError>();

    public List<CustomizationError> Warnings => ((IEnumerable<CustomizationError>) this.Issues).Where<CustomizationError>((Func<CustomizationError, bool>) (issue => issue.Severity == 0)).ToList<CustomizationError>();

    public string WarningsMessage
    {
      get
      {
        string warningsMessage = (string) null;
        if (this.HasWarnings)
        {
          foreach (CustomizationError warning in this.Warnings)
          {
            if (!string.IsNullOrEmpty(warningsMessage))
              warningsMessage += Environment.NewLine;
            warningsMessage += string.Format(Tools.GetString("msgCOSWarnings"), (object) warning.Message);
          }
        }
        return warningsMessage;
      }
    }

    public string ErrorsMessage
    {
      get
      {
        string errorsMessage = (string) null;
        if (this.HasErrors)
        {
          foreach (CustomizationError error in this.Errors)
          {
            if (!string.IsNullOrEmpty(errorsMessage))
              errorsMessage += Environment.NewLine;
            errorsMessage += string.Format(Tools.GetString("msgCOSErrors"), (object) error.Message);
          }
        }
        return errorsMessage;
      }
    }
  }
}
