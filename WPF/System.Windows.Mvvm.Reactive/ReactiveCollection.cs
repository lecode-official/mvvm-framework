
#region Using Directives

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a collection where the changing of items can be observed.
    /// </summary>
    /// <typeparam name="T">The type of the items of the collection.</typeparam>
    public class ReactiveCollection<T> : IReactiveCollection<T>, ICollection<T>, IList<T>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{T}"/> instance.
        /// </summary>
        /// <param name="collection">The collection from which the <see cref="ReactiveCollection{T}"/> is to be constructed.</param>
        public ReactiveCollection(IEnumerable<T> collection)
        {
            this.collection = collection.ToList();
            this.collectionChangedSubject.ObserveOnDispatcher().Subscribe(x =>
            {
                this.collectionChanged?.Invoke(this, x);
                this.propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Count)));
                this.propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsEmpty)));
            });
        }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCollection{T}"/> instance.
        /// </summary>
        public ReactiveCollection()
            : this(new List<T>())
        { }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the internal collection on which all the operations are performed.
        /// </summary>
        private List<T> collection;

        /// <summary>
        /// Contains a subject, which is fired, whenever the collection is changed.
        /// </summary>
        private Subject<NotifyCollectionChangedEventArgs> collectionChangedSubject = new Subject<NotifyCollectionChangedEventArgs>();

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
        /// Contains a subject, which fires before a new item is added to the collection.
        /// </summary>
        private Subject<T> beforeItemAdded;

        /// <summary>
        /// Gets an observable, which fires before a new item is added to the collection.
        /// </summary>
        public IObservable<T> BeforeItemAdded
        {
            get
            {
                if (this.beforeItemAdded == null)
                    this.beforeItemAdded = new Subject<T>();
                return this.beforeItemAdded.AsObservable();
            }
        }

        /// <summary>
        /// Contains a subject, which fires after a new item is added to the collection.
        /// </summary>
        private Subject<T> itemAdded;

        /// <summary>
        /// Gets an observable, which fires after a new item is added to the collection.
        /// </summary>
        public IObservable<T> ItemAdded
        {
            get
            {
                if (this.itemAdded == null)
                    this.itemAdded = new Subject<T>();
                return this.itemAdded.AsObservable();
            }
        }

        /// <summary>
        /// Contains a subject, which fires before an item is removed to the collection.
        /// </summary>
        private Subject<T> beforeItemRemoved;

        /// <summary>
        /// Gets an observable, which fires before an item is removed to the collection.
        /// </summary>
        public IObservable<T> BeforeItemRemoved
        {
            get
            {
                if (this.beforeItemRemoved == null)
                    this.beforeItemRemoved = new Subject<T>();
                return this.beforeItemRemoved.AsObservable();
            }
        }

        /// <summary>
        /// Contains a subject, which fires after an item is removed to the collection.
        /// </summary>
        private Subject<T> itemRemoved;

        /// <summary>
        /// Gets an observable, which fires after an item is removed to the collection.
        /// </summary>
        public IObservable<T> ItemRemoved
        {
            get
            {
                if (this.itemRemoved == null)
                    this.itemRemoved = new Subject<T>();
                return this.itemRemoved.AsObservable();
            }
        }

        /// <summary>
        /// Contains an observable, which fires when the count of the collection has changed.
        /// </summary>
        private IObservable<int> countChanged;

        /// <summary>
        /// Gets an observable, which fires when the count of the collection has changed.
        /// </summary>
        public IObservable<int> CountChanged
        {
            get
            {
                if (this.countChanged == null)
                    this.countChanged = this.collectionChangedSubject.Select(x => this.collection.Count).AsObservable();
                return this.countChanged.AsObservable();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the items in the specified collection to the collection.
        /// </summary>
        /// <param name="collection">The collection whose items are to be added to the collection.</param>
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (T item in collection.ToList())
            {
                this.beforeItemAdded?.OnNext(item);
                this.collection.Add(item);
                this.itemAdded?.OnNext(item);
            }
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Removes the items in the specified collection from the collection.
        /// </summary>
        /// <param name="collection">The collection that contains all the items that are to be removed from the collection.</param>
        public void RemoveRange(IEnumerable<T> collection)
        {
            foreach (T item in collection.ToList())
            {
                int index = this.collection.IndexOf(item);
                if (index == -1)
                    continue;

                this.beforeItemRemoved?.OnNext(item);
                this.collection.Remove(item);
                this.itemRemoved?.OnNext(item);
            }
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
                return this.collection.Count;
            }
        }

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
        public void Add(T item)
        {
            this.beforeItemAdded?.OnNext(item);
            this.collection.Add(item);
            this.itemAdded?.OnNext(item);
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            List<T> items = this.collection.ToList();

            foreach(T item in items)
                this.beforeItemRemoved?.OnNext(item);

            this.collection.Clear();

            foreach (T item in items)
                this.itemRemoved?.OnNext(item);
            
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Determines whether the specified item is in the collection.
        /// </summary>
        /// <param name="item">The item for which is to be determined whether it is in the collection.</param>
        /// <returns>Returns <c>true</c> if the specified item is in the collection and <c>false</c> otherwise.</returns>
        public bool Contains(T item) => this.collection.Contains(item);

        /// <summary>
        /// Copies the content of the collection to the specified array, beginning from the specified index.
        /// </summary>
        /// <param name="array">The array into which the content of the collection is to be copied.</param>
        /// <param name="arrayIndex">The index at from which the copying is to be started.</param>
        public void CopyTo(T[] array, int arrayIndex) => this.collection.CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="item">The item that is to be removed from the collection.</param>
        /// <returns>Returns <c>true</c> if the item was removed from the collection and <c>false</c> otherwise.</returns>
        public bool Remove(T item)
        {
            int index = this.collection.IndexOf(item);
            if (index == -1)
                return false;

            this.beforeItemRemoved?.OnNext(item);
            bool result = this.collection.Remove(item);
            this.itemRemoved?.OnNext(item);
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return result;
        }

        /// <summary>
        /// Gets the count of items that are in the collection.
        /// </summary>
        int ICollection.Count
        {
            get
            {
                return (this.collection as ICollection).Count;
            }
        }

        /// <summary>
        /// Gets an object with which the access to the collection can be synchronized.
        /// </summary>
        object ICollection.SyncRoot
        {
            get
            {
                return (this.collection as ICollection).SyncRoot;
            }
        }

        /// <summary>
        /// Gets a value that determines whether the collection is synchronized (thread-safe).
        /// </summary>
        bool ICollection.IsSynchronized
        {
            get
            {
                return (this.collection as ICollection).IsSynchronized;
            }
        }

        /// <summary>
        /// Copies the content of the collection to the specified array, beginning from the specified index.
        /// </summary>
        /// <param name="array">The array into which the content of the collection is to be copied.</param>
        /// <param name="index">The index at from which the copying is to be started.</param>
        void ICollection.CopyTo(Array array, int index) => (this.collection as ICollection).CopyTo(array, index);

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
                return this.collection[index];
            }

            set
            {
                T oldItem = this.collection[index];

                this.beforeItemRemoved?.OnNext(oldItem);
                this.beforeItemAdded?.OnNext(value);
                this.collection[index] = value;
                this.itemRemoved?.OnNext(oldItem);
                this.itemAdded?.OnNext(value);
                this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }

        /// <summary>
        /// Determines the index of the specified item.
        /// </summary>
        /// <param name="item">The item for which the index is to be determined.</param>
        /// <returns>Returns the index of the specified item or -1 if the specified item is not in the collection.</returns>
        public int IndexOf(T item) => this.collection.IndexOf(item);

        /// <summary>
        /// Inserts the specified item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which the specified item is to be inserted.</param>
        /// <param name="item">The item that is to be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        public void Insert(int index, T item)
        {
            this.beforeItemAdded?.OnNext(item);
            this.collection.Insert(index, item);
            this.itemAdded?.OnNext(item);
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item that is to be removed.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        public void RemoveAt(int index)
        {
            T item = this.collection[index];
            this.beforeItemRemoved?.OnNext(item);
            this.collection.RemoveAt(index);
            this.itemRemoved?.OnNext(item);
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

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
        /// Gets a value that determines whether the collection has a fixed size (<see cref="ReactiveCollection{T}"/> never has a fixed size).
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
                return this.collection[index];
            }

            set
            {
                T oldItem = this.collection[index];
                T newItem = (T)value;

                this.beforeItemRemoved?.OnNext(oldItem);
                this.beforeItemAdded?.OnNext(newItem);
                this.collection[index] = newItem;
                this.itemRemoved?.OnNext(oldItem);
                this.itemAdded?.OnNext(newItem);
                this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
            }
        }

        /// <summary>
        /// Adds the specified item to the collection.
        /// </summary>
        /// <param name="value">The item that is to be added to the collection.</param>
        int IList.Add(object value)
        {
            T item = (T)value;

            this.beforeItemAdded?.OnNext(item);
            this.collection.Add(item);
            this.itemAdded?.OnNext(item);
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));

            return this.collection.IndexOf(item);
        }

        /// <summary>
        /// Determines whether the specified item is in the collection.
        /// </summary>
        /// <param name="value">The item for which is to be determined whether it is in the collection.</param>
        /// <returns>Returns <c>true</c> if the specified item is in the collection and <c>false</c> otherwise.</returns>
        bool IList.Contains(object value) => (this.collection as IList).Contains(value);

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        void IList.Clear()
        {
            List<T> items = this.collection.ToList();

            foreach (T item in items)
                this.beforeItemRemoved?.OnNext(item);

            this.collection.Clear();

            foreach (T item in items)
                this.itemRemoved?.OnNext(item);

            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Determines the index of the specified item.
        /// </summary>
        /// <param name="value">The item for which the index is to be determined.</param>
        /// <returns>Returns the index of the specified item or -1 if the specified item is not in the collection.</returns>
        int IList.IndexOf(object value) =>  (this.collection as IList).IndexOf(value);

        /// <summary>
        /// Inserts the specified item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which the specified item is to be inserted.</param>
        /// <param name="value">The item that is to be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        void IList.Insert(int index, object value)
        {
            T item = (T)value;

            this.beforeItemAdded?.OnNext(item);
            this.collection.Insert(index, item);
            this.itemAdded?.OnNext(item);
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="value">The item that is to be removed from the collection.</param>
        void IList.Remove(object value)
        {
            T item = (T)value;
            int index = this.collection.IndexOf(item);
            if (index == -1)
                return;

            this.beforeItemRemoved?.OnNext(item);
            this.collection.Remove(item);
            this.itemRemoved?.OnNext(item);
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item that is to be removed.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is out of range, then an <see cref="ArgumentOutOfRangeException"/> exception is thrown.</exception>
        void IList.RemoveAt(int index)
        {
            T item = this.collection[index];
            this.beforeItemRemoved?.OnNext(item);
            this.collection.RemoveAt(index);
            this.itemRemoved?.OnNext(item);
            this.collectionChangedSubject.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        #endregion

        #region IEnumerable Implementation

        /// <summary>
        /// Retrieves an enumerator for the collection.
        /// </summary>
        /// <returns>Returns an enumerator for the collection.</returns>
        public IEnumerator<T> GetEnumerator() => this.collection.GetEnumerator();

        /// <summary>
        /// Retrieves an enumerator for the collection.
        /// </summary>
        /// <returns>Returns an enumerator for the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.collection.GetEnumerator();

        #endregion
    }
}