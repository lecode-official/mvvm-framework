
#region Using Directives

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;

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
        /// <param name="fileName">The name of the configuration file.</param>
        public ConfigContext(string fileName)
        {
            this.fileName = fileName;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the name of the configuration file.
        /// </summary>
        private string fileName;

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads all data from the configuration file into the context.
        /// </summary>
        public virtual async Task LoadAsync()
        {
            if (!File.Exists(this.fileName))
                await this.SaveChangesAsync();
            await Task.Run(() => JsonConvert.PopulateObject(File.ReadAllText(this.fileName), this));
        }

        /// <summary>
        /// Saves all changes made to the context to the configuration file.
        /// </summary>
        public virtual Task SaveChangesAsync()
        {
            return Task.Run(() => File.WriteAllText(this.fileName, JsonConvert.SerializeObject(this, new JsonSerializerSettings { ContractResolver = new WritablePropertiesOnlyResolver(), TypeNameHandling = TypeNameHandling.Auto, TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple })));
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