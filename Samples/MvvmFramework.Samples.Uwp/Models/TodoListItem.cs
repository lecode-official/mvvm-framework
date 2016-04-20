
#region Using Directives

using System;

#endregion

namespace MvvmFramework.Samples.Uwp.Models
{
    /// <summary>
    /// Represents a single item on the todo list of the user.
    /// </summary>
    public class TodoListItem
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the unique identifier of the <see cref="TodoListItem"/>, which is a global unique ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the title of the <see cref="TodoListItem"/>.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a detailed description of the <see cref="TodoListItem"/>, which contains the steps to finish it.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value that determines whether the <see cref="TodoListItem"/> has already been finished an can therefore be removed from the todo list.
        /// </summary>
        public bool IsFinished { get; set; }

        #endregion
    }
}