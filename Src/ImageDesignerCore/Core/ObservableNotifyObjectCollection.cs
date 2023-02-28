// ObservableNotifyObjectCollection.cs
// Type: Microsoft.WindowsPhone.ImageDesigner.Core.ObservableNotifyObjectCollection
// Assembly: ImageDesignerCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b3f029d4c9c2ec30
// MVID: A00BBFA4-FB4D-4867-990A-673A22716507
// Assembly location: C:\Users\Admin\Desktop\ImageDesigner\64\ImageDesigner\ImageDesignerCore.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageDesigner.Core
{
  public class ObservableNotifyObjectCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
  {
    private HashSet<string> _observableProperties;

    public ObservableNotifyObjectCollection(params string[] observableProperties) => this._observableProperties = new HashSet<string>((IEnumerable<string>) ((IEnumerable<string>) observableProperties).ToList<string>());

    public void AddItems(IEnumerable<T> list) => list.ToList<T>().ForEach((Action<T>) (o => this.Add(o)));

    public event ObjectPropertyChangedEventHandler ObjectPropertyChanged;

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
      base.OnCollectionChanged(args);
      if (args.NewItems != null)
      {
        foreach (INotifyPropertyChanged newItem in (IEnumerable) args.NewItems)
          newItem.PropertyChanged += new PropertyChangedEventHandler(this.OnObjectPropertyChanged);
      }
      if (args.OldItems == null)
        return;
      foreach (INotifyPropertyChanged oldItem in (IEnumerable) args.OldItems)
        oldItem.PropertyChanged -= new PropertyChangedEventHandler(this.OnObjectPropertyChanged);
    }

    private void OnObjectPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
      if (this.ObjectPropertyChanged == null || !this._observableProperties.Contains<string>(args.PropertyName, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
        return;
      this.ObjectPropertyChanged((object) this, new ObjectPropertyChangedEventArgs(sender, args.PropertyName));
    }
  }
}
