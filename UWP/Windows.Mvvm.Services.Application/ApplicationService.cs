
#region Using Directives

using System;
using System.Globalization;
using System.Windows.Threading;
using Windows.UI.Xaml;

#endregion

namespace Windows.Mvvm.Services.Application
{
    /// <summary>
    /// Represents a service, that provides functionality to manage the application life-cycle.
    /// </summary>
    public class ApplicationService
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the culture and the UI culture of the application.
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get
            {
                return CultureInfo.CurrentCulture;
            }

            set
            {
                CultureInfo.CurrentCulture = value;
                CultureInfo.CurrentUICulture = value;
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
                return UI.Xaml.Application.Current.Resources;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public virtual void Shutdown()
        {
            UI.Xaml.Application.Current.Exit();
        }
        
        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when the application is being resumed from the suspended state.
        /// </summary>
        public event EventHandler<object> Resuming
        {
            add
            {
                UI.Xaml.Application.Current.Resuming += value;
            }

            remove
            {
                UI.Xaml.Application.Current.Resuming -= value;
            }
        }

        /// <summary>
        /// Occurs when the applicaiton is being suspended.
        /// </summary>
        public event SuspendingEventHandler Suspending
        {
            add
            {
                UI.Xaml.Application.Current.Suspending += value;
            }

            remove
            {
                UI.Xaml.Application.Current.Suspending -= value;
            }
        }

        #endregion
    }
}