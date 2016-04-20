
#region Using Directives

using System;
using System.Threading.Tasks;

#endregion

namespace Windows.Mvvm.Services.Dialog
{
    /// <summary>
    /// Represents a command for a message box (commands are mapped to the message box dialog buttons).
    /// </summary>
    public class MessageBoxDialogCommand
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the label of the message box dialog, which is the text that is being displayed on the corresponding button.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the delegate, which is being executed, when the corresponding message box dialog button is clicked.
        /// </summary>
        public Func<Task> Command { get; set; }

        #endregion
    }
}