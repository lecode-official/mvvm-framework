
#region Using Directives

using System;
using System.Threading.Tasks;

#endregion

namespace System.Windows.Mvvm.Services.Navigation
{
    /// <summary>
    /// Represents the interface which is implemented by all view models.
    /// </summary>
    public interface IViewModel : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets a value that determine whether the view of this view model is currently displayed.
        /// </summary>
        bool IsInView { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Is called when the view model is created (before the user is navigated to the view and before the OnNavigateToAsync event is called). After the view model was created, it is cached and reused until it is destroyed, therefore OnActivateAsync
        /// is only called once in the life cycle of a view model.
        /// </summary>
        Task OnActivateAsync();

        /// <summary>
        /// Is called before the view model is navigated to. Other than OnActivateAsync, OnNavigateToAsync is called everytime the user navigates to this view model.
        /// </summary>
        /// <param name="e">The event arguments, that allows the navigation to be cancelled.</param>
        Task OnNavigateToAsync(NavigationEventArgs e);

        /// <summary>
        /// Is called before the view model is navigated away from. Other than OnDeactivateAsync, OnNavigateFromAsync is called everytime the user navigates away from this view model.
        /// </summary>
        /// <param name="e">The event arguments, that allows the navigation to be cancelled.</param>
        Task OnNavigateFromAsync(NavigationEventArgs e);

        /// <summary>
        /// Is called when the view model gets deactivated. The view model only gets deactivated when the navigation stack of the window, that contains the view of this view model, is cleared, or when the windows, containing the view of
        /// this view model, is closed. Therefore OnDeactivateAsync is only called once in the lifecycle of a view model.
        /// </summary>
        Task OnDeactivateAsync();

        #endregion
    }
}