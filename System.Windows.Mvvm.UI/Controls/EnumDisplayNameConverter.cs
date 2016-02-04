
#region Using Directives

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Windows.Data;

#endregion

namespace System.Windows.Controls
{
    /// <summary>
    /// Represents a value converter that is used to get the localized name of an enumeration value.
    /// </summary>
    public class EnumDisplayNameConverter : IValueConverter
    {
        #region IValueConverter Implementation

        /// <summary>
        /// Convertes the value parameter from an enumeration value to the localized name of the enumeration value.
        /// </summary>
        /// <param name="value">The value of an enum that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always string.</param>
        /// <param name="parameter">A paramter that is not used in this implementation.</param>
        /// <param name="culture">The culture information of the current culture, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns the localized name of the enumeration value in the value parameter or an empty string if it either is not an enumeration value or if the enumeration value has no localization.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Checks if the value provided is null, if so then tells the converter that the value is unset
            if (value == null)
                return DependencyProperty.UnsetValue;

            // Checks if the enum type, from which is to be converted, defines the value that is to be converted to its localized name, if not, then tells the converter, that the value is unset
            if (!(value is Enum) || !Enum.IsDefined(value.GetType(), value))
                return DependencyProperty.UnsetValue;

            // Checks if the enumeration value has a display name attribute, if not then tells the converter that the value is unset, otherwise the localized display name is returned
            DisplayAttribute displayAttribute = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DisplayAttribute), false).OfType<DisplayAttribute>().FirstOrDefault();
            if (displayAttribute == null)
                return DependencyProperty.UnsetValue;

            // Creates a new resource manager in order to get the localized display name for the enumeration value
            ResourceManager resourceManager = new ResourceManager(displayAttribute.ResourceType);

            // Retrieves the localized display name and returns it if it exists, if it does not exist, then just the resource name is returned
            string name = resourceManager.GetString(displayAttribute.Name);
            return string.IsNullOrEmpty(name) ? displayAttribute.Name : name;
        }

        /// <summary>
        /// Converting a localized name back to its enumeration value is impossible. Therefore this method is not implemented.
        /// </summary>
        /// <param name="value">Not supported in this implementation.</param>
        /// <param name="targetType">Not supported in this implementation.</param>
        /// <param name="parameter">Not supported in this implementation.</param>
        /// <param name="culture">Not supported in this implementation.</param>
        /// <exception cref="NotImplementedException">Since this functionality is unsupported, the method always throws a <see cref="NotImplementedException"/> exception.</exception>
        /// <returns>Returns nothing, since an <see cref="NotImplementedException"/> is raised.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}