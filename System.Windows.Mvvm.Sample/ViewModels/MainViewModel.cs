
#region Using Directives

using ReactiveUI;
using System.Windows.Mvvm.Reactive;
using System.Windows.Mvvm.Sample.Repositories;
using System.Threading.Tasks;
using System.Windows.Mvvm.Services.Navigation;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;

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

        /// <summary>
        /// Gets the items of the todo list.
        /// </summary>
        public ReactiveList<TodoListItemViewModel> TodoListItems { get; private set; } = new ReactiveList<TodoListItemViewModel> { ChangeTrackingEnabled = true };

        /// <summary>
        /// Contains the currently selected todo list item.
        /// </summary>
        private TodoListItemViewModel selectedTodoListItem;

        /// <summary>
        /// Gets or sets the currently selected todo list item.
        /// </summary>
        public TodoListItemViewModel SelectedTodoListItem
        {
            get
            {
                return this.selectedTodoListItem;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedTodoListItem, value);
            }
        }

        /// <summary>
        /// Gets the command, which marks the currently selected todo list item as finished.
        /// </summary>
        public ReactiveCommand<Unit> MarkTodoListItemAsFinishedCommand { get; private set; }

        /// <summary>
        /// Gets the command, which removes the currently selected item from the todo list.
        /// </summary>
        public ReactiveCommand<Unit> RemoveTodoListItemCommand { get; private set; }

        #endregion

        #region ReactiveViewModel Implementation

        /// <summary>
        /// Is called when the view model is created. Initializes the commands of the view model.
        /// </summary>
        public override Task OnActivateAsync()
        {
            // Initializes the command, which marks the currently selected todo list item as finished
            this.MarkTodoListItemAsFinishedCommand = ReactiveCommand.CreateAsyncTask(x =>
            {
                this.SelectedTodoListItem.IsFinished = true;
                this.todoListItemsRepository.MarkTodoListItemAsFinished(this.SelectedTodoListItem.Id);
                return Task.FromResult(Unit.Default);
            });

            // Initializes the command, which removes the currently selected todo list item
            this.MarkTodoListItemAsFinishedCommand = ReactiveCommand.CreateAsyncTask(x =>
            {
                this.TodoListItems.Remove(this.SelectedTodoListItem);
                this.todoListItemsRepository.RemoveTodoListItem(this.SelectedTodoListItem.Id);
                this.SelectedTodoListItem = null;
                return Task.FromResult(Unit.Default);
            });

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
            this.TodoListItems.AddRange(this.todoListItemsRepository.GetTodoListItems().Select(item => new TodoListItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                IsFinished = item.IsFinished
            }));

            // Since no asynchronous operation was performed, an empty task is returned
            return Task.FromResult(0);
        }

        #endregion
    }
}