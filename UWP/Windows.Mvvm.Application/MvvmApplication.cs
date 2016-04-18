
#region Using Directives

using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

#endregion

namespace Windows.Mvvm.Application
{
    /// <summary>
    /// Represents the base class for applications based on the MVVM pattern.
    /// </summary>
    public class MvvmApplication : UI.Xaml.Application
    {
        #region Constructors

        /// <summary>
        /// Initializesa a new <see cref="MvvmApplication"/> instance.
        /// </summary>
        public MvvmApplication()
        {
            // Signs up for the unhandled exception event, which is raised when an exception ramains unhandled by the user code, and dispatches it, so that the application is able to respond to it
            this.UnhandledException += (sender, eventArguments) => this.OnUnhandledExceptionAsync(eventArguments).Wait();

            // Signs up for the resuming event, which is raised when the application is being resumed, and dispatches it, so that the application is able to respond to it
            this.Resuming += (sender, eventArguments) => this.OnResumingAsync().Wait();

            // Signs up for the suspending event, which is raised just before the app is being suspended, it that case the suspension is deferred, so that the application is able to repond to it
            this.Suspending += async (sender, eventArguments) =>
            {
                SuspendingDeferral deferral = eventArguments.SuspendingOperation.GetDeferral();
                await this.OnSuspendingAsync();
                deferral.Complete();
            };
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets called when the app was activated by the user.
        /// </summary>
        /// <param name="eventArguments">The event argument, that contain more information on the activation of the application.</param>
        protected virtual Task OnActivatedAsync(IActivatedEventArgs eventArguments) => Task.FromResult(0);

        /// <summary>
        /// Gets called when the application is being resumed.
        /// </summary>
        protected virtual Task OnResumingAsync() => Task.FromResult(0);

        /// <summary>
        /// Gets called when the app is being suspended. Can be used to save the current application state.
        /// </summary>
        protected virtual Task OnSuspendingAsync() => Task.FromResult(0);

        /// <summary>
        /// Gets called if an exception was thrown that was not handled by user-code.
        /// </summary>
        /// <param name="eventArguments">The event arguments that contain further information about the exception that was not properly handled by user-code.</param>
        protected virtual Task OnUnhandledExceptionAsync(UnhandledExceptionEventArgs eventArguments) => Task.FromResult(0);

        #endregion

        #region Application Implementation

        /// <summary>
        /// Gets callen wehn the applicaiton is being activated via a launch.
        /// </summary>
        /// <param name="e">The event arguments, that contain more information about the lauch of the application.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs eventArguments) => await this.OnActivatedAsync(eventArguments);

        /// <summary>
        /// Gets called when the application is being activated any other way that the normal way.
        /// </summary>
        /// <param name="eventArguments">The event arguments, that contain more information about the activation of the application.</param>
        protected override async void OnActivated(IActivatedEventArgs eventArguments) => await this.OnActivatedAsync(eventArguments);

        #endregion
    }
}