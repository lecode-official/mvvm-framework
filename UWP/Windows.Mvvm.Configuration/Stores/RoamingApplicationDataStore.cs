
#region Using Directives

using Windows.Storage;

#endregion

namespace Windows.Mvvm.Configuration.Stores
{
    /// <summary>
    /// Represents a store that uses the roaming application data to save the configuration data.
    /// </summary>
    public class RoamingApplicationDataStore : ApplicationDataStore
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="RoamingApplicationDataStore"/> instance.
        /// </summary>
        public RoamingApplicationDataStore()
            : this("RoamingConfiguration")
        { }

        /// <summary>
        /// Initializes a new <see cref="RoamingApplicationDataStore"/> instance.
        /// </summary>
        /// <param name="containerName">The name of the container in which the configuration data is to be stored.</param>
        public RoamingApplicationDataStore(string containerName)
            : base(ApplicationData.Current.RoamingSettings.CreateContainer(containerName, ApplicationDataCreateDisposition.Always))
        { }

        #endregion
    }
}