using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace OWCE.Extensions
{
    // Minimal replacement for MvvmHelpers.ObservableRangeCollection (the package is no longer maintained).
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        public ObservableRangeCollection() : base() { }

        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection) { }

        public void AddRange(IEnumerable<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (var item in collection)
            {
                Items.Add(item);
            }

            NotifyReset();
        }

        public void ReplaceRange(IEnumerable<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            Items.Clear();
            foreach (var item in collection)
            {
                Items.Add(item);
            }

            NotifyReset();
        }

        void NotifyReset()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
