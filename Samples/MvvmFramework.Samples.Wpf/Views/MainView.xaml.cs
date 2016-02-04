
#region Using Directives

using System.Windows.Controls;
using System.Windows.Mvvm.Sample.ViewModels;
using System.Windows.Mvvm.Services.Navigation;

#endregion

namespace System.Windows.Mvvm.Sample.Views
{
    /// <summary>
    /// Represents the main view of the application.
    /// </summary>
    [ViewModel(typeof(MainViewModel))]
    public partial class MainView : Page
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="MainView"/> instance.
        /// </summary>
        public MainView()
        {
            InitializeComponent();
        }

        #endregion
    }
}