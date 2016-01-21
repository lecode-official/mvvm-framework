
#region Using Directives

using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
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
        /// <param name="kernel">The Ninject kernel, which is used to instantiate the views and their corresponding view models.</param>
        public WindowNavigationService(IKernel kernel)
        {
            // Validates the arguments
            if (kernel == null)
                throw new ArgumentNullException("kernel");

            // Stores the Ninject kernel for later use
            this.kernel = kernel;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the Ninject kernel, which is used to instantiate the views and their corresponding view models.
        /// </summary>
        private IKernel kernel;

        /// <summary>
        /// Contains the navigation managers of all open windows.
        /// </summary>
        private ICollection<NavigationService> navigationServices = new List<NavigationService>();

        /// <summary>
        /// Contains all cached types of the assembly of a window that has been created.
        /// </summary>
        private Type[] assemblyTypes = null;

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new window and its view model.
        /// </summary>
        /// <typeparam name="TWindow">The type of window that is to be created.</typeparam>
        /// <param name="parameters">The parameters that are passed to the window's view model.</param>
        /// <exception cref="InvalidOperationException">If the the window or it's view model can not be instantiated, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns the navigation result, the new window, and the new view model of the window.</returns>
        private async Task<WindowCreationResult> CreateWindowAsync<TWindow>(dynamic parameters) where TWindow : Window
        {
            // Creates a new navigation service for the window
            NavigationService navigationService = new NavigationService(this.kernel);

            // Determines the type of the view model, which can be done via attribute or convention
            Type windowViewModelType = null;
            IViewModel windowViewModel = null;
            ViewModelAttribute windowViewModelAttribute = typeof(TWindow).GetCustomAttributes<ViewModelAttribute>().FirstOrDefault();
            if (windowViewModelAttribute != null)
                windowViewModelType = windowViewModelAttribute.ViewModelType;
            else if (this.assemblyTypes == null)
                this.assemblyTypes = typeof(TWindow).Assembly.GetTypes();
            windowViewModelType = this.assemblyTypes.FirstOrDefault(type => type.Name == string.Concat(typeof(TWindow).Name, "ViewModel"));
            
            // Checks if the window has a view model attribute, if so then the type specified in the attribute is used to instantiate a new view model for the window
            if (windowViewModelType != null)
            {
                // Safely instantiates the corresponding view model for the view
                object temporaryWindowViewModel = null;
                try
                {
                    try
                    {
                        // Creates the view model via dependency injection (the navigation service is injected as an optional constructor argument, this is helpful, because the view model does not have to retrieve it itself)
                        TypeMatchingConstructorArgument constructorArgument = new TypeMatchingConstructorArgument(typeof(NavigationService), (context, target) => navigationService);
                        temporaryWindowViewModel = this.kernel.Get(windowViewModelType, constructorArgument);
                    }
                    catch (ActivationException e)
                    {
                        throw new InvalidOperationException(Resources.Localization.WindowNavigationService.WindowViewModelCouldNotBeInstantiatedExceptionMessage, e);
                    }

                    // Checks if the user provided any custom parameters
                    if (parameters != null)
                    {
                        // Cycles through all properties of the parameters
                        foreach (PropertyDescriptor parameter in TypeDescriptor.GetProperties(parameters))
                        {
                            // Gets the information about the parameter in the window view model
                            PropertyInfo parameterPropertyInfo = temporaryWindowViewModel.GetType().GetProperty(parameter.Name);

                            // Checks if the property was found, the types match and if the setter is implemented, if not then the value cannot be assigned and we turn to the next parameter
                            if (parameterPropertyInfo == null || !parameterPropertyInfo.CanWrite || parameter.GetValue(parameters) == null)
                                continue;

                            // Sets the value of the parameter property in the window view model to the value provided in the parameters
                            parameterPropertyInfo.SetValue(temporaryWindowViewModel, parameter.GetValue(parameters));
                        }
                    }

                    // Converts the view model to the right base class and swaps the temporary view model with the final one
                    windowViewModel = temporaryWindowViewModel as IViewModel;
                    if (windowViewModel == null)
                        throw new InvalidOperationException(Resources.Localization.WindowNavigationService.WrongViewModelTypeExceptionMessage);
                    temporaryWindowViewModel = null;
                }
                finally
                {
                    // Checks if the temporary window view model is not null, this only happens if an error occurred, therefore the window view model is safely disposed of
                    if (temporaryWindowViewModel != null && temporaryWindowViewModel is IDisposable)
                        (temporaryWindowViewModel as IDisposable).Dispose();
                }
            }

            // Calls the activate event and then the navigate event of the window view model
            if (windowViewModel != null)
            {
                // Raises the activate event of the new window view model
                await windowViewModel.OnActivateAsync();

                // Raises the on navigate to event of the new window view model, and checks if it allows to be navigated to
                NavigationEventArgs eventArguments = new NavigationEventArgs(NavigationReason.WindowOpened);
                await windowViewModel.OnNavigateToAsync(eventArguments);
                if (eventArguments.Cancel)
                {
                    // Since the window view model does not allow to be navigated to, the new window view model is deactivated, disposed of, and the navigation is aborted
                    await windowViewModel.OnDeactivateAsync();
                    windowViewModel.Dispose();
                    return new WindowCreationResult { NavigationResult = NavigationResult.Canceled };
                }
            }

            // Safely instantiates the new window
            Exception exception = null;
            try
            {
                // Instantiates the new window and sets the view model
                navigationService.Window = this.kernel.Get<TWindow>();
                if (!navigationService.Window.IsInitialized)
                {
                    MethodInfo initializeComponentMethod = navigationService.Window.GetType().GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
                    if (initializeComponentMethod != null)
                        initializeComponentMethod.Invoke(navigationService.Window, new object[0]);
                }
                navigationService.Window.DataContext = windowViewModel;

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
                    if (this.WindowClosed != null)
                        this.WindowClosed(this, new WindowEventArgs(navigationService));
                };

                // Returns the result
                return new WindowCreationResult
                {
                    Window = navigationService.Window,
                    ViewModel = windowViewModel,
                    NavigationResult = NavigationResult.Navigated,
                    NavigationService = navigationService
                };
            }
            catch (ActivationException e)
            {
                exception = e;
            }

            // Since an error occurred, the new window view model is deactivated, disposed of, and the exception is rethrown
            await windowViewModel.OnDeactivateAsync();
            windowViewModel.Dispose();
            throw new InvalidOperationException(Resources.Localization.WindowNavigationService.WindowCouldNotBeInstantiatedExceptionMessage, exception);
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
        public async Task<WindowNavigationResult> NavigateAsync<TWindow, TView>(dynamic windowParameters, dynamic viewParameters, bool isMainWindow = false)
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
            if (this.WindowCreated != null)
                this.WindowCreated(this, new WindowEventArgs(result.NavigationService));

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
        /// Creates a new window, displays it, and navigates to a view within the window.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <typeparam name="TView">The type of the view to which is navigated within the window.</typeparam>
        /// <param name="viewParameters">The parameters that are passeed to the view model of the view.</param>
        /// <param name="isMainWindow">Determines whether the new window is set as the main window of the application. If the main window is closed, then the application is shut down.</param>
        /// <exception cref="InvalidOperationException">If the view, the window, or either of the view models can not be instantiated, or the window does not support navigation, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public async Task<WindowNavigationResult> NavigateAsync<TWindow, TView>(dynamic viewParameters, bool isMainWindow = false)
            where TWindow : Window
            where TView : Page
        {
            return await this.NavigateAsync<TWindow, TView>(null, viewParameters, isMainWindow);
        }

        /// <summary>
        /// Creates a new window and displays it.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <param name="parameters">The parameters that are passed to the view model of the window.</param>
        /// <param name="isMainWindow">Determines whether the new window is set as the main window of the application. If the main window is closed, then the application is shut down.</param>
        /// <exception cref="InvalidOperationException">If the the window or it's view model can not be instantiated, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public async Task<WindowNavigationResult> NavigateAsync<TWindow>(dynamic parameters, bool isMainWindow) where TWindow : Window
        {
            // Creates the new window
            WindowCreationResult result = await this.CreateWindowAsync<TWindow>(parameters);

            // Checks if the window could be created
            if (result.NavigationResult == NavigationResult.Canceled)
                return new WindowNavigationResult { Result = NavigationResult.Canceled };

            // Creates and adds the new navigation manager to the list of navigation managers
            this.navigationServices.Add(result.NavigationService);
            if (this.WindowCreated != null)
                this.WindowCreated(this, new WindowEventArgs(result.NavigationService));

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
        public async Task<WindowNavigationResult> NavigateAsync<TWindow>(bool isMainWindow) where TWindow : Window
        {
            return await this.NavigateAsync<TWindow>(null, isMainWindow);
        }

        /// <summary>
        /// Creates a new window and displays it.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window that is to be created.</typeparam>
        /// <exception cref="InvalidOperationException">If the the window or it's view model can not be instantiated, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns whether the navigation was successful or cancelled.</returns>
        public async Task<WindowNavigationResult> NavigateAsync<TWindow>() where TWindow : Window
        {
            return await this.NavigateAsync<TWindow>(null, false);
        }

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
                throw new ArgumentNullException("viewModel");

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
                throw new ArgumentNullException("navigationService");

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
                throw new ArgumentNullException("viewModel");

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
                throw new ArgumentNullException("navigationService");

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
        public async Task<NavigationResult> CloseWindowAsync(IViewModel viewModel)
        {
            return await this.CloseWindowAsync(viewModel, false);
        }

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
                throw new ArgumentNullException("viewModel");

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
        public async Task<NavigationResult> CloseWindowAsync(NavigationService navigationService)
        {
            return await this.CloseWindowAsync(navigationService, false);
        }

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
                throw new ArgumentNullException("navigationService");

            // Closes the window
            if (await navigationService.CloseWindowAsync(forceClose) == NavigationResult.Canceled)
                return NavigationResult.Canceled;

            // Removes the navigation manager from the list of window navigation managers
            this.navigationServices.Remove(navigationService);
            if (this.WindowClosed != null)
                this.WindowClosed(this, new WindowEventArgs(navigationService));

            // Since the window was closed, navigated is returned
            return NavigationResult.Navigated;
        }

        /// <summary>
        /// Retrieves the navigation service for the window that has the specified view model or contains a view with the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model for which the navigation service is to be retrieved.</param>
        /// <returns>Returns the navigation service for the view model if it exists.</returns>
        public NavigationService GetNavigationService(IViewModel viewModel)
        {
            return this.navigationServices.SingleOrDefault(navigationManager => navigationManager.WindowViewModel == viewModel || navigationManager.CurrentViewModel == viewModel);
        }

        /// <summary>
        /// Retrieves the navigation services for the specified type of window.
        /// </summary>
        /// <typeparam name="TWindow">The type of the window for which existing navigation services are to be retrieved.</typeparam>
        /// <returns>Returns the navigation services for the type of window.</returns>
        public IEnumerable<NavigationService> GetNavigationServices<TWindow>() where TWindow : Window
        {
            return this.navigationServices.Where(navigationService => navigationService.Window != null && navigationService.Window is TWindow).ToList();
        }

        /// <summary>
        /// Shuts down the window navigation service by closing all views and windows and destorying their view models.
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