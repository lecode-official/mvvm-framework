
#region Using Directives

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a collection view for the <see cref="ReactiveCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of items in the reactive collection.</typeparam>
    public class ReactiveCollectionView<T> : ICollectionView
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollectionView{T}"/> instance.
        /// </summary>
        /// <param name="reactiveCollection"></param>
        public ReactiveCollectionView(ReactiveCollection<T> reactiveCollection)
        {
            this.reactiveCollection = reactiveCollection;
            this.collectionView = CollectionViewSource.GetDefaultView(this.reactiveCollection as INotifyCollectionChanged) as ListCollectionView;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the reactive collection on which the view is based.
        /// </summary>
        private ReactiveCollection<T> reactiveCollection;

        /// <summary>
        /// Contains the internal collection view on which all operations are performed.
        /// </summary>
        private ListCollectionView collectionView;

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Contains the where predicate by which the items of the collection are being filtered.
        /// </summary>
        private Predicate<T> where;

        /// <summary>
        /// Gets or sets the where predicate by which the items of the collection are being filtered.
        /// </summary>
        public Predicate<T> Where
        {
            get
            {
                return this.where;
            }

            set
            {
                if (value == null)
                    this.collectionView.Filter = item => true;
                else
                    this.collectionView.Filter = item => value((T)item);
                this.where = value;
                this.collectionView.Refresh();
            }
        }
        
        /// <summary>
        /// Contains the comparison by which the items of the collection are being ordered.
        /// </summary>
        private Comparison<T> orderBy;

        /// <summary>
        /// Gets or sets the comparison by which the items of the collection are being ordered.
        /// </summary>
        public Comparison<T> OrderBy
        {
            get
            {
                return this.orderBy;
            }

            set
            {
                if (value == null)
                    this.collectionView.CustomSort = Comparer<T>.Create((itemX, itemY) => 1);
                else
                    this.collectionView.CustomSort = Comparer<T>.Create(value);
                this.orderBy = value;
                this.collectionView.Refresh();
            }
        }

        #endregion

        #region ICollectionView Implementation

        /// <summary>
        /// Gets a value that determines whether the collection can be filtered (the <see cref="ReactiveCollectionView{T}"/> can always be filtered).
        /// </summary>
        bool ICollectionView.CanFilter
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value that determines whether the collection can be grouped (the <see cref="ReactiveCollectionView{T}"/> can never be grouped).
        /// </summary>
        bool ICollectionView.CanGroup
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value that determines whether the collection can be sorted (the <see cref="ReactiveCollectionView{T}"/> can always be sorted).
        /// </summary>
        bool ICollectionView.CanSort
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the culture which is used for the ordering of the collection items (is not used by this implementation).
        /// </summary>
        CultureInfo ICollectionView.Culture
        {
            get
            {
                return this.collectionView.Culture;
            }

            set
            {
                this.collectionView.Culture = value;
            }
        }

        /// <summary>
        /// Gets the current item of the collection view.
        /// </summary>
        object ICollectionView.CurrentItem
        {
            get
            {
                return this.collectionView.CurrentItem;
            }
        }

        /// <summary>
        /// Gets the index of the current item of the collection view.
        /// </summary>
        int ICollectionView.CurrentPosition
        {
            get
            {
                return this.collectionView.CurrentPosition;
            }
        }

        /// <summary>
        /// Gets or sets the where predicate by which the items of the collection are being filtered.
        /// </summary>
        Predicate<object> ICollectionView.Filter
        {
            get
            {
                return this.collectionView.Filter;
            }

            set
            {
                this.collectionView.Filter = value;
            }
        }

        /// <summary>
        /// Gets the descriptions of the groupds of the collection view (is not used by this implementation, because grouping is not supported).
        /// </summary>
        ObservableCollection<GroupDescription> ICollectionView.GroupDescriptions
        {
            get
            {
                return this.collectionView.GroupDescriptions;
            }
        }

        /// <summary>
        /// Gets the grousp of the collection view (always returns an empty collection, because grouping is not supported by this implementation).
        /// </summary>
        ReadOnlyObservableCollection<object> ICollectionView.Groups
        {
            get
            {
                return this.collectionView.Groups;
            }
        }

        /// <summary>
        /// Gets a value that determines whether the current item is after the last item of the collection view.
        /// </summary>
        bool ICollectionView.IsCurrentAfterLast
        {
            get
            {
                return this.collectionView.IsCurrentAfterLast;
            }
        }

        /// <summary>
        /// Gets a value that determines whether the current item is before the first item of the collection view.
        /// </summary>
        bool ICollectionView.IsCurrentBeforeFirst
        {
            get
            {
                return this.collectionView.IsCurrentBeforeFirst;
            }
        }

        /// <summary>
        /// Gets a value that determines whether the collection view is empty.
        /// </summary>
        bool ICollectionView.IsEmpty
        {
            get
            {
                return this.collectionView.IsEmpty;
            }
        }

        /// <summary>
        /// Gets the sort description for the collection view.
        /// </summary>
        SortDescriptionCollection ICollectionView.SortDescriptions
        {
            get
            {
                return this.collectionView.SortDescriptions;
            }
        }

        /// <summary>
        /// Gets the collection on which the collection view is based.
        /// </summary>
        IEnumerable ICollectionView.SourceCollection
        {
            get
            {
                return this.collectionView.SourceCollection;
            }
        }

        /// <summary>
        /// An event, which is raised when the collection has changed.
        /// </summary>
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add
            {
                (this.collectionView as INotifyCollectionChanged).CollectionChanged += value;
            }

            remove
            {
                (this.collectionView as INotifyCollectionChanged).CollectionChanged -= value;
            }
        }

        /// <summary>
        /// An event, which is raised after the current item has changed.
        /// </summary>
        event EventHandler ICollectionView.CurrentChanged
        {
            add
            {
                this.collectionView.CurrentChanged += value;
            }

            remove
            {
                this.collectionView.CurrentChanged -= value;
            }
        }

        /// <summary>
        /// An event, which is raised before the current item is changed.
        /// </summary>
        event CurrentChangingEventHandler ICollectionView.CurrentChanging
        {
            add
            {
                this.collectionView.CurrentChanging += value;
            }

            remove
            {
                this.collectionView.CurrentChanging -= value;
            }
        }

        /// <summary>
        /// Determines whether the specified item is in the collection view.
        /// </summary>
        /// <param name="item">The item for which is to be determined whether it is in the collection view.</param>
        /// <returns>Returns <c>true</c> if the specified item is in the collection view.</returns>
        bool ICollectionView.Contains(object item) => this.collectionView.Contains(item);

        /// <summary>
        /// Switches into a deferral with which the automatic update of the collection view can be deferred when multiple changes are performed.
        /// </summary>
        /// <returns>Returns a disposable object with which the deferral can be ended.</returns>
        IDisposable ICollectionView.DeferRefresh() => this.collectionView.DeferRefresh();

        /// <summary>
        /// Retrieves an enumerator for the collection view.
        /// </summary>
        /// <returns>Returns an enumerator for the collection view.</returns>
        IEnumerator IEnumerable.GetEnumerator() => (this.collectionView as IEnumerable).GetEnumerator();

        /// <summary>
        /// Sets the current item to the specified item.
        /// </summary>
        /// <param name="item">The item that is to be set as the current item.</param>
        /// <returns>Returns <c>true</c> if the current item could be set and <c>false</c> otherwise.</returns>
        bool ICollectionView.MoveCurrentTo(object item) => this.collectionView.MoveCurrentTo(item);

        /// <summary>
        /// Sets the current item to the first item in the collection view.
        /// </summary>
        /// <returns>Returns <c>true</c> if the current item could be set and <c>false</c> otherwise.</returns>
        bool ICollectionView.MoveCurrentToFirst() => this.collectionView.MoveCurrentToFirst();

        /// <summary>
        /// Sets the current item to the last item in the collection view.
        /// </summary>
        /// <returns>Returns <c>true</c> if the current item could be set and <c>false</c> otherwise.</returns>
        bool ICollectionView.MoveCurrentToLast() => this.collectionView.MoveCurrentToLast();

        /// <summary>
        /// Sets the current item to the next item after the current item.
        /// </summary>
        /// <returns>Returns <c>true</c> if the current item could be set and <c>false</c> otherwise.</returns>
        bool ICollectionView.MoveCurrentToNext() => this.collectionView.MoveCurrentToNext();

        /// <summary>
        /// Sets the current item to the item at the specified index.
        /// </summary>
        /// <param name="position">The index of the item which is to be set as the current item.</param>
        /// <returns>Returns <c>true</c> if the current item could be set and <c>false</c> otherwise.</returns>
        bool ICollectionView.MoveCurrentToPosition(int position) => this.collectionView.MoveCurrentToPosition(position);

        /// <summary>
        /// Sets the current item to the item before the current item.
        /// </summary>
        /// <returns>Returns <c>true</c> if the current item could be set and <c>false</c> otherwise.</returns>
        bool ICollectionView.MoveCurrentToPrevious() =>  this.collectionView.MoveCurrentToPrevious();

        /// <summary>
        /// Refreshes the collection view.
        /// </summary>
        void ICollectionView.Refresh() => this.collectionView.Refresh();

        #endregion
    }
}