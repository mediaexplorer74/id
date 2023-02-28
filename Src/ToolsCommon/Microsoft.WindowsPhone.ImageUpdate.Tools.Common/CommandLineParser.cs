// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageUpdate.Tools.Common.CommandLineParser
// Assembly: ToolsCommon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 660CFE2F-EC6E-431A-97AF-69194F68E20E
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ToolsCommon.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
  public class CommandLineParser
  {
    private const string c_applicationNameString = "RESERVED_ID_APPLICATION_NAME";
    private const char DEFAULT_SWITCH = '/';
    private const char DEFAULT_DELIM = ':';
    private const string SWITCH_TOKEN = "switchToken";
    private const string ID_TOKEN = "idToken";
    private const string DELIM_TOKEN = "delimToken";
    private const string VALUE_TOKEN = "valueToken";
    private const int USAGE_COL1 = 25;
    private const int USAGE_WIDTH = 79;
    private char m_switchChar = '/';
    private char m_delimChar = ':';
    private string m_Syntax = "";
    private List<CommandLineParser.CArgument> m_declaredSwitches = new List<CommandLineParser.CArgument>();
    private List<CommandLineParser.CArgument> m_declaredParams = new List<CommandLineParser.CArgument>();
    private uint m_iRequiredParams;
    private List<CommandLineParser.CArgGroups> m_argGroups = new List<CommandLineParser.CArgGroups>();
    private SortedList<string, string> m_aliases = new SortedList<string, string>();
    private bool m_caseSensitive;
    private string m_lastError = "";
    private string m_usageCmdLine = "";
    private string m_usageArgs = "";
    private string m_usageGroups = "";
    private bool m_parseSuccess;

    public CommandLineParser() => this.BuildRegularExpression();

    public CommandLineParser(char yourOwnSwitch, char yourOwnDelimiter)
      : this()
    {
      this.m_switchChar = yourOwnSwitch;
      this.m_delimChar = yourOwnDelimiter;
    }

    public void SetOptionalSwitchNumeric(
      string id,
      string description,
      double defaultValue,
      double minRange,
      double maxRange)
    {
      this.DeclareNumericSwitch(id, description, true, defaultValue, minRange, maxRange);
    }

    public void SetOptionalSwitchNumeric(string id, string description, double defaultValue) => this.DeclareNumericSwitch(id, description, true, defaultValue, (double) int.MinValue, (double) int.MaxValue);

    public void SetRequiredSwitchNumeric(
      string id,
      string description,
      double minRange,
      double maxRange)
    {
      this.DeclareNumericSwitch(id, description, false, 0.0, minRange, maxRange);
    }

    public void SetRequiredSwitchNumeric(string id, string description) => this.DeclareNumericSwitch(id, description, false, 0.0, (double) int.MinValue, (double) int.MaxValue);

    public void SetOptionalSwitchString(
      string id,
      string description,
      string defaultValue,
      params string[] possibleValues)
    {
      this.DeclareStringSwitch(id, description, true, defaultValue, true, possibleValues);
    }

    public void SetOptionalSwitchString(
      string id,
      string description,
      string defaultValue,
      bool isPossibleValuesCaseSensitive,
      params string[] possibleValues)
    {
      this.DeclareStringSwitch(id, description, true, defaultValue, isPossibleValuesCaseSensitive, possibleValues);
    }

    public void SetOptionalSwitchString(string id, string description) => this.DeclareStringSwitch(id, description, true, "", true);

    public void SetRequiredSwitchString(
      string id,
      string description,
      params string[] possibleValues)
    {
      this.DeclareStringSwitch(id, description, false, "", true, possibleValues);
    }

    public void SetRequiredSwitchString(
      string id,
      string description,
      bool isPossibleValuesCaseSensitive,
      params string[] possibleValues)
    {
      this.DeclareStringSwitch(id, description, false, "", isPossibleValuesCaseSensitive, possibleValues);
    }

    public void SetRequiredSwitchString(string id, string description) => this.DeclareStringSwitch(id, description, false, "", true);

    public void SetOptionalSwitchBoolean(string id, string description, bool defaultValue) => this.DeclareBooleanSwitch(id, description, true, defaultValue);

    public void SetOptionalParameterNumeric(
      string id,
      string description,
      double defaultValue,
      double minRange,
      double maxRange)
    {
      this.DeclareParam_Numeric(id, description, true, defaultValue, minRange, maxRange);
    }

    public void SetOptionalParameterNumeric(string id, string description, double defaultValue) => this.DeclareParam_Numeric(id, description, true, defaultValue, (double) int.MinValue, (double) int.MaxValue);

    public void SetRequiredParameterNumeric(
      string id,
      string description,
      double minRange,
      double maxRange)
    {
      this.DeclareParam_Numeric(id, description, false, 0.0, minRange, maxRange);
    }

    public void SetRequiredParameterNumeric(string id, string description) => this.DeclareParam_Numeric(id, description, false, 0.0, (double) int.MinValue, (double) int.MaxValue);

    public void SetOptionalParameterString(
      string id,
      string description,
      string defaultValue,
      params string[] possibleValues)
    {
      this.DeclareStringParam(id, description, true, defaultValue, true, possibleValues);
    }

    public void SetOptionalParameterString(
      string id,
      string description,
      string defaultValue,
      bool isPossibleValuesCaseSensitive,
      params string[] possibleValues)
    {
      this.DeclareStringParam(id, description, true, defaultValue, isPossibleValuesCaseSensitive, possibleValues);
    }

    public void SetOptionalParameterString(string id, string description) => this.DeclareStringParam(id, description, true, "", true);

    public void SetRequiredParameterString(
      string id,
      string description,
      params string[] possibleValues)
    {
      this.DeclareStringParam(id, description, false, "", true, possibleValues);
    }

    public void SetRequiredParameterString(
      string id,
      string description,
      bool isPossibleValuesCaseSensitive,
      params string[] possibleValues)
    {
      this.DeclareStringParam(id, description, false, "", isPossibleValuesCaseSensitive, possibleValues);
    }

    public void SetRequiredParameterString(string id, string description) => this.DeclareStringParam(id, description, false, "", true);

    public bool ParseCommandLine()
    {
      this.SetFirstArgumentAsAppName();
      return this.ParseString(Environment.CommandLine);
    }

    public bool ParseString(string argumentsLine, bool isFirstArgTheAppName)
    {
      if (isFirstArgTheAppName)
        this.SetFirstArgumentAsAppName();
      return this.ParseString(argumentsLine);
    }

    public bool ParseString(string argumentsLine)
    {
      if (argumentsLine == null)
        throw new ArgumentNullException(nameof (argumentsLine));
      if (this.m_parseSuccess)
        throw new ParseFailedException("Cannot parse twice!");
      this.SetOptionalSwitchBoolean("?", "Displays this usage string", false);
      int num = 0;
      argumentsLine = argumentsLine.TrimStart() + " ";
      for (Match match = new Regex(this.m_Syntax).Match(argumentsLine); match.Success; match = match.NextMatch())
      {
        string token = match.Result("${switchToken}");
        string ID = match.Result("${idToken}");
        string delim = match.Result("${delimToken}");
        string val = match.Result("${valueToken}").TrimEnd();
        if (val.StartsWith("\"", StringComparison.CurrentCulture) && val.EndsWith("\"", StringComparison.CurrentCulture))
          val = val.Substring(1, val.Length - 2);
        if (ID.Length == 0)
        {
          if (!this.InputParam(val, num++))
            return false;
        }
        else
        {
          if (ID == "?")
          {
            this.m_lastError = "Usage Info requested";
            this.m_parseSuccess = false;
            return false;
          }
          if (!this.InputSwitch(token, ID, delim, val))
            return false;
        }
      }
      foreach (CommandLineParser.CArgument declaredSwitch in this.m_declaredSwitches)
      {
        if (!declaredSwitch.isOptional && !declaredSwitch.isAssigned)
        {
          this.m_lastError = "Required switch '" + declaredSwitch.Id + "' was not assigned a value";
          return false;
        }
      }
      foreach (CommandLineParser.CArgument declaredParam in this.m_declaredParams)
      {
        if (!declaredParam.isOptional && !declaredParam.isAssigned)
        {
          this.m_lastError = "Required parameter '" + declaredParam.Id + "' was not assigned a value";
          return false;
        }
      }
      this.m_parseSuccess = this.IsGroupRulesKept();
      return this.m_parseSuccess;
    }

    public object GetSwitch(string id)
    {
      if (!this.m_parseSuccess)
        throw new ParseFailedException(this.LastError);
      return ((!(id == "RESERVED_ID_APPLICATION_NAME") ? this.FindExactArg(id, this.m_declaredSwitches) : throw new ParseException("RESERVED_ID_APPLICATION_NAME is a reserved internal id and must not be used")) ?? throw new NoSuchArgumentException("switch", id)).GetValue();
    }

    public double GetSwitchAsNumeric(string id) => (double) this.GetSwitch(id);

    public string GetSwitchAsString(string id) => (string) this.GetSwitch(id);

    public bool GetSwitchAsBoolean(string id) => (bool) this.GetSwitch(id);

    public bool IsAssignedSwitch(string id)
    {
      if (!this.m_parseSuccess)
        throw new ParseFailedException(this.LastError);
      return ((!(id == "RESERVED_ID_APPLICATION_NAME") ? this.FindExactArg(id, this.m_declaredSwitches) : throw new ParseException("RESERVED_ID_APPLICATION_NAME is a reserved internal id and must not be used")) ?? throw new NoSuchArgumentException("switch", id)).isAssigned;
    }

    public object GetParameter(string id)
    {
      if (!this.m_parseSuccess)
        throw new ParseFailedException(this.LastError);
      return ((!(id == "RESERVED_ID_APPLICATION_NAME") ? this.FindExactArg(id, this.m_declaredParams) : throw new ParseException("RESERVED_ID_APPLICATION_NAME is a reserved internal id and must not be used")) ?? throw new NoSuchArgumentException("parameter", id)).GetValue();
    }

    public double GetParameterAsNumeric(string id) => (double) this.GetParameter(id);

    public string GetParameterAsString(string id) => (string) this.GetParameter(id);

    public bool IsAssignedParameter(string id)
    {
      if (!this.m_parseSuccess)
        throw new ParseFailedException(this.LastError);
      return ((!(id == "RESERVED_ID_APPLICATION_NAME") ? this.FindExactArg(id, this.m_declaredParams) : throw new ParseException("RESERVED_ID_APPLICATION_NAME is a reserved internal id and must not be used")) ?? throw new NoSuchArgumentException("parameter", id)).isAssigned;
    }

    public object[] GetParameterList()
    {
      int num = this.IsFirstArgumentAppName() ? 1 : 0;
      if (this.m_declaredParams.Count == num)
        return (object[]) null;
      object[] parameterList = new object[this.m_declaredParams.Count - num];
      for (int index = num; index < this.m_declaredParams.Count; ++index)
        parameterList[index - num] = this.m_declaredParams[index].GetValue();
      return parameterList;
    }

    public Array SwitchesList()
    {
      Array instance = Array.CreateInstance(typeof (object), this.m_declaredSwitches.Count, 2);
      for (int index = 0; index < this.m_declaredSwitches.Count; ++index)
      {
        instance.SetValue((object) this.m_declaredSwitches[index].Id, index, 1);
        instance.SetValue(this.m_declaredSwitches[index].GetValue(), index, 0);
      }
      return instance;
    }

    public void SetAlias(string alias, string treatedAs)
    {
      if (!(alias != treatedAs))
        return;
      this.m_aliases[alias] = treatedAs;
    }

    [CLSCompliant(false)]
    public void DefineSwitchGroup(uint minAppear, uint maxAppear, params string[] ids)
    {
      if (ids == null)
        throw new ArgumentNullException(nameof (ids));
      if (ids.Length < 2 || maxAppear < minAppear || maxAppear == 0U)
        throw new BadGroupException("A group must have at least two members");
      if (minAppear == 0U && (long) maxAppear == (long) ids.Length)
        return;
      if ((long) minAppear > (long) ids.Length)
        throw new BadGroupException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "You cannot have {0} appearance(s) in a group of {1} switch(es)!", new object[2]
        {
          (object) minAppear,
          (object) ids.Length
        }));
      foreach (string id in ids)
      {
        if (this.FindExactArg(id, this.m_declaredSwitches) == null)
          throw new NoSuchArgumentException("switch", id);
      }
      CommandLineParser.CArgGroups cargGroups = new CommandLineParser.CArgGroups(minAppear, maxAppear, ids);
      this.m_argGroups.Add(cargGroups);
      if (this.m_usageGroups.Length == 0)
        this.m_usageGroups = "NOTES:" + Environment.NewLine;
      CommandLineParser commandLineParser = this;
      commandLineParser.m_usageGroups = commandLineParser.m_usageGroups + " - " + cargGroups.RangeDescription() + Environment.NewLine;
    }

    public string UsageString() => this.UsageString(new FileInfo(Environment.GetCommandLineArgs()[0]).Name);

    public string UsageString(string appName)
    {
      string str = "";
      if (this.m_lastError.Length != 0)
        str = ">> " + this.m_lastError + Environment.NewLine + Environment.NewLine;
      return str + "Usage: " + appName + this.m_usageCmdLine + Environment.NewLine + Environment.NewLine + this.m_usageArgs + Environment.NewLine + this.m_usageGroups + Environment.NewLine;
    }

    public bool CaseSensitive
    {
      get => this.m_caseSensitive;
      set
      {
        this.m_caseSensitive = value;
        this.CheckNotAmbiguous();
      }
    }

    public string LastError => this.m_lastError.Length == 0 ? "There was no error" : this.m_lastError;

    private void SetFirstArgumentAsAppName()
    {
      if (this.m_declaredParams.Count > 0 && this.m_declaredParams[0].Id == "RESERVED_ID_APPLICATION_NAME")
        return;
      this.CheckNotAmbiguous("RESERVED_ID_APPLICATION_NAME");
      this.m_declaredParams.Insert(0, (CommandLineParser.CArgument) new CommandLineParser.CStringArgument("RESERVED_ID_APPLICATION_NAME", "the application's name", false, "", true, new string[0]));
      ++this.m_iRequiredParams;
    }

    private void BuildRegularExpression() => this.m_Syntax = "\\G((?<switchToken>[\\+\\-" + (object) this.m_switchChar + "]{1})(?<idToken>[\\w|?]+)(?<delimToken>[" + (object) this.m_delimChar + "]?))?(?<valueToken>(\"[^\"]*\"|\\S*)\\s+){1}";

    private void DeclareNumericSwitch(
      string id,
      string description,
      bool fIsOptional,
      double defaultValue,
      double minRange,
      double maxRange)
    {
      if (id.Length == 0)
        throw new EmptyArgumentDeclaredException();
      this.CheckNotAmbiguous(id);
      CommandLineParser.CArgument cargument = (CommandLineParser.CArgument) new CommandLineParser.CNumericArgument(id, description, fIsOptional, defaultValue, minRange, maxRange);
      this.m_declaredSwitches.Add(cargument);
      this.AddUsageInfo(cargument, true, (object) defaultValue);
    }

    private void DeclareStringSwitch(
      string id,
      string description,
      bool fIsOptional,
      string defaultValue,
      bool isPossibleValuesCaseSensitive,
      params string[] possibleValues)
    {
      if (id.Length == 0)
        throw new EmptyArgumentDeclaredException();
      this.CheckNotAmbiguous(id);
      CommandLineParser.CArgument cargument = (CommandLineParser.CArgument) new CommandLineParser.CStringArgument(id, description, fIsOptional, defaultValue, isPossibleValuesCaseSensitive, possibleValues);
      this.m_declaredSwitches.Add(cargument);
      this.AddUsageInfo(cargument, true, (object) defaultValue);
    }

    private void DeclareBooleanSwitch(
      string id,
      string description,
      bool fIsOptional,
      bool defaultValue)
    {
      if (id.Length == 0)
        throw new EmptyArgumentDeclaredException();
      this.CheckNotAmbiguous(id);
      CommandLineParser.CArgument cargument = (CommandLineParser.CArgument) new CommandLineParser.CBooleanArgument(id, description, fIsOptional, defaultValue);
      this.m_declaredSwitches.Add(cargument);
      this.AddUsageInfo(cargument, true, (object) defaultValue);
    }

    private void DeclareParam_Numeric(
      string id,
      string description,
      bool fIsOptional,
      double defaultValue,
      double minRange,
      double maxRange)
    {
      if (id.Length == 0)
        throw new EmptyArgumentDeclaredException();
      if (!fIsOptional && (long) this.m_declaredParams.Count > (long) this.m_iRequiredParams)
        throw new RequiredParameterAfterOptionalParameterException();
      this.CheckNotAmbiguous(id);
      CommandLineParser.CArgument cargument = (CommandLineParser.CArgument) new CommandLineParser.CNumericArgument(id, description, fIsOptional, defaultValue, minRange, maxRange);
      if (!fIsOptional)
        ++this.m_iRequiredParams;
      this.m_declaredParams.Add(cargument);
      this.AddUsageInfo(cargument, false, (object) defaultValue);
    }

    private void DeclareStringParam(
      string id,
      string description,
      bool fIsOptional,
      string defaultValue,
      bool isPossibleValuesCaseSensitive,
      params string[] possibleValues)
    {
      if (id.Length == 0)
        throw new EmptyArgumentDeclaredException();
      if (!fIsOptional && (long) this.m_declaredParams.Count > (long) this.m_iRequiredParams)
        throw new RequiredParameterAfterOptionalParameterException();
      this.CheckNotAmbiguous(id);
      CommandLineParser.CArgument cargument = (CommandLineParser.CArgument) new CommandLineParser.CStringArgument(id, description, fIsOptional, defaultValue, isPossibleValuesCaseSensitive, possibleValues);
      if (!fIsOptional)
        ++this.m_iRequiredParams;
      this.m_declaredParams.Add(cargument);
      this.AddUsageInfo(cargument, false, (object) defaultValue);
    }

    private void AddUsageInfo(CommandLineParser.CArgument arg, bool isSwitch, object defVal)
    {
      this.m_usageCmdLine += arg.isOptional ? " [" : " ";
      if (isSwitch)
      {
        if (arg.GetType() != typeof (CommandLineParser.CBooleanArgument))
        {
          CommandLineParser commandLineParser = this;
          commandLineParser.m_usageCmdLine = commandLineParser.m_usageCmdLine + this.m_switchChar.ToString() + arg.Id + this.m_delimChar.ToString() + "x";
        }
        else if (arg.Id == "?")
        {
          CommandLineParser commandLineParser = this;
          commandLineParser.m_usageCmdLine = commandLineParser.m_usageCmdLine + this.m_switchChar.ToString() + "?";
        }
        else
        {
          CommandLineParser commandLineParser = this;
          commandLineParser.m_usageCmdLine = commandLineParser.m_usageCmdLine + "[+|-]" + arg.Id;
        }
      }
      else
        this.m_usageCmdLine += arg.Id;
      this.m_usageCmdLine += arg.isOptional ? "]" : "";
      string str1 = (arg.Id == "?" || isSwitch && arg.GetType() != typeof (CommandLineParser.CBooleanArgument) ? this.m_switchChar.ToString() : "") + arg.Id;
      if (arg.isOptional)
        str1 = "[" + str1 + "]";
      string str2 = "  " + str1.PadRight(22, '·') + " " + arg.description;
      if (arg.Id != "?")
      {
        str2 = str2 + ". Values: " + arg.possibleValues();
        if (arg.isOptional)
          str2 = str2 + "; default= " + defVal.ToString();
      }
      StringBuilder stringBuilder = new StringBuilder();
      while (str2.Length > 0)
      {
        if (str2.Length <= 79)
        {
          CommandLineParser commandLineParser = this;
          commandLineParser.m_usageArgs = commandLineParser.m_usageArgs + str2 + Environment.NewLine;
          break;
        }
        int num = 79;
        while (num > 69 && str2[num] != ' ')
          --num;
        if (num <= 69)
          num = 79;
        CommandLineParser commandLineParser1 = this;
        commandLineParser1.m_usageArgs = commandLineParser1.m_usageArgs + str2.Substring(0, num) + Environment.NewLine;
        str2 = str2.Substring(num).TrimStart();
        if (str2.Length > 0)
        {
          stringBuilder.Append("".PadLeft(25, ' '));
          stringBuilder.Append(str2);
          str2 = stringBuilder.ToString();
          stringBuilder.Remove(0, stringBuilder.Length);
        }
      }
    }

    private bool InputSwitch(string token, string ID, string delim, string val)
    {
      if (this.m_aliases.ContainsKey(ID))
        ID = this.m_aliases[ID];
      CommandLineParser.CArgument similarArg = this.FindSimilarArg(ID, this.m_declaredSwitches);
      if (similarArg == null)
        return false;
      if (similarArg.GetType() == typeof (CommandLineParser.CBooleanArgument))
      {
        similarArg.SetValue(token);
        if (delim.Length == 0 && val.Length == 0)
          return true;
        this.m_lastError = "A boolean switch cannot be followed by a delimiter. Use \"-booleanFlag\", not \"-booleanFlag" + (object) this.m_delimChar + "\"";
        return false;
      }
      if (delim.Length == 0)
      {
        this.m_lastError = "you must use the delimiter '" + (object) this.m_delimChar + "', e.g. \"" + (object) this.m_switchChar + "arg" + (object) this.m_delimChar + "x\"";
        return false;
      }
      if (similarArg.SetValue(val))
        return true;
      this.m_lastError = "Switch '" + ID + "' cannot accept '" + val + "' as a value";
      return false;
    }

    [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", Justification = "paramIndex is validated.")]
    private bool InputParam(string val, int paramIndex)
    {
      if (int.MaxValue == paramIndex)
      {
        this.m_lastError = "paramIndex must be less than Int32.MaxValue";
        return false;
      }
      if (this.m_declaredParams.Count < paramIndex + 1)
      {
        this.m_lastError = "Command-line has too many parameters";
        return false;
      }
      CommandLineParser.CArgument declaredParam = this.m_declaredParams[paramIndex];
      if (declaredParam.SetValue(val))
        return true;
      this.m_lastError = "Parameter '" + declaredParam.Id + "' did not have a legal value";
      return false;
    }

    private CommandLineParser.CArgument FindExactArg(
      string argID,
      List<CommandLineParser.CArgument> list)
    {
      foreach (CommandLineParser.CArgument exactArg in list)
      {
        if (string.Compare(exactArg.Id, argID, !this.CaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
          return exactArg;
      }
      return (CommandLineParser.CArgument) null;
    }

    private CommandLineParser.CArgument FindSimilarArg(
      string argSubstringID,
      List<CommandLineParser.CArgument> list)
    {
      argSubstringID = this.CaseSensitive ? argSubstringID : argSubstringID.ToUpper(CultureInfo.InvariantCulture);
      CommandLineParser.CArgument similarArg = (CommandLineParser.CArgument) null;
      foreach (CommandLineParser.CArgument cargument in list)
      {
        string str1 = this.CaseSensitive ? cargument.Id : cargument.Id.ToUpper(CultureInfo.InvariantCulture);
        if (str1.StartsWith(argSubstringID, StringComparison.CurrentCulture))
        {
          if (similarArg != null)
          {
            string str2 = this.CaseSensitive ? similarArg.Id : similarArg.Id.ToUpper(CultureInfo.InvariantCulture);
            this.m_lastError = "Ambiguous ID: '" + argSubstringID + "' matches both '" + str2 + "' and '" + str1 + "'";
            return (CommandLineParser.CArgument) null;
          }
          similarArg = cargument;
        }
      }
      if (similarArg == null)
        this.m_lastError = "No such argument '" + argSubstringID + "'";
      return similarArg;
    }

    private void CheckNotAmbiguous() => this.CheckNotAmbiguous("");

    private void CheckNotAmbiguous(string newId)
    {
      this.CheckNotAmbiguous(newId, this.m_declaredSwitches);
      this.CheckNotAmbiguous(newId, this.m_declaredParams);
    }

    private void CheckNotAmbiguous(string newID, List<CommandLineParser.CArgument> argList)
    {
      foreach (CommandLineParser.CArgument cargument1 in argList)
      {
        if (string.Compare(cargument1.Id, newID, !this.CaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
          throw new ArgumentAlreadyDeclaredException(cargument1.Id);
        if (newID.Length != 0 && (cargument1.Id.StartsWith(newID, !this.CaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) || newID.StartsWith(cargument1.Id, !this.CaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
          throw new AmbiguousArgumentException(cargument1.Id, newID);
        foreach (CommandLineParser.CArgument cargument2 in argList)
        {
          if (!cargument1.Equals((object) cargument2))
          {
            if (string.Compare(cargument1.Id, cargument2.Id, !this.CaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
              throw new ArgumentAlreadyDeclaredException(cargument1.Id);
            if (cargument1.Id.StartsWith(cargument2.Id, !this.CaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) || cargument2.Id.StartsWith(cargument1.Id, !this.CaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
              throw new AmbiguousArgumentException(cargument1.Id, cargument2.Id);
          }
        }
      }
    }

    private bool IsGroupRulesKept()
    {
      foreach (CommandLineParser.CArgGroups argGroup in this.m_argGroups)
      {
        uint num = 0;
        foreach (string argID in argGroup.Args)
        {
          CommandLineParser.CArgument exactArg = this.FindExactArg(argID, this.m_declaredSwitches);
          if (exactArg != null && exactArg.isAssigned)
            ++num;
        }
        if (!argGroup.InRange(num))
        {
          this.m_lastError = argGroup.RangeDescription();
          return false;
        }
      }
      return true;
    }

    private bool IsFirstArgumentAppName() => this.m_declaredParams.Count > 0 && this.m_declaredParams[0].Id == "RESERVED_ID_APPLICATION_NAME";

    internal abstract class CArgument
    {
      protected object m_val = (object) "";
      protected bool m_fIsAssigned;
      private string m_id = "";
      private string m_description = "";
      private bool m_fIsOptional = true;

      protected CArgument(string id, string desc, bool fIsOptional)
      {
        this.m_id = id;
        this.m_description = desc;
        this.m_fIsOptional = fIsOptional;
      }

      public string Id => this.m_id;

      public object GetValue() => this.m_val;

      public abstract bool SetValue(string val);

      public abstract string possibleValues();

      public string description => this.m_description.Length == 0 ? this.m_id : this.m_description;

      public bool isOptional => this.m_fIsOptional;

      public bool isAssigned => this.m_fIsAssigned;
    }

    internal class CNumericArgument : CommandLineParser.CArgument
    {
      private double m_minRange = double.MinValue;
      private double m_maxRange = double.MaxValue;

      public CNumericArgument(
        string id,
        string desc,
        bool fIsOptional,
        double defVal,
        double minRange,
        double maxRange)
        : base(id, desc, fIsOptional)
      {
        this.m_val = (object) defVal;
        this.m_minRange = minRange;
        this.m_maxRange = maxRange;
      }

      public override bool SetValue(string val)
      {
        int num = this.isAssigned ? 1 : 0;
        this.m_fIsAssigned = true;
        try
        {
          if (val.ToLowerInvariant().StartsWith("0x", StringComparison.CurrentCulture))
            this.m_val = (object) (double) int.Parse(val.Substring(2), NumberStyles.HexNumber, (IFormatProvider) CultureInfo.InvariantCulture);
          else
            this.m_val = (object) double.Parse(val, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture);
        }
        catch (ArgumentNullException ex)
        {
            Debug.WriteLine("[ex] Null Argument Exception: " + ex.Message);
            return false;
        }
        catch (FormatException ex)
        {
            Debug.WriteLine("[ex] Format Exception: " + ex.Message);

            return false;
        }
        catch (OverflowException ex)
        {
            Debug.WriteLine("[ex] Overflow Exception: " + ex.Message);
            return false;
        }
        return (double) this.m_val >= this.m_minRange && (double) this.m_val <= this.m_maxRange;
      }

      public override string possibleValues() => "between " + (object) this.m_minRange + " and " + (object) this.m_maxRange;
    }

    internal class CStringArgument : CommandLineParser.CArgument
    {
      private string[] m_possibleVals = new string[1]{ "" };
      private bool m_fIsPossibleValsCaseSensitive = true;

      public CStringArgument(
        string id,
        string desc,
        bool fIsOptional,
        string defVal,
        bool isPossibleValuesCaseSensitive,
        params string[] possibleValues)
        : base(id, desc, fIsOptional)
      {
        this.m_possibleVals = possibleValues;
        this.m_val = (object) defVal;
        this.m_fIsPossibleValsCaseSensitive = isPossibleValuesCaseSensitive;
      }

      public override bool SetValue(string val)
      {
        int num = this.isAssigned ? 1 : 0;
        this.m_fIsAssigned = true;
        this.m_val = (object) val;
        if (this.m_possibleVals.Length == 0)
          return true;
        foreach (string possibleVal in this.m_possibleVals)
        {
          if ((string) this.m_val == possibleVal || !this.m_fIsPossibleValsCaseSensitive && string.Compare((string) this.m_val, possibleVal, StringComparison.OrdinalIgnoreCase) == 0)
            return true;
        }
        return false;
      }

      public override string possibleValues()
      {
        if (this.m_possibleVals.Length == 0)
          return "free text";
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("{");
        stringBuilder.Append(this.m_fIsPossibleValsCaseSensitive ? this.m_possibleVals[0] : this.m_possibleVals[0].ToLowerInvariant());
        for (int index = 1; index < this.m_possibleVals.Length; ++index)
        {
          stringBuilder.Append("|");
          stringBuilder.Append(this.m_fIsPossibleValsCaseSensitive ? this.m_possibleVals[index] : this.m_possibleVals[index].ToLowerInvariant());
        }
        stringBuilder.Append("}");
        return stringBuilder.ToString();
      }
    }

    internal class CBooleanArgument : CommandLineParser.CArgument
    {
      public CBooleanArgument(string id, string desc, bool fIsOptional, bool defVal)
        : base(id, desc, fIsOptional)
      {
        this.m_val = (object) defVal;
      }

      public override bool SetValue(string token)
      {
        int num = this.isAssigned ? 1 : 0;
        this.m_fIsAssigned = true;
        this.m_val = (object) (token != "-");
        return true;
      }

      public override string possibleValues() => "precede by [+] or [-]";
    }

    internal class CArgGroups
    {
      public uint m_minAppear;
      public uint m_maxAppear;
      private string[] m_args;

      public CArgGroups(uint min, uint max, params string[] args)
      {
        this.m_minAppear = min;
        this.m_maxAppear = max;
        this.m_args = args;
      }

      public bool InRange(uint num) => num >= this.m_minAppear && num <= this.m_maxAppear;

      public string[] Args => this.m_args;

      public string ArgList()
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("{");
        foreach (string str in this.Args)
        {
          stringBuilder.Append(",");
          stringBuilder.Append(str);
        }
        return stringBuilder.ToString().Replace("{,", "{") + "}";
      }

      public string RangeDescription()
      {
        if (this.m_minAppear == 1U && this.m_maxAppear == 1U)
          return "one of the switches " + this.ArgList() + " must be used exclusively";
        if (this.m_minAppear == 1U && (long) this.m_maxAppear == (long) this.Args.Length)
          return "one or more of the switches " + this.ArgList() + " must be used";
        if (this.m_minAppear == 1U && this.m_maxAppear > 1U)
          return "one (but not more than " + (object) this.m_maxAppear + ") of the switches " + this.ArgList() + " must be used";
        if (this.m_minAppear == 0U && this.m_maxAppear == 1U)
          return "only one of the switches " + this.ArgList() + " can be used";
        return this.m_minAppear == 0U && this.m_maxAppear > 1U ? "only " + (object) this.m_maxAppear + " of the switches " + this.ArgList() + " can be used" : "between " + (object) this.m_minAppear + " and " + (object) this.m_maxAppear + " of the switches " + this.ArgList() + " must be used";
      }
    }
  }
}
