
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

        /// <summary>
        /// Shows a message box dialog.
        /// </summary>
        /// <param name="message">The message that is displayed in the message box dialog.</param>
        /// <param name="title">The title that is displayed in the message box dialog.</param>
        /// <param name="messageBoxButton">The message box buttons that are to be displayed in the message box dialog.</param>
        /// <returns>Returns the buttont that was pressed by the user.</returns>
        public virtual async Task<DialogResult> ShowMessageBoxDialogAsync(string message, string title, MessageBoxButton messageBoxButton)
        {
            // Creates a new task completion source, which will be resolved when the message box dialog has been closed
            TaskCompletionSource<DialogResult> taskCompletionSource = new TaskCompletionSource<DialogResult>();

            // Creates the message box dialog commands for the specified message box buttons
            MessageBoxDialogCommands messageBoxDialogCommands = new MessageBoxDialogCommands();
            if (messageBoxButton == MessageBoxButton.Okay || messageBoxButton == MessageBoxButton.OkayCancel)
            {
                messageBoxDialogCommands.DefaultCommand = new MessageBoxDialogCommand
                {
                    Label = "Okay",
                    Command = () =>
                    {
                        taskCompletionSource.TrySetResult(DialogResult.Okay);
                        return Task.FromResult(0);
                    }
                };
            }
            if (messageBoxButton == MessageBoxButton.YesNo || messageBoxButton == MessageBoxButton.YesNoCancel)
            {
                messageBoxDialogCommands.DefaultCommand = new MessageBoxDialogCommand
                {
                    Label = "Yes",
                    Command = () =>
                    {
                        taskCompletionSource.TrySetResult(DialogResult.Yes);
                        return Task.FromResult(0);
                    }
                };
            }
            if (messageBoxButton == MessageBoxButton.OkayCancel || messageBoxButton == MessageBoxButton.YesNoCancel)
            {
                messageBoxDialogCommands.CancelCommand = new MessageBoxDialogCommand
                {
                    Label = "Cancel",
                    Command = () =>
                    {
                        taskCompletionSource.TrySetResult(DialogResult.Cancel);
                        return Task.FromResult(0);
                    }
                };
            }
            if (messageBoxButton == MessageBoxButton.YesNo)
            {
                messageBoxDialogCommands.CancelCommand = new MessageBoxDialogCommand
                {
                    Label = "No",
                    Command = () =>
                    {
                        taskCompletionSource.TrySetResult(DialogResult.No);
                        return Task.FromResult(0);
                    }
                };
            }
            if (messageBoxButton == MessageBoxButton.YesNoCancel)
            {
                messageBoxDialogCommands.Commands.Add(new MessageBoxDialogCommand
                {
                    Label = "No",
                    Command = () =>
                    {
                        taskCompletionSource.TrySetResult(DialogResult.No);
                        return Task.FromResult(0);
                    }
                });
            }

            // Shows the message box dialog
            await this.ShowMessageBoxDialogAsync(message, title, messageBoxDialogCommands);

            // Gets the result of the message box dialog and returns it
            return await taskCompletionSource.Task;
        }

        #endregion
    }
}