
namespace Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents an enumeration for the reasons that state why a navigation happened.
    /// </summary>
    public enum NavigationReason
    {
        /// <summary>
        /// The navigation happened because the user navigates to a new view (may be cancelled).
        /// </summary>
        Navigation,

        /// <summary>
        /// The navigation happened because a new window was opened (may be cancelled).
        /// </summary>
        WindowOpened,

        /// <summary>
        /// The navigation happened because the window is being closed (may be cancelled).
        /// </summary>
        WindowClosing,

        /// <summary>
        /// The navigation happened because the window got closed (may not be cancelled).
        /// </summary>
        WindowClosed,

        /// <summary>
        /// The navigation happened because the application is being shut down (may not be cancelled).
        /// </summary>
        ApplicationExit
    }
}