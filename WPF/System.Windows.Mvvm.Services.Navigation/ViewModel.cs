
#region Using Directives

using System.Threading.Tasks;

#endregion

namespace System.Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents the standard implementation of the <see cref="IViewModel"/> interface.
    /// </summary>
    public class ViewModel
    {
        #region Public Properties

        /// <summary>
        /// Gets a value that determine whether the view of this view model is currently displayed.
        /// </summary>
        public bool IsInView { get; set; }

        /// <summary>
        /// Gets or sets a value that determines whether the object has already been disposed of or not.
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion

        #region IViewModel Implementation

        /// <summary>
        /// Is called when the view model is created (before the user is navigated to the view and before the OnNavigateTo even is called). After the view model was created, it is cached and reused until it is destroyed, therefore OnActivate
        /// is only called once in the life cycle of a view model.
        /// </summary>
        public virtual Task OnActivateAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Is called before the view model is navigated to. Other than OnActivate, OnNavigateTo is called everytime the user navigates to this view model.
        /// </summary>
        /// <param name="e">The event arguments, that allows the navigation to be cancelled.</param>
        public virtual Task OnNavigateToAsync(NavigationEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Is called before the view model is navigated away from. Other than OnDeactivate, OnNavigateFrom is called everytime the user navigates away from this view model.
        /// </summary>
        /// <param name="e">The event arguments, that allows the navigation to be cancelled.</param>
        public virtual Task OnNavigateFromAsync(NavigationEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Is called when the view model gets deactivated. The view model only gets deactivated when the navigation stack of the window, that contains the view of this view model, is cleared, or when the windows, containing the view of
        /// this view model, is closed. Therefore OnDeactivate is only called once in the life cycle of a view model.
        /// </summary>
        public virtual Task OnDeactivateAsync()
        {
            return Task.FromResult(0);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Performs the actual disposing logic by calling the custom disposal logic of the view model implementation.
        /// </summary>
        /// <param name="disposing">Determines whether managed or unmanaged objects are disposed of. If true managed and unmanaged resources must be disposed of otherwise only unmanaged resources must be disposed of.</param>
        protected virtual void Dispose(bool disposing)
        {
            // This view model can only be disposed of if it has not been disposed of before
            if (this.IsDisposed)
                return;

            // Sets the flag to state that the view model has been disposed of
            this.IsDisposed = true;
        }

        /// <summary>
        /// Disposes of the resources that the view model acquired.
        /// </summary>
        public void Dispose()
        {
            // Disposes of the managed as well as the unmanaged resources of the view model
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method, therefore GC.SupressFinalize is called to take this object off the finalization queue and prevent finalization code for this object from executing a second time
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}