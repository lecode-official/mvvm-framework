
#region Using Directives

using System.Diagnostics;

#endregion

namespace System.Windows.Mvvm.Services.OperatingSystem
{
    /// <summary>
    /// Represents a service, that provides functionality to manage the operating system life-cycle.
    /// </summary>
    public class OperatingSystemService
    {
        #region Public Methods

        /// <summary>
        /// Shuts down the operating system.
        /// </summary>
        public void Shutdown()
        {
            Process.Start("shutdown", "/s /t 0");
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the user ends the Windows session by logging off or shutting down the operating system.
        /// </summary>
        public event SessionEndingCancelEventHandler SessionEnding
        {
            add
            {
                System.Windows.Application.Current.SessionEnding += value;
            }

            remove
            {
                System.Windows.Application.Current.SessionEnding -= value;
            }
        }

        #endregion
    }
}