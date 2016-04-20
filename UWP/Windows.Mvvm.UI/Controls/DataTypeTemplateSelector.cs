
#region Using Directives

using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

#endregion

namespace Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Represents a generic data template selector.
    /// </summary>
    [ContentProperty(Name = nameof(Templates))]
    public class DataTypeTemplateSelector : DataTemplateSelector
    {
        #region Public Properties

        /// <summary>
        /// Contains all templates.
        /// </summary>
        private Collection<TypedDataTemplate> templates = new Collection<TypedDataTemplate>();

        /// <summary>
        /// Gets all templates.
        /// </summary>
        public Collection<TypedDataTemplate> Templates
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
        /// <returns>Returns the selected data template if one is found, otherwise null is returned.</returns>
        protected override DataTemplate SelectTemplateCore(object item) => item == null ? null : this.Templates.Where(template => template.DataType == item.GetType()).FirstOrDefault();

        /// <summary>
        /// Selects a template depending on the type of the object,
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>Returns the selected data template if one is found, otherwise null is returned.</returns>
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => item == null ? null : this.Templates.Where(template => template.DataType == item.GetType()).FirstOrDefault();

        #endregion
    }
}