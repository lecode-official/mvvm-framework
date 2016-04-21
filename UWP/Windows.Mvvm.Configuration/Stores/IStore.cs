
#region Using Directives

using System.Threading.Tasks;

#endregion

namespace Windows.Mvvm.Configuration.Stores
{
    /// <summary>
    /// Represents the interface for the storage method of the configuration data.
    /// </summary>
    public interface IStore
    {
        #region Methods

        /// <summary>
        /// Loads the stores configuration data and returns them as serialized string.
        /// </summary>
        /// <returns>Returns the loaded data.</returns>
        Task<string> LoadAsync();

        /// <summary>
        /// Stores the configuration data.
        /// </summary>
        /// <param name="content">The serializes configuration data.</param>
        Task StoreAsync(string content);

        #endregion
    }
}