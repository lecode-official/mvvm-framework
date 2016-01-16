
#region Using Directives

using System;
using System.Collections.Generic;

#endregion

namespace System.Windows.Mvvm.Application
{
    /// <summary>
    /// Represents the event arguments that are passed to the started event of the <see cref="MvvmApplication"/>.
    /// </summary>
    public class ApplicationStartedEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ApplicationStartedEventArgs"/> instance.
        /// </summary>
        /// <param name="commandLineArguments">The command line arguments with which the application has been started.</param>
        /// <param name="isFirstInstance">
        /// A value that determines whether this application has been started as the first instance or whether there is already another instance, which is running (can for example be used to prevent multiple instances).
        /// </param>
        public ApplicationStartedEventArgs(IEnumerable<string> commandLineArguments, bool isFirstInstance)
        {
            this.CommandLineArguments = commandLineArguments;
            this.IsFirstInstance = isFirstInstance;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the command line arguments with which the application has been started.
        /// </summary>
        public IEnumerable<string> CommandLineArguments { get; private set; }

        /// <summary>
        /// Gets a value that determines whether this application has been started as the first instance or whether there is already another instance, which is running (can for example be used to prevent multiple instances).
        /// </summary>
        public bool IsFirstInstance { get; private set; }

        #endregion
    }
}