
#region Using Directives

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Converts a type to the visible value, if the parameter has the same value as the type.
    /// </summary>
    public class TypeToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Implementation

        /// <summary>
        /// Converts the value parameter to a visibility value.
        /// </summary>
        /// <param name="value">The value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always visibility.</param>
        /// <param name="parameter">A parameter that determines the value to which the value has to be equal to return visible.</param>
        /// <param name="language">The name of the language, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns true if the value parameter has the same value as the parameter parameter.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Checks if the value provided is null, if so then tell the converter that the value is unset
            if (value == null)
                return DependencyProperty.UnsetValue;

            // Gets the value as type and check if it is null, if so then tells the converter, that the type of the object is used
            Type typeValue = value as Type;
            if (typeValue == null)
                typeValue = value.GetType();

            // Gets the parameter and check if it is null, if so then tells the converter, that the value is unset
            Type parameterValue = parameter as Type;
            if (parameterValue == null)
                return DependencyProperty.UnsetValue;

            // If the parameter equals the value that is to be converted, then returns visible, otherwise hidden is returned
            return typeValue == parameterValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a visibility value back to a type.
        /// </summary>
        /// <param name="value">The value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is type.</param>
        /// <param name="parameter">A parameter whose enum value is returned.</param>
        /// <param name="language">The name of the language, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns the type of the parameter.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Checks if the value provided is null, if so then tell the converter that the value is unset
            if (value == null)
                return DependencyProperty.UnsetValue;

            // Gets the parameter and check if it is null, if so then tells the converter, that the value is unset
            Type parameterValue = parameter as Type;
            if (parameterValue == null)
                return DependencyProperty.UnsetValue;

            // Then the parameter is converted to the type and returned
            return (Visibility)value == Visibility.Visible ? parameterValue : DependencyProperty.UnsetValue;
        }

        #endregion
    }
}