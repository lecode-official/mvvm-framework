
#region Using Directives

using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Mvvm.Reactive;
using System.Windows.Mvvm.Sample.Models;
using System.Windows.Mvvm.Sample.Repositories;
using System.Windows.Mvvm.Sample.Views;
using System.Windows.Mvvm.Services.Navigation;

#endregion

namespace System.Windows.Mvvm.Sample.ViewModels
{
    /// <summary>
    /// Represents the view model for the main view of the application.
    /// </summary>
    public class MainViewModel : ViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="MainViewModel"/> instance.
        /// </summary>
        /// <param name="todoListItemsRepository">The todo list items repository, which can be used to manage the items on the todo list.</param>
        public MainViewModel(NavigationService navigationService, TodoListItemsRepository todoListItemsRepository)
        {
            this.navigationService = navigationService;
            this.todoListItemsRepository = todoListItemsRepository;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the navigation service, which is used to navigate between views.
        /// </summary>
        private readonly NavigationService navigationService;

        /// <summary>
        /// Contains the todo list items repository, which can be used to manage the items on the todo list.
        /// </summary>
        private readonly TodoListItemsRepository todoListItemsRepository;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items of the todo list.
        /// </summary>
        public ReactiveUI.ReactiveList<TodoListItemViewModel> TodoListItems { get; private set; } = new ReactiveUI.ReactiveList<TodoListItemViewModel> { ChangeTrackingEnabled = true };

        /// <summary>
        /// Gets or sets the currently selected todo list item.
        /// </summary>
        public ReactiveProperty<TodoListItemViewModel> SelectedTodoListItem { get; } = new ReactiveProperty<TodoListItemViewModel>();

        /// <summary>
        /// Gets the command, which marks the currently selected todo list item as finished.
        /// </summary>
        public ReactiveCommand MarkTodoListItemAsFinishedCommand { get; private set; }

        /// <summary>
        /// Gets the command, which removes the currently selected item from the todo list.
        /// </summary>
        public ReactiveCommand RemoveTodoListItemCommand { get; private set; }

        /// <summary>
        /// Gets the command, which navigates the user to the create todo list item view.
        /// </summary>
        public ReactiveCommand CreateTodoListItemCommand { get; private set; }

        #endregion

        #region ReactiveViewModel Implementation

        /// <summary>
        /// Is called when the view model is created. Initializes the commands of the view model.
        /// </summary>
        public override Task OnActivateAsync()
        {
            // Initializes the command, which marks the currently selected todo list item as finished
            this.MarkTodoListItemAsFinishedCommand = new ReactiveCommand(() =>
            {
                this.SelectedTodoListItem.Value.IsFinished.Value = true;
                this.todoListItemsRepository.MarkTodoListItemAsFinished(this.SelectedTodoListItem.Value.Id.Value);
                return Task.FromResult(0);
            }, this.SelectedTodoListItem.Changed.Select(x => this.SelectedTodoListItem != null));

            // Initializes the command, which removes the currently selected todo list item
            this.RemoveTodoListItemCommand = new ReactiveCommand(() =>
            {
                this.TodoListItems.Remove(this.SelectedTodoListItem.Value);
                this.todoListItemsRepository.RemoveTodoListItem(this.SelectedTodoListItem.Value.Id.Value);
                this.SelectedTodoListItem.Value = null;
                return Task.FromResult(0);
            }, this.SelectedTodoListItem.Changed.Select(x => this.SelectedTodoListItem != null));

            // Initializes the command, which navigates the user to the create todo list item view
            this.CreateTodoListItemCommand = new ReactiveCommand(async () =>  await this.navigationService.NavigateAsync<CreateTodoListItemView>());

            // Since no asynchronous operation was performed, an empty task is returned
            return Task.FromResult(0);
        }

        /// <summary>
        /// Is called when the user is navigated to the view of this view model. Loads the todo list items from storage.
        /// </summary>
        /// <param name="e">The navigation arguments, that contain more information about the navigation.</param>
        public override Task OnNavigateToAsync(NavigationEventArgs e)
        {
            // Clears the current list of todo list items first
            this.TodoListItems.Clear();

            // Loads all the todo list items from the repository and stores them in a list, so that the view has access to them
            foreach (TodoListItem todoListItem in this.todoListItemsRepository.GetTodoListItems().ToList())
                this.TodoListItems.Add(new TodoListItemViewModel(todoListItem));

            // Since no asynchronous operation was performed, an empty task is returned
            return Task.FromResult(0);
        }

        #endregion
    }
}