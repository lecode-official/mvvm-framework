
#region Using Directives

using System;
using System.Collections.Generic;
using System.InversionOfControl.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#endregion

namespace Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents a service that manages the creation and the life-cycle of the windows in the application.
    /// </summary>
    public class WindowNavigationService
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="WindowNavigationService"/> instance.
        /// </summary>
        /// <param name="iocContainer">The IOC container which is used to instantiate the windows and their corresponding view models.</param>
        public WindowNavigationService(IReadOnlyIocContainer iocContainer)
        {
            // Validates the arguments
            if (iocContainer == null)
                throw new ArgumentNullException(nameof(iocContainer));

            // Stores the IOC container for later use
            this.iocContainer = iocContainer;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the IOC container which is used to instantiate the windows and their corresponding view models.
        /// </summary>
        private readonly IReadOnlyIocContainer iocContainer;

        /// <summary>
        /// Contains the navigation services of all open windows.
        /// </summary>
        private ICollection<NavigationService> navigationServices = new List<NavigationService>();
        
        #endregion
        
        #region Private Methods

        /// <summary>
        /// Creates a new window.
        /// </summary>
        /// <param name="anchorViewId">The ID of the anchor window of the new window.</param>
        /// <param name="anchorViewSizePrefenrece">The preferred size for the anchor window.</param>
        /// <param name="newApplicationViewSizePrefenrece">The preferred size for the new window.</param>
        /// <returns>Returns a window creation result, which contains the a flag that determines whether the creation was successful and the navigation service, for the newly created window.</returns>
        private async Task<WindowCreationResult> CreateWindowAsync(Nullable<int> anchorViewId, ViewSizePreference anchorViewSizePrefenrece, ViewSizePreference newApplicationViewSizePrefenrece)
        {
            // Creates the result of the window creation a new navigation service for the window
            WindowCreationResult windowCreationResult = new WindowCreationResult();
            
            // Actually creates the new window and initializes it on its dispatcher
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Creates the navigation frame and the navigation service, which are needed to navigate within the newly created window
                Window.Current.Content = new Frame();
                windowCreationResult.NavigationService = new NavigationService(this.iocContainer, Window.Current.Content as Frame);

                // Signs up for the closed event of the window, when the closed event is raised, then the life-cycle methods of the view model is called and the the window is disposed of
                Window.Current.Closed += async (sender, e) => await this.CloseWindowAsync(windowCreationResult.NavigationService);

                // Makes sure that the window is properly activated
                Window.Current.Activate();
            });

            // Shows the window and determines whether the creation was successful
            if (anchorViewId.HasValue)
                windowCreationResult.WasSuccessful = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(windowCreationResult.NavigationService.Window.Id, newApplicationViewSizePrefenrece, anchorViewId.Value, anchorViewSizePrefenrece);
            else
                windowCreationResult.WasSuccessful = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(windowCreationResult.NavigationService.Window.Id, newApplicationViewSizePrefenrece);

            // Returns the result
            return windowCreationResult;
        }

        #endregion

        #region Public Methods
        
        public Task<WindowNavigationResult> NavigateAsync<TView>(object viewParameters, NavigationService anchorWindowNavigationService, ViewSizePreference newApplicationViewSizePrefenrece) => this.NavigateAsync<TView>(viewParameters, anchorWindowNavigationService, newApplicationViewSizePrefenrece, ViewSizePreference.Default);

        public Task<WindowNavigationResult> NavigateAsync<TView>(object viewParameters, ViewSizePreference newApplicationViewSizePrefenrece) => this.NavigateAsync<TView>(viewParameters, null, newApplicationViewSizePrefenrece, ViewSizePreference.Default);

        public Task<WindowNavigationResult> NavigateAsync<TView>(NavigationService anchorWindowNavigationService, ViewSizePreference newApplicationViewSizePrefenrece) => this.NavigateAsync<TView>(null, anchorWindowNavigationService, newApplicationViewSizePrefenrece, ViewSizePreference.Default);

        public Task<WindowNavigationResult> NavigateAsync<TView>(ViewSizePreference newApplicationViewSizePrefenrece) => this.NavigateAsync<TView>(null, null, newApplicationViewSizePrefenrece, ViewSizePreference.Default);

        public Task<WindowNavigationResult> NavigateAsync<TView>(object viewParameters) => this.NavigateAsync<TView>(viewParameters, null, ViewSizePreference.Default, ViewSizePreference.Default);

        public Task<WindowNavigationResult> NavigateAsync<TView>() => this.NavigateAsync<TView>(null, null, ViewSizePreference.Default, ViewSizePreference.Default);

        public async Task<WindowNavigationResult> NavigateAsync<TView>(object viewParameters, NavigationService anchorWindowNavigationService, ViewSizePreference newApplicationViewSizePrefenrece, ViewSizePreference anchorViewSizePrefenrece)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes the window of the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model of the window or the view model of the view that is hosted in the window that is to be closed.</param>
        /// <exception cref="ArgumentNullException">If the specified view model is <c>null</c>, an <see cref="ArgumentNullException"/> exception is thrown</exception>
        /// <exception cref="InvalidOperationException">If the view model does not belong to a window, an <see cref="InvalidOperationException"/> exception is thrown.</exception>
        public async Task CloseWindowAsync(IViewModel viewModel)
        {
            // Validates the paramters
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            // Gets the navigation service for the specified view model, if no navigation service could be found, an invalid operation exception is thrown
            NavigationService navigationService = this.GetNavigationService(viewModel);
            if (navigationService == null)
                throw new InvalidOperationException("The navigation service for the view model could not be found.");

            // Closes the window
            await this.CloseWindowAsync(navigationService);
        }
        
        /// <summary>
        /// Closes the window of the specified navigation manager.
        /// </summary>
        /// <param name="navigationService">The navigation manager whose window is to be closed.</param>
        /// <exception cref="ArgumentNullException">If the specified navigation manager is <c>null</c>, then an <see cref="ArgumentNullException"/> exception is thrown.</exception>
        public async Task CloseWindowAsync(NavigationService navigationService)
        {
            // Validates the parameters
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            // Closes the window
            await navigationService.CloseWindowAsync();

            // Removes the navigation manager from the list of window navigation managers
            this.navigationServices.Remove(navigationService);
            this.WindowClosed?.Invoke(this, new WindowEventArgs(navigationService));
        }

        /// <summary>
        /// Retrieves the navigation service for the window that currently displays the view with the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model for which the navigation service is to be retrieved.</param>
        /// <returns>Returns the navigation service for the view model if it exists.</returns>
        public NavigationService GetNavigationService(IViewModel viewModel) => this.navigationServices.FirstOrDefault(navigationService => navigationService.CurrentViewModel == viewModel);
        
        /// <summary>
        /// Shuts down the window navigation service by closing all views and windows and destroying their view models.
        /// </summary>
        public async Task DestroyAsync()
        {
            // Destroys all windows by closing them and destroying their view models
            await Task.WhenAll(this.navigationServices.Select(navigationService => navigationService.CloseWindowAsync()));
            this.navigationServices.Clear();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Is raised when the window navigation service created a new window.
        /// </summary>
        public event EventHandler<WindowEventArgs> WindowCreated;

        /// <summary>
        /// Is raised when the window navigation service closes a window.
        /// </summary>
        public event EventHandler<WindowEventArgs> WindowClosed;

        #endregion

        #region Nested Types

        /// <summary>
        /// Represents the result of the creation of a window.
        /// </summary>
        private class WindowCreationResult
        {
            #region Public Properties
            
            /// <summary>
            /// Gets or sets the navigation service, which was created for the window.
            /// </summary>
            public NavigationService NavigationService { get; set; }

            /// <summary>
            /// Gets or sets the result of the creation of the window.
            /// </summary>
            public bool WasSuccessful { get; set; }

            #endregion
        }

        #endregion
    }
}