
#region Using Directives

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

#endregion

namespace System.Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Represents a generic data template selector.
    /// </summary>
    [ContentProperty("Templates")]
    public class DataTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        #region Public Properties

        /// <summary>
        /// Contains all templates.
        /// </summary>
        private Collection<Template> templates = new Collection<Template>();

        /// <summary>
        /// Gets all templates.
        /// </summary>
        public Collection<Template> Templates
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
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Checks if an item is given
            if (item == null)
                return null;

            // Returns the template for the item that is given
            return this.Templates.Where(template => template.Type == item.GetType()).Select(template => template.DataTemplate).FirstOrDefault();
        }

        #endregion
    }
}