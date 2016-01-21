
#region Using Directives

using System.Windows.Mvvm.Sample.ViewModels;
using System.Windows.Mvvm.Services.Navigation;

#endregion

namespace System.Windows.Mvvm.Sample
{
    /// <summary>
    /// Represents the main window of the application, which hosts the navigation frame in which all pages are rendered.
    /// </summary>
    [ViewModel(typeof(MainWindowViewModel))]
    public partial class MainWindow : Window
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="MainWindow"/> instance.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion
    }
}