
#region Using Directives

using System.Globalization;
using System.Windows.Data;

#endregion

namespace System.Windows.Controls
{
    /// <summary>
    /// Convertes boolean values into visibility values and vice versa. Unlike the standard boolean to visibility converter it inverts the boolean values, which means, that false converts to visible and true converts to collapsed.
    /// </summary>
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Implementation

        /// <summary>
        /// Convertes the boolean value parameter to a visibility value. True converts to collapsed and false converts to visible.
        /// </summary>
        /// <param name="value">The boolean value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always <see cref="Visibility"/>.</param>
        /// <param name="parameter">A parameter for the conversion. Not used in this converter, so it should always be null.</param>
        /// <param name="culture">The culture information of the current culture, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns <see cref="Visible"/> if the value is false and <see cref="Collapsed"/> if the value is true.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Visible;
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Convertes the visibility value parameter to a boolean value. Collapsed converts to true and visible converts to false.
        /// </summary>
        /// <param name="value">The visibility value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always <see cref="bool"/>.</param>
        /// <param name="parameter">A parameter for the conversion. Not used in this converter, so it should always be null.</param>
        /// <param name="culture">The culture information of the current culture, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns false if the value is <see cref="Visible"/> and true if the value is <see cref="Collapsed"/>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (Visibility)value == Visibility.Visible ? false : true;

        #endregion
    }
}