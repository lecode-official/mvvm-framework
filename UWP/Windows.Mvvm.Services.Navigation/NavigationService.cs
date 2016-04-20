
#region Using Directives

using System;
using System.Collections.Generic;
using System.InversionOfControl.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
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
        /// <param name="window">The window for which the navigation service is to be created.</param>
        /// <param name="applicationView">The application view for which the navigation service is to be created.</param>
        /// <param name="navigationFrame">The frame with in which the actual navigation between views takes place.</param>
        internal NavigationService(IReadOnlyIocContainer iocContainer, Window window, ApplicationView applicationView, Frame navigationFrame)
        {
            // Validates the arguments
            if (iocContainer == null)
                throw new ArgumentNullException(nameof(iocContainer));
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            if (applicationView == null)
                throw new ArgumentNullException(nameof(applicationView));
            if (navigationFrame == null)
                throw new ArgumentNullException(nameof(navigationFrame));

            // Stores the IoC container, the window, the application view, and the frame for later use
            this.iocContainer = iocContainer;
            this.window = window;
            this.ApplicationView = applicationView;
            this.navigationFrame = navigationFrame;
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
        /// Contains the window of the navigation service.
        /// </summary>
        private readonly Window window;

        /// <summary>
        /// Contains the frame with in which the actual navigation between views takes place.
        /// </summary>
        private readonly Frame navigationFrame;

        /// <summary>
        /// Contains the stack of views for the backwards navigation.
        /// </summary>
        private readonly Stack<KeyValuePair<Page, IViewModel>> navigationStack = new Stack<KeyValuePair<Page, IViewModel>>();

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets or sets the view that is currently being viewed in the window. If the window does not support navigation, then the current view is always null.
        /// </summary>
        internal Page CurrentView { get; set; }

        /// <summary>
        /// Gets or sets the view model of the view that is currently in view of the window that is being managed by this navigation service.
        /// </summary>
        internal IViewModel CurrentViewModel { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the application view, which represents the window, managed by the navigation service.
        /// </summary>
        public ApplicationView ApplicationView { get; private set; }

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

        #region Internal Methods

        /// <summary>
        /// Closes the window that is managed by this navigation service.
        /// </summary>
        /// <param name="isCancellable">Determines whether the closing of the application can be cancelled by the view model.</param>
        /// <returns>Returns a value that determines whether the window was closed or its closing was cancelled. If <see cref="p:isCancellable"/> is set to <c>false</c>, <c>NavigationResult.Navigated</c> is always returned.</returns>
        internal Task<NavigationResult> CloseWindowAsync(bool isCancellable) => this.CloseWindowAsync(isCancellable, !isCancellable ? NavigationReason.WindowClosed : NavigationReason.WindowClosing);

        /// <summary>
        /// Closes the window that is managed by this navigation service.
        /// </summary>
        /// <param name="isCancellable">Determines whether the closing of the application can be cancelled by the view model.</param>
        /// <param name="navigationReason">The navigation reason that is sent to the view model.</param>
        /// <returns>Returns a value that determines whether the window was closed or its closing was cancelled. If <see cref="p:isCancellable"/> is set to <c>false</c>, <c>NavigationResult.Navigated</c> is always returned.</returns>
        internal async Task<NavigationResult> CloseWindowAsync(bool isCancellable, NavigationReason navigationReason)
        {
            // Creates new event arguments for the navigation events
            NavigationEventArgs eventArguments = null;

            // Checks if the current view model exists, if so then its life-cycle events must be invoked
            if (this.CurrentViewModel != null)
            {
                // Checks if the window supports navigation and if the the current view has a view model, if so, then the on navigate from event is raised for the view model of the current view
                eventArguments = new NavigationEventArgs(navigationReason);
                await this.CurrentViewModel.OnNavigateFromAsync(eventArguments);
                if (eventArguments.Cancel && isCancellable && navigationReason != NavigationReason.WindowClosed)
                    return NavigationResult.Canceled;

                // Deactivates the view model of the current view and disposes of it
                await this.CurrentViewModel.OnDeactivateAsync();
                this.CurrentViewModel.Dispose();
            }

            // Clears the navigation stack
            await this.ClearNavigationStackAsync();

            // Closes the window
            this.window.Close();

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
            ViewModelAttribute viewModelAttribute = typeof(TView).GetTypeInfo().GetCustomAttributes<ViewModelAttribute>().FirstOrDefault();
            if (viewModelAttribute != null)
            {
                viewModelType = viewModelAttribute.ViewModelType;
            }
            else
            {
                this.assemblyTypes = this.assemblyTypes ?? typeof(TView).GetTypeInfo().Assembly.GetTypes();
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
                    throw new InvalidOperationException("The view model could not be instantiated.", e);
                }

                // Checks whether the view model implements the IViewModel interface
                if (viewModel == null)
                    throw new InvalidOperationException("The view model type is incorrect. View models must implement the IViewModel interface.");
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

            // Adds the old view to the navigation stack
            if (this.CurrentViewModel != null)
                this.CurrentViewModel.IsInView = false;
            if (this.CurrentView != null)
                this.navigationStack.Push(new KeyValuePair<Page, IViewModel>(this.CurrentView, this.CurrentViewModel));

            // Instantiates the new view
            TaskCompletionSource<Page> taskCompletionSource = new TaskCompletionSource<Page>();
            this.navigationFrame.Navigated += (sender, e) => taskCompletionSource.SetResult(e.Content as Page);
            this.navigationFrame.Navigate(typeof(TView));
            this.CurrentView = await taskCompletionSource.Task;

            // Sets the view model as data context of the view and sets the new current view model
            this.CurrentView.DataContext = viewModel;
            this.CurrentViewModel = viewModel;

            // Sets an indicator in the new view model, that it is now in view
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

            // Navigates to the view that was on top of the navigation stack (the data context is set to null before navigation and is reset afterwards, because this causes all the bindings that do not call the property changed event to properly update; this is, for example helpful if database models are used in views for performance reasons, because they do not implement INotifyPropertyChanged)
            Page view = this.navigationStack.Pop().Key;
            this.CurrentView = view;
            this.CurrentViewModel = viewModel;
            this.CurrentView.DataContext = null;
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            this.navigationFrame.Navigated += (sender, e) => taskCompletionSource.SetResult(true);
            this.navigationFrame.GoBack();
            await taskCompletionSource.Task;
            if (this.CurrentViewModel != null)
                this.CurrentViewModel.IsInView = true;
            this.CurrentView.DataContext = this.CurrentViewModel;

            // Since the back navigation was successful, a positive result is returned
            return NavigationResult.Navigated;
        }

        #endregion
    }
}