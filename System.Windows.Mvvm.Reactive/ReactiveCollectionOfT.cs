
#region Using Directives

using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents an implementation a reactive list with sorting and filtering options and exposure of an implementation of the <see cref="ICollectionView"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public class ReactiveCollection<T> : ReactiveList<T> where T : class
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{T}"/> instance.
        /// </summary>
        public ReactiveCollection()
            : this(new List<T>()) { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{T}"/> instance.
        /// </summary>
        /// <param name="initialContents">The initial items of the collection.</param>
        public ReactiveCollection(IEnumerable<T> initialContents)
            : base(initialContents)
        {
            // Initializes the internal collection view, which is exposed as items source
            this.collectionView = CollectionViewSource.GetDefaultView(this) as ListCollectionView;
        }

        #endregion

        #region Private Fields

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
    }
}