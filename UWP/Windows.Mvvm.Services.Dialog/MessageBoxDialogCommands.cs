
#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Popups;

#endregion

namespace Windows.Mvvm.Services.Dialog
{
    /// <summary>
    /// Represents a set of commands for a message box dialog. The individual commands are mapped to the buttons of the message box dialog.
    /// </summary>
    public class MessageBoxDialogCommands
    {
        #region Internal Properties
        
        /// <summary>
        /// Contains the UI commands, which are actually bound to the message box dialog.
        /// </summary>
        private List<UICommand> uiCommands;

        /// <summary>
        /// Gets the UI commands, which are actually bound to the message box dialog.
        /// </summary>
        public IEnumerable<UICommand> UICommands
        {
            get
            {
                // Checks if the UI commands have already been created, if not then they are lazily initialized.
                if (this.uiCommands == null)
                {
                    // Creates a new list for the UI commands
                    this.uiCommands = new List<UICommand>();

                    // Adds the default command
                    this.uiCommands.Add(new UICommand
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = this.DefaultCommand.Label,
                        Invoked = async c =>
                        {
                            if (this.DefaultCommand.Command != null)
                                await this.DefaultCommand.Command();
                        }
                    });

                    // Adds the cancel command
                    this.uiCommands.Add(new UICommand
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = this.CancelCommand.Label,
                        Invoked = async c =>
                        {
                            if (this.CancelCommand.Command != null)
                                await this.CancelCommand.Command();
                        }
                    });

                    // Adds the rest of the commands
                    this.uiCommands.AddRange(this.Commands.Select(command => new UICommand
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = command.Label,
                        Invoked = async c =>
                        {
                            if (command.Command != null)
                                await command.Command();
                        }
                    }));
                }

                // Returns the created UI commands
                return this.uiCommands;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the default command, which is the command, that is executed when the return key is pressed.
        /// </summary>
        public MessageBoxDialogCommand DefaultCommand { get; set; }

        /// <summary>
        /// Gets or sets the cancel command, which is the command, that is executed when the escape key is pressed.
        /// </summary>
        public MessageBoxDialogCommand CancelCommand { get; set; }

        /// <summary>
        /// Gets or sets the rest of the commands, which are represented as their own buttons in the message box dialog.
        /// </summary>
        public ICollection<MessageBoxDialogCommand> Commands { get; set; } = new Collection<MessageBoxDialogCommand>();

        #endregion
    }
}