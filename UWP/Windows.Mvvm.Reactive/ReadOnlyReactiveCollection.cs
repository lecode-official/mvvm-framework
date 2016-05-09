
#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

#endregion

namespace Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a reactive collection, which hides away the item manipulation functionality and only exposes it to sub-classes. This class is not meant for direct usage and must be derived from.
    /// </summary>
    /// <typeparam name="T">The type of the items of the collection.</typeparam>
    public abstract class ReadOnlyReactiveCollection<T> : IReactiveCollection<T>
    {
        #region Protected Properties

        /// <summary>
        /// Contains a subject, which is fired, whenever the collection is changed.
        /// </summary>
        private Subject<NotifyCollectionChangedEventArgs> collectionChangedSubject;

        /// <summary>
        /// Gets a subject, which is fired, whenever the collection is changed.
        /// </summary>
        protected Subject<NotifyCollectionChangedEventArgs> CollectionChangedSubject
        {
            get
            {
                // Checks if the collection changed subject already exists, if not then it is lazily created and initialized
                if (this.collectionChangedSubject == null)
                {
                    this.collectionChangedSubject = new Subject<NotifyCollectionChangedEventArgs>();
                    this.CollectionChangedSubject.ObserveOnDispatcher().Subscribe(x =>
                    {
                        this.collectionChanged?.Invoke(this, x);
                        this.propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Count)));
                        this.propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsEmpty)));
                    });
                }

                // Returns the collection changed subject
                return this.collectionChangedSubject;
            }
        }

        /// <summary>
        /// Gets or sets the internal collection on which all the operations are performed.
        /// </summary>
        protected List<T> Collection { get; set; } = new List<T>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value that determines whether the collection is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.Count == 0;
            }
        }

        /// <summary>
        /// Gets a subject, which fires before a new item is added to the collection.
        /// </summary>
        protected Subject<T> BeforeItemAddedSubject { get; private set; }

        /// <summary>
        /// Gets an observable, which fires before a new item is added to the collection.
        /// </summary>
        public IObservable<T> BeforeItemAdded
        {
            get
            {
                if (this.BeforeItemAddedSubject == null)
                    this.BeforeItemAddedSubject = new Subject<T>();
                return this.BeforeItemAdded.AsObservable();
            }
        }

        /// <summary>
        /// Gets a subject, which fires after a new item is added to the collection.
        /// </summary>
        protected Subject<T> ItemAddedSubject { get; private set; }

        /// <summary>
        /// Gets an observable, which fires after a new item is added to the collection.
        /// </summary>
        public IObservable<T> ItemAdded
        {
            get
            {
                if (this.ItemAddedSubject == null)
                    this.ItemAddedSubject = new Subject<T>();
                return this.ItemAddedSubject.AsObservable();
            }
        }

        /// <summary>
        /// Gets a subject, which fires before an item is removed to the collection.
        /// </summary>
        protected Subject<T> BeforeItemRemovedSubject { get; private set; }

        /// <summary>
        /// Gets an observable, which fires before an item is removed to the collection.
        /// </summary>
        public IObservable<T> BeforeItemRemoved
        {
            get
            {
                if (this.BeforeItemRemovedSubject == null)
                    this.BeforeItemRemovedSubject = new Subject<T>();
                return this.BeforeItemRemovedSubject.AsObservable();
            }
        }

        /// <summary>
        /// Gets a subject, which fires after an item is removed to the collection.
        /// </summary>
        protected Subject<T> ItemRemovedSubject { get; private set; }

        /// <summary>
        /// Gets an observable, which fires after an item is removed to the collection.
        /// </summary>
        public IObservable<T> ItemRemoved
        {
            get
            {
                if (this.ItemRemovedSubject == null)
                    this.ItemRemovedSubject = new Subject<T>();
                return this.ItemRemovedSubject.AsObservable();
            }
        }

        /// <summary>
        /// Gets an observable, which fires when the count of the collection has changed.
        /// </summary>
        protected IObservable<int> CountChangedSubject { get; private set; }

        /// <summary>
        /// Gets an observable, which fires when the count of the collection has changed.
        /// </summary>
        public IObservable<int> CountChanged
        {
            get
            {
                if (this.CountChangedSubject == null)
                    this.CountChangedSubject = this.CollectionChangedSubject.Select(x => this.Collection.Count).AsObservable();
                return this.CountChangedSubject.AsObservable();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Adds the items in the specified collection to the collection.
        /// </summary>
        /// <param name="collection">The collection whose items are to be added to the collection.</param>
        protected void AddRange(IEnumerable<T> collection)
        {
            foreach (T item in collection.ToList())
            {
                this.BeforeItemAddedSubject?.OnNext(item);
                this.Collection.Add(item);
                this.ItemAddedSubject?.OnNext(item);
            }
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Removes the items in the specified collection from the collection.
        /// </summary>
        /// <param name="collection">The collection that contains all the items that are to be removed from the collection.</param>
        protected void RemoveRange(IEnumerable<T> collection)
        {
            foreach (T item in collection.ToList())
            {
                int index = this.Collection.IndexOf(item);
                if (index == -1)
                    continue;

                this.BeforeItemRemovedSubject?.OnNext(item);
                this.Collection.Remove(item);
                this.ItemRemovedSubject?.OnNext(item);
            }
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion

        #region INotifyCollectionChanged Implementation

        /// <summary>
        /// An event, which is raised when the collection has changed.
        /// </summary>
        private event NotifyCollectionChangedEventHandler collectionChanged;

        /// <summary>
        /// An event, which is raised when the collection has changed.
        /// </summary>
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add
            {
                this.collectionChanged += value;
            }

            remove
            {
                this.collectionChanged -= value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// An event, which is raised when a property of the reactive collection has changed.
        /// </summary>
        private event PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// An event, which is raised when a property of the reactive collection has changed.
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                this.propertyChanged += value;
            }

            remove
            {
                this.propertyChanged -= value;
            }
        }

        #endregion

        #region ICollection Implementation

        /// <summary>
        /// Gets the count of items that are in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return this.Collection.Count;
            }
        }

        /// <summary>
        /// Adds the specified item to the collection.
        /// </summary>
        /// <param name="item">The item that is to be added to the collection.</param>
        protected void Add(T item)
        {
            this.BeforeItemAddedSubject?.OnNext(item);
            this.Collection.Add(item);
            this.ItemAddedSubject?.OnNext(item);
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, this.Collection.IndexOf(item)));
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected void Clear()
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
        /// Determines whether the specified item is in the collection.
        /// </summary>
        /// <param name="item">The item for which is to be determined whether it is in the collection.</param>
        /// <returns>Returns <c>true</c> if the specified item is in the collection and <c>false</c> otherwise.</returns>
        public bool Contains(T item) => this.Collection.Contains(item);

        /// <summary>
        /// Copies the content of the collection to the specified array, beginning from the specified index.
        /// </summary>
        /// <param name="array">The array into which the content of the collection is to be copied.</param>
        /// <param name="arrayIndex">The index at from which the copying is to be started.</param>
        public void CopyTo(T[] array, int arrayIndex) => this.Collection.CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="item">The item that is to be removed from the collection.</param>
        /// <returns>Returns <c>true</c> if the item was removed from the collection and <c>false</c> otherwise.</returns>
        protected bool Remove(T item)
        {
            int index = this.Collection.IndexOf(item);
            if (index == -1)
                return false;

            this.BeforeItemRemovedSubject?.OnNext(item);
            bool result = this.Collection.Remove(item);
            this.ItemRemovedSubject?.OnNext(item);
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return result;
        }

        /// <summary>
        /// Gets the count of items that are in the collection.
        /// </summary>
        int ICollection.Count
        {
            get
            {
                return (this.Collection as ICollection).Count;
            }
        }

        /// <summary>
        /// Gets an object with which the access to the collection can be synchronized.
        /// </summary>
        object ICollection.SyncRoot
        {
            get
            {
                return (this.Collection as ICollection).SyncRoot;
            }
        }

        /// <summary>
        /// Gets a value that determines whether the collection is synchronized (thread-safe).
        /// </summary>
        bool ICollection.IsSynchronized
        {
            get
            {
                return (this.Collection as ICollection).IsSynchronized;
            }
        }

        /// <summary>
        /// Copies the content of the collection to the specified array, beginning from the specified index.
        /// </summary>
        /// <param name="array">The array into which the content of the collection is to be copied.</param>
        /// <param name="index">The index at from which the copying is to be started.</param>
        void ICollection.CopyTo(Array array, int index) => (this.Collection as ICollection).CopyTo(array, index);

        #endregion

        #region IList Implementation

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item that is to be retrieved or set.</param>
        /// <exception cref="IndexOutOfRangeException">If the specified index is out of range, then an <see cref="IndexOutOfRangeException"/> exception is thrown.</exception>
        /// <returns>Returns the item at the specified index.</returns>
        public T this[int index]
        {
            get
            {
                return this.Collection[index];
            }

            protected set
            {
                T oldItem = this.Collection[index];

                this.BeforeItemRemovedSubject?.OnNext(oldItem);
                this.BeforeItemAddedSubject?.OnNext(value);
                this.Collection[index] = value;
                this.ItemRemovedSubject?.OnNext(oldItem);
                this.ItemAddedSubject?.OnNext(value);
                this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }

        /// <summary>
        /// Determines the index of the specified item.
        /// </summary>
        /// <param name="item">The item for which the index is to be determined.</param>
        /// <returns>Returns the index of the specified item or -1 if the specified item is not in the collection.</returns>
        public int IndexOf(T item) => this.Collection.IndexOf(item);

        /// <summary>
        /// Inserts the specified item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which the specified item is to be inserted.</param>
        /// <param name="item">The item that is to be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        protected void Insert(int index, T item)
        {
            this.BeforeItemAddedSubject?.OnNext(item);
            this.Collection.Insert(index, item);
            this.ItemAddedSubject?.OnNext(item);
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item that is to be removed.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        protected void RemoveAt(int index)
        {
            T item = this.Collection[index];
            this.BeforeItemRemovedSubject?.OnNext(item);
            this.Collection.RemoveAt(index);
            this.ItemRemovedSubject?.OnNext(item);
            this.CollectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        /// <summary>
        /// Gets a value that determines whether the collection is read-only (<see cref="ReadOnlyReactiveCollection{T}"/> is never read-only).
        /// </summary>
        bool IList.IsReadOnly
        {
            get
            {
                return true;
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
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Adds the specified item to the collection.
        /// </summary>
        /// <param name="value">The item that is to be added to the collection.</param>
        int IList.Add(object value)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the index of the specified item.
        /// </summary>
        /// <param name="value">The item for which the index is to be determined.</param>
        /// <returns>Returns the index of the specified item or -1 if the specified item is not in the collection.</returns>
        int IList.IndexOf(object value) => (this.Collection as IList).IndexOf(value);

        /// <summary>
        /// Inserts the specified item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which the specified item is to be inserted.</param>
        /// <param name="value">The item that is to be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="value">The item that is to be removed from the collection.</param>
        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item that is to be removed.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Implementation

        /// <summary>
        /// Retrieves an enumerator for the collection.
        /// </summary>
        /// <returns>Returns an enumerator for the collection.</returns>
        public IEnumerator<T> GetEnumerator() => this.Collection.GetEnumerator();

        /// <summary>
        /// Retrieves an enumerator for the collection.
        /// </summary>
        /// <returns>Returns an enumerator for the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.Collection.GetEnumerator();

        #endregion
    }
}