
#region Using Directives

using System.Threading.Tasks;
using System.Windows.Mvvm.Application;
using System.Windows.Mvvm.Sample.Repositories;

#endregion

namespace System.Windows.Mvvm.Sample
{
    /// <summary>
    /// Represents the MVVM application and serves as an entry-point to the application.
    /// </summary>
    public partial class App : MvvmApplication
    {
        #region MvvmApplication Implementation

        /// <summary>
        /// This is the entry-piont to the application, which gets called as soon as the application has finished starting up.
        /// </summary>
        /// <param name="eventArguments">The event arguments, that contains all necessary information about the application startup like the command line arguments.</param>
        protected override Task OnStartedAsync(ApplicationStartedEventArgs eventArguments)
        {
            // Binds the todo list item repository to the Ninject kernel, so that it can be automatically injected into view models
            this.Kernel.Bind<TodoListItemsRepository>().ToSelf().InSingletonScope();

            // Since no asynchronous operation was performed, an empty task is returned
            return Task.FromResult(0);
        }

        #endregion
    }
}