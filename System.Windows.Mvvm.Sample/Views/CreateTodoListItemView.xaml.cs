
#region Using Directives

using System.Windows.Controls;
using System.Windows.Mvvm.Sample.ViewModels;
using System.Windows.Mvvm.Services.Navigation;

#endregion

namespace System.Windows.Mvvm.Sample.Views
{
    /// <summary>
    /// Represents a view where the user is able to create a new todo list item.
    /// </summary>
    [ViewModel(typeof(CreateTodoListItemViewModel))]
    public partial class CreateTodoListItemView : Page
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CreateTodoListItemView"/> instance.
        /// </summary>
        public CreateTodoListItemView()
        {
            InitializeComponent();
        }

        #endregion
    }
}