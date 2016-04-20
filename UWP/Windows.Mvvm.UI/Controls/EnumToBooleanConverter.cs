
#region Using Directives

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Converts an enum to a boolean value, if the parameter has the same value as the enum value.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        #region IValueConverter Implementation

        /// <summary>
        /// Converts the value parameter to a boolean value.
        /// </summary>
        /// <param name="value">The value of an enum that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always boolean.</param>
        /// <param name="parameter">A parameter that determines the value to which the value has to be equal to return true.</param>
        /// <param name="language">The name of the language, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns true if the value parameter has the same value as the parameter parameter.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Checks if the value provided is null, if so then tell the converter that the value is unset
            if (value == null || !(value is Enum))
                return DependencyProperty.UnsetValue;

            // Gets the parameter and check if it is null, if so then tells the converter, that the value is unset
            string enumValueName = parameter as string;
            if (enumValueName == null)
                return DependencyProperty.UnsetValue;

            // Checks if the enum type, from which is to be converted defines the value that is to be converted to boolean, if not, then tell the converter, that the value is unset
            if (!Enum.IsDefined(value.GetType(), value))
                return DependencyProperty.UnsetValue;

            // Converts the parameter value to the enum type
            object parameterEnumValue = Enum.Parse(value.GetType(), enumValueName);

            // If the parameter equals the value that is to be converted, then returns true, otherwise false is returned
            return parameterEnumValue.Equals(value);
        }

        /// <summary>
        /// Converts a boolean value back to an enum value.
        /// </summary>
        /// <param name="value">The value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is an enum type.</param>
        /// <param name="parameter">A parameter whose enum value is returned.</param>
        /// <param name="language">The name of the language, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns the enum value of the parameter.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Checks if the value provided is null, if so then tell the converter that the value is unset
            if (value == null)
                return DependencyProperty.UnsetValue;

            // Gets the parameter and check if it is null, if so then tell the converter, that the value is unset
            string enumValueName = parameter as string;
            if (enumValueName == null)
                return DependencyProperty.UnsetValue;

            // Then the parameter is converted to the enum type and returned
            return (bool)value ? Enum.Parse(targetType, enumValueName) : DependencyProperty.UnsetValue;
        }

        #endregion
    }
}