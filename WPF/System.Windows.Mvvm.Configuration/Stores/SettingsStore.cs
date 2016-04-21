
#region Using Directives

using System.Configuration;
using System.Threading.Tasks;

#endregion

namespace System.Windows.Mvvm.Configuration.Stores
{
    /// <summary>
    /// Represents a store that uses the settings to save the configuration data.
    /// </summary>
    public class SettingsStore : IStore
    {
        #region Constructors
        
        /// <summary>
        /// Initializes a new <see cref="SettingsStore"/> instance.
        /// </summary>
        /// <param name="fileName">The settings that are used to store the configuration data.</param>
        public SettingsStore(ApplicationSettingsBase settings)
            : this(settings, "Configuration")
        { }

        /// <summary>
        /// Initializes a new <see cref="SettingsStore"/> instance.
        /// </summary>
        /// <param name="fileName">The settings that are used to store the configuration data.</param>
        /// <param name="propertyName">The name of the property which is used to store the settings.</param>
        public SettingsStore(ApplicationSettingsBase settings, string propertyName)
        {
            this.Settings = settings;
            this.PropertyName = propertyName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the settings that are used to store the configuration data.
        /// </summary>
        public ApplicationSettingsBase Settings { get; private set; }

        /// <summary>
        /// Gets or sets the name for the property which is used to store the settings.
        /// </summary>
        public string PropertyName { get; set; }

        #endregion

        #region IStore Implementation

        /// <summary>
        /// Loads the stores configuration data and returns them as serialized string.
        /// </summary>
        /// <returns>Returns the loaded data.</returns>
        public Task<string> LoadAsync()
        {
            return Task.FromResult(this.Settings[this.PropertyName] as string);
        }

        /// <summary>
        /// Stores the configuration data.
        /// </summary>
        /// <param name="content">The serializes configuration data.</param>
        public Task StoreAsync(string content)
        {
            // Saves the settings
            this.Settings[this.PropertyName] = content;
            this.Settings.Save();
            
            // Raises the configuration changed event
            this.ConfigurationChanged?.Invoke(this, new EventArgs());

            // Returns an empty task as no asynchronous operation has been performed
            return Task.FromResult(0);
        }

        /// <summary>
        /// An event, which is raised, when the configuration has possibly changed.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        #endregion
    }
}