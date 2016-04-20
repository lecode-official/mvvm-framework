
#region Using Directives

using MvvmFramework.Samples.Uwp.Models;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace MvvmFramework.Samples.Uwp.Repositories
{
    /// <summary>
    /// Represents a simple in-memory repository for the <see cref="TodoListItem"/>s.
    /// </summary>
    public class TodoListItemsRepository
    {
        #region Private Static Fields

        /// <summary>
        /// Contains a static list of todo list items (usually this data should be stored in a database or a file, but the sake of briefness an in-memory store is used).
        /// </summary>
        private static List<TodoListItem> todoListItems = new List<TodoListItem>
        {
            new TodoListItem
            {
                Title = "Feed the dog",
                Description = "Don't forgot to buy Bingo's favorite dog chow."
            },

            new TodoListItem
            {
                Title = "Buy milk",
                Description = "Lactose free please."
            }
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all available todo list items.
        /// </summary>
        /// <returns>Returns all available todo list items.</returns>
        public IEnumerable<TodoListItem> GetTodoListItems() => TodoListItemsRepository.todoListItems;

        /// <summary>
        /// Gets the specified todo list item.
        /// </summary>
        /// <param name="id">The ID of the todo list item that is to be retrieved.</param>
        /// <returns>Returns the todo list item with the specified ID.</returns>
        public TodoListItem GetTodoListItem(string id) => TodoListItemsRepository.todoListItems.FirstOrDefault(item => item.Id == id);

        /// <summary>
        /// Adds a new item to the todo list.
        /// </summary>
        /// <param name="title">The title of the new todo list item.</param>
        /// <param name="description">The description of the new todo list item.</param>
        /// <returns>Returns the created todo list item.</returns>
        public TodoListItem CreateTodoListItem(string title, string description)
        {
            TodoListItem todoListItem = new TodoListItem
            {
                Title = title,
                Description = description
            };
            TodoListItemsRepository.todoListItems.Add(todoListItem);
            return todoListItem;
        }

        /// <summary>
        /// Removes the specified item from the todo list.
        /// </summary>
        /// <param name="id">The ID of the todo item that is to be removed.</param>
        public void RemoveTodoListItem(string id) => TodoListItemsRepository.todoListItems.Remove(this.GetTodoListItem(id));

        /// <summary>
        /// Marks the specified todo list item as finished.
        /// </summary>
        /// <param name="id">The ID of the todo list item that is to be marked as finished.</param>
        public void MarkTodoListItemAsFinished(string id) => this.GetTodoListItem(id).IsFinished = true;

        #endregion
    }
}