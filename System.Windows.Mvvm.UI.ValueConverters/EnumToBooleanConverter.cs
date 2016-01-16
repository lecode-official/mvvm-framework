
#region Using Directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace System.Windows.Mvvm.UI.ValueConverters
{
    /// <summary>
    /// Converts an enum to a boolean value, if the parameter has the same value as the enum value.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        #region IValueConverter Implementation

        /// <summary>
        /// Convertes the value parameter to a boolean value.
        /// </summary>
        /// <param name="value">The value of an enum that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always boolean.</param>
        /// <param name="parameter">A paramter that determines the value to which the value has to be equal to return true.</param>
        /// <param name="culture">The culture information of the current culture, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns true if the value parameter has the same value as the paramter parameter.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the value provided is null, if so then tell the converter that the value is unset
            if (value == null)
                return DependencyProperty.UnsetValue;

            // Get the parameter and check if it is null, if so then tell the converter, that the value is unset
            string enumValueName = parameter as string;
            if (enumValueName == null)
                return DependencyProperty.UnsetValue;

            // Check if the enum type, from which is to be converted defines the value that is to be converted to boolean, if not, then tell the converter, that the value is unset
            if (!Enum.IsDefined(value.GetType(), value))
                return DependencyProperty.UnsetValue;

            // Convert the parameter value to the enum type
            object parameterEnumValue = Enum.Parse(value.GetType(), enumValueName);

            // If the parameter equals the value that is to be converted, then return true, otherwise false is returned
            if (parameterEnumValue.Equals(value))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Converts a boolean value back to an enum value.
        /// </summary>
        /// <param name="value">The value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is an enum type.</param>
        /// <param name="parameter">A paramter whose enum value is returned.</param>
        /// <param name="culture">The culture information of the current culture, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns the enum value of the parameter.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the value provided is null, if so then tell the converter that the value is unset
            if (value == null)
                return DependencyProperty.UnsetValue;

            // Get the parameter and check if it is null, if so then tell the converter, that the value is unset
            string enumValueName = parameter as string;
            if (enumValueName == null)
                return DependencyProperty.UnsetValue;

            // Then the parameter is converted to the enum type and returned
            return (bool) value ? Enum.Parse(targetType, enumValueName) : DependencyProperty.UnsetValue;
        }

        #endregion
    }
}