using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SheetMusicOrganizer.ViewModel.Tools
{
    public class SmartCollection<T> : ObservableCollection<T>
    {
        public SmartCollection() : base()
        {
        }

        public SmartCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public SmartCollection(List<T> list) : base(list)
        {
        }

        //doesnt seem to be a better way to do it
        //https://stackoverflow.com/questions/7449196/how-can-i-raise-a-collectionchanged-event-on-an-observablecollection-and-pass-i
        /*public void AddRange(IEnumerable<T> range)
        {
            /*foreach (var item in range)
            {
                Items.Add(item);
            }

            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));*/
        /*foreach (var item in range)
            Add(item);
    }

    public void RemoveRange(IEnumerable<T> range)
    {
        foreach (var item in range)
            Remove(item);
    }*/

        public void Reset(IEnumerable<T>? range = null)
        {
            this.Items.Clear();

            if(range != null)
                foreach (var item in range)
                    this.Items.Add(item);

            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /*
        public void Reset(T item)
        {
            this.Items.Clear();
            Items.Add(item);

            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    */
    }
}
