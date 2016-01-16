
#region Using Directives

using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents an implementation a reactive derived list with sorting and filtering options and exposure of an implementation of the <see cref="ICollectionView"/> interface.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TTarget">The type of the target items that are created by the select function.</typeparam>
    public class ReactiveDerivedCollection<TSource, TTarget> : ReactiveObject, IEnumerable<TTarget>
        where TSource : class
        where TTarget : class
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveDerivedCollection{TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="select">The select function that created the target items from the source items.</param>
        public ReactiveDerivedCollection(Func<TSource, TTarget> select)
            : this(select, new List<TSource>()) { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveDerivedCollection{TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="select">The select function that created the target items from the source items.</param>
        /// <param name="initialContents">The initial items of the collection.</param>
        public ReactiveDerivedCollection(Func<TSource, TTarget> select, IEnumerable<TSource> initialContents)
        {
            // Stores the select function
            this.Select = select;

            // Initializes the reactive list
            this.reactiveList = new ReactiveList<TSource>(initialContents);

            // Initializes the derived collection
            this.derivedCollection = this.reactiveList.CreateDerivedCollection(this.Select);

            // Initializes the internal collection view, which is exposed as items source
            this.collectionView = CollectionViewSource.GetDefaultView(this.derivedCollection) as ListCollectionView;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the interal list which is used to manage the derived collection.
        /// </summary>
        private ReactiveList<TSource> reactiveList;

        /// <summary>
        /// Contains the derived collection of target items.
        /// </summary>
        private IReactiveDerivedList<TTarget> derivedCollection;

        /// <summary>
        /// Contains the internal collection view, which is exposed as items source.
        /// </summary>
        private ListCollectionView collectionView;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the select function that created the target items from the source items.
        /// </summary>
        public Func<TSource, TTarget> Select { get; private set; }

        /// <summary>
        /// Contains the filter that is used in the items source for filtering out data.
        /// </summary>
        private Predicate<TTarget> where;

        /// <summary>
        /// Gets or sets the filter that is used in the items source for filtering out data.
        /// </summary>
        public Predicate<TTarget> Where
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
                    this.collectionView.Filter = item => value(item as TTarget);
                this.collectionView.Refresh();

                // Changes the value of the filter
                this.RaiseAndSetIfChanged(ref this.where, value);
            }
        }

        /// <summary>
        /// Contains a function which is used by the items source for sorting the list.
        /// </summary>
        private Comparison<TTarget> orderBy;

        /// <summary>
        /// Gets or sets a function which is used by the items source for sorting the list.
        /// </summary>
        public Comparison<TTarget> OrderBy
        {
            get
            {
                return this.orderBy;
            }

            set
            {
                // Sets the new sorting
                if (value == null)
                    this.collectionView.CustomSort = Comparer<TTarget>.Create((itemX, itemY) => 1);
                else
                    this.collectionView.CustomSort = Comparer<TTarget>.Create(value);
                this.collectionView.Refresh();

                // Changes the value of the sorting
                this.RaiseAndSetIfChanged(ref this.orderBy, value);
            }
        }

        /// <summary>
        /// Contains the object that observes whether an item in the set has been changed.
        /// </summary>
        private IObservable<TSource> itemChanged;

        /// <summary>
        /// Gets or sets an object that observes whether an item in the set has been changed.
        /// </summary>
        public IObservable<TSource> ItemChanged
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
        private IObservable<TSource> itemAdded;

        /// <summary>
        /// Gets or sets an object that observes whether an item has been added to the set.
        /// </summary>
        public IObservable<TSource> ItemAdded
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
        private IObservable<TSource> itemRemoved;

        /// <summary>
        /// Gets or sets an object that observes whether an item has been removed from the set.
        /// </summary>
        public IObservable<TSource> ItemRemoved
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
        /// Gets the collection view which is used as items source in bindings. The <see cref="ItemsSource"/> property takes care of the <see cref="Where"/> and <see cref="OrderBy"/> functions and contains items created by the <see cref="Select"/> function.
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
        public IEnumerator<TTarget> GetEnumerator()
        {
            return this.ItemsSource.Cast<TTarget>().GetEnumerator();
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