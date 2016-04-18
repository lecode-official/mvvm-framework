
#region Using Directives

using System.Collections.Generic;
using System.InversionOfControl.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

#endregion

namespace System.Windows.Mvvm.Services.Navigation
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
        internal NavigationService(IReadOnlyIocContainer iocContainer)
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
        /// Contains the IOC container which is used to instantiate the views and their corresponding view models.
        /// </summary>
        private readonly IReadOnlyIocContainer iocContainer;

        /// <summary>
        /// Contains all cached types of the assembly. The types are used when activating a view model convention-based.
        /// </summary>
        private Type[] assemblyTypes = null;

        /// <summary>
        /// Contains the frame of the window in which the child views are to be displayed. If the window does not have a navigation frame, then the user cannot navigate in it.
        /// </summary>
        private Frame navigationFrame;

        /// <summary>
        /// Contains the stack of views for the backwards navigation.
        /// </summary>
        private readonly Stack<KeyValuePair<Page, IViewModel>> navigationStack = new Stack<KeyValuePair<Page, IViewModel>>();

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

                // Disables the navigation UI of the frame, since the navigation service is now responsible for navigating between views
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

        /// <summary>
        /// Gets or sets the naming convention for view models. The function gets the name of the view type and returns the name of the corresponding view model. This function is used for convention-based view model activation. The default implementation adds "Model" to the name of the view.
        /// </summary>
        public Func<string, string> ViewModelNamingConvention { get; set; } = viewName => string.Concat(viewName, "Model");

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
        /// Closes the window that is managed by this navigation service.
        /// </summary>
        /// <param name="isCancellable">Determines whether the closing of the application can be cancelled by the view model.</param>
        /// <returns>Returns a value that determines whether the window was closed or its closing was cancelled. If <see cref="forceClose"/> is set to <c>true</c>, <c>NavigationResult.Navigated</c> is always returned.</returns>
        internal Task<NavigationResult> CloseWindowAsync(bool isCancellable) => this.CloseWindowAsync(isCancellable, !isCancellable ? NavigationReason.WindowClosed : NavigationReason.WindowClosing);

        /// <summary>
        /// Closes the window that is managed by this navigation service.
        /// </summary>
        /// <param name="isCancellable">Determines whether the closing of the application can be cancelled by the view model.</param>
        /// <param name="navigationReason">The navigation reason that is sent to the view model.</param>
        /// <returns>Returns a value that determines whether the window was closed or its closing was cancelled. If <see cref="isCancellable"/> is set to <c>false</c>, <c>NavigationResult.Navigated</c> is always returned.</returns>
        internal Task<NavigationResult> CloseWindowAsync(bool isCancellable, NavigationReason navigationReason) => this.CloseWindowAsync(isCancellable, navigationReason, true);

        /// <summary>
        /// Closes the window that is managed by this navigation service.
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
            while (this.navigationStack.Any())
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
        public Task<NavigationResult> NavigateAsync<TView>() where TView : Page => this.NavigateAsync<TView>(null);

        /// <summary>
        /// Navigates the user to the specified view.
        /// </summary>
        /// <param name="parameters">The parameters that are to be passed to the view model.</param>
        /// <typeparam name="TView">The type of the view to which the user is to be navigated.</typeparam>
        /// <exception cref="InvalidOperationException">If the view or the view model can not be instantiated, or the window does not support navigation, an <see cref="InvalidOperationException"/> is thrown.</exception>
        /// <returns>Returns <see cref="NavigationResult.Navigated"/> if the user was successfully navigated and <see cref="NavigationResult.Canceled"/> otherwise.</returns>
        public async Task<NavigationResult> NavigateAsync<TView>(object parameters) where TView : Page
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
            ViewModelAttribute viewModelAttribute = typeof(TView).GetCustomAttributes<ViewModelAttribute>().FirstOrDefault();
            if (viewModelAttribute != null)
            {
                viewModelType = viewModelAttribute.ViewModelType;
            }
            else
            {
                this.assemblyTypes = this.assemblyTypes ?? typeof(TView).Assembly.GetTypes();
                string viewModelName = this.ViewModelNamingConvention(typeof(TView).Name);
                viewModelType = this.assemblyTypes.FirstOrDefault(type => type.Name == viewModelName);
            }

            // Instantiates the new view model
            IViewModel viewModel = null;
            if (viewModelType != null)
            {
                try
                {
                    // Lets the IOC container instantiate the view model, so that all dependencies can be injected (including the navigation service itself, which is set as an explitic constructor argument)
                    viewModel = this.iocContainer.GetInstance(viewModelType, this).Inject(parameters) as IViewModel;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(Resources.Localization.NavigationService.ViewModelCouldNotBeInstantiatedExceptionMessage, e);
                }

                // Checks whether the view model implements the IViewModel interface
                if (viewModel == null)
                    throw new InvalidOperationException(Resources.Localization.NavigationService.WrongViewModelTypeExceptionMessage);
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
                // Lets the IOC container instantiate the view, so that all dependencies can be injected
                view = this.iocContainer.GetInstance<TView>();
            }
            catch (Exception e)
            {
                // Since an error occurred, the new window view model is deactivated and disposed of
                if (viewModel != null)
                {
                    await viewModel.OnDeactivateAsync();
                    viewModel.Dispose();
                }

                // Rethrows the exception
                throw new InvalidOperationException(Resources.Localization.NavigationService.ViewCouldNotBeInstantiatedExceptionMessage, e);
            }

            // Since view is a framework element it must be properly initialized
            if (!view.IsInitialized)
            {
                MethodInfo initializeComponentMethod = view.GetType().GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
                if (initializeComponentMethod != null)
                    initializeComponentMethod.Invoke(view, new object[0]);
            }

            // Sets the view model as data context of the view
            view.DataContext = viewModel;

            // Adds the old view to the navigation stack
            if (this.CurrentViewModel != null)
                this.CurrentViewModel.IsInView = false;
            if (this.CurrentView != null)
                this.navigationStack.Push(new KeyValuePair<Page, IViewModel>(this.CurrentView, this.CurrentViewModel));

            // Sets the current view and view model
            this.CurrentView = view;
            this.CurrentViewModel = viewModel;

            // Navigates the user to the new view
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