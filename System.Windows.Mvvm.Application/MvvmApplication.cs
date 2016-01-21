
#region Using Directives

using Ninject;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

#endregion

namespace System.Windows.Mvvm.Application
{
    /// <summary>
    /// Represents the base class for applications based on the MVVM pattern.
    /// </summary>
    public class MvvmApplication : Windows.Application, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// Contains a mutex that is used to prevent an application to open twice.
        /// </summary>
        private Mutex singleInstanceMutex = null;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the Ninject kernel, which is used to instantiate views and view models.
        /// </summary>
        protected IKernel Kernel { get; private set; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets called when there already is another instance of the same application running. This callback method is called before <see cref="OnStartedAsync"/> is invoked.
        /// </summary>
        protected virtual Task OnStartedAsAdditionalInstanceAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Gets called after the application startup. This can be overridden by the user to implement custom startup logic and displaying views.
        /// </summary>
        /// <param name="eventArguments">The arguments that contain more information about the application startup and the navigation service, which can be used to navigate to the intial window and view.</param>
        protected virtual Task OnStartedAsync(ApplicationStartedEventArgs eventArguments)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Gets called right before the application quits. This can be overridden by the user to implement custom shutdown logic.
        /// </summary>
        protected virtual Task OnExitAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Gets called if an exception was thrown that was not handled by user-code.
        /// </summary>
        /// <param name="eventArguments">The event arguments that contain further information about the exception that was not properly handled by user-code.</param>
        protected virtual Task OnUnhandledExceptionAsync(UnhandledExceptionEventArgs eventArguments)
        {
            return Task.FromResult(0);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Is called once the application has started.
        /// </summary>
        /// <param name="e">The event arguments that contain more information about the application startup.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            // Calls the base implementation of this method
            base.OnStartup(e);

            // Safely instantiates the Ninject kernel (since it implements IDisposible we are using the pattern suggested in the documentation of the CA2000)
            StandardKernel temporaryKernel = null;
            try
            {
                // Creates the new kernel
                temporaryKernel = new StandardKernel();

                // Swaps the temporary kernel with the final one
                this.Kernel = temporaryKernel;
                temporaryKernel = null;
            }
            finally
            {
                // Checks if the temporary kernel is not null, this can only happen if an error occurred, if so the temporary kernel is disposed of, so that all resources are safely removed from memory
                if (temporaryKernel != null)
                    temporaryKernel.Dispose();
            }

            // Determines whether this application instance is the first instance of the application or whether another instance is already running (this can be used to force the application to be a single instance application)
            bool isFirstApplicationInstance;
            Mutex mutex = new Mutex(true, Assembly.GetExecutingAssembly().FullName, out isFirstApplicationInstance);

            // As this is the first instance of the application, the mutex is set and stores, so that it can be disposed of when this instance closes
            if (isFirstApplicationInstance)
                this.singleInstanceMutex = mutex;

            // Signs up for the unhandled exception event, which is raised when an exception was thrown, which was not handled by user-code
            AppDomain.CurrentDomain.UnhandledException += async (sender, eventArguments) => await this.OnUnhandledExceptionAsync(eventArguments);

            // Calls the on started method where the user is able to call his own code to set up the application
            await this.OnStartedAsync(new ApplicationStartedEventArgs(e.Args, isFirstApplicationInstance));
        }

        /// <summary>
        /// Is called just before the application is shut down.
        /// </summary>
        /// <param name="e">The event arguments that contain more information about the application shutdown.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            // Calls the base implementation of this method
            base.OnExit(e);

            // Calls the on exit event handler, where the user is able to do custom shutdown operations
            this.OnExitAsync().Wait();

            // Calls the dispose method of the application
            this.Dispose(true);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Disposes of all managed and unmanaged resources that have been allocated. This method can be overridden by sub-classes in order to implement custom disposal logic.
        /// </summary>
        /// <param name="disposing">
        /// Determines whether only unmanaged, or managed and unmaned resources should be disposed of. This is needed when the method is called from the destructor, because when the destructor is called all managed resources have already been disposed of.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // Checks if managed resources should be disposed of
            if (disposing)
            {
                // Disposes of the kernel if it has not been disposed of, yet
                if (this.Kernel != null && !this.Kernel.IsDisposed)
                {
                    this.Kernel.Dispose();
                    this.Kernel = null;
                }

                // Disposes of the mutex, which was created to determine whether this instance is the first instance of the application
                if (this.singleInstanceMutex != null)
                {
                    this.singleInstanceMutex.Dispose();
                    this.singleInstanceMutex = null;
                }
            }
        }

        /// <summary>
        /// Disposes of all managed and unmanaged resources that were allocated.
        /// </summary>
        public void Dispose()
        {
            // Calls the virtual dispose method, which may be overridden by sub-classes in order to dispose of their resources
            this.Dispose(true);

            // Suppresses the finalizer for this object, because all resources have already been disposed of
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Finalizers

        /// <summary>
        /// Destroys this object instance (when the dispose method has already been called the finalization for this object instance is suppressed and therefore the finalizer is not called).
        /// </summary>
        ~MvvmApplication()
        {
            // Since the object is being finalized, all other managed resources have already been disposed of, therefore only the unmanaged resources are being disposed of
            this.Dispose(false);
        }

        #endregion
    }
}