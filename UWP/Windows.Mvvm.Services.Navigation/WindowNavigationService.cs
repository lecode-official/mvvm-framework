
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
        /// <param name="anchorWindowSizePreference">The preferred size for the anchor window.</param>
        /// <param name="newWindowSizePreference">The preferred size for the new window.</param>
        /// <returns>Returns a window creation result, which contains the a flag that determines whether the creation was successful and the navigation service, for the newly created window.</returns>
        private async Task<WindowCreationResult> CreateWindowAsync(Nullable<int> anchorViewId, ViewSizePreference anchorWindowSizePreference, ViewSizePreference newWindowSizePreference)
        {
            // Creates the result of the window creation a new navigation service for the window
            WindowCreationResult windowCreationResult = new WindowCreationResult();

            // Checks if this is the first creation of a new window (which is when there are no navigation services, yet), if so then the default window, which is created at application startup, is taken, otherwise a new window is created
            CoreApplicationView newWindow;
            if (this.navigationServices.Any())
                newWindow = CoreApplication.CreateNewView();
            else
                newWindow = CoreApplication.GetCurrentView();

            // Actually creates the new window and initializes it on its dispatcher
            await newWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Creates the navigation frame and the navigation service, which are needed to navigate within the newly created window
                Window.Current.Content = new Frame();
                windowCreationResult.NavigationService = new NavigationService(this.iocContainer, Window.Current, ApplicationView.GetForCurrentView(), Window.Current.Content as Frame);

                // Signs up for the closed event of the window, when the closed event is raised, then the life-cycle methods of the view model is called and the the window is disposed of
                Window.Current.Closed += async (sender, e) => await this.CloseWindowAsync(windowCreationResult.NavigationService);

                // Makes sure that the window is properly activated
                Window.Current.Activate();
            });

            // Shows the window and determines whether the creation was successful (but the window only needs to shown, if this is not the first navigation, because the default window, that is created at application startup, is already open)
            if (this.navigationServices.Any())
            {
                if (anchorViewId.HasValue)
                    windowCreationResult.WasSuccessful = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(windowCreationResult.NavigationService.ApplicationView.Id, newWindowSizePreference, anchorViewId.Value, anchorWindowSizePreference);
                else
                    windowCreationResult.WasSuccessful = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(windowCreationResult.NavigationService.ApplicationView.Id, newWindowSizePreference);
            }

            // Returns the result
            return windowCreationResult;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new window and navigates to the specified view.
        /// </summary>
        /// <typeparam name="TView">The type of the view to which the user should be navigated in the new window.</typeparam>
        /// <param name="parameters">The parameters, which are injected in to the view model of the view.</param>
        /// <param name="newWindowSizePreference">The size preference for the new window, which determines how the new window is arranged in respect to the anchor window.</param>
        /// <param name="anchorWindowNavigationService">The navigation service of the window, which is the anchor for the new window. This can be used to specify how the two windows are being arranged by Windows.</param>
        /// <returns>Returns the result of the navigation.</returns>
        public Task<WindowNavigationResult> NavigateAsync<TView>(object parameters, ViewSizePreference newWindowSizePreference, NavigationService anchorWindowNavigationService) where TView : Page => this.NavigateAsync<TView>(parameters, newWindowSizePreference, anchorWindowNavigationService, ViewSizePreference.Default);

        /// <summary>
        /// Creates a new window and navigates to the specified view.
        /// </summary>
        /// <typeparam name="TView">The type of the view to which the user should be navigated in the new window.</typeparam>
        /// <param name="parameters">The parameters, which are injected in to the view model of the view.</param>
        /// <param name="newWindowSizePreference">The size preference for the new window, which determines how the new window is arranged in respect to the window from which the new window is being opened.</param>
        /// <returns>Returns the result of the navigation.</returns>
        public Task<WindowNavigationResult> NavigateAsync<TView>(object parameters, ViewSizePreference newWindowSizePreference) where TView : Page => this.NavigateAsync<TView>(parameters, newWindowSizePreference, null, ViewSizePreference.Default);

        /// <summary>
        /// Creates a new window and navigates to the specified view.
        /// </summary>
        /// <typeparam name="TView">The type of the view to which the user should be navigated in the new window.</typeparam>
        /// <param name="newWindowSizePreference">The size preference for the new window, which determines how the new window is arranged in respect to the anchor window.</param>
        /// <param name="anchorWindowNavigationService">The navigation service of the window, which is the anchor for the new window. This can be used to specify how the two windows are being arranged by Windows.</param>
        /// <returns>Returns the result of the navigation.</returns>
        public Task<WindowNavigationResult> NavigateAsync<TView>(ViewSizePreference newWindowSizePreference, NavigationService anchorWindowNavigationService) where TView : Page => this.NavigateAsync<TView>(null, newWindowSizePreference, anchorWindowNavigationService, ViewSizePreference.Default);

        /// <summary>
        /// Creates a new window and navigates to the specified view.
        /// </summary>
        /// <typeparam name="TView">The type of the view to which the user should be navigated in the new window.</typeparam>
        /// <param name="newWindowSizePreference">The size preference for the new window, which determines how the new window is arranged in respect to the window from which the new window is being opened.</param>
        /// <returns>Returns the result of the navigation.</returns>
        public Task<WindowNavigationResult> NavigateAsync<TView>(ViewSizePreference newWindowSizePreference) where TView : Page => this.NavigateAsync<TView>(null, newWindowSizePreference, null, ViewSizePreference.Default);

        /// <summary>
        /// Creates a new window and navigates to the specified view.
        /// </summary>
        /// <typeparam name="TView">The type of the view to which the user should be navigated in the new window.</typeparam>
        /// <param name="parameters">The parameters, which are injected in to the view model of the view.</param>
        /// <returns>Returns the result of the navigation.</returns>
        public Task<WindowNavigationResult> NavigateAsync<TView>(object parameters) where TView : Page => this.NavigateAsync<TView>(parameters, ViewSizePreference.Default, null, ViewSizePreference.Default);

        /// <summary>
        /// Creates a new window and navigates to the specified view.
        /// </summary>
        /// <typeparam name="TView">The type of the view to which the user should be navigated in the new window.</typeparam>
        /// <returns>Returns the result of the navigation.</returns>
        public Task<WindowNavigationResult> NavigateAsync<TView>() where TView : Page => this.NavigateAsync<TView>(null, ViewSizePreference.Default, null, ViewSizePreference.Default);

        /// <summary>
        /// Creates a new window and navigates to the specified view.
        /// </summary>
        /// <typeparam name="TView">The type of the view to which the user should be navigated in the new window.</typeparam>
        /// <param name="parameters">The parameters, which are injected in to the view model of the view.</param>
        /// <param name="newWindowSizePreference">The size preference for the new window, which determines how the new window is arranged in respect to the anchor window.</param>
        /// <param name="anchorWindowNavigationService">The navigation service of the window, which is the anchor for the new window. This can be used to specify how the two windows are being arranged by Windows.</param>
        /// <param name="anchorWindowSizePreference">The size preference for the anchor window, which determines how the anchor window is arranged in respect to the new window.</param>
        /// <returns>Returns the result of the navigation.</returns>
        public async Task<WindowNavigationResult> NavigateAsync<TView>(object parameters, ViewSizePreference newWindowSizePreference, NavigationService anchorWindowNavigationService, ViewSizePreference anchorWindowSizePreference) where TView : Page
        {
            // Creates the new window
            WindowCreationResult result = await this.CreateWindowAsync(anchorWindowNavigationService?.ApplicationView.Id, anchorWindowSizePreference, newWindowSizePreference);

            // Checks if the window could be created, if not then the navigation can not be performed
            if (!result.WasSuccessful)
                return new WindowNavigationResult { Result = NavigationResult.Canceled };

            // Navigates to the specified view
            if (await result.NavigationService.NavigateAsync<TView>(parameters) == NavigationResult.Canceled)
            {
                // Since the view could not be navigated to, the new window is closed, and the navigation is aborted
                await result.NavigationService.CloseWindowAsync();
                return new WindowNavigationResult { Result = NavigationResult.Canceled };
            }

            // Adds the new navigation service to the list of navigation services and invokes the window created event
            this.navigationServices.Add(result.NavigationService);
            this.WindowCreated?.Invoke(this, new WindowEventArgs(result.NavigationService));

            // Since the navigation was successful, the navigation service is returned
            return new WindowNavigationResult
            {
                Result = NavigationResult.Navigated,
                NavigationService = result.NavigationService
            };
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