
#region Using Directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;

#endregion

namespace Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a reactive implementation of command.
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter of the command.</typeparam>
    /// <typeparam name="TResult">The type of the result of the command.</typeparam>
    public class ReactiveCommand<TParameter, TResult> : ICommand, INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter, TResult}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="IsExecuting"/> observable.</param>
        /// <param name="blockOnExecution">Determines whether can execute always returns false while the command is being executed.</param>
        public ReactiveCommand(Func<TParameter, Task<TResult>> execute, IObservable<bool> canExecute, bool blockOnExecution)
        {
            // Stores the execute delegate, when no delegate was specified, then an empty delegate is used
            this.execute = execute ?? (parameter => Task.FromResult(default(TResult)));

            // Creates the observable for can execute
            this.CanExecute = Observable.CombineLatest(new List<IObservable<bool>>
            {
                canExecute != null ? canExecute.StartWith(false) : Observable.Return(true),
                blockOnExecution ? this.IsExecutingChanged.Select(x => !x) : Observable.Return(true),
                Observable.Return(true)
            }, latestResults => latestResults.All(result => result));

            // Subscribes to the can execute observable, it stores the latest value, which is used for the can execute method, also it raises the can execute changed event
            this.CanExecute.ObserveOnDispatcher().Subscribe(newValue =>
            {
                this.latestCanExecuteValue = newValue;
                this.canExecuteChanged?.Invoke(this, new EventArgs());
            });

            // Subscribes to the changing of the is executing property and invokes the property changed event
            this.IsExecutingChanged.ObserveOnDispatcher().Subscribe(newValue => this.propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsExecuting))));
        }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter, TResult}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="IsExecuting"/> observable.</param>
        public ReactiveCommand(Func<TParameter, Task<TResult>> execute, IObservable<bool> canExecute)
            : this(execute, canExecute, true)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter, TResult}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        public ReactiveCommand(Func<TParameter, Task<TResult>> execute)
            : this(execute, null, true)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter, TResult}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="IsExecuting"/> observable.</param>
        /// <param name="blockOnExecution">Determines whether can execute always returns false while the command is being executed.</param>
        public ReactiveCommand(Func<TParameter, TResult> execute, IObservable<bool> canExecute, bool blockOnExecution)
            : this(parameter => Task.FromResult(execute(parameter)), canExecute, blockOnExecution)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter, TResult}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="IsExecuting"/> observable.</param>
        public ReactiveCommand(Func<TParameter, TResult> execute, IObservable<bool> canExecute)
            : this(parameter => Task.FromResult(execute(parameter)), canExecute, true)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter, TResult}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        public ReactiveCommand(Func<TParameter, TResult> execute)
            : this(parameter => Task.FromResult(execute(parameter)), null, true)
        { }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the delegate that is to be executed when the command is being executed.
        /// </summary>
        private readonly Func<TParameter, Task<TResult>> execute;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets an observable, which determines whether the command can be executed or not.
        /// </summary>
        public IObservable<bool> CanExecute { get; private set; }

        /// <summary>
        /// Contains the latest value of the can execute observable. This is used as the return value of the can execute method.
        /// </summary>
        private bool latestCanExecuteValue;

        /// <summary>
        /// Contains a subject, which determines whether the command is currently being executed.
        /// </summary>
        private BehaviorSubject<bool> isExecutingChangedSubject = new BehaviorSubject<bool>(false);

        /// <summary>
        /// Gets an observable, which determines whether the command is currently being executed.
        /// </summary>
        public IObservable<bool> IsExecutingChanged
        {
            get
            {
                return this.isExecutingChangedSubject.AsObservable();
            }
        }

        /// <summary>
        /// Gets a value that determines whether the command is currently being exeucted or not.
        /// </summary>
        public bool IsExecuting { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        /// <returns>Returns the result of the command execution.</returns>
        public async Task<TResult> ExecuteAsync(TParameter parameter)
        {
            // Sets the is executing subject to true, because the command is currently being executed
            this.IsExecuting = true;
            this.isExecutingChangedSubject.OnNext(this.IsExecuting);

            // Invokes the delegate 
            TResult result = await this.execute(parameter);
            this.IsExecuting = false;
            this.isExecutingChangedSubject.OnNext(this.IsExecuting);
            return result;
        }

        #endregion

        #region ICommand Implementation

        /// <summary>
        /// An event, which is raised when the can execute changes.
        /// </summary>
        private event EventHandler canExecuteChanged;

        /// <summary>
        /// An event, which is raised when the can execute changes.
        /// </summary>
        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                this.canExecuteChanged += value;
            }

            remove
            {
                this.canExecuteChanged -= value;
            }
        }

        /// <summary>
        /// Determines whether the command can be executed right now.
        /// </summary>
        /// <param name="parameter">The parameter of the command.</param>
        /// <returns>Returns <c>true</c> when the command can be executed and <c>false</c> otherwise.</returns>
        bool ICommand.CanExecute(object parameter) => this.latestCanExecuteValue;

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The parameter of the command.</param>
        async void ICommand.Execute(object parameter) => await this.ExecuteAsync((TParameter)parameter);

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// An event, which is raised when a property in the reactive command has changed.
        /// </summary>
        private event PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// An event, which is raised when a property in the reactive command has changed.
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
    }
}