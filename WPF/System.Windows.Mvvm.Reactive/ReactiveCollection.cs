
#region Using Directives

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a collection where the changing of items can be observed.
    /// </summary>
    /// <typeparam name="T">The type of the items of the collection.</typeparam>
    public class ReactiveCollection<T> : ReadOnlyReactiveCollection<T>, ICollection<T>, IList<T>, IList
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{T}"/> instance.
        /// </summary>
        /// <param name="collection">The collection from which the <see cref="ReactiveCollection{T}"/> is to be constructed.</param>
        public ReactiveCollection(IEnumerable<T> collection)
        {
            this.Collection = collection.ToList();
        }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{T}"/> instance.
        /// </summary>
        public ReactiveCollection()
            : this(new List<T>())
        { }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Adds the items in the specified collection to the collection.
        /// </summary>
        /// <param name="collection">The collection whose items are to be added to the collection.</param>
        public new void AddRange(IEnumerable<T> collection) => base.AddRange(collection);

        /// <summary>
        /// Removes the items in the specified collection from the collection.
        /// </summary>
        /// <param name="collection">The collection that contains all the items that are to be removed from the collection.</param>
        public new void RemoveRange(IEnumerable<T> collection) => base.AddRange(collection);

        #endregion
        
        #region ICollection Implementation
        
        /// <summary>
        /// Gets a value that determines whether the collection is read-only (<see cref="ReactiveCollection{T}"/> is never read-only).
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the specified item to the collection.
        /// </summary>
        /// <param name="item">The item that is to be added to the collection.</param>
        public new void Add(T item) => base.Add(item);

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public new void Clear() => base.Clear();

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="item">The item that is to be removed from the collection.</param>
        /// <returns>Returns <c>true</c> if the item was removed from the collection and <c>false</c> otherwise.</returns>
        public new bool Remove(T item) => base.Remove(item);

        #endregion

        #region IList Implementation

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item that is to be retrieved or set.</param>
        /// <exception cref="IndexOutOfRangeException">If the specified index is out of range, then an <see cref="IndexOutOfRangeException"/> exception is thrown.</exception>
        /// <returns>Returns the item at the specified index.</returns>
        public new T this[int index]
        {
            get
            {
                return base[index];
            }

            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Inserts the specified item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which the specified item is to be inserted.</param>
        /// <param name="item">The item that is to be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        public new void Insert(int index, T item) => base.Insert(index, item);

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item that is to be removed.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        public new void RemoveAt(int index) => base.RemoveAt(index);

        /// <summary>
        /// Gets a value that determines whether the collection is read-only (<see cref="ReactiveCollection{T}"/> is never read-only).
        /// </summary>
        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value that determines whether the collection has a fixed size (<see cref="ReadOnlyReactiveCollection{T}"/> never has a fixed size).
        /// </summary>
        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item that is to be retrieved or set.</param>
        /// <exception cref="IndexOutOfRangeException">If the specified index is out of range, then an <see cref="IndexOutOfRangeException"/> exception is thrown.</exception>
        /// <returns>Returns the item at the specified index.</returns>
        object IList.this[int index]
        {
            get
            {
                return this.Collection[index];
            }

            set
            {
                T oldItem = this.Collection[index];
                T newItem = (T)value;

                this.BeforeItemRemovedSubject?.OnNext(oldItem);
                this.BeforeItemAddedSubject?.OnNext(newItem);
                this.Collection[index] = newItem;
                this.ItemRemovedSubject?.OnNext(oldItem);
                this.ItemAddedSubject?.OnNext(newItem);
                this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
            }
        }

        /// <summary>
        /// Adds the specified item to the collection.
        /// </summary>
        /// <param name="value">The item that is to be added to the collection.</param>
        int IList.Add(object value)
        {
            T item = (T)value;

            this.BeforeItemAddedSubject?.OnNext(item);
            this.Collection.Add(item);
            this.ItemAddedSubject?.OnNext(item);
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));

            return this.Collection.IndexOf(item);
        }

        /// <summary>
        /// Determines whether the specified item is in the collection.
        /// </summary>
        /// <param name="value">The item for which is to be determined whether it is in the collection.</param>
        /// <returns>Returns <c>true</c> if the specified item is in the collection and <c>false</c> otherwise.</returns>
        bool IList.Contains(object value) => (this.Collection as IList).Contains(value);

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        void IList.Clear()
        {
            List<T> items = this.Collection.ToList();

            foreach (T item in items)
                this.BeforeItemRemovedSubject?.OnNext(item);

            this.Collection.Clear();

            foreach (T item in items)
                this.ItemRemovedSubject?.OnNext(item);

            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Determines the index of the specified item.
        /// </summary>
        /// <param name="value">The item for which the index is to be determined.</param>
        /// <returns>Returns the index of the specified item or -1 if the specified item is not in the collection.</returns>
        int IList.IndexOf(object value) =>  (this.Collection as IList).IndexOf(value);

        /// <summary>
        /// Inserts the specified item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which the specified item is to be inserted.</param>
        /// <param name="value">The item that is to be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        void IList.Insert(int index, object value)
        {
            T item = (T)value;

            this.BeforeItemAddedSubject?.OnNext(item);
            this.Collection.Insert(index, item);
            this.ItemAddedSubject?.OnNext(item);
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="value">The item that is to be removed from the collection.</param>
        void IList.Remove(object value)
        {
            T item = (T)value;
            int index = this.Collection.IndexOf(item);
            if (index == -1)
                return;

            this.BeforeItemRemovedSubject?.OnNext(item);
            this.Collection.Remove(item);
            this.ItemRemovedSubject?.OnNext(item);
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item that is to be removed.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        void IList.RemoveAt(int index)
        {
            T item = this.Collection[index];
            this.BeforeItemRemovedSubject?.OnNext(item);
            this.Collection.RemoveAt(index);
            this.ItemRemovedSubject?.OnNext(item);
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        #endregion
    }
}