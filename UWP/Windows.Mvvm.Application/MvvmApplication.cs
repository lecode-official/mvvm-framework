
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
        #region Protected Methods
        
        /// <summary>
        /// Gets called when the application was normally started by the user, i.e. the application is launched for the first time, was terminated by the operating system, or previously closed by the user.
        /// </summary>
        /// <param name="eventArguments">The event argument, that contain more information on the launching of the application.</param>
        protected virtual Task OnLaunchedAsync(LaunchActivatedEventArgs eventArguments) => Task.FromResult(0);

        /// <summary>
        /// Gets called when the app was activated by the user, i.e. the application was previously in the running or the suspended state.
        /// </summary>
        /// <param name="eventArguments">The event argument, that contain more information on the activation of the application.</param>
        protected virtual Task OnActivatedAsync(LaunchActivatedEventArgs eventArguments) => Task.FromResult(0);

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
        /// Gets called when the application is being started or brought back from the background or from suspension, by the user.
        /// </summary>
        /// <param name="e">The event arguments, that contain more information about the launch of the application.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Calls the base implementation of this method
            base.OnLaunched(e);

            // Signs up for the unhandled exception event, which is raised when an exception ramains unhandled by the user code, and dispatches it, so that the application is able to respond to it
            this.UnhandledException += (sender, eventArguments) => this.OnUnhandledExceptionAsync(eventArguments).Wait();

            // Signs up for the resuming event, which is raised when the application is being resumed, and dispatches it, so that the application is able to respond to it
            this.Resuming += async (sender, eventArguments) => await this.OnResumingAsync();

            // Signs up for the suspending event, which is raised just before the app is being suspended, it that case the suspension is deferred, so that the application is able to repond to it
            this.Suspending += async (sender, eventArguments) =>
            {
                SuspendingDeferral deferral = eventArguments.SuspendingOperation.GetDeferral();
                await this.OnSuspendingAsync();
                deferral.Complete();
            };

            // Checks whether the application was just started or whether the application was previously running or in suspension and dispatches the calls accordingly, so that the application is able to respond to it
            switch (e.PreviousExecutionState)
            {
                case ApplicationExecutionState.Running:
                case ApplicationExecutionState.Suspended:
                    await this.OnLaunchedAsync(e);
                    break;
                default:
                    await this.OnActivatedAsync(e);
                    break;
            }
        }

        #endregion
    }
}