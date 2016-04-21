
#region Using Directives

using Windows.Storage;

#endregion

namespace Windows.Mvvm.Configuration.Stores
{
    /// <summary>
    /// Represents a store that uses the local application data to save the configuration data.
    /// </summary>
    public class LocalApplicationDataStore : ApplicationDataStore
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="LocalApplicationDataStore"/> instance.
        /// </summary>
        public LocalApplicationDataStore()
            : this("LocalConfiguration")
        { }

        /// <summary>
        /// Initializes a new <see cref="LocalApplicationDataStore"/> instance.
        /// </summary>
        /// <param name="containerName">The name of the container in which the configuration data is to be stored.</param>
        public LocalApplicationDataStore(string containerName)
            : base(ApplicationData.Current.LocalSettings.CreateContainer(containerName, ApplicationDataCreateDisposition.Always))
        { }

        #endregion
    }
}