
#region Using Directives

using System.Reactive;
using System.Threading.Tasks;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a reactive implementation of command.
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter of the command.</typeparam>
    public class ReactiveCommand<TParameter> : ReactiveCommand<TParameter, Unit>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="p:IsExecuting"/> observable.</param>
        /// <param name="blockOnExecution">Determines whether can execute always returns false while the command is being executed.</param>
        public ReactiveCommand(Func<TParameter, Task> execute, IObservable<bool> canExecute, bool blockOnExecution)
            : base(async parameter =>
            {
                await execute(parameter);
                return Unit.Default;
            }, canExecute, blockOnExecution)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="p:IsExecuting"/> observable.</param>
        public ReactiveCommand(Func<TParameter, Task> execute, IObservable<bool> canExecute)
            : base(async parameter =>
            {
                await execute(parameter);
                return Unit.Default;
            }, canExecute)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        public ReactiveCommand(Func<TParameter, Task> execute)
            : base(async parameter =>
            {
                await execute(parameter);
                return Unit.Default;
            })
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="IsExecuting"/> observable.</param>
        /// <param name="blockOnExecution">Determines whether can execute always returns false while the command is being executed.</param>
        public ReactiveCommand(Action<TParameter> execute, IObservable<bool> canExecute, bool blockOnExecution)
            : this(parameter =>
            {
                execute(parameter);
                return Task.FromResult(Unit.Default);
            }, canExecute, blockOnExecution)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        /// <param name="canExecute">An observable, which determines whether the command can be executed or not. If <c>null</c> is specified, then the constructor defaults to the <see cref="IsExecuting"/> observable.</param>
        public ReactiveCommand(Action<TParameter> execute, IObservable<bool> canExecute)
            : this(parameter =>
            {
                execute(parameter);
                return Task.FromResult(Unit.Default);
            }, canExecute, true)
        { }

        /// <summary>
        /// Initializes a new <see cref="ReactiveCommand{TParameter}"/> instance.
        /// </summary>
        /// <param name="execute">The delegate that is to be executed when the command is being executed.</param>
        public ReactiveCommand(Action<TParameter> execute)
            : this(parameter =>
            {
                execute(parameter);
                return Task.FromResult(Unit.Default);
            }, null, true)
        { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public new Task ExecuteAsync(TParameter parameter) => base.ExecuteAsync(parameter);

        #endregion
    }
}