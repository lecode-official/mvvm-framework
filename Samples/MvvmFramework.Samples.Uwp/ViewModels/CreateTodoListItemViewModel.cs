
#region Using Directivs

using MvvmFramework.Samples.Uwp.Repositories;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using Windows.Mvvm.Reactive;
using Windows.Mvvm.Services.Application;
using Windows.Mvvm.Services.Navigation;

#endregion

namespace MvvmFramework.Samples.Uwp.ViewModels
{
    /// <summary>
    /// Represents a view model for the create todo list item view.
    /// </summary>
    public class CreateTodoListItemViewModel : ReactiveViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CreateTodoListItemViewModel"/> instance.
        /// </summary>
        /// <param name="navigationService">The navigation service, which is used to navigate between views.</param>
        /// <param name="applicationService">The application service, which can be used to manage the application lifecycle.</param>
        /// <param name="todoListItemsRepository">The todo list items repository, which can be used to manage the items on the todo list.</param>
        public CreateTodoListItemViewModel(NavigationService navigationService, ApplicationService applicationService, TodoListItemsRepository todoListItemsRepository)
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
        /// Contains the title of the new todo list item.
        /// </summary>
        private string title;

        /// <summary>
        /// Gets or sets the title of the new todo list item.
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.title, value);
            }
        }

        /// <summary>
        /// Contains the title of the new todo list item.
        /// </summary>
        private string description;

        /// <summary>
        /// Gets or sets the description of the new todo list item.
        /// </summary>
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.description, value);
            }
        }

        /// <summary>
        /// Gets the command, which cancels the creation dialog and returns to the previous view.
        /// </summary>
        public ReactiveCommand<NavigationResult> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the command, which saves the new todo list item and navigates the user back to the main view.
        /// </summary>
        public ReactiveCommand<NavigationResult> SaveCommand { get; private set; }

        /// <summary>
        /// Gets the command, which shuts down the application.
        /// </summary>
        public ReactiveCommand<Unit> ShutdownApplicationCommand { get; private set; }

        #endregion

        #region ReactiveViewModel Implementation

        /// <summary>
        /// Is called when the view model is created. Initializes the commands of the view model.
        /// </summary>
        public override Task OnActivateAsync()
        {
            // Initializes the command, which cancels the creation and returns to the previous view
            this.CancelCommand = ReactiveCommand.CreateAsyncTask(async x => await this.navigationService.NavigateBackAsync());

            // Initializes the command, which creates the new todo list item and navigates the user to the main view
            this.SaveCommand = ReactiveCommand.CreateAsyncTask(async x =>
            {
                // Creates the new todo list item
                this.todoListItemsRepository.CreateTodoListItem(this.Title, this.Description);

                // Navigates the user back to the main view
                return await this.navigationService.NavigateBackAsync();
            });

            // Initializes the command, which shuts down the application
            this.ShutdownApplicationCommand = ReactiveCommand.CreateAsyncTask(x =>
            {
                this.applicationService.Shutdown();
                return Task.FromResult(Unit.Default);
            });

            // Since no asynchronous operation was performed, an empty task is returned
            return Task.FromResult(0);
        }

        #endregion
    }
}