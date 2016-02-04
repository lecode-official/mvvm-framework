
#region Using Directives

using ReactiveUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;
using System.Collections;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents an implementation a reactive derived list with sorting and filtering options and exposure of an implementation of the <see cref="ICollectionView"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public class ReactiveDerivedCollection<T> : ReactiveObject, IEnumerable<T> where T : class
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveDerivedCollection{T}"/> instance.
        /// </summary>
        public ReactiveDerivedCollection()
            : this(new List<T>()) { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveDerivedCollection{T}"/> instance.
        /// </summary>
        /// <param name="initialContents">The initial items of the collection.</param>
        public ReactiveDerivedCollection(IEnumerable<T> initialContents)
        {
            // Initializes the reactive list
            this.reactiveList = new ReactiveList<T>(initialContents);

            // Initializes the internal collection view, which is exposed as items source
            this.collectionView = CollectionViewSource.GetDefaultView(this.reactiveList) as ListCollectionView;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the interal list which is used to manage the derived collection.
        /// </summary>
        private ReactiveList<T> reactiveList;

        /// <summary>
        /// Contains the internal collection view, which is exposed as items source.
        /// </summary>
        private ListCollectionView collectionView;

        #endregion

        #region Public Properties

        /// <summary>
        /// Contains the filter that is used in the items source for filtering out data.
        /// </summary>
        private Predicate<T> where;

        /// <summary>
        /// Gets or sets the filter that is used in the items source for filtering out data.
        /// </summary>
        public Predicate<T> Where
        {
            get
            {
                return this.where;
            }

            set
            {
                // Sets the new filter
                if (value == null)
                    this.collectionView.Filter = item => true;
                else
                    this.collectionView.Filter = item => value(item as T);
                this.collectionView.Refresh();

                // Changes the value of the filter
                this.RaiseAndSetIfChanged(ref this.where, value);
            }
        }

        /// <summary>
        /// Contains a function which is used by the items source for sorting the list.
        /// </summary>
        private Comparison<T> orderBy;

        /// <summary>
        /// Gets or sets a function which is used by the items source for sorting the list.
        /// </summary>
        public Comparison<T> OrderBy
        {
            get
            {
                return this.orderBy;
            }

            set
            {
                // Sets the new sorting
                if (value == null)
                    this.collectionView.CustomSort = Comparer<T>.Create((itemX, itemY) => 1);
                else
                    this.collectionView.CustomSort = Comparer<T>.Create(value);
                this.collectionView.Refresh();

                // Changes the value of the sorting
                this.RaiseAndSetIfChanged(ref this.orderBy, value);
            }
        }

        /// <summary>
        /// Contains the object that observes whether an item in the set has been changed.
        /// </summary>
        private IObservable<T> itemChanged;

        /// <summary>
        /// Gets or sets an object that observes whether an item in the set has been changed.
        /// </summary>
        public IObservable<T> ItemChanged
        {
            get
            {
                return this.itemChanged;
            }

            set
            {
                // Subscribes for the observable
                if (value != null)
                    value.ObserveOnDispatcher().Where(x => this.reactiveList.Contains(x)).Subscribe(x => this.reactiveList[this.reactiveList.IndexOf(x)] = x);

                // Sets the new value
                this.RaiseAndSetIfChanged(ref this.itemChanged, value);
            }
        }
        
        /// <summary>
        /// Contains the object that observes whether an item has been added to the set.
        /// </summary>
        private IObservable<T> itemAdded;

        /// <summary>
        /// Gets or sets an object that observes whether an item has been added to the set.
        /// </summary>
        public IObservable<T> ItemAdded
        {
            get
            {
                return this.itemAdded;
            }

            set
            {
                // Subscribes for the observable
                if (value != null)
                    value.ObserveOnDispatcher().Subscribe(x =>
                    {
                        if (!this.reactiveList.Contains(x))
                            this.reactiveList.Add(x);
                        else
                            this.reactiveList[this.reactiveList.IndexOf(x)] = x;
                    });

                // Sets the new value
                this.RaiseAndSetIfChanged(ref this.itemAdded, value);
            }
        }

        /// <summary>
        /// Contains the object that observes whether an item has been removed from the set.
        /// </summary>
        private IObservable<T> itemRemoved;

        /// <summary>
        /// Gets or sets an object that observes whether an item has been removed from the set.
        /// </summary>
        public IObservable<T> ItemRemoved
        {
            get
            {
                return this.itemRemoved;
            }

            set
            {
                // Subscribes for the observable
                if (value != null)
                    value.ObserveOnDispatcher().Where(x => this.reactiveList.Contains(x)).Subscribe(x => this.reactiveList.Remove(x));

                // Sets the new value
                this.RaiseAndSetIfChanged(ref this.itemRemoved, value);
            }
        }

        /// <summary>
        /// Gets the collection view which is used as items source in bindings. The <see cref="ItemsSource"/> property takes care of the <see cref="Where"/> and <see cref="OrderBy"/> functions.
        /// </summary>
        public ICollectionView ItemsSource
        {
            get
            {
                return this.collectionView;
            }
        }

        #endregion

        #region IEnumerable Implementation

        /// <summary>
        /// Gets the enumerator for the derived collection.
        /// </summary>
        /// <returns>Returns the enumerator for the derived collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.ItemsSource.Cast<T>().GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator for the derived collection.
        /// </summary>
        /// <returns>Returns the enumerator for the derived collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.ItemsSource.GetEnumerator();
        }
        #endregion
    }
}