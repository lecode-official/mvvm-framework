
#region Using Directives

using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents an implementation a reactive list with sorting and filtering options and exposure of an implementation of the <see cref="ICollectionView"/> interface.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TTarget">The type of the target items that are created by the select function.</typeparam>
    public class ReactiveCollection<TSource, TTarget> : ReactiveList<TSource> 
        where TSource : class
        where TTarget : class
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="select">The select function that created the target items from the source items.</param>
        public ReactiveCollection(Func<TSource, TTarget> select)
            : this(select, new List<TSource>()) { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="select">The select function that created the target items from the source items.</param>
        /// <param name="initialContents">The initial items of the collection.</param>
        public ReactiveCollection(Func<TSource, TTarget> select, IEnumerable<TSource> initialContents)
            : base(initialContents)
        {
            // Stores the select function
            this.Select = select;

            // Initializes the derived collection
            this.derivedCollection = this.CreateDerivedCollection(this.Select);

            // Initializes the internal collection view, which is exposed as items source
            this.collectionView = CollectionViewSource.GetDefaultView(this.derivedCollection) as ListCollectionView;
        }

        #endregion

        #region Private Fields

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
    }
}