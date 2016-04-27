
#region Using Directives

using System.Threading.Tasks;
using System.Windows.Mvvm.Reactive;
using System.Windows.Mvvm.Services.Application;
using System.Windows.Mvvm.Services.Navigation;

#endregion

namespace System.Windows.Mvvm.Sample.ViewModels
{
    /// <summary>
    /// Represents the view model for the main window.
    /// </summary>
    public class MainWindowViewModel : ViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="MainWindowViewModel"/> instance.
        /// </summary>
        /// <param name="applicationService">The application service, which can be used to manage the application lifecycle.</param>
        public MainWindowViewModel(ApplicationService applicationService)
        {
            this.applicationService = applicationService;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the application service, which can be used to manage the application lifecycle.
        /// </summary>
        private readonly ApplicationService applicationService;

        #endregion

        #region Public Properties

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
            // Initializes the command, which shuts down the application
            this.ShutdownApplicationCommand = new ReactiveCommand(() =>
            {
                this.applicationService.Shutdown();
                return Task.FromResult(0);
            });

            // Since no asynchronous operation was performed, an empty task is returned
            return Task.FromResult(0);
        }

        #endregion
    }
}