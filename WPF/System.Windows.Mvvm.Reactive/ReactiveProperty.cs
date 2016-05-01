
#region Using Directives

using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a property, which can be observed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    public class ReactiveProperty<T> : INotifyPropertyChanged, IObservable<T>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveProperty{T}"/> instance.
        /// </summary>
        /// <param name="value">The initial value of the reactive property.</param>
        /// <param name="onlyRaiseIfChanged">A value that determines if the <see cref="Changing"/> and <see cref="Changed"/> observables are only raised if the value has actually changed.</param>
        public ReactiveProperty(T value, bool onlyRaiseIfChanged)
        {
            // Stores the parameters for later use
            this.Value = value;
            this.OnlyRaiseIfChanged = onlyRaiseIfChanged;

            // Subscribes to the Changed observable and fires the PropertyChanged event if the Changed observable has fired
            this.Changed.ObserveOnDispatcher().Subscribe(x =>
            {
                this.propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
                this.propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HasValue)));
            });
        }

        /// <summary>
        /// Initializes a new <see cref="ReactiveProperty{T}"/> instance.
        /// </summary>
        /// <param name="value">The initial value of the reactive property.</param>
        public ReactiveProperty(T value)
            : this(value, true)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveProperty{T}"/> instance.
        /// </summary>
        public ReactiveProperty()
            : this(default(T))
        { }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains a subject, into which the current value of the reactive property is always pushed.
        /// </summary>
        private BehaviorSubject<T> reactivePropertyValueSubject;

        #endregion

        #region Public Properties

        /// <summary>
        /// Contains the value of the reactive property.
        /// </summary>
        private T value;

        /// <summary>
        /// Gets or sets the value of the reactive property.
        /// </summary>
        public T Value
        {
            get
            {
                return this.value;
            }

            set
            {
                // Checks if the subscriber only wants to be notified of actual changes, if so and the value does not have changed, then nothing needs to be done
                if (this.OnlyRaiseIfChanged && object.Equals(this.value, value))
                    return;

                // Raises the changing and changed subjects, updates the reactive property value observable, and sets the new value
                this.changing?.OnNext(this.value);
                this.value = value;
                this.reactivePropertyValueSubject?.OnNext(this.value);
                this.changed.OnNext(this.value);
            }
        }

        /// <summary>
        /// Gets a value that determines whether the property has a value (or if the value is <c>default(T)</c>).
        /// </summary>
        public bool HasValue
        {
            get
            {
                return !object.Equals(this.Value, default(T));
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if the <see cref="Changing"/> and <see cref="Changed"/> observables are only raised if the value has actually changed.
        /// </summary>
        public bool OnlyRaiseIfChanged { get; set; }

        /// <summary>
        /// Contains a subject, which determines whether the property is currently being changed.
        /// </summary>
        private Subject<T> changing;

        /// <summary>
        /// Gets an observable, which determines whether the property is currently being changed.
        /// </summary>
        public IObservable<T> Changing
        {
            get
            {
                if (this.changing == null)
                    this.changing = new Subject<T>();
                return this.changing.AsObservable();
            }
        }

        /// <summary>
        /// Contains a subject, which determines whether the property has been changed.
        /// </summary>
        private Subject<T> changed = new Subject<T>();

        /// <summary>
        /// Gets an observable, which determines whether the property has been changed.
        /// </summary>
        public IObservable<T> Changed
        {
            get
            {
                return this.changed.AsObservable();
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// An event, which is invoked when the property has changed
        /// </summary>
        private event PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// An event, which is invoked when the property has changed
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

        #region IObservable Implementation

        /// <summary>
        /// Subscribes to the reactive property observable, which always gives the observer the current value of the reactive property.
        /// </summary>
        /// <param name="observer">The observer of the reactive property value.</param>
        /// <returns>Returns a disposable, which can be used by the observer to stop receiving messages from the observable.</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            // Lazily initializes the behavior subject for the reactive property value (this reduces the overhead while no one is observing the reactive property value)
            if (this.reactivePropertyValueSubject == null)
                this.reactivePropertyValueSubject = new BehaviorSubject<T>(this.value);

            // Subscribes the user to the reactive property value
            return this.reactivePropertyValueSubject.Subscribe(observer);
        }

        #endregion
    }
}