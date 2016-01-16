
#region Using Directives

using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Mvvm.Services.Application;

#endregion

namespace System.Windows.Mvvm.Services.Dialog
{
    /// <summary>
    /// Represents a service for viewing dialogs. This is needed to abstract away the details from the view models, so that the view models can be unit tested better (view models should not have any dependencies to view code).
    /// </summary>
    public class DialogService
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DialogService"/> instance.
        /// </summary>
        /// <param name="applicationService">A service, which can be used to manage the application life-cycle.</param>
        public DialogService(ApplicationService applicationService)
        {
            this.applicationService = applicationService;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains a service, which can be used to manage the application life-cycle.
        /// </summary>
        private readonly ApplicationService applicationService;

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays a message box to the user.
        /// </summary>
        /// <param name="message">The message that is to be displayed.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="messageBoxButton">Determines the buttons that are displayed in the message box.</param>
        /// <param name="messageBoxIcon">Determines the icon that is displayed in the message box.</param>
        /// <returns>Returns the result of the message box, which contains the button that was clicked by the user.</returns>
        public virtual async Task<DialogResult> ShowMessageBoxDialogAsync(string message, string title, MessageBoxButton messageBoxButton, MessageBoxIcon messageBoxIcon)
        {
            // Since the message box must be invoked on the UI thread, the invocation is dispatched to the UI thread
            return await this.applicationService.CurrentDispatcher.InvokeAsync(() =>
            {
                // Get the message box buttons that are to be displayed
                System.Windows.MessageBoxButton buttons = (System.Windows.MessageBoxButton)messageBoxButton;

                // Get the message box icon that is to be displayed
                System.Windows.MessageBoxImage icon = (System.Windows.MessageBoxImage)messageBoxIcon;

                // Prompt the message
                System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(message, title, buttons, icon);

                // Returns the result of the message box invocation
                return (DialogResult)messageBoxResult;
            });
        }

        /// <summary>
        /// Displays a file open dialog to the user.
        /// </summary>
        /// <param name="title">The title of the file open dialog.</param>
        /// <param name="filter">The file type restrictions, that should be put on the file open dialog.</param>
        /// <returns>Returns the dialog result and the resulting file name.</returns>
        public virtual async Task<DialogResult<string>> ShowOpenFileDialogAsync(string title, ICollection<FileTypeRestriction> filter)
        {
            // Validates the parameters
            if (filter == null)
                throw new ArgumentNullException("filter");

            // Since the dialog box must be invoked on the UI thread, the invocation is dispatched to the UI thread
            return await this.applicationService.CurrentDispatcher.InvokeAsync(() =>
            {
                // Creates the file open dialog and shows it to the user
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Title = title,
                    Filter = string.Join("|", filter.Select(restriction => restriction.Restriction))
                };
                bool? result = openFileDialog.ShowDialog();

                // Returns the dialog result
                if (result == true)
                    return new DialogResult<string>(DialogResult.Okay, openFileDialog.FileName);
                else if (result == false)
                    return new DialogResult<string>(DialogResult.Cancel, string.Empty);
                else
                    return new DialogResult<string>(DialogResult.None, string.Empty);
            });
        }

        /// <summary>
        /// Displays a file save dialog to the user.
        /// </summary>
        /// <param name="title">The title of the file save dialog.</param>
        /// <param name="filter">The file type restrictions, that should be put on the file save dialog.</param>
        /// <param name="defaultExtension">The file extension, that is used by default for the new file (without dot and star, e.g. "txt" instead of "*.txt").</param>
        /// <returns>Returns the dialog result and the resulting file name.</returns>
        public virtual async Task<DialogResult<string>> ShowSaveFileDialogAsync(string title, ICollection<FileTypeRestriction> filter, string defaultExtension)
        {
            // Validates the parameters
            if (filter == null)
                throw new ArgumentNullException("filter");

            // Since the dialog box must be invoked on the UI thread, the invocation is dispatched to the UI thread
            return await this.applicationService.CurrentDispatcher.InvokeAsync(() =>
            {
                // Creates the file save dialog and shows it to the user
                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    Title = title,
                    Filter = string.Join("|", filter.Select(restriction => restriction.Restriction)),
                    DefaultExt = defaultExtension
                };
                bool? result = saveFileDialog.ShowDialog();

                // Returns the dialog result
                if (result == true)
                    return new DialogResult<string>(DialogResult.Okay, saveFileDialog.FileName);
                else if (result == false)
                    return new DialogResult<string>(DialogResult.Cancel, string.Empty);
                else
                    return new DialogResult<string>(DialogResult.None, string.Empty);
            });
        }

        /// <summary>
        /// Displays a folder browser dialog to the user.
        /// </summary>
        /// <param name="description">The description that is displayed inside the folder browser dialog.</param>
        /// <returns>Returns the dialog result and the path that the user has selected.</returns>
        public virtual async Task<DialogResult<string>> ShowFolderBrowseDialogAsync(string description)
        {
            // Since the dialog box must be invoked on the UI thread, the invocation is dispatched to the UI thread
            return await this.applicationService.CurrentDispatcher.InvokeAsync(() =>
            {
                // Creates the folder browser dialog, shows it to the user and returns the dialog result and the selected path
                using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    // Sets the description of the folder browser dialog
                    folderBrowserDialog.Description = description;

                    // Displays the folder browser dialog to the user and returns the dialog result
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                        return new DialogResult<string>(DialogResult.Okay, folderBrowserDialog.SelectedPath);
                    else
                        return new DialogResult<string>(DialogResult.Cancel, string.Empty);
                }
            });
        }

        #endregion
    }
}