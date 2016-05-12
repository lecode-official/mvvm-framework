
#region Using Directives

using System.Globalization;
using System.Linq;
using System.Windows.Data;

#endregion

namespace System.Windows.Controls
{
    /// <summary>
    /// Converts an enum to the visible value, if the parameter has the same value as the enum value.
    /// </summary>
    public class EnumToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Implementation

        /// <summary>
        /// Converts the value parameter to a visibility value.
        /// </summary>
        /// <param name="value">The value of an enum that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always visibility.</param>
        /// <param name="parameter">A parameter that determines the value to which the value has to be equal to return visible.</param>
        /// <param name="culture">The culture information of the current culture, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns true if the value parameter has the same value as the parameter parameter.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Checks if the value provided is null, if so then tell the converter that the value is unset
            if (value == null || !(value is Enum))
                return DependencyProperty.UnsetValue;

            // Gets the parameter and check if it is null, if so then tells the converter, that the value is unset
            string enumValueName = parameter as string;
            if (enumValueName == null)
                return DependencyProperty.UnsetValue;

            // Checks if the enum type, from which is to be converted defines the value that is to be converted to visibility, if not, then tells the converter, that the value is unset
            if (!Enum.IsDefined(value.GetType(), value))
                return DependencyProperty.UnsetValue;

            // Converts the parameter value to the enum type
            object parameterEnumValue = Enum.Parse(value.GetType(), enumValueName);

            // If the parameter equals the value that is to be converted, then returns visible, otherwise hidden is returned
            return parameterEnumValue.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a visibility value back to an enum value.
        /// </summary>
        /// <param name="value">The value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is an enum type.</param>
        /// <param name="parameter">A parameter whose enum value is returned.</param>
        /// <param name="culture">The culture information of the current culture, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns the enum value of the parameter.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Determines whether the target type is nullable
            bool isNullable = targetType.Name == typeof(Nullable<>).Name;
            Type enumerationType = isNullable ? targetType.GenericTypeArguments.FirstOrDefault() : targetType;

            // Gets the parameter and check if it is null, if so then the default value for the target type is returned
            string enumValueName = parameter as string;
            if (enumValueName == null)
                throw new ArgumentException("The parameter was not set.");

            // Converts the parameter to its target value
            if ((Visibility)value == Visibility.Visible)
                return Enum.Parse(enumerationType, enumValueName);
            else
                return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}