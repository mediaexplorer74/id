// Decompiled with JetBrains decompiler
// Type: Microsoft.WindowsPhone.ImageDesigner.UI.Common.Transition
// Assembly: ImageDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: AACE6177-DC79-444E-B25E-2FEDC45F69E4
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesigner.exe

using System.Windows;

namespace Microsoft.WindowsPhone.ImageDesigner.UI.Common
{
  public class Transition : FrameworkElement
  {
    public static DependencyProperty SourceProperty = DependencyProperty.Register(nameof (Source), typeof (object), typeof (Transition), new PropertyMetadata((PropertyChangedCallback) ((obj, args) => ((Transition) obj).Swap())));
    public static DependencyProperty DisplayAProperty = DependencyProperty.Register(nameof (DisplayA), typeof (object), typeof (Transition));
    public static DependencyProperty DisplayBProperty = DependencyProperty.Register(nameof (DisplayB), typeof (object), typeof (Transition));
    public static DependencyProperty StateProperty = DependencyProperty.Register(nameof (State), typeof (Transition.TransitionState), typeof (Transition), new PropertyMetadata((object) Transition.TransitionState.A));

    public object Source
    {
      get => this.GetValue(Transition.SourceProperty);
      set => this.SetValue(Transition.SourceProperty, value);
    }

    public object DisplayA
    {
      get => this.GetValue(Transition.DisplayAProperty);
      set => this.SetValue(Transition.DisplayAProperty, value);
    }

    public object DisplayB
    {
      get => this.GetValue(Transition.DisplayBProperty);
      set => this.SetValue(Transition.DisplayBProperty, value);
    }

    public Transition.TransitionState State
    {
      get => (Transition.TransitionState) this.GetValue(Transition.StateProperty);
      set => this.SetValue(Transition.StateProperty, (object) value);
    }

    private void Swap()
    {
      if (this.State == Transition.TransitionState.A)
      {
        this.DisplayB = this.Source;
        this.State = Transition.TransitionState.B;
      }
      else
      {
        this.DisplayA = this.Source;
        this.State = Transition.TransitionState.A;
      }
    }

    public enum TransitionState
    {
      A,
      B,
    }
  }
}
