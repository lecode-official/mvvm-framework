
#region Using Directives

using Ninject;
using Ninject.Parameters;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

#endregion

namespace System.Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents a manager, that manages the navigation of a window.
    /// </summary>
    public class NavigationService
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="NavigationService"/> instance.
        /// </summary>
        /// <param name="kernel">The Ninjet kernel, which is used to instantiate views and view models.</param>
        internal NavigationService(IKernel kernel)
        {
            // Stores the kernel for later use
            this.kernel = kernel;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the Ninjet kernel, which is used to instantiate views and view models.
        /// </summary>
        private IKernel kernel;

        /// <summary>
        /// Contains the frame of the window in which the child views are to be displayed. If the window does not have a navigation frame, then the user cannot navigate in it.
        /// </summary>
        private Frame navigationFrame;

        /// <summary>
        /// Contains the stack of views for the backwards navigation.
        /// </summary>
        private Stack<KeyValuePair<Page, IViewModel>> navigationStack = new Stack<KeyValuePair<Page, IViewModel>>();
        
        /// <summary>
        /// Contains all cached types of the assembly of a view that has been created.
        /// </summary>
        private Type[] assemblyTypes = null;

        #endregion

        #region Internal Properties

        /// <summary>
        /// Contains the window for which the navigation is to be managed.
        /// </summary>
        private Window window;

        /// <summary>
        /// Gets or sets the window for which the navigation is to be managed.
        /// </summary>
        internal Window Window
        {
            get
            {
                return this.window;
            }

            set
            {
                // Sets the new window
                this.window = value;

                // Searches the view for a frame, which will be used for the navigation and stores it
                if (value != null)
                    this.navigationFrame = NavigationService.FindNavigationFrame(value);
                else
                    this.navigationFrame = null;

                // Disables the navigation UI of the frame, since the navigation manager is now responsible for navigating between views
                if (this.navigationFrame != null)
                    this.navigationFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            }
        }

        /// <summary>
        /// Gets or sets the view model of the window.
        /// </summary>
        internal IViewModel WindowViewModel { get; set; }

        /// <summary>
        /// Gets or sets the view that is currently being viewed in the window. If the window does not support navigation, then the current view is always null.
        /// </summary>
        internal Page CurrentView { get; set; }

        /// <summary>
        /// Gets or sets the view model of the current view.
        /// </summary>
        internal IViewModel CurrentViewModel { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value that determines whether the window supports navigation.
        /// </summary>
        public bool SupportsNavigation
        {
            get
            {
                return this.navigationFrame != null;
            }
        }

        /// <summary>
        /// Gets a value that determines whether the user is able to navigate back.
        /// </summary>
        public bool CanNavigateBack
        {
            get
            {
                return this.navigationStack.Any();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Traverses the logical tree of the window in order to find the frame that is used for the navigation.
        /// </summary>
        /// <param name="element">The element (i.e. the window), which is to be searched for the navigation frame.</param>
        /// <returns>Returns the first frame that is found and null if no frame could be found.</returns>
        private static Frame FindNavigationFrame(FrameworkElement element)
        {
            // Cycles through all children of the element and checks whether it contains a frame
            foreach (object child in LogicalTreeHelper.GetChildren(element))
            {
                // Checks if the child is a frame, if so it is returned, otherwise its children are traversed to find the frame
                Frame frame = child as Frame;
                FrameworkElement frameworkElement = child as FrameworkElement;
                if (frame != null)
                {
                    return frame;
                }
                else if (frameworkElement != null)
                {
                    frame = NavigationService.FindNavigationFrame(frameworkElement);
                    if (frame != null)
                        return frame;
                }
            }

            // If the window did not contain the frame, then null is returned
            return null;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Closes the window that is managed by this navigation manager.
        /// </summary>
        /// <param name="isCancellable">Determines whether the closing of the application can be cancelled by the view model.</param>
        /// <returns>Returns a value that determines whether the window was closed or its closing was cancelled. If <see cref="forceClose"/> is set to <c>true</c>, <c>NavigationResult.Navigated</c> is always returned.</returns>
        internal async Task<NavigationResult> CloseWindowAsync(bool isCancellable)
        {
            return await this.CloseWindowAsync(isCancellable, !isCancellable ? NavigationReason.WindowClosed : NavigationReason.WindowClosing);
        }

        /// <summary>
        /// Closes the window that is managed by this navigation manager.
        /// </summary>
        /// <param name="isCancellable">Determines whether the closing of the application can be cancelled by the view model.</param>
        /// <param name="navigationReason">The navigation reason that is sent to the view model.</param>
        /// <returns>Returns a value that determines whether the window was closed or its closing was cancelled. If <see cref="isCancellable"/> is set to <c>false</c>, <c>NavigationResult.Navigated</c> is always returned.</returns>
        internal async Task<NavigationResult> CloseWindowAsync(bool isCancellable, NavigationReason navigationReason)
        {
            return await this.CloseWindowAsync(isCancellable, navigationReason, true);
        }

        /// <summary>
        /// Closes the window that is managed by this navigation manager.
        /// </summary>
        /// <param name="isCancellable">Determines whether the closing of the application can be cancelled by the view model.</param>
        /// <param name="navigationReason">The navigation reason that is sent to the view model.</param>
        /// <param name="closeWindow">Indicates whether the window should be closed or the view models should just be disposed of.</param>
        /// <returns>Returns a value that determines whether the window was closed or its closing was cancelled. If <see cref="isCancellable"/> is set to <c>false</c>, <c>NavigationResult.Navigated</c> is always returned.</returns>
        internal async Task<NavigationResult> CloseWindowAsync(bool isCancellable, NavigationReason navigationReason, bool closeWindow)
        {
            // Creates new event arguments for the navigation events
            NavigationEventArgs eventArguments = null;

            // Checks if the window supports navigation and if the the current view has a view model, if so, then the on navigate from event is raised for the view model of the current view
            if (this.SupportsNavigation && this.CurrentViewModel != null)
            {
                eventArguments = new NavigationEventArgs(navigationReason);
                await this.CurrentViewModel.OnNavigateFromAsync(eventArguments);
                if (eventArguments.Cancel && isCancellable && navigationReason != NavigationReason.WindowClosed)
                    return NavigationResult.Canceled;
            }

            // Raises the on navigate from event on the view model of the window, if the window has one
            if (this.WindowViewModel != null)
            {
                eventArguments = new NavigationEventArgs(navigationReason);
                await this.WindowViewModel.OnNavigateFromAsync(eventArguments);
                if (eventArguments.Cancel && isCancellable && navigationReason != NavigationReason.WindowClosed)
                    return NavigationResult.Canceled;
            }

            // Deactivates the view model of the current view, of the window, and disposes of them
            if (this.SupportsNavigation && this.CurrentViewModel != null)
            {
                await this.CurrentViewModel.OnDeactivateAsync();
                this.CurrentViewModel.Dispose();
            }
            if (this.WindowViewModel != null)
            {
                await this.WindowViewModel.OnDeactivateAsync();
                this.WindowViewModel.Dispose();
            }

            // Clears the navigation stack
            await this.ClearNavigationStackAsync();

            // Closes the window
            if (closeWindow)
                this.Window.Close();

            // Since the window was closed successfully, a positive result is returned
            return NavigationResult.Navigated;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears the navigation stack and deactivates all view models.
        /// </summary>
        public async Task ClearNavigationStackAsync()
        {
            // Cylces over all views and view models that are on the navigation stack
            for (int i = 0; i < this.navigationStack.Count; i++)
            {
                // Gets the view and the view model
                KeyValuePair<Page, IViewModel> viewViewModelPair = this.navigationStack.Pop();

                // Deactivates and disposes of the view model
                if (viewViewModelPair.Value != null)
                {
                    await viewViewModelPair.Value.OnDeactivateAsync();
                    viewViewModelPair.Value.Dispose();
                }
            }
        }

        /// <summary>
        /// Navigates the user to the specified view.
        /// </summary>
        /// <typeparam name="TView">The type of the view to which the user is to be navigated.</typeparam>
        /// <exception cref="InvalidOperationException">If the view or the view model can not be instantiated, or the window does not support navigation, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns <see cref="NavigationResult.Navigated"/> if the user was successfully navigated and <see cref="NavigationResult.Canceled"/> otherwise.</returns>
        public async Task<NavigationResult> NavigateAsync<TView>() where TView : Page
        {
            return await this.NavigateAsync<TView>(null);
        }

        /// <summary>
        /// Navigates the user to the specified view.
        /// </summary>
        /// <param name="parameters">The parameters that are to be passed to the view model.</param>
        /// <typeparam name="TView">The type of the view to which the user is to be navigated.</typeparam>
        /// <exception cref="InvalidOperationException">If the view or the view model can not be instantiated, or the window does not support navigation, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns <see cref="NavigationResult.Navigated"/> if the user was successfully navigated and <see cref="NavigationResult.Canceled"/> otherwise.</returns>
        public async Task<NavigationResult> NavigateAsync<TView>(dynamic parameters) where TView : Page
        {
            // Checks if the current window supports navigation
            if (!this.SupportsNavigation)
                throw new InvalidOperationException(Resources.Localization.NavigationService.NavigationNotSupportedExceptionMessage);

            // Raises the on navigated from event of the current view model, if the view model does not allow to be navigated away from, then the navigation is aborted
            NavigationEventArgs eventArguments = null;
            if (this.CurrentViewModel != null)
            {
                eventArguments = new NavigationEventArgs(NavigationReason.Navigation);
                await this.CurrentViewModel.OnNavigateFromAsync(eventArguments);
                if (eventArguments.Cancel)
                    return NavigationResult.Canceled;
            }

            // Determines the type of the view model, which can be done via attribute or convention
            Type viewModelType = null;
            IViewModel viewModel = null;
            ViewModelAttribute viewModelAttribute = typeof(TView).GetCustomAttributes<ViewModelAttribute>().FirstOrDefault();
            if (viewModelAttribute != null)
            {
                viewModelType = viewModelAttribute.ViewModelType;
            }
            else
            {
                this.assemblyTypes = this.assemblyTypes ?? typeof(TView).Assembly.GetTypes();
                viewModelType = this.assemblyTypes.FirstOrDefault(type => type.Name == string.Concat(typeof(TView).Name, "Model"));
            }
            
            // Checks if the view has a view model attribute, if so then the type specified in the attribute is used to instantiate a new view model for the view
            if (viewModelType != null)
            {
                // Safely instantiates the corresponding view model for the view
                object temporaryViewModel = null;
                try
                {
                    try
                    {
                        // Creates the view model via dependency injection (the navigation service is injected as an optional constructor argument, this is helpful, because the view model does not have to retrieve it itself)
                        TypeMatchingConstructorArgument constructorArgument = new TypeMatchingConstructorArgument(typeof(NavigationService), (context, target) => this);
                        temporaryViewModel = this.kernel.Get(viewModelType, constructorArgument);
                    }
                    catch (ActivationException e)
                    {
                        throw new InvalidOperationException(Resources.Localization.NavigationService.ViewModelCouldNotBeInstantiatedExceptionMessage, e);
                    }

                    // Checks if the user provided any custom parameters
                    if (parameters != null)
                    {
                        // Cycles through all properties of the parameters
                        foreach (PropertyDescriptor parameter in TypeDescriptor.GetProperties(parameters))
                        {
                            // Gets the information about the parameter in the view model
                            PropertyInfo parameterPropertyInfo = temporaryViewModel.GetType().GetProperty(parameter.Name);

                            // Checks if the property was found, the types match and if the setter is implemented, if not then the value cannot be assigned and we turn to the next parameter
                            if (parameterPropertyInfo == null || !parameterPropertyInfo.CanWrite || parameter.GetValue(parameters) == null)
                                continue;

                            // Sets the value of the parameter property in the view model to the value provided in the parameters
                            parameterPropertyInfo.SetValue(temporaryViewModel, parameter.GetValue(parameters));
                        }
                    }

                    // Converts the view model to the right base class and swaps the temporary view model with the final one
                    viewModel = temporaryViewModel as IViewModel;
                    if (viewModel == null)
                        throw new InvalidOperationException(Resources.Localization.NavigationService.WrongViewModelTypeExceptionMessage);
                    temporaryViewModel = null;
                }
                finally
                {
                    // Checks if the temporary view model is not null, this only happens if an error occurred, therefore the view model is safely disposed of
                    if (temporaryViewModel != null && temporaryViewModel is IDisposable)
                        (temporaryViewModel as IDisposable).Dispose();
                }
            }

            // Calls the activate event and then the navigate event of the view model
            if (viewModel != null)
            {
                // Raises the activate event of the new view model
                await viewModel.OnActivateAsync();

                // Raises the on navigate to event of the new view model, and checks if it allows to be navigated to
                eventArguments = new NavigationEventArgs(NavigationReason.Navigation);
                await viewModel.OnNavigateToAsync(eventArguments);
                if (eventArguments.Cancel)
                {
                    // Since the view model does not allow to be navigated to, the new view model is deactivated, disposed of, and the navigation is aborted
                    await viewModel.OnDeactivateAsync();
                    viewModel.Dispose();
                    return NavigationResult.Canceled;
                }
            }

            // Instantiates the new view
            Page view = null;
            try
            {
                view = this.kernel.Get<TView>();
                if (!view.IsInitialized)
                {
                    MethodInfo initializeComponentMethod = view.GetType().GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
                    if (initializeComponentMethod != null)
                        initializeComponentMethod.Invoke(view, new object[0]);
                }
            }
            catch (ActivationException e)
            {
                throw new InvalidOperationException(Resources.Localization.NavigationService.ViewCouldNotBeInstantiatedExceptionMessage, e);
            }

            // Navigates the user to the new view
            if (this.CurrentViewModel != null)
                this.CurrentViewModel.IsInView = false;
            if (this.CurrentView != null)
                this.navigationStack.Push(new KeyValuePair<Page, IViewModel>(this.CurrentView, this.CurrentViewModel));
            this.CurrentView = view;
            this.CurrentViewModel = viewModel;
            this.CurrentView.DataContext = this.CurrentViewModel;
            this.navigationFrame.Navigate(this.CurrentView);
            if (this.CurrentViewModel != null)
                this.CurrentViewModel.IsInView = true;

            // Since the navigation was successful, Navigated is returned as a result of the navigation
            return NavigationResult.Navigated;
        }

        /// <summary>
        /// Navigates the user to the previous view. If there is no previous view, then nothing is done.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the window does not support navigation, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns <see cref="NavigationResult.Navigated"/> if the user was successfully navigated and <see cref="NavigationResult.Canceled"/> otherwise.</returns>
        public async Task<NavigationResult> NavigateBackAsync()
        {
            // Checks if the current window supports navigation
            if (!this.SupportsNavigation)
                throw new InvalidOperationException(Resources.Localization.NavigationService.NavigationNotSupportedExceptionMessage);

            // Checks if there are any elements on the navigation stack, if not then nothing is done
            if (!this.CanNavigateBack)
                return NavigationResult.Canceled;

            // Raises the on navigated from event of the current view model, if the view model does not allow to be navigated away from, then the navigation is aborted
            NavigationEventArgs eventArguments = null;
            if (this.CurrentViewModel != null)
            {
                eventArguments = new NavigationEventArgs(NavigationReason.Navigation);
                await this.CurrentViewModel.OnNavigateFromAsync(eventArguments);
                if (eventArguments.Cancel)
                    return NavigationResult.Canceled;
            }

            // Raises the on navigate to event of the view model of the view that is on top of the navigation stack, if the view model does not allow to be navigated away from, then the navigation is aborted
            IViewModel viewModel = this.navigationStack.Peek().Value;
            if (viewModel != null)
            {
                eventArguments = new NavigationEventArgs(NavigationReason.Navigation);
                await viewModel.OnNavigateToAsync(eventArguments);
                if (eventArguments.Cancel)
                    return NavigationResult.Canceled;
            }

            // Deactivates and disposes of the current view model
            if (this.CurrentViewModel != null)
            {
                await this.CurrentViewModel.OnDeactivateAsync();
                this.CurrentViewModel.Dispose();
            }

            // Navigates to the view that was on top of the navigation stack (the data context is set to null before navigation and is reset afterwards, because this causes all the bindings that do not call
            // the property changed event to properly update; this is, for example helpful if database models are used in views for performance reasons, because they do not implement INotifyPropertyChanged)
            Page view = this.navigationStack.Pop().Key;
            this.CurrentView = view;
            this.CurrentViewModel = viewModel;
            this.CurrentView.DataContext = null;
            this.navigationFrame.Navigate(this.CurrentView);
            if (this.CurrentViewModel != null)
                this.CurrentViewModel.IsInView = true;
            this.CurrentView.DataContext = this.CurrentViewModel;

            // Since the back navigation was successful, a positive result is returned
            return NavigationResult.Navigated;
        }

        #endregion
    }
}