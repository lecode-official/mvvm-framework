
#region Using Directives

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Convertes boolean values into their negated boolean value (<c>true</c> => <c>false</c>, <c>false</c> => <c>true</c>).
    /// </summary>
    public class InvertedBooleanConverter : IValueConverter
    {
        #region IValueConverter Implementation

        /// <summary>
        /// Convertes the boolean value parameter to its inverted boolean value. <c>true</c> converts to <c>false</c> and <c>false</c> converts to <c>true</c>.
        /// </summary>
        /// <param name="value">The boolean value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always <see cref="bool"/>.</param>
        /// <param name="parameter">A parameter for the conversion. Not used in this converter, so it should always be null.</param>
        /// <param name="language">The name of the language, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns <c>false</c> if the value is <c>true</c> and <c>true</c> if the value is <c>false</c>.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            return (bool)value ? false : true;
        }

        /// <summary>
        /// Convertes the boolean value parameter to its inverted boolean value. <c>true</c> converts to <c>false</c> and <c>false</c> converts to <c>true</c>.
        /// </summary>
        /// <param name="value">The visibility value that is to be converted.</param>
        /// <param name="targetType">The type to which the value is to be converted. In this case it is always <see cref="bool"/>.</param>
        /// <param name="parameter">A parameter for the conversion. Not used in this converter, so it should always be null.</param>
        /// <param name="language">The name of the language, so that parsing can be adjusted to cultural conventions.</param>
        /// <returns>Returns <c>false</c> if the value is <c>true</c> and <c>true</c> if the value is <c>false</c>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language) => (bool)value ? false : true;

        #endregion
    }
}