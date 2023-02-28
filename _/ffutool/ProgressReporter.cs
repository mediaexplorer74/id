// Decompiled with JetBrains decompiler
// Type: FFUTool.ProgressReporter
// Assembly: FFUTool, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E16401E-1D4B-42FF-8522-F3B0C09CB0D5
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ffutool.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace FFUTool
{
  public class ProgressReporter
  {
    private const double OneMegabyte = 1048576.0;
    private int width;
    private Stopwatch stopwatch;
    private int summaryCount;
    private Queue<Tuple<double, long>> progressPoints;

    public ProgressReporter(int progressWidth)
    {
      this.width = progressWidth;
      this.stopwatch = Stopwatch.StartNew();
      this.summaryCount = 0;
      this.progressPoints = new Queue<Tuple<double, long>>();
    }

    public string CreateProgressDisplay(long position, long totalLength)
    {
      double num1 = (double) position / (double) totalLength;
      if (num1 > 1.0)
        num1 = 1.0;
      int num2 = (int) Math.Floor(50.0 * num1);
      StringBuilder stringBuilder = new StringBuilder(2 * this.width);
      for (int index = 0; index < this.width; ++index)
        stringBuilder.Append('\b');
      stringBuilder.Append('[');
      for (int index = 0; index < num2; ++index)
        stringBuilder.Append('=');
      if (num2 < 50)
      {
        stringBuilder.Append('>');
        ++num2;
      }
      for (int index = num2; index < 50; ++index)
        stringBuilder.Append(' ');
      stringBuilder.Append("]  ");
      stringBuilder.AppendFormat("{0:0.00%}", (object) num1);
      stringBuilder.AppendFormat(" {0}", (object) this.GetSpeedString(position));
      for (int length = stringBuilder.Length; length < 2 * this.width - 1; ++length)
        stringBuilder.Append(' ');
      if (position == totalLength)
      {
        this.stopwatch.Stop();
        if (Interlocked.Add(ref this.summaryCount, 1) == 1)
        {
          double num3 = (double) totalLength / 1048576.0;
          stringBuilder.AppendLine();
          stringBuilder.AppendFormat("Transferred {0:0.00} MB in {1:0.00} seconds, overall rate {2:0.00} MB/s.", (object) num3, (object) this.stopwatch.Elapsed.TotalSeconds, (object) (num3 / this.stopwatch.Elapsed.TotalSeconds));
        }
        else
          stringBuilder.Clear();
      }
      return stringBuilder.ToString();
    }

    private string GetSpeedString(long position)
    {
      string speedString = string.Empty;
      this.progressPoints.Enqueue(new Tuple<double, long>(this.stopwatch.Elapsed.TotalSeconds, position));
      if (this.progressPoints.Count >= 8)
      {
        speedString = this.GetSpeedFromPoints(this.progressPoints.ToArray());
        this.progressPoints.Dequeue();
      }
      return speedString;
    }

    private string GetSpeedFromPoints(Tuple<double, long>[] points)
    {
      double num1 = 0.0;
      for (int index = 1; index < points.Length; ++index)
      {
        double num2 = (double) (points[index].Item2 - points[index - 1].Item2) / 1048576.0;
        double num3 = points[index].Item1 - points[index - 1].Item1;
        num1 += num2 / num3 / (double) (points.Length - 1);
      }
      return string.Format("({0:0.00} MB/s)", (object) num1);
    }
  }
}
