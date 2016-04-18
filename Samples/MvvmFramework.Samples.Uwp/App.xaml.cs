
#region Using Directives

using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Mvvm.Application;
using Windows.UI.Xaml;

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

        #region MvvmApplication Implementation

        /// <summary>
        /// Gets called when the app was activated by the user.
        /// </summary>
        /// <param name="eventArguments">The event argument, that contain more information on the activation of the application.</param>
        protected override async Task OnActivatedAsync(IActivatedEventArgs eventArguments)
        {
            // Calls the base implementation
            await base.OnActivatedAsync(eventArguments);
        }

        /// <summary>
        /// Gets called when the application is being resumed.
        /// </summary>
        protected override async Task OnResumingAsync()
        {
            // Calls the base implementation
            await base.OnResumingAsync();
        }

        /// <summary>
        /// Gets called when the app is being suspended. Can be used to save the current application state.
        /// </summary>
        protected override async Task OnSuspendingAsync()
        {
            // Calls the base implementation
            await base.OnSuspendingAsync();
        }

        /// <summary>
        /// Gets called if an exception was thrown that was not handled by user-code.
        /// </summary>
        /// <param name="eventArguments">The event arguments that contain further information about the exception that was not properly handled by user-code.</param>
        protected override async Task OnUnhandledExceptionAsync(UnhandledExceptionEventArgs eventArguments)
        {
            // Calls the base implementation
            await base.OnUnhandledExceptionAsync(eventArguments);
        }

        #endregion
    }
}