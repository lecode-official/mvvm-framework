
#region Using Directives

using System.Windows.Mvvm.Reactive;
using System.Windows.Mvvm.Sample.Repositories;

#endregion

namespace System.Windows.Mvvm.Sample.ViewModels
{
    /// <summary>
    /// Represents the view model for the main view of the application.
    /// </summary>
    public class MainViewModel : ReactiveViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="MainViewModel"/> instance.
        /// </summary>
        /// <param name="todoListItemsRepository">The todo list items repository, which can be used to manage the items on the todo list.</param>
        public MainViewModel(TodoListItemsRepository todoListItemsRepository)
        {
            this.todoListItemsRepository = todoListItemsRepository;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the todo list items repository, which can be used to manage the items on the todo list.
        /// </summary>
        private readonly TodoListItemsRepository todoListItemsRepository;

        #endregion

        #region Public Properties



        #endregion

        #region ReactiveViewModel Implementation



        #endregion
    }
}