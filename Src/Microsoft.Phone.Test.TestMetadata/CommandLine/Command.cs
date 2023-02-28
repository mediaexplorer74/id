// Command.cs
// Type: Microsoft.Phone.Test.TestMetadata.CommandLine.Command
// Assembly: Microsoft.Phone.Test.TestMetadata, Version=8.1.1702.2001, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: 8D6FC749-8FAD-45FC-9FEA-2CC8150A9765
// Assembly location: C:\Users\Admin\Desktop\d\Microsoft.Phone.Test.TestMetadata.dll

using System;
using System.IO;

namespace Microsoft.Phone.Test.TestMetadata.CommandLine
{
  public abstract class Command : IDisposable
  {
    ~Command() => this.Dispose(false);

    public int ReturnCode { get; set; }

    public TextWriter Output { get; set; }

    public TextWriter Error { get; set; }

    public CommandSpecification Specification { get; set; }

    public void Run()
    {
      if (this.Output == null)
        throw new CommandException("No output writer has been given for the command.");
      this.RunImplementation();
    }

    public void Load(CommandOptionCollection options) => this.LoadImplementation(options);

    protected abstract void RunImplementation();

    protected virtual void LoadImplementation(CommandOptionCollection options)
    {
    }

    protected virtual void Dispose(bool suppressFinalize)
    {
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }
  }
}
