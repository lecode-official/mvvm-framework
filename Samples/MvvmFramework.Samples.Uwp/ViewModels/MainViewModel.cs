
#region Using Directives

using MvvmFramework.Samples.Uwp.Models;
using MvvmFramework.Samples.Uwp.Repositories;
using MvvmFramework.Samples.Uwp.Views;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Mvvm.Reactive;
using Windows.Mvvm.Services.Application;
using Windows.Mvvm.Services.Navigation;

#endregion

namespace MvvmFramework.Samples.Uwp.ViewModels
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
        /// <param name="navigationService">The navigation service, which is used to navigate between views.</param>
        /// <param name="applicationService">The application service, which can be used to manage the application lifecycle.</param>
        /// <param name="todoListItemsRepository">The todo list items repository, which can be used to manage the items on the todo list.</param>
        public MainViewModel(NavigationService navigationService, ApplicationService applicationService, TodoListItemsRepository todoListItemsRepository)
        {
            this.navigationService = navigationService;
            this.applicationService = applicationService;
            this.todoListItemsRepository = todoListItemsRepository;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the navigation service, which is used to navigate between views.
        /// </summary>
        private readonly NavigationService navigationService;

        /// <summary>
        /// Contains the application service, which can be used to manage the application lifecycle.
        /// </summary>
        private readonly ApplicationService applicationService;

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

        /// <summary>
        /// Gets the command, which shuts down the application.
        /// </summary>
        public ReactiveCommand ShutdownApplicationCommand { get; private set; }

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
            }, this.SelectedTodoListItem.Select(x => x != null));

            // Initializes the command, which removes the currently selected todo list item
            this.RemoveTodoListItemCommand = new ReactiveCommand(() =>
            {
                this.todoListItemsRepository.RemoveTodoListItem(this.SelectedTodoListItem.Value.Id.Value);
                this.TodoListItems.Remove(this.SelectedTodoListItem.Value);
                this.SelectedTodoListItem.Value = null;
                return Task.FromResult(0);
            }, this.SelectedTodoListItem.Select(x => x != null));

            // Initializes the command, which shuts down the application
            this.ShutdownApplicationCommand = new ReactiveCommand(() =>
            {
                this.applicationService.Shutdown();
                return Task.FromResult(0);
            });

            // Initializes the command, which navigates the user to the create todo list item view
            this.CreateTodoListItemCommand = new ReactiveCommand(async () => await this.navigationService.NavigateAsync<CreateTodoListItemView>());

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