
#region Using Directives

using System.IO;
using System.Threading.Tasks;

#endregion

namespace System.Windows.Mvvm.Configuration.Stores
{
    /// <summary>
    /// Represents a store that uses the file system to save the configuration data.
    /// </summary>
    public class FileStore : IStore
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="FileStore"/> instance.
        /// </summary>
        /// <param name="fileName">The name of the file where the configuration data is stored.</param>
        public FileStore(string fileName)
        {
            this.FileName = fileName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the file name of the file that is used to store the configuration data.
        /// </summary>
        public string FileName { get; private set; }

        #endregion

        #region IStore Implementation

        /// <summary>
        /// Loads the stores configuration data and returns them as serialized string.
        /// </summary>
        /// <returns>Returns the loaded data.</returns>
        public Task<string> LoadAsync()
        {
            if (!File.Exists(this.FileName))
                return Task.FromResult<string>(null);
            return Task.Run(() => File.ReadAllText(this.FileName));
        }

        /// <summary>
        /// Stores the configuration data.
        /// </summary>
        /// <param name="content">The serializes configuration data.</param>
        public async Task StoreAsync(string content)
        {
            // Writes the configuration data to the file
            await Task.Run(() => File.WriteAllText(this.FileName, content));

            // Raises the configuration changed event
            this.ConfigurationChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// An event, which is raised, when the configuration has possibly changed.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        #endregion
    }
}