
#region Using Directives

using System;
using System.Reactive;
using System.Threading.Tasks;

#endregion

namespace Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a reactive implementation of command.
    /// </summary>
    public class ReactiveCommand : ReactiveCommand<object, Unit>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="p:IsExecuting"/> observable.</param>
        /// <param name="blockOnExecution">Determines whether can execute always returns false while the command is being executed.</param>
        public ReactiveCommand(Func<Task> execute, IObservable<bool> canExecute, bool blockOnExecution)
            : base(async (parameter) =>
            {
                await execute();
                return Unit.Default;
            }, canExecute, blockOnExecution)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="p:IsExecuting"/> observable.</param>
        public ReactiveCommand(Func<Task> execute, IObservable<bool> canExecute)
            : base(async parameter =>
            {
                await execute();
                return Unit.Default;
            }, canExecute)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        public ReactiveCommand(Func<Task> execute)
            : base(async parameter =>
            {
                await execute();
                return Unit.Default;
            })
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="IsExecuting"/> observable.</param>
        /// <param name="blockOnExecution">Determines whether can execute always returns false while the command is being executed.</param>
        public ReactiveCommand(Action execute, IObservable<bool> canExecute, bool blockOnExecution)
            : this(() =>
            {
                execute();
                return Task.FromResult(Unit.Default);
            }, canExecute, blockOnExecution)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="IsExecuting"/> observable.</param>
        public ReactiveCommand(Action execute, IObservable<bool> canExecute)
            : this(() =>
            {
                execute();
                return Task.FromResult(Unit.Default);
            }, canExecute, true)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        public ReactiveCommand(Action execute)
            : this(() =>
            {
                execute();
                return Task.FromResult(Unit.Default);
            }, null, true)
        { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the command.
        /// </summary>
        public Task ExecuteAsync() => base.ExecuteAsync(null);

        #endregion
    }
}