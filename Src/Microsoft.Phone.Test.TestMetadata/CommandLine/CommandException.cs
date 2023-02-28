// Decompiled with JetBrains decompiler
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.CommandException
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  [Serializable]
  public class CommandException : Exception
  {
    private Command _command;

    public CommandException()
    {
    }

    public CommandException(string message)
      : base(message)
    {
    }

    public CommandException(string message, Exception exception)
      : base(message, exception)
    {
    }

    protected CommandException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public CommandException(string message, Command command)
      : base(message)
    {
      this._command = command;
    }

    public Command Command
    {
      get => this._command;
      set => this._command = value;
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context) => base.GetObjectData(info, context);
  }
}
