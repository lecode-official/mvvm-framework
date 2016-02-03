
#region Using Directives

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using System.Windows.Mvvm.Configuration.Stores;

#endregion

namespace System.Windows.Mvvm.Configuration
{
    /// <summary>
    /// Represents the base class for a context which can load and store configuration data.
    /// </summary>
    public class ConfigContext
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ConfigContext"/> instance.
        /// </summary>
        public ConfigContext()
            : this(null)
        { }

        /// <summary>
        /// Initializes a new <see cref="ConfigContext"/> instance.
        /// </summary>
        /// <param name="store">The store that is used by the configuration context to load and store data.</param>
        public ConfigContext(IStore store)
        {
            this.Store = store;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the store that is used by the configuration context to load and store data.
        /// </summary>
        public IStore Store { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads all data from the configuration file into the context.
        /// </summary>
        public virtual async Task LoadAsync()
        {
            // If no store is set, the data is not persisted
            if (this.Store == null)
                return;

            // Gets the data from the store
            string data = await this.Store.LoadAsync();
            if (string.IsNullOrWhiteSpace(data))
                return;

            // Deserializes the data
            await Task.Run(() => JsonConvert.PopulateObject(data, this));
        }

        /// <summary>
        /// Saves all changes made to the context to the configuration file.
        /// </summary>
        public virtual async Task SaveChangesAsync()
        {
            // If no store is set, the data is not persisted
            if (this.Store == null)
                return;

            // Serializes the data
            string data = await Task.Run(() => JsonConvert.SerializeObject(this, new JsonSerializerSettings { ContractResolver = new WritablePropertiesOnlyResolver(), TypeNameHandling = TypeNameHandling.Auto, TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple }));

            // Stores the data
            await this.Store.StoreAsync(data);
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// Represents a data contract resolver that does not serialize read-only properties.
        /// </summary>
        private class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            #region Overridden Methods

            /// <summary>
            /// Generates a list of properties that are to be serialized.
            /// </summary>
            /// <param name="type">The type that is to be serialized.</param>
            /// <param name="memberSerialization">The member serialization.</param>
            /// <returns>Returns the list of created properties.</returns>
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization).Where(p => p.Writable).ToList();
            }

            #endregion
        }

        #endregion
    }
}