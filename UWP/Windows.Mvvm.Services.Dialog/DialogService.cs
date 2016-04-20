
#region Using Directives

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;

#endregion

namespace Windows.Mvvm.Services.Dialog
{
    /// <summary>
    /// Represents a service for viewing dialogs. This is needed to abstract away the details from the view models, so that the view models can be unit tested better (view models should not have any dependencies to view code).
    /// </summary>
    public class DialogService
    {
        #region Public Methods

        /// <summary>
        /// Shows a message box dialog.
        /// </summary>
        /// <param name="message">The message that is displayed in the message box dialog.</param>
        /// <param name="title">The title that is displayed in the message box dialog.</param>
        /// <param name="messageBoxDialogCommands">The commands, which are bound to the buttons of the message box dialog.</param>
        public virtual async Task ShowMessageBoxDialogAsync(string message, string title, MessageBoxDialogCommands messageBoxDialogCommands)
        {
            // Executes the message box dialog on the dispatcher thread of the current core application view, this is needed so that the code is always executed on the UI thread
            await CoreApplication.GetCurrentView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Creates a new message  message box dialog
                MessageDialog messageDialog = new MessageDialog(message, title);

                // Adds all the commands to the message box dialog
                foreach (UICommand command in messageBoxDialogCommands.UICommands)
                    messageDialog.Commands.Add(command);

                // Sets the default and the cancel command, which are executed when the return key or the escape key is pressed respectively
                if (messageBoxDialogCommands.DefaultCommand != null)
                    messageDialog.DefaultCommandIndex = 0u;
                else
                    messageDialog.DefaultCommandIndex = uint.MaxValue;
                if (messageBoxDialogCommands.CancelCommand != null)
                    messageDialog.CancelCommandIndex = messageBoxDialogCommands.DefaultCommand != null ? 1u : 0u;
                else
                    messageDialog.CancelCommandIndex = uint.MaxValue;

                // Shows the message box
                await messageDialog.ShowAsync();
            });
        }

        #endregion
    }
}