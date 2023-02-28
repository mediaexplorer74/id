// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Common.HelpProvider
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using Microsoft.WindowsPhone.ImageDesigner.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Common
{
  public static class HelpProvider
  {
    public static readonly DependencyProperty HelpKeywordProperty = DependencyProperty.RegisterAttached("HelpKeyword", typeof (string), typeof (HelpProvider));
    private static readonly Dictionary<string, string> HelpLinks = ((Func<Dictionary<string, string>>) (() => new Dictionary<string, string>()
    {
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.GettingStarted),
        "html/794277eb-442e-4d08-81b2-4a77837832c8.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.SettingUp),
        "html/ceb5b0ff-8c35-4bc8-a861-79c908f1ab46.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.ModifyImage),
        "html/e20c7f8f-132f-485f-9c4b-cc5b630bed3e.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.PickOutputLocation),
        "html/e20c7f8f-132f-485f-9c4b-cc5b630bed3e.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.SelectImageType),
        "html/215a9300-f481-4106-814e-a128597c3b80.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.DescribeImage),
        "html/215a9300-f481-4106-814e-a128597c3b80.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.SelectTemplates),
        "html/c27938dd-7cc8-4b8a-9767-0e468d6d87b0.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.CustomizeOS),
        "html/c27938dd-7cc8-4b8a-9767-0e468d6d87b0.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.CustomizationChoice),
        "html/c27938dd-7cc8-4b8a-9767-0e468d6d87b0.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.BuildImage),
        "html/4afde6d8-b5a2-4646-8168-6b8375049adf.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.BuildSuccess),
        "html/4afde6d8-b5a2-4646-8168-6b8375049adf.htm"
      },
      {
        Enum.GetName(typeof (IDStates), (object) IDStates.FlashImage),
        "html/1c287073-b1da-4cc5-b8b7-5d005402df34.htm"
      },
      {
        "AppPinning",
        "html/c27938dd-7cc8-4b8a-9767-0e468d6d87b0.htm"
      },
      {
        "RetailSigning",
        "html/0e441e62-1ccf-4708-9f19-4d382d3bd5b7.htm"
      }
    }))();

    static HelpProvider() => CommandManager.RegisterClassCommandBinding(typeof (FrameworkElement), new CommandBinding((ICommand) ApplicationCommands.Help, new ExecutedRoutedEventHandler(HelpProvider.ShowHelp), new CanExecuteRoutedEventHandler(HelpProvider.CanHelpExecute)));

    public static void InitializeHelp()
    {
      CommandBinding commandBinding = new CommandBinding((ICommand) ApplicationCommands.Help);
      commandBinding.CanExecute += new CanExecuteRoutedEventHandler(HelpProvider.CanHelpExecute);
      commandBinding.Executed += new ExecutedRoutedEventHandler(HelpProvider.ShowHelp);
      System.Windows.Application.Current.MainWindow.CommandBindings.Add(commandBinding);
    }

    public static void SetHelpKeyword(DependencyObject obj, string value) => obj.SetValue(HelpProvider.HelpKeywordProperty, (object) value);

    public static string GetHelpKeyword(DependencyObject obj)
    {
      string helpKeyword = (string) null;
      if (obj != null)
        helpKeyword = obj.GetValue(HelpProvider.HelpKeywordProperty) as string;
      return helpKeyword;
    }

    public static string GetHelpUrl(string keyword)
    {
      string helpUrl = (string) null;
      if (keyword != null && HelpProvider.HelpLinks.ContainsKey(keyword))
        helpUrl = HelpProvider.HelpLinks[keyword];
      return helpUrl;
    }

    public static string GetDefaultHelpTopic()
    {
      string defaultHelpTopic = (string) null;
      IDController controller = IDContext.Instance.Controller;
      if (controller != null)
        defaultHelpTopic = HelpProvider.HelpLinks[Enum.GetName(typeof (IDStates), (object) controller.CurrentState)];
      return defaultHelpTopic;
    }

    public static void ShowHelp() => HelpProvider.ShowHelpInternal(HelpProvider.GetDefaultHelpTopic());

    public static void ShowHelp(DependencyObject d)
    {
      string helpUrl = HelpProvider.GetHelpUrl(HelpProvider.GetHelpKeyword(d));
      if (helpUrl != null)
        HelpProvider.ShowHelpInternal(helpUrl);
      else
        HelpProvider.ShowHelp();
    }

    public static void ShowHelp(string keyword) => HelpProvider.ShowHelpInternal(!HelpProvider.HelpLinks.ContainsKey(keyword) ? HelpProvider.GetDefaultHelpTopic() : HelpProvider.HelpLinks[keyword]);

    private static void ShowHelpInternal(string helpUrl)
    {
      string helpDocPath = Tools.GetHelpDocPath();
      if (helpDocPath == null)
        CustomMessageBox.ShowMessage(Tools.GetString("txtError"), Tools.GetString("txtNoHelpDoc"));
      else
        Help.ShowHelp((Control) null, helpDocPath, helpUrl);
    }

    private static void CanHelpExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

    private static void ShowHelp(object sender, ExecutedRoutedEventArgs e) => HelpProvider.ShowHelp((DependencyObject) (sender as FrameworkElement));
  }
}
