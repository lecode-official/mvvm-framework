
#region Using Directives

using Windows.UI.Xaml.Controls;

#endregion

namespace MvvmFramework.Samples.Uwp.Views
{
    /// <summary>
    /// Represents a view where the user is able to create a new todo list item.
    /// </summary>
    public sealed partial class CreateTodoListItemView : Page
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CreateTodoListItemView"/> instance.
        /// </summary>
        public CreateTodoListItemView()
        {
            this.InitializeComponent();
        }

        #endregion
    }
}