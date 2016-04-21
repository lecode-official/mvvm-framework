
#region Using Directives

using System;
using System.Threading.Tasks;
using Windows.Storage;

#endregion

namespace Windows.Mvvm.Configuration.Stores
{
    /// <summary>
    /// Represents a store that uses the application data to save the configuration data.
    /// </summary>
    public class ApplicationDataStore : IStore
    {
        #region Constructors
        
        /// <summary>
        /// Initializes a new <see cref="ApplicationDataStore"/> instance.
        /// </summary>
        /// <param name="applicationDataContainer">The application data container in which the configuration is to be stored.</param>
        public ApplicationDataStore(ApplicationDataContainer applicationDataContainer)
        {
            // Stores the application data container and name of the container in which the coinfiguration is to be stored
            this.ApplicationDataContainer = applicationDataContainer;

            // Subscribes to the data changed event of the application data, when the application data changes, then the configuration changed event is raised
            ApplicationData.Current.DataChanged += (sender, e) => this.ConfigurationChanged?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the application data container in which the configuration is to be stored.
        /// </summary>
        public ApplicationDataContainer ApplicationDataContainer { get; private set; }
        
        #endregion
        
        #region IStore Implementation

        /// <summary>
        /// Loads the stores configuration data and returns them as serialized string.
        /// </summary>
        /// <returns>Returns the loaded data.</returns>
        public Task<string> LoadAsync()
        {
            // Gets the configuration data
            if (!this.ApplicationDataContainer.Values.ContainsKey("Configuration"))
                return Task.FromResult(null as string);
            return Task.FromResult(this.ApplicationDataContainer.Values["Configuration"] as string);
        }

        /// <summary>
        /// Stores the configuration data.
        /// </summary>
        /// <param name="content">The serializes configuration data.</param>
        public Task StoreAsync(string content)
        {
            // Stores the new content
            this.ApplicationDataContainer.Values["Configuration"] = content;
            ApplicationData.Current.SignalDataChanged();
            
            // Returns an empty task because no asynchronous operation has been performed
            return Task.FromResult(0);
        }

        /// <summary>
        /// An event, which is raised, when the configuration has possibly changed.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        #endregion
    }
}