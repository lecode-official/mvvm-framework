
namespace Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents a wrapper object, which encapsulates the result of creating a new window and navigating to it.
    /// </summary>
    public class WindowNavigationResult
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the result of the navigation (e.g. the navigation was cancelled).
        /// </summary>
        public NavigationResult Result { get; set; }

        /// <summary>
        /// Gets or sets the navigation service for the newly created window. May be <c>null</c> if the window does not support navigation or the navigation was cancelled.
        /// </summary>
        public NavigationService NavigationService { get; set; }

        #endregion
    }
}