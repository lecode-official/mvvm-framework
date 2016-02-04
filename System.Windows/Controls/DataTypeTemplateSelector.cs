
#region Using Directives

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Markup;

#endregion

namespace System.Windows.Controls
{
    /// <summary>
    /// Represents a generic data template selector.
    /// </summary>
    [ContentProperty(nameof(Templates))]
    public class DataTypeTemplateSelector : DataTemplateSelector
    {
        #region Public Properties

        /// <summary>
        /// Contains all templates.
        /// </summary>
        private Collection<DataTemplate> templates = new Collection<DataTemplate>();

        /// <summary>
        /// Gets all templates.
        /// </summary>
        public Collection<DataTemplate> Templates
        {
            get
            {
                return this.templates;
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Selects a template depending on the type of the object,
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>Returns the selected data template if one is found, otherwise null is returned.</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container) => item == null ? null : this.Templates.Where(template => template.DataType as Type == item.GetType()).FirstOrDefault();

        #endregion
    }
}