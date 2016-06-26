
#region Using Directives

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Mvvm.Reactive;
using System.Windows.Mvvm.Sample.Repositories;
using System.Windows.Mvvm.Services.Dialog;
using System.Windows.Mvvm.Services.Navigation;

#endregion

namespace System.Windows.Mvvm.Sample.ViewModels
{
    /// <summary>
    /// Represents a view model for the create todo list item view.
    /// </summary>
    public class CreateTodoListItemViewModel : ViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CreateTodoListItemViewModel"/> instance.
        /// </summary>
        /// <param name="navigationService">The navigation service, which is used to navigate between views.</param>
        /// <param name="dialogService">The dialog service, which provides access to the various dialogs offered by Windows.</param>
        /// <param name="todoListItemsRepository">The todo list items repository, which can be used to manage the items on the todo list.</param>
        public CreateTodoListItemViewModel(NavigationService navigationService, DialogService dialogService, TodoListItemsRepository todoListItemsRepository)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.todoListItemsRepository = todoListItemsRepository;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the navigation service, which is used to navigate between views.
        /// </summary>
        private readonly NavigationService navigationService;

        /// <summary>
        /// Contains the dialog service, which provides access to the various dialogs offered by Windows.
        /// </summary>
        private readonly DialogService dialogService;

        /// <summary>
        /// Contains the todo list items repository, which can be used to manage the items on the todo list.
        /// </summary>
        private readonly TodoListItemsRepository todoListItemsRepository;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the title property of the new todo list item.
        /// </summary>
        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>(self => self.Select(value => new ValidationResult(string.IsNullOrWhiteSpace(value) ? "Please specify a title." : null)));

        /// <summary>
        /// Gets the description property of the new todo list item.
        /// </summary>
        public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>(self => self.Select(value => new ValidationResult(string.IsNullOrWhiteSpace(value) ? "Please describe the todo item." : null)));

        /// <summary>
        /// Gets the command, which cancels the creation dialog and returns to the previous view.
        /// </summary>
        public ReactiveCommand CancelCommand { get; private set; }

        /// <summary>
        /// Gets the command, which saves the new todo list item and navigates the user back to the main view.
        /// </summary>
        public ReactiveCommand SaveCommand { get; private set; }

        #endregion

        #region ReactiveViewModel Implementation

        /// <summary>
        /// Is called when the view model is created. Initializes the commands of the view model.
        /// </summary>
        public override Task OnActivateAsync()
        {
            // Initializes the command, which cancels the creation and returns to the previous view
            this.CancelCommand = new ReactiveCommand(async () =>
            {
                if (await this.dialogService.ShowMessageBoxDialogAsync("Do you really want to cancel the creation of the todo list item.", "Confirm cancellation", Services.Dialog.MessageBoxButton.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    await this.navigationService.NavigateBackAsync();
            });

            // Initializes the command, which creates the new todo list item and navigates the user to the main view
            this.SaveCommand = new ReactiveCommand(async () =>
            {
                // Creates the new todo list item
                this.todoListItemsRepository.CreateTodoListItem(this.Title.Value, this.Description.Value);

                // Navigates the user back to the main view
                await this.navigationService.NavigateBackAsync();
            }, Observable.CombineLatest(this.Title.IsValidChanged, this.Description.IsValidChanged, (first, second) => first && second));

            // Since no asynchronous operation was performed, an empty task is returned
            return Task.FromResult(0);
        }

        #endregion
    }
}