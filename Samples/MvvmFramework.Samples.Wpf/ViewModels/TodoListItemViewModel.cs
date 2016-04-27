
#region Using Directives

using System.Windows.Mvvm.Reactive;
using System.Windows.Mvvm.Sample.Models;

#endregion

namespace System.Windows.Mvvm.Sample.ViewModels
{
    /// <summary>
    /// Represents a view model for a single todo list item.
    /// </summary>
    public class TodoListItemViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="TodoListItemViewModel"/> instance.
        /// </summary>
        /// <param name="todoListItem">The model from which the view model is to be generated.</param>
        public TodoListItemViewModel(TodoListItem todoListItem)
        {
            this.Id.Value = todoListItem.Id;
            this.Title.Value = todoListItem.Title;
            this.Description.Value = todoListItem.Description;
            this.IsFinished.Value = todoListItem.IsFinished;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the unique identifier of the todo list item, which is a global unique ID.
        /// </summary>
        public ReactiveProperty<string> Id { get; } = new ReactiveProperty<string>();

        /// <summary>
        /// Gets or sets the title of the todo list item.
        /// </summary>
        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>();
        
        /// <summary>
        /// Gets or sets a detailed description of the todo list item, which contains the steps to finish it.
        /// </summary>
        public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>();

        /// <summary>
        /// Gets or sets a value that determines whether the todo list item has already been finished an can therefore be removed from the todo list.
        /// </summary>
        public ReactiveProperty<bool> IsFinished { get; } = new ReactiveProperty<bool>();

        #endregion
    }
}