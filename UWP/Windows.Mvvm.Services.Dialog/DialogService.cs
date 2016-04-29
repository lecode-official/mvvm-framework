
#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
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
                // Creates a new message message box dialog
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

        /// <summary>
        /// Displays a file open dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select one or multiple files.</param>
        /// <param name="fileTypeFilter">The file type filter, which determines which kinds of files are displayed to the user.</param>
        /// <param name="suggestedStartLocation">The suggested start location, which is initally displayed by the open file dialog.</param>
        /// <param name="viewMode">The view mode, which determines whether the open file dialog displays the files as a list or as thumbnails.</param>
        /// <param name="isMultiselect">Determines whether the user is able to select multiple files.</param>
        /// <returns>Returns the dialog result and the files that were selected by the user.</returns>
        public virtual async Task<DialogResult<IEnumerable<StorageFile>>> ShowOpenFileDialogAsync(string commitButtonText, IEnumerable<string> fileTypeFilter, PickerLocationId suggestedStartLocation, PickerViewMode viewMode, bool isMultiselect)
        {
            // Creates a new task completion source for the result of the open file dialog
            TaskCompletionSource<IEnumerable<StorageFile>> taskCompletionSource = new TaskCompletionSource<IEnumerable<StorageFile>>();

            // Executes the open file dialog on the dispatcher thread of the current core application view, this is needed so that the code is always executed on the UI thread
            await CoreApplication.GetCurrentView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Creates a new open file dialog
                FileOpenPicker fileOpenPicker = new FileOpenPicker
                {
                    CommitButtonText = commitButtonText,
                    SuggestedStartLocation = suggestedStartLocation,
                    ViewMode = viewMode
                };
                foreach (string currentFileTypeFilter in fileTypeFilter)
                    fileOpenPicker.FileTypeFilter.Add(currentFileTypeFilter == "*" ? currentFileTypeFilter : $".{currentFileTypeFilter}");

                // Shows the open file dialog and stores the result
                if (isMultiselect)
                    taskCompletionSource.TrySetResult(await fileOpenPicker.PickMultipleFilesAsync());
                else
                    taskCompletionSource.TrySetResult(new List<StorageFile> { await fileOpenPicker.PickSingleFileAsync() });
            });

            // Checks if the user cancelled the open file dialog, if so then cancel is returned, otherwise okay is returned with the files that were picked by the user
            IEnumerable<StorageFile> storageFiles = await taskCompletionSource.Task;
            if (storageFiles == null || !storageFiles.Any() || storageFiles.Any(storageFile => storageFile == null))
                return new DialogResult<IEnumerable<StorageFile>>(DialogResult.Cancel, null);
            else
                return new DialogResult<IEnumerable<StorageFile>>(DialogResult.Okay, storageFiles);
        }

        /// <summary>
        /// Displays a file open dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select a file.</param>
        /// <param name="fileTypeFilter">The file type filter, which determines which kinds of files are displayed to the user.</param>
        /// <param name="suggestedStartLocation">The suggested start location, which is initally displayed by the open file dialog.</param>
        /// <param name="viewMode">The view mode, which determines whether the open file dialog displays the files as a list or as thumbnails.</param>
        /// <returns>Returns the dialog result and the file that was selected by the user.</returns>
        public virtual async Task<DialogResult<StorageFile>> ShowOpenFileDialogAsync(string commitButtonText, IEnumerable<string> fileTypeFilter, PickerLocationId suggestedStartLocation, PickerViewMode viewMode)
        {
            // Shows the open file dialog to the user
            DialogResult<IEnumerable<StorageFile>> dialogResult = await this.ShowOpenFileDialogAsync(commitButtonText, fileTypeFilter, suggestedStartLocation, viewMode, false);

            // Checks if the user picked a file, in that case the first one is returned
            if (dialogResult.Result == DialogResult.Cancel)
                return new DialogResult<StorageFile>(DialogResult.Cancel, null);
            else
                return new DialogResult<StorageFile>(DialogResult.Okay, dialogResult.ResultValue.FirstOrDefault());
        }

        /// <summary>
        /// Displays a file open dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select one or multiple files.</param>
        /// <param name="fileTypeFilter">The file type filter, which determines which kinds of files are displayed to the user.</param>
        /// <param name="isMultiselect">Determines whether the user is able to select multiple files.</param>
        /// <returns>Returns the dialog result and the files that were selected by the user.</returns>
        public virtual Task<DialogResult<IEnumerable<StorageFile>>> ShowOpenFileDialogAsync(string commitButtonText, IEnumerable<string> fileTypeFilter, bool isMultiselect) => this.ShowOpenFileDialogAsync(commitButtonText, fileTypeFilter, PickerLocationId.Unspecified, PickerViewMode.Thumbnail, isMultiselect);

        /// <summary>
        /// Displays a file open dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select a file.</param>
        /// <param name="fileTypeFilter">The file type filter, which determines which kinds of files are displayed to the user.</param>
        /// <returns>Returns the dialog result and the file that was selected by the user.</returns>
        public virtual Task<DialogResult<StorageFile>> ShowOpenFileDialogAsync(string commitButtonText, IEnumerable<string> fileTypeFilter) => this.ShowOpenFileDialogAsync(commitButtonText, fileTypeFilter, PickerLocationId.Unspecified, PickerViewMode.Thumbnail);

        /// <summary>
        /// Displays a save file dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select a file.</param>
        /// <param name="fileTypeChoices">The file type filter, which determines which kinds of files are available to the user.</param>
        /// <param name="suggestedFileName">The suggested name of the file that is to be saved.</param>
        /// <param name="defaultFileExtension">The suggested extension of the file that is to be saved.</param>
        /// <param name="suggestedStartLocation">The suggested start location, which is initally displayed by the save file dialog.</param>
        /// <returns>Returns the dialog result with the file that was selected by the user.</returns>
        public virtual async Task<DialogResult<StorageFile>> ShowSaveFileDialogAsync(string commitButtonText, IEnumerable<FileTypeRestriction> fileTypeChoices, string suggestedFileName, string defaultFileExtension, PickerLocationId suggestedStartLocation)
        {
            // Creates a new task completion source for the result of the save file dialog
            TaskCompletionSource<StorageFile> taskCompletionSource = new TaskCompletionSource<StorageFile>();

            // Executes the save file dialog on the dispatcher thread of the current core application view, this is needed so that the code is always executed on the UI thread
            await CoreApplication.GetCurrentView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Creates a new save file dialog
                FileSavePicker fileSavePicker = new FileSavePicker
                {
                    CommitButtonText = commitButtonText,
                    SuggestedStartLocation = suggestedStartLocation
                };
                if (!string.IsNullOrWhiteSpace(suggestedFileName))
                    fileSavePicker.SuggestedFileName = suggestedFileName;
                if (!string.IsNullOrWhiteSpace(defaultFileExtension))
                    fileSavePicker.DefaultFileExtension = defaultFileExtension == "*" ? defaultFileExtension : $".{defaultFileExtension}";
                foreach (FileTypeRestriction fileTypeRestriction in fileTypeChoices)
                    fileSavePicker.FileTypeChoices.Add(fileTypeRestriction.FileTypesDescription, fileTypeRestriction.FileTypes.Select(fileType => fileType == "*" ? fileType : $".{fileType}").ToList());

                // Shows the save file dialog and stores the result
                taskCompletionSource.TrySetResult(await fileSavePicker.PickSaveFileAsync());
            });

            // Checks if the user cancelled the save file dialog, if so then cancel is returned, otherwise okay is returned with the file that was picked by the user
            StorageFile storageFile = await taskCompletionSource.Task;
            if (storageFile == null)
                return new DialogResult<StorageFile>(DialogResult.Cancel, null);
            else
                return new DialogResult<StorageFile>(DialogResult.Okay, storageFile);
        }

        /// <summary>
        /// Displays a save file dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select a file.</param>
        /// <param name="fileTypeChoices">The file type filter, which determines which kinds of files are available to the user.</param>
        /// <param name="suggestedFileName">The suggested name of the file that is to be saved.</param>
        /// <param name="defaultExtension">The suggested extension of the file that is to be saved.</param>
        /// <returns>Returns the dialog result with the file that was selected by the user.</returns>
        public virtual Task<DialogResult<StorageFile>> ShowSaveFileDialogAsync(string commitButtonText, IEnumerable<FileTypeRestriction> fileTypeChoices, string suggestedFileName, string defaultExtension) => this.ShowSaveFileDialogAsync(commitButtonText, fileTypeChoices, suggestedFileName, defaultExtension, PickerLocationId.Unspecified);

        /// <summary>
        /// Displays a save file dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select a file.</param>
        /// <param name="fileTypeChoices">The file type filter, which determines which kinds of files are available to the user.</param>
        /// <returns>Returns the dialog result with the file that was selected by the user.</returns>
        public virtual Task<DialogResult<StorageFile>> ShowSaveFileDialogAsync(string commitButtonText, IEnumerable<FileTypeRestriction> fileTypeChoices) => this.ShowSaveFileDialogAsync(commitButtonText, fileTypeChoices, null, null);

        /// <summary>
        /// Displays a save file dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select a file.</param>
        /// <param name="fileTypeChoices">The file type filter, which determines which kinds of files are available to the user.</param>
        /// <param name="suggestedSaveFile">The file which is suggested to the user as the file to which he is saving.</param>
        /// <param name="suggestedStartLocation">The suggested start location, which is initally displayed by the save file dialog.</param>
        /// <returns>Returns the dialog result with the file that was selected by the user.</returns>
        public virtual async Task<DialogResult<StorageFile>> ShowSaveFileDialogAsync(string commitButtonText, IEnumerable<FileTypeRestriction> fileTypeChoices, StorageFile suggestedSaveFile, PickerLocationId suggestedStartLocation)
        {
            // Creates a new task completion source for the result of the save file dialog
            TaskCompletionSource<StorageFile> taskCompletionSource = new TaskCompletionSource<StorageFile>();

            // Executes the save file dialog on the dispatcher thread of the current core application view, this is needed so that the code is always executed on the UI thread
            await CoreApplication.GetCurrentView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Creates a new save file dialog
                FileSavePicker fileSavePicker = new FileSavePicker
                {
                    CommitButtonText = commitButtonText,
                    SuggestedStartLocation = suggestedStartLocation
                };
                if (suggestedSaveFile != null)
                    fileSavePicker.SuggestedSaveFile = suggestedSaveFile;
                foreach (FileTypeRestriction fileTypeRestriction in fileTypeChoices)
                    fileSavePicker.FileTypeChoices.Add(fileTypeRestriction.FileTypesDescription, fileTypeRestriction.FileTypes.Select(fileType => fileType == "*" ? fileType : $".{fileType}").ToList());

                // Shows the save file dialog and stores the result
                taskCompletionSource.TrySetResult(await fileSavePicker.PickSaveFileAsync());
            });

            // Checks if the user cancelled the save file dialog, if so then cancel is returned, otherwise okay is returned with the file that was picked by the user
            StorageFile storageFile = await taskCompletionSource.Task;
            if (storageFile == null)
                return new DialogResult<StorageFile>(DialogResult.Cancel, null);
            else
                return new DialogResult<StorageFile>(DialogResult.Okay, storageFile);
        }

        /// <summary>
        /// Displays a save file dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select a file.</param>
        /// <param name="fileTypeFilter">The file type filter, which determines which kinds of files are available to the user.</param>
        /// <param name="suggestedSaveFile">The file which is suggested to the user as the file to which he is saving.</param>
        /// <returns>Returns the dialog result with the file that was selected by the user.</returns>
        public virtual Task<DialogResult<StorageFile>> ShowSaveFileDialogAsync(string commitButtonText, IEnumerable<FileTypeRestriction> fileTypeFilter, StorageFile suggestedSaveFile) => this.ShowSaveFileDialogAsync(commitButtonText, fileTypeFilter, suggestedSaveFile, PickerLocationId.Unspecified);

        /// <summary>
        /// Displays a folder browser dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select a folder.</param>
        /// <param name="suggestedStartLocation">The suggested start location, which is initally displayed by the browse folder dialog.</param>
        /// <param name="viewMode">The view mode, which determines whether the browse folder dialog displays the folders as a list or as thumbnails.</param>
        /// <returns>Returns the dialog result and the folder that was selected by the user.</returns>
        public virtual async Task<DialogResult<StorageFolder>> ShowFolderBrowseDialogAsync(string commitButtonText, PickerLocationId suggestedStartLocation, PickerViewMode viewMode)
        {
            // Creates a new task completion source for the result of the folder browser dialog
            TaskCompletionSource<StorageFolder> taskCompletionSource = new TaskCompletionSource<StorageFolder>();

            // Executes the folder browser dialog on the dispatcher thread of the current core application view, this is needed so that the code is always executed on the UI thread
            await CoreApplication.GetCurrentView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Creates a new browse folder dialog
                FolderPicker folderPicker = new FolderPicker
                {
                    CommitButtonText = commitButtonText,
                    SuggestedStartLocation = suggestedStartLocation,
                    ViewMode = viewMode
                };

                // Shows the browse folder dialog and stores the result
                taskCompletionSource.TrySetResult(await folderPicker.PickSingleFolderAsync());
            });

            // Checks if the user cancelled the browse folder dialog, if so then cancel is returned, otherwise okay is returned with the folder that was picked by the user
            StorageFolder storageFolder = await taskCompletionSource.Task;
            if (storageFolder == null)
                return new DialogResult<StorageFolder>(DialogResult.Cancel, null);
            else
                return new DialogResult<StorageFolder>(DialogResult.Okay, storageFolder);
        }

        /// <summary>
        /// Displays a folder browser dialog to the user.
        /// </summary>
        /// <param name="commitButtonText">The text, that is displayed on the commit button, that the user has to press to select a folder.</param>
        /// <returns>Returns the dialog result and the folder that was selected by the user.</returns>
        public virtual Task<DialogResult<StorageFolder>> ShowFolderBrowseDialogAsync(string commitButtonText) => this.ShowFolderBrowseDialogAsync(commitButtonText, PickerLocationId.Unspecified, PickerViewMode.List);

        #endregion
    }
}