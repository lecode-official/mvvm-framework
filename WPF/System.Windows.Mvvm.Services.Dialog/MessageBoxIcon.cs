
namespace System.Windows.Mvvm.Services.Dialog
{
    /// <summary>
    /// Represents the kind of icons that can be used in a message box that is displayed using the <see cref="DialogService"/>.
    /// </summary>
    public enum MessageBoxIcon
    {
        /// <summary>
        /// No icon is displayed.
        /// </summary>
        None = 0,

        /// <summary>
        /// A question mark icon is displayed.
        /// </summary>
        Question = 32,

        /// <summary>
        /// An error symbol icon is displayed.
        /// </summary>
        Error = 16,

        /// <summary>
        /// An exclamation mark icon is displayed.
        /// </summary>
        Exclamation = 48
    }
}