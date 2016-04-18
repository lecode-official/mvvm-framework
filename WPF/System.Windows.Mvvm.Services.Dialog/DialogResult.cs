
namespace System.Windows.Mvvm.Services.Dialog
{
    /// <summary>
    /// The result of a dialog.
    /// </summary>
    public enum DialogResult
    {
        /// <summary>
        /// The dialog had no result.
        /// </summary>
        None = 0,

        /// <summary>
        /// The user clicked okay.
        /// </summary>
        Okay = 1,

        /// <summary>
        /// The user canceled the dialog.
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// The user clicked yes.
        /// </summary>
        Yes = 6,

        /// <summary>
        /// The user clicked no.
        /// </summary>
        No = 7
    }
}