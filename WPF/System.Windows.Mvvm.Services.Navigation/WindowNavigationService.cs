
#region Using Directives

using System.Collections.Generic;
using System.InversionOfControl.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

#endregion

namespace System.Windows.Mvvm.Services.Navigation
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
        private IReadOnlyIocContainer iocContainer;

        /// <summary>
        /// Contains the navigation services of all open windows.
        /// </summary>
        private ICollection<NavigationService> navigationServices = new List<NavigationService>();

        /// <summary>
        /// Contains all cached types of the assembly. The types are used when activating a view model convention-based.
        /// </summary>
        private Type[] assemblyTypes = null;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the naming convention for view models. The function gets the name of the window type and returns the name of the corresponding view model. This function is used for convention-based view model activation. The default implementation adds "ViewModel" to the name of the window.
        /// </summary>
        public Func<string, string> ViewModelNamingConvention { get; set; } = windowName => string.Concat(windowName, "ViewModel");

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new window and its view model.
        /// </summary>
        /// <typeparam name="TWindow">The type of window that is to be created.</typeparam>
        /// <param name="parameters">The parameters that are passed to the window's view model.</param>
        /// <exception cref="InvalidOperationException">If the the window or it's view model can not be instantiated, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns the navigation result, the new window, and the new view model of the window.</returns>
        private async Task<WindowCreationResult> CreateWindowAsync<TWindow>(object parameters) where TWindow : Window
        {
            // Creates a new navigation service for the window
            NavigationService navigationService = new NavigationService(this.iocContainer);

            // Determines the type of the view model, which can be done via attribute or convention
            Type windowViewModelType = null;
            ViewModelAttribute viewModelAttribute = typeof(TWindow).GetCustomAttributes<ViewModelAttribute>().FirstOrDefault();
            if (viewModelAttribute != null)
            {
                windowViewModelType = viewModelAttribute.ViewModelType;
            }
            else
            {
                this.assemblyTypes = this.assemblyTypes ?? typeof(TWindow).Assembly.GetTypes();
                string viewModelName = this.ViewModelNamingConvention(typeof(TWindow).Name);
                windowViewModelType = this.assemblyTypes.FirstOrDefault(type => type.Name == viewModelName);
            }

            // Instantiates the new view model
            if (windowViewModelType != null)
            {
                try
                {
                    // Lets the IOC container instantiate the view model, so that all dependencies can be injected (including the navigation service itself, which is set as an explitic constructor argument)
                    navigationService.WindowViewModel = this.iocContainer.GetInstance(windowViewModelType, navigationService).Inject(parameters) as IViewModel;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(Resources.Localization.WindowNavigationService.WindowViewModelCouldNotBeInstantiatedExceptionMessage, e);
                }

                // Checks whether the view model implements the IViewModel interface
                if (navigationService.WindowViewModel == null)
                    throw new InvalidOperationException(Resources.Localization.WindowNavigationService.WrongViewModelTypeExceptionMessage);
            }

            // Calls the activate event and then the navigate event of the window view model
            if (navigationService.WindowViewModel != null)
            {
                // Raises the activate event of the new window view model
                await navigationService.WindowViewModel.OnActivateAsync();

                // Raises the on navigate to event of the new window view model, and checks if it allows to be navigated to
                NavigationEventArgs eventArguments = new NavigationEventArgs(NavigationReason.WindowOpened);
                await navigationService.WindowViewModel.OnNavigateToAsync(eventArguments);
                if (eventArguments.Cancel)
                {
                    // Since the window view model does not allow to be navigated to, the new window view model is deactivated, disposed of, and the navigation is aborted
                    await navigationService.WindowViewModel.OnDeactivateAsync();
                    navigationService.WindowViewModel.Dispose();
                    return new WindowCreationResult { NavigationResult = NavigationResult.Canceled };
                }
            }

            // Instantiates the new window
            try
            {
                // Lets the IOC container instantiate the window, so that all dependencies can be injected
                navigationService.Window = this.iocContainer.GetInstance<TWindow>();
            }
            catch (Exception e)
            {
                // Since an error occurred, the new window view model is deactivated and disposed of
                if (navigationService.WindowViewModel != null)
                {
                    await navigationService.WindowViewModel.OnDeactivateAsync();
                    navigationService.WindowViewModel.Dispose();
                }

                // Rethrows the exception
                throw new InvalidOperationException(Resources.Localization.WindowNavigationService.WindowCouldNotBeInstantiatedExceptionMessage, e);
            }

            // Since window is a framework element it must be properly initialized
            if (!navigationService.Window.IsInitialized)
            {
                MethodInfo initializeComponentMethod = navigationService.Window.GetType().GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
                if (initializeComponentMethod != null)
                    initializeComponentMethod.Invoke(navigationService.Window, new object[0]);
            }

            // Sets the view model as data context of the window
            navigationService.Window.DataContext = navigationService.WindowViewModel;

            // Subscribes to the closing event of the window, so that the window can be properly closed (with all the lifecycle callbacks)
            navigationService.Window.Closing += (sender, e) =>
            {
                // Calls the close window method in order to execute the lifecycle methods (the window is not closed within this call)
                if (navigationService.CloseWindowAsync(true, NavigationReason.WindowClosing, false).Result == NavigationResult.Canceled)
                {
                    e.Cancel = true;
                    return;
                }

                // Removes the navigation manager from the list of window navigation managers
                this.navigationServices.Remove(navigationService);
                this.WindowClosed?.Invoke(this, new WindowEventArgs(navigationService));
            };

            // Returns the result
            return new WindowCreationResult
            {
                Window = navigationService.Window,
                ViewModel = navigationService.WindowViewModel,
                NavigationResult = NavigationResult.Navigated,
                NavigationService = navigationService
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new window, displays it, and navigates to a view within the window.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <typeparam name="TView">The type of the view to which is navigated within the window.</typeparam>
        /// <param name="windowParameters">The parameters that are passed to the view model of the window.</param>
        /// <param name="viewParameters">The parameters that are passeed to the view model of the view.</param>
        /// <param name="isMainWindow">Determines whether the new window is set as the main window of the application. If the main window is closed, the application is shut down.</param>
        /// <exception cref="InvalidOperationException">If the view, the window, or either of the view models can not be instantiated, or the window does not support navigation, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public async Task<WindowNavigationResult> NavigateAsync<TWindow, TView>(object windowParameters, object viewParameters, bool isMainWindow = false)
            where TWindow : Window
            where TView : Page
        {
            // Creates the new window
            WindowCreationResult result = await this.CreateWindowAsync<TWindow>(windowParameters);

            // Checks if the window could be created
            if (result.NavigationResult == NavigationResult.Canceled)
                return new WindowNavigationResult { Result = NavigationResult.Canceled };

            // Checks if the window supports navigation
            if (!result.NavigationService.SupportsNavigation)
            {
                // Since the window does not support navigation, the window view model is deactivated and disposed of
                await result.ViewModel.OnDeactivateAsync();
                result.ViewModel.Dispose();
                throw new InvalidOperationException(Resources.Localization.WindowNavigationService.NavigationNotSupportedExceptionMessage);
            }

            // Navigates to the specified view
            if (await result.NavigationService.NavigateAsync<TView>(viewParameters) == NavigationResult.Canceled)
            {
                // Since the view could not be navigated to, the new window view model is deactivated, disposed of, and the navigation is aborted
                await result.ViewModel.OnDeactivateAsync();
                result.ViewModel.Dispose();
                return new WindowNavigationResult { Result = NavigationResult.Canceled };
            }

            // Adds the new navigation service to the list of navigation services
            this.navigationServices.Add(result.NavigationService);
            this.WindowCreated?.Invoke(this, new WindowEventArgs(result.NavigationService));

            // Sets the window as the new main window, if the user requested us to do so
            if (Application.Current != null)
            {
                if (isMainWindow)
                    Application.Current.MainWindow = result.Window;
                else if (Application.Current.MainWindow != null && result.Window != Application.Current.MainWindow)
                    result.Window.Owner = Application.Current.MainWindow;
            }

            // Opens the new window
            result.Window.Show();

            // Sets the ownership of all opened windows
            if (isMainWindow)
            {
                foreach (Window childWindow in this.navigationServices.Select(navigationService => navigationService.Window).Where(childWindow => childWindow != result.Window).ToList())
                    childWindow.Owner = result.Window;
            }

            // Since the navigation was successful, Navigated is returned as a result of the navigation
            return new WindowNavigationResult
            {
                Result = NavigationResult.Navigated,
                NavigationService = result.NavigationService
            };
        }

        /// <summary>
        /// Creates a new window, displays it, and navigates to a view within the window.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <typeparam name="TView">The type of the view to which is navigated within the window.</typeparam>
        /// <param name="viewParameters">The parameters that are passeed to the view model of the view.</param>
        /// <param name="isMainWindow">Determines whether the new window is set as the main window of the application. If the main window is closed, then the application is shut down.</param>
        /// <exception cref="InvalidOperationException">If the view, the window, or either of the view models can not be instantiated, or the window does not support navigation, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public Task<WindowNavigationResult> NavigateAsync<TWindow, TView>(object viewParameters, bool isMainWindow = false)
            where TWindow : Window
            where TView : Page
        {
            return this.NavigateAsync<TWindow, TView>(null, viewParameters, isMainWindow);
        }

        /// <summary>
        /// Creates a new window and displays it.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <param name="parameters">The parameters that are passed to the view model of the window.</param>
        /// <param name="isMainWindow">Determines whether the new window is set as the main window of the application. If the main window is closed, then the application is shut down.</param>
        /// <exception cref="InvalidOperationException">If the the window or it's view model can not be instantiated, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public async Task<WindowNavigationResult> NavigateAsync<TWindow>(object parameters, bool isMainWindow) where TWindow : Window
        {
            // Creates the new window
            WindowCreationResult result = await this.CreateWindowAsync<TWindow>(parameters);

            // Checks if the window could be created
            if (result.NavigationResult == NavigationResult.Canceled)
                return new WindowNavigationResult { Result = NavigationResult.Canceled };

            // Creates and adds the new navigation manager to the list of navigation managers
            this.navigationServices.Add(result.NavigationService);
            this.WindowCreated?.Invoke(this, new WindowEventArgs(result.NavigationService));

            // Sets the window as the new main window, if the user requested us to do so
            if (Application.Current != null)
            {
                if (isMainWindow)
                    Application.Current.MainWindow = result.Window;
                else if (Application.Current.MainWindow != null)
                    result.Window.Owner = Application.Current.MainWindow;
            }

            // Opens the new window
            result.Window.Show();

            // Sets the ownership of all opened windows
            if (isMainWindow)
            {
                foreach (Window childWindow in this.navigationServices.Select(navigationService => navigationService.Window).Where(childWindow => childWindow != result.Window).ToList())
                    childWindow.Owner = result.Window;
            }

            // Since the navigation was successful, Navigated is returned as a result of the navigation
            return new WindowNavigationResult
            {
                Result = NavigationResult.Navigated,
                NavigationService = result.NavigationService
            };
        }

        /// <summary>
        /// Creates a new window and displays it.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <param name="isMainWindow">Determines whether the new window is set as the main window of the application. If the main window is closed, then the application is shut down.</param>
        /// <exception cref="InvalidOperationException">If the the window or it's view model can not be instantiated, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public Task<WindowNavigationResult> NavigateAsync<TWindow>(bool isMainWindow) where TWindow : Window => this.NavigateAsync<TWindow>(null, isMainWindow);

        /// <summary>
        /// Creates a new window and displays it.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <exception cref="InvalidOperationException">If the the window or it's view model can not be instantiated, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public Task<WindowNavigationResult> NavigateAsync<TWindow>() where TWindow : Window => this.NavigateAsync<TWindow>(null, false);

        /// <summary>
        /// Creates a new window, displays it, and navigates to a view within the window. It shows the window as a dialog and waits till the window has closed.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <typeparam name="TView">The type of the view to which is navigated within the window.</typeparam>
        /// <param name="windowParameters">The parameters that are passed to the view model of the window.</param>
        /// <param name="viewParameters">The parameters that are passeed to the view model of the view.</param>
        /// <exception cref="InvalidOperationException">If the view, the window, or either of the view models can not be instantiated, or the window does not support navigation, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public async Task<WindowNavigationResult> NavigateDialogAsync<TWindow, TView>(object windowParameters, object viewParameters)
          where TWindow : Window
          where TView : Page
        {
            // Creates the new window
            WindowCreationResult result = await this.CreateWindowAsync<TWindow>(windowParameters);

            // Checks if the window could be created
            if (result.NavigationResult == NavigationResult.Canceled)
                return new WindowNavigationResult { Result = NavigationResult.Canceled };

            // Checks if the window supports navigation
            if (!result.NavigationService.SupportsNavigation)
            {
                // Since the window does not support navigation, the window view model is deactivated and disposed of
                await result.ViewModel.OnDeactivateAsync();
                result.ViewModel.Dispose();
                throw new InvalidOperationException(Resources.Localization.WindowNavigationService.NavigationNotSupportedExceptionMessage);
            }

            // Navigates to the specified view
            if (await result.NavigationService.NavigateAsync<TView>(viewParameters) == NavigationResult.Canceled)
            {
                // Since the view could not be navigated to, the new window view model is deactivated, disposed of, and the navigation is aborted
                await result.ViewModel.OnDeactivateAsync();
                result.ViewModel.Dispose();
                return new WindowNavigationResult { Result = NavigationResult.Canceled };
            }

            // Adds the new navigation service to the list of navigation services
            this.navigationServices.Add(result.NavigationService);
            this.WindowCreated?.Invoke(this, new WindowEventArgs(result.NavigationService));

            // Opens the new window
            result.Window.ShowDialog();

            // Since the navigation was successful, Navigated is returned as a result of the navigation
            return new WindowNavigationResult
            {
                Result = NavigationResult.Navigated,
                NavigationService = result.NavigationService
            };
        }

        /// <summary>
        /// Creates a new window, displays it, and navigates to a view within the window. It shows the window as a dialog and waits till the window has closed.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <typeparam name="TView">The type of the view to which is navigated within the window.</typeparam>
        /// <param name="viewParameters">The parameters that are passeed to the view model of the view.</param>
        /// <param name="isMainWindow">Determines whether the new window is set as the main window of the application. If the main window is closed, then the application is shut down.</param>
        /// <exception cref="InvalidOperationException">If the view, the window, or either of the view models can not be instantiated, or the window does not support navigation, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public Task<WindowNavigationResult> NavigateDialogAsync<TWindow, TView>(object viewParameters)
            where TWindow : Window
            where TView : Page
        {
            return this.NavigateDialogAsync<TWindow, TView>(null, viewParameters);
        }

        /// <summary>
        /// Creates a new window and displays it. It shows the window as a dialog and waits till the window has closed.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <param name="parameters">The parameters that are passed to the view model of the window.</param>
        /// <param name="isMainWindow">Determines whether the new window is set as the main window of the application. If the main window is closed, then the application is shut down.</param>
        /// <exception cref="InvalidOperationException">If the the window or it's view model can not be instantiated, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public async Task<WindowNavigationResult> NavigateDialogAsync<TWindow>(object parameters) where TWindow : Window
        {
            // Creates the new window
            WindowCreationResult result = await this.CreateWindowAsync<TWindow>(parameters);

            // Checks if the window could be created
            if (result.NavigationResult == NavigationResult.Canceled)
                return new WindowNavigationResult { Result = NavigationResult.Canceled };

            // Creates and adds the new navigation manager to the list of navigation managers
            this.navigationServices.Add(result.NavigationService);
            this.WindowCreated?.Invoke(this, new WindowEventArgs(result.NavigationService));

            // Opens the new window
            result.Window.ShowDialog();
            
            // Since the navigation was successful, Navigated is returned as a result of the navigation
            return new WindowNavigationResult
            {
                Result = NavigationResult.Navigated,
                NavigationService = result.NavigationService
            };
        }

        /// <summary>
        /// Creates a new window and displays it. It shows the window as a dialog and waits till the window has closed.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <param name="isMainWindow">Determines whether the new window is set as the main window of the application. If the main window is closed, then the application is shut down.</param>
        /// <exception cref="InvalidOperationException">If the the window or it's view model can not be instantiated, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public Task<WindowNavigationResult> NavigateDialogAsync<TWindow>() where TWindow : Window => this.NavigateDialogAsync<TWindow>(null);

        /// <summary>
        /// Gets the state of the window of the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model of the window or the view model of the view that is hosted in the window.</param>
        /// <exception cref="ArgumentNullException">If the specified view model is <c>null</c>, an <see cref="ArgumentNullException"/> exception is thrown.</exception>
        /// <exception cref="InvalidOperationException">If the view model does not belong to a window, an <see cref="InvalidOperationException"/> exception is thrown.</exception>
        public WindowState GetWindowState(IViewModel viewModel)
        {
            // Validates the paramters
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            // Gets the navigation service for the specified view model, if no navigation service could be found, an invalid operation exception is thrown
            NavigationService navigationService = this.GetNavigationService(viewModel);
            if (navigationService == null)
                throw new InvalidOperationException(Resources.Localization.WindowNavigationService.NavigationServiceNotFoundExceptionMessage);

            // Returns the window state of the window
            return navigationService.Window.WindowState;
        }

        /// <summary>
        /// Gets the state of the window of the specified view model.
        /// </summary>
        /// <param name="navigationService">The navigation service of the window.</param>
        /// <exception cref="ArgumentNullException">If the specified navigation service is <c>null</c>, an <see cref="ArgumentNullException"/> exception is thrown.</exception>
        public WindowState GetWindowState(NavigationService navigationService)
        {
            // Validates the paramters
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            // Returns the window state of the window
            return navigationService.Window.WindowState;
        }

        /// <summary>
        /// Changes the state of the window of the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model of the window or the view model of the view that is hosted in the window that is to be maximized.</param>
        /// <param name="newState">The new state of the window.</param>
        /// <exception cref="ArgumentNullException">If the specified view model is <c>null</c>, an <see cref="ArgumentNullException"/> exception is thrown</exception>
        /// <exception cref="InvalidOperationException">If the view model does not belong to a window, an <see cref="InvalidOperationException"/> exception is thrown.</exception>
        public void SetWindowState(IViewModel viewModel, WindowState newState)
        {
            // Validates the paramters
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            // Gets the navigation service for the specified view model, if no navigation service could be found, an invalid operation exception is thrown
            NavigationService navigationService = this.GetNavigationService(viewModel);
            if (navigationService == null)
                throw new InvalidOperationException(Resources.Localization.WindowNavigationService.NavigationServiceNotFoundExceptionMessage);

            // Sets the new window state of the window
            navigationService.Window.WindowState = newState;
        }

        /// <summary>
        /// Changes the state of the window of the specified navigation service.
        /// </summary>
        /// <param name="navigationService">The navigation service of the window that is to be maximized.</param>
        /// <param name="newState">The new state of the window.</param>
        /// <exception cref="ArgumentNullException">If the specified navigation service is <c>null</c>, an <see cref="ArgumentNullException"/> exception is thrown.</exception>
        public void SetWindowState(NavigationService navigationService, WindowState newState)
        {
            // Validates the paramters
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            // Sets the new window state of the window
            navigationService.Window.WindowState = newState;
        }

        /// <summary>
        /// Closes the window of the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model of the window or the view model of the view that is hosted in the window that is to be closed.</param>
        /// <exception cref="ArgumentNullException">If the specified view model is <c>null</c>, an <see cref="ArgumentNullException"/> exception is thrown</exception>
        /// <exception cref="InvalidOperationException">If the view model does not belong to a window, an <see cref="InvalidOperationException"/> exception is thrown.</exception>
        /// <returns>Returns <c>NavigationResult.Navigated</c> if the window could be closed and <c>NavigationResult.Cancelled</c> if the navigation was cancelled.</returns>
        public Task<NavigationResult> CloseWindowAsync(IViewModel viewModel) => this.CloseWindowAsync(viewModel, false);

        /// <summary>
        /// Closes the window of the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model of the window or the view model of the view that is hosted in the window that is to be closed.</param>
        /// <param name="forceClose">Determines whether the view models may abort the closing of the window.</param>
        /// <exception cref="ArgumentNullException">If the specified view model is <c>null</c>, an <see cref="ArgumentNullException"/> exception is thrown</exception>
        /// <exception cref="InvalidOperationException">If the view model does not belong to a window, an <see cref="InvalidOperationException"/> exception is thrown.</exception>
        /// <returns>Returns <c>NavigationResult.Navigated</c> if the window could be closed and <c>NavigationResult.Cancelled</c> if the navigation was cancelled.</returns>
        public async Task<NavigationResult> CloseWindowAsync(IViewModel viewModel, bool forceClose)
        {
            // Validates the paramters
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            // Gets the navigation service for the specified view model, if no navigation service could be found, an invalid operation exception is thrown
            NavigationService navigationService = this.GetNavigationService(viewModel);
            if (navigationService == null)
                throw new InvalidOperationException(Resources.Localization.WindowNavigationService.NavigationServiceNotFoundExceptionMessage);

            // Closes the window
            return await this.CloseWindowAsync(navigationService, forceClose);
        }

        /// <summary>
        /// Closes the window of the specified navigation manager.
        /// </summary>
        /// <param name="navigationService">The navigation manager whose window is to be closed.</param>
        /// <exception cref="ArgumentNullException">If the specified navigation manager is <c>null</c>, then an <see cref="ArgumentNullException"/> exception is thrown.</exception>
        /// <returns>Returns <c>NavigationResult.Navigated</c> if the window could be closed and <c>NavigationResult.Cancelled</c> if the navigation was cancelled.</returns>
        public Task<NavigationResult> CloseWindowAsync(NavigationService navigationService) => this.CloseWindowAsync(navigationService, false);

        /// <summary>
        /// Closes the window of the specified navigation manager.
        /// </summary>
        /// <param name="navigationService">The navigation manager whose window is to be closed.</param>
        /// <param name="forceClose">Determines whether the view models may abort the closing of the window.</param>
        /// <exception cref="ArgumentNullException">If the specified navigation manager is <c>null</c>, then an <see cref="ArgumentNullException"/> exception is thrown.</exception>
        /// <returns>Returns <c>NavigationResult.Navigated</c> if the window could be closed and <c>NavigationResult.Cancelled</c> if the navigation was cancelled.</returns>
        public async Task<NavigationResult> CloseWindowAsync(NavigationService navigationService, bool forceClose)
        {
            // Validates the parameters
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            // Closes the window
            if (await navigationService.CloseWindowAsync(!forceClose) == NavigationResult.Canceled)
                return NavigationResult.Canceled;

            // Removes the navigation manager from the list of window navigation managers
            this.navigationServices.Remove(navigationService);
            this.WindowClosed?.Invoke(this, new WindowEventArgs(navigationService));

            // Since the window was closed, navigated is returned
            return NavigationResult.Navigated;
        }

        /// <summary>
        /// Retrieves the navigation service for the window that has the specified view model or contains a view with the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model for which the navigation service is to be retrieved.</param>
        /// <returns>Returns the navigation service for the view model if it exists.</returns>
        public NavigationService GetNavigationService(IViewModel viewModel) => this.navigationServices.SingleOrDefault(navigationManager => navigationManager.WindowViewModel == viewModel || navigationManager.CurrentViewModel == viewModel);

        /// <summary>
        /// Retrieves the navigation services for the specified type of window.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window for which existing navigation services are to be retrieved.</typeparam>
        /// <returns>Returns the navigation services for the type of window.</returns>
        public IEnumerable<NavigationService> GetNavigationServices<TWindow>() where TWindow : Window => this.navigationServices.Where(navigationService => navigationService.Window != null && navigationService.Window is TWindow).ToList();

        /// <summary>
        /// Shuts down the window navigation service by closing all views and windows and destroying their view models.
        /// </summary>
        public async Task DestroyAsync()
        {
            // Destroys all windows by closing them and destroying their view models
            await Task.WhenAll(this.navigationServices.Select(navigationService => navigationService.CloseWindowAsync(false, NavigationReason.ApplicationExit)));
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

        #region Nested Classes

        /// <summary>
        /// Represents the result of the creation of a window.
        /// </summary>
        private class WindowCreationResult
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the window that has been created.
            /// </summary>
            public Window Window { get; set; }

            /// <summary>
            /// Gets or sets the view model of the window that has been created.
            /// </summary>
            public IViewModel ViewModel { get; set; }

            /// <summary>
            /// Gets or sets the navigation service, which was created for the window.
            /// </summary>
            public NavigationService NavigationService { get; set; }

            /// <summary>
            /// Gets or sets the result of the "navigation", which happened when the window was created.
            /// </summary>
            public NavigationResult NavigationResult { get; set; }
            
            #endregion
        }

        #endregion
    }
}