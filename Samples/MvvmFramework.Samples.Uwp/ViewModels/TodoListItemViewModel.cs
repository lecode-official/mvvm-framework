
#region Using Directives

using ReactiveUI;

#endregion

namespace MvvmFramework.Samples.Uwp.ViewModels
{
    /// <summary>
    /// Represents a view model for a single todo list item.
    /// </summary>
    public class TodoListItemViewModel : ReactiveObject
    {
        #region Public Properties

        /// <summary>
        /// Contains the unique identifier of the todo list item, which is a global unique ID.
        /// </summary>
        private string id;

        /// <summary>
        /// Gets or sets the unique identifier of the todo list item, which is a global unique ID.
        /// </summary>
        public string Id
        {
            get
            {
                return this.id;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.id, value);
            }
        }

        /// <summary>
        /// Contains the title of the todo list item.
        /// </summary>
        private string title;

        /// <summary>
        /// Gets or sets the title of the todo list item.
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.title, value);
            }
        }

        /// <summary>
        /// Contains a detailed description of the todo list item, which contains the steps to finish it.
        /// </summary>
        private string description;

        /// <summary>
        /// Gets or sets a detailed description of the todo list item, which contains the steps to finish it.
        /// </summary>
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.description, value);
            }
        }

        /// <summary>
        /// Contains a value that determines whether the todo list item has already been finished an can therefore be removed from the todo list.
        /// </summary>
        private bool isFinished;

        /// <summary>
        /// Gets or sets a value that determines whether the todo list item has already been finished an can therefore be removed from the todo list.
        /// </summary>
        public bool IsFinished
        {
            get
            {
                return this.isFinished;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isFinished, value);
            }
        }

        #endregion
    }
}