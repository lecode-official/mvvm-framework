
#region Using Directives

using MvvmFramework.Samples.Uwp.Repositories;
using MvvmFramework.Samples.Uwp.Views;
using System.InversionOfControl.Abstractions;
using System.InversionOfControl.Abstractions.SimpleIoc;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Mvvm.Application;
using Windows.Mvvm.Services.Application;
using Windows.Mvvm.Services.Dialog;
using Windows.Mvvm.Services.Navigation;

#endregion

namespace MvvmFramework.Samples.Uwp
{
    /// <summary>
    /// Represents the entry-point to the MVVM sample application.
    /// </summary>
    sealed partial class App : MvvmApplication
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="App"/> instance.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the IOC container which is used by the navigation service to activate the views and view models.
        /// </summary>
        private IIocContainer iocContainer;

        #endregion

        #region MvvmApplication Implementation

        /// <summary>
        /// Gets called when the app was activated by the user.
        /// </summary>
        /// <param name="eventArguments">The event argument, that contain more information on the activation of the application.</param>
        protected override async Task OnActivatedAsync(IActivatedEventArgs eventArguments)
        {
            // Makes sure that the initialization takes only place, when the application was previously not running
            if (eventArguments.PreviousExecutionState != ApplicationExecutionState.Running && eventArguments.PreviousExecutionState != ApplicationExecutionState.Suspended)
            {
                // Initializes the IOC container; in this sample the Simple IOC is used
                this.iocContainer = new SimpleIocContainer();

                // Binds all repositories and services that are needed for the application to the IoC container
                this.iocContainer.RegisterType<IReadOnlyIocContainer>(() => this.iocContainer);
                this.iocContainer.RegisterType<TodoListItemsRepository>(Scope.Singleton);
                this.iocContainer.RegisterType<WindowNavigationService>(Scope.Singleton);
                this.iocContainer.RegisterType<ApplicationService>(Scope.Singleton);
                this.iocContainer.RegisterType<DialogService>(Scope.Singleton);

                // Navigates the user to the main view
                WindowNavigationService windowNavigationService = this.iocContainer.GetInstance<WindowNavigationService>();
                await windowNavigationService.NavigateAsync<MainView>();
            }
        }

        #endregion
    }
}