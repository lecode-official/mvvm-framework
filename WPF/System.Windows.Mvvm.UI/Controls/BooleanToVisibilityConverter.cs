﻿
#region Using Directives

using System.Globalization;
using System.Windows.Data;

#endregion

namespace System.Windows.Controls
{
    /// <summary>
    /// Convertes boolean values into visibility values and vice versa. Unlike the standard boolean to visibility converter it does not return <c>DependencyProperty.UnsetValue</c>, when the input value is <c>null</c>, but it returns
    /// <c>Visibility.Collapsed</c>. This is very helpful, when the binding is asynchronous, because in an asynchronous binding, the value can be undetermined (i.e. <c>null</c>) for a short amount of time, which results in the control to
    /// be displayed for a short period of time, even if the binding value eventually evaluates to <c>false</c>.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Implementation

        /// <summary>
        /// Convertes the boolean value parameter to a visibility value. True converts to visible and false converts to collapsed.
        /// </summary>
        /// <param name="value">The boolean value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always <see cref="Visibility"/>.</param>
        /// <param name="parameter">A parameter for the conversion. Not used in this converter, so it should always be null.</param>
        /// <param name="culture">The culture information of the current culture, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns <see cref="Visibility.Visible"/> if the value is false and <see cref="Visibility.Collapsed"/> if the value is true.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Convertes the visibility value parameter to a boolean value. Visible converts to true and collapsed converts to false.
        /// </summary>
        /// <param name="value">The visibility value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always <see cref="bool"/>.</param>
        /// <param name="parameter">A parameter for the conversion. Not used in this converter, so it should always be null.</param>
        /// <param name="culture">The culture information of the current culture, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns false if the value is <see cref="Visibility.Visible"/> and true if the value is <see cref="Visibility.Collapsed"/>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNullable = targetType == typeof(Nullable<bool>);
            if (value is Visibility && (Visibility)value == Visibility.Visible)
                return true;
            else
                return isNullable ? (Nullable<bool>)null : false;
        }

        #endregion
    }
}