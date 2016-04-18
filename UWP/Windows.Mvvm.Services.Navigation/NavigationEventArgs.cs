
#region Using Directives

using System.ComponentModel;

#endregion

namespace Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents the event arguments that are passed to the navigation events of the view models.
    /// </summary>
    public class NavigationEventArgs : CancelEventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="NavigationEventArgs"/> instance.
        /// </summary>
        /// <param name="reason">The reason of the navigation.</param>
        public NavigationEventArgs(NavigationReason reason)
        {
            this.Reason = reason;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the reason of the navigation. If the reason of the navigation is <c>NavigationReason.WindowClosed</c>, the event cannot be cancelled.
        /// </summary>
        public NavigationReason Reason { get; set; }

        #endregion
    }
}