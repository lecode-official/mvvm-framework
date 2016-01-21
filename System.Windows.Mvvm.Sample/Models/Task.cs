
namespace System.Windows.Mvvm.Sample.Models
{
    /// <summary>
    /// Represents a single task on the todo list of the user.
    /// </summary>
    public class Task
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the unique identifier of the <see cref="Task"/>, which is a global unique ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the title of the <see cref="Task"/>.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a detailed description of the <see cref="Task"/>, which contains the steps to finish it.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value that determines whether the <see cref="Task"/> has already been finished an can therefore be removed from the todo list.
        /// </summary>
        public bool IsFinished { get; set; }

        #endregion
    }
}