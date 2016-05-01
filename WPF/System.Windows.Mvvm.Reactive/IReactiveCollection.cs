
#region Using Directives

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents an interface for the different types of reactive collections.
    /// </summary>
    /// <typeparam name="T">The type of items that the reactive collection manages.</typeparam>
    public interface IReactiveCollection<T> : INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyCollection<T>, IReadOnlyList<T>, IEnumerable<T>, IEnumerable, ICollection, IList
    {
        #region Properties

        /// <summary>
        /// Gets an observable, which fires before a new item is added to the collection.
        /// </summary>
        IObservable<T> BeforeItemAdded { get; }

        /// <summary>
        /// Gets an observable, which fires after a new item is added to the collection.
        /// </summary>
        IObservable<T> ItemAdded { get; }

        /// <summary>
        /// Gets an observable, which fires before an item is removed to the collection.
        /// </summary>
        IObservable<T> BeforeItemRemoved { get; }

        /// <summary>
        /// Gets an observable, which fires after an item is removed to the collection.
        /// </summary>
        IObservable<T> ItemRemoved { get; }

        /// <summary>
        /// Gets an observable, which fires when the count of the collection has changed.
        /// </summary>
        IObservable<int> CountChanged { get; }

        #endregion
    }
}