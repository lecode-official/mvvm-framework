
#region Using Directives

using System.Globalization;
using System.Threading;
using System.Windows.Threading;

#endregion

namespace System.Windows.Mvvm.Services.Application
{
    /// <summary>
    /// Represents a service, that provides functionality to manage the application life-cycle.
    /// </summary>
    public class ApplicationService
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the culture and the UI culture of the current Thread.
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }

            set
            {
                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
            }
        }

        /// <summary>
        /// Gets the dispatcher that is associated with the current thread.
        /// </summary>
        public virtual Dispatcher CurrentDispatcher
        {
            get
            {
                return Dispatcher.CurrentDispatcher;
            }
        }
        
        /// <summary>
        /// Gets the XAML resources that have been defined application-wide.
        /// </summary>
        public ResourceDictionary ApplicationResources
        {
            get
            {
                return Windows.Application.Current.Resources;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shuts down the application with a standard error code (0x0).
        /// </summary>
        public virtual void Shutdown()
        {
            Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Shuts down the application with the provided exit code.
        /// </summary>
        /// <param name="exitCode">The exit code that is handed to the operating system to specify the result of the application. This could be an error or success code.</param>
        public virtual void Shutdown(int exitCode)
        {
            // Shuts down the application with the provided exit code
            Windows.Application.Current.Shutdown(exitCode);
        }

        /// <summary>
        /// Shuts down the current instance of the application and directly launches a new instance. Be careful using this method, since the underlying implementation has problems with preserving the command line arguments.
        /// </summary>
        public virtual void Restart()
        {
            // Restarts the application and shuts down the current one
            Forms.Application.Restart();
            Windows.Application.Current.Shutdown();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when an application becomes the foreground application.
        /// </summary>
        public event EventHandler Activated
        {
            add
            {
                Windows.Application.Current.Activated += value;
            }

            remove
            {
                Windows.Application.Current.Activated -= value;
            }
        }

        /// <summary>
        /// Occurs when an application stops being the foreground application.
        /// </summary>
        public event EventHandler Deactivated
        {
            add
            {
                Windows.Application.Current.Deactivated += value;
            }

            remove
            {
                Windows.Application.Current.Deactivated -= value;
            }
        }

        /// <summary>
        /// Occurs just before an application shuts down, and cannot be canceled.
        /// </summary>
        public event ExitEventHandler Exit
        {
            add
            {
                Windows.Application.Current.Exit += value;
            }

            remove
            {
                Windows.Application.Current.Exit -= value;
            }
        }

        /// <summary>
        /// Occurs when the user ends the Windows session by logging off or shutting down the operating system.
        /// </summary>
        public event SessionEndingCancelEventHandler SessionEnding
        {
            add
            {
                Windows.Application.Current.SessionEnding += value;
            }

            remove
            {
                Windows.Application.Current.SessionEnding -= value;
            }
        }

        #endregion
    }
}