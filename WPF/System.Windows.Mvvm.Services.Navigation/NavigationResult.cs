
namespace System.Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents an enumeration that contains the result value of a navigation.
    /// </summary>
    public enum NavigationResult
    {
        /// <summary>
        /// The navigation was cancelled by the view model.
        /// </summary>
        Canceled,

        /// <summary>
        /// The navigation was successful.
        /// </summary>
        Navigated
    }
}