using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace AnonymizationTool.Behaviors
{
    public class SelectedItemsBehavior : Behavior<ListView>
    {
        public static DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(IEnumerable), typeof(SelectedItemsBehavior), new PropertyMetadata(OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SelectedItemsBehavior).OnSelectedItemsChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
        }

        private void OnSelectedItemsChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            var oldCollection = oldValue as INotifyCollectionChanged;

            if(oldCollection != null)
            {
                oldCollection.CollectionChanged -= OnSelectedItemsCollectionChanged;
            }

            var newCollection = newValue as INotifyCollectionChanged;

            if(newCollection != null)
            {
                newCollection.CollectionChanged += OnSelectedItemsCollectionChanged;
            }
        }

        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(AssociatedObject == null)
            {
                return;
            }

            foreach(var addedItem in e.NewItems)
            {
                AssociatedObject.SelectedItems.Add(addedItem);
            }

            foreach(var removedItem in e.OldItems)
            {
                AssociatedObject.SelectedItems.Remove(removedItem);
            }
        }

        public IEnumerable SelectedItems
        {
            get { return (IEnumerable)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnListViewSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= OnListViewSelectionChanged;
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var boundList = SelectedItems as IList;

            if(boundList == null)
            {
                // SelectedItems is not a list and thus cannot be motifed
                SelectedItems = AssociatedObject.SelectedItems;
                return;
            }

            // Mofify the underlying collection
            foreach(var addedItem in e.AddedItems)
            {
                boundList.Add(addedItem);
            }

            foreach(var removedItem in e.RemovedItems)
            {
                boundList.Remove(removedItem);
            }
        }
    }
}
