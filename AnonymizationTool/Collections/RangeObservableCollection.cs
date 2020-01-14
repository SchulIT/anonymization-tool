using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AnonymizationTool.Collections
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool suppressCollectionChangedEvent = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (suppressCollectionChangedEvent == false)
            {
                base.OnCollectionChanged(e);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if(items == null)
            {
                throw new ArgumentNullException("Items must not be null");
            }

            suppressCollectionChangedEvent = true;

            foreach(T item in items)
            {
                Add(item);
            }

            suppressCollectionChangedEvent = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
