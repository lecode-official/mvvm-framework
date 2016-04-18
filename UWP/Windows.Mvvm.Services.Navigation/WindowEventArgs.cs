
#region Using Directives

using System;

#endregion

namespace Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents the event arguments that are passed to the window events of the window navigation service.
    /// </summary>
    public class WindowEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="WindowEventArgs"/> instance.
        /// </summary>
        /// <param name="navigationService">The navigation service which is affected by the event.</param>
        public WindowEventArgs(NavigationService navigationService)
        {
            this.NavigationService = navigationService;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the navigation service which is affected by the event.
        /// </summary>
        public NavigationService NavigationService { get; set; }

        #endregion
    }
}