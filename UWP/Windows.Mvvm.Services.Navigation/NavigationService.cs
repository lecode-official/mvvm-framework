
#region Using Directives

using System;
using System.InversionOfControl.Abstractions;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

#endregion

namespace Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents a manager, that manages the navigation of a views within a window.
    /// </summary>
    public class NavigationService
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="NavigationService"/> instance.
        /// </summary>
        /// <param name="iocContainer">The IOC container which is used to instantiate the views and their corresponding view models.</param>
        /// <param name="navigationFrame">The frame with in which the actual navigation between views takes place.</param>
        internal NavigationService(IReadOnlyIocContainer iocContainer, Frame navigationFrame)
        {
            // Validates the arguments
            if (iocContainer == null)
                throw new ArgumentNullException(nameof(iocContainer));

            // Gets the application view, which represents the current window
            this.Window = ApplicationView.GetForCurrentView();

            // Stores the IoC container and the frame for later use
            this.iocContainer = iocContainer;
            this.navigationFrame = navigationFrame;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the IOC container which is used to instantiate the views and their corresponding view models.
        /// </summary>
        private readonly IReadOnlyIocContainer iocContainer;

        /// <summary>
        /// Contains the frame with in which the actual navigation between views takes place.
        /// </summary>
        private readonly Frame navigationFrame;

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets or sets the view model of the view that is currently in view of the window that is being managed by this navigation service.
        /// </summary>
        internal IViewModel CurrentViewModel { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the application view, which represents the window, managed by the navigation service.
        /// </summary>
        public ApplicationView Window { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Closes the window that is managed by this navigation service.
        /// </summary>
        public Task CloseWindowAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}