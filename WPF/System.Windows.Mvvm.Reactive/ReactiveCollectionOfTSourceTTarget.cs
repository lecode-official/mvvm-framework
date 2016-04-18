
#region Using Directives

using ReactiveUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
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
        where TTarget : class, new()
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="update">The update method that updates the target items from the source items. This is needed, because otherwise the objects would be created anew, which is less efficient and destroys object identity.</param>
        public ReactiveCollection(Action<TSource, TTarget> update)
            : this(x => new TTarget(), update, new List<TSource>())
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="select">The select function that created the target items from the source items.</param>
        /// <param name="update">The update method that updates the target items from the source items. This is needed, because otherwise the objects would be created anew, which is less efficient and destroys object identity.</param>
        public ReactiveCollection(Func<TSource, TTarget> select, Action<TSource, TTarget> update)
            : this(select, update, new List<TSource>())
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="update">The update method that updates the target items from the source items. This is needed, because otherwise the objects would be created anew, which is less efficient and destroys object identity.</param>
        /// <param name="initialContents">The initial items of the collection.</param>
        public ReactiveCollection(Action<TSource, TTarget> update, IEnumerable<TSource> initialContents)
            : this(null, update, initialContents)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{TSource, TTarget}"/> instance.
        /// </summary>
        /// <param name="select">The select function that created the target items from the source items.</param>
        /// <param name="update">The update method that updates the target items from the source items. This is needed, because otherwise the objects would be created anew, which is less efficient and destroys object identity.</param>
        /// <param name="initialContents">The initial items of the collection.</param>
        public ReactiveCollection(Func<TSource, TTarget> select, Action<TSource, TTarget> update, IEnumerable<TSource> initialContents)
            : base(initialContents)
        {
            // Stores the select function and the update method
            this.Update = update;
            this.Select = select != null ? select : source =>
            {
                TTarget target = new TTarget();
                this.Update(source, target);
                return target;
            };

            // Initializes the derived collection
            this.derivedList = new ReactiveList<TTarget>();

            // Initializes the internal collection view, which is exposed as items source
            this.collectionView = CollectionViewSource.GetDefaultView(this.derivedList) as ListCollectionView;

            // Subscribes to the item changes event, so that the items can be updated in the internal list
            this.ItemChanged.ObserveOnDispatcher().Subscribe(eventArguments => this.Update(eventArguments.Sender, this.derivedList[this.IndexOf(eventArguments.Sender)]));
        }
        
        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the derived collection of target items.
        /// </summary>
        private ReactiveList<TTarget> derivedList;

        /// <summary>
        /// Contains the internal collection view, which is exposed as items source.
        /// </summary>
        private ListCollectionView collectionView;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the select function that creates the target items from the source items.
        /// </summary>
        public Func<TSource, TTarget> Select { get; private set; }

        /// <summary>
        /// Gets or sets the update method that updates the target items from the source items. This is needed, because otherwise the objects would be created anew, which is less efficient and destroys object identity.
        /// </summary>
        public Action<TSource, TTarget> Update { get; private set; }

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

        #region ReactiveList Implementation

        /// <summary>
        /// Adds a new item to the collection.
        /// </summary>
        /// <param name="item">The item that is to be added to the list.</param>
        public override void Add(TSource item)
        {
            // Calls the base implementation
            base.Add(item);

            // Adds the new item to the internal list as well
            this.derivedList.Add(this.Select(item));

            // Updates the collection view
            this.collectionView.Refresh();
        }
        
        /// <summary>
        /// Removes all the items from the collection.
        /// </summary>
        public override void Clear()
        {
            // Calls the base implementation
            base.Clear();

            // Clears the internal list as well
            this.derivedList.Clear();

            // Updates the collection view
            this.collectionView.Refresh();
        }

        /// <summary>
        /// Inserts an item at the specified location in the collection.
        /// </summary>
        /// <param name="index">The location where the item should be inserted.</param>
        /// <param name="item">The item that is to be inserted.</param>
        public override void Insert(int index, TSource item)
        {
            // Calls the base implementation
            base.Insert(index, item);

            // Inserts the item into the internal list as well
            this.derivedList.Insert(index, this.Select(item));

            // Updates the collection view
            this.collectionView.Refresh();
        }
        
        /// <summary>
        /// Moves the item at the specified location to a new location.
        /// </summary>
        /// <param name="oldIndex">The current location of the item that is to be moved.</param>
        /// <param name="newIndex">The new location to which the item is to be moved.</param>
        public override void Move(int oldIndex, int newIndex)
        {
            // Calls the base implementation
            base.Move(oldIndex, newIndex);

            // Moves the item in the internal collection as well
            this.derivedList.Move(oldIndex, newIndex);

            // Updates the collection view
            this.collectionView.Refresh();
        }

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Remove(TSource item)
        {
            // Removes the specified item from the internal list as well
            int index = base.IndexOf(item);
            if (index < 0)
                return false;
            this.derivedList.RemoveAt(index);

            // Updates the collection view
            this.collectionView.Refresh();

            // Calls the base implementation
            return base.Remove(item);
        }

        /// <summary>
        /// Removes the item at the specified location.
        /// </summary>
        /// <param name="index">The index of the item that is to be removed.</param>
        public override void RemoveAt(int index)
        {
            // Removes the specified
            base.RemoveAt(index);

            // Removes the item from the internal list as well
            this.derivedList.RemoveAt(index);

            // Updates the collection view
            this.collectionView.Refresh();
        }

        /// <summary>
        /// The indexer, which lets the user access the items of the collection via the array notation.
        /// </summary>
        /// <param name="index">The index of the item that is to be accessed.</param>
        /// <returns>Returns the item at the specified location.</returns>
        public override TSource this[int index]
        {
            get
            {
                return base[index];
            }

            set
            {
                base[index] = value;
                this.derivedList[index] = this.Select(value);
            }
        }

        #endregion
    }
}