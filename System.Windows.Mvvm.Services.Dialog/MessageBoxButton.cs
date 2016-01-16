
namespace System.Windows.Mvvm.Services.Dialog
{
    /// <summary>
    /// Represents the kind of buttons that can be used in a message box that is displayed using the <see cref="DialogService"/>.
    /// </summary>
    public enum MessageBoxButton
    {
        /// <summary>
        /// An okay button is displayed.
        /// </summary>
        Okay = 0,

        /// <summary>
        /// An okay and a cancel button is displayed.
        /// </summary>
        OkayCancel = 1,

        /// <summary>
        /// A yes, no, and cancel button is displayed.
        /// </summary>
        YesNoCancel = 3,

        /// <summary>
        /// A yes and a no button is displayed.
        /// </summary>
        YesNo = 4
    }
}