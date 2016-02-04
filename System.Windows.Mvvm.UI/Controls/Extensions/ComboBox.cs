
#region Using Directives

using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;

#endregion

namespace System.Windows.Controls.Extensions
{
    /// <summary>
    /// Represents an attached property for combo box validation and input.
    /// </summary>
    public static class ComboBox
    {
        #region Attached Properties

        /// <summary>
        /// Contains the dependency property that contains a regex that validates the input.
        /// </summary>
        public static readonly DependencyProperty ValueRegexProperty = DependencyProperty.RegisterAttached("ValueRegex", typeof(string), typeof(ComboBox), new UIPropertyMetadata("*", (sender, e) =>
        {
            // Gets the combo box to which the property is attached
            System.Windows.Controls.ComboBox comboBox = sender as System.Windows.Controls.ComboBox;
            if (comboBox == null)
                return;

            string oldText = comboBox.Text;
            comboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler((x, y) =>
            {
                // Checks if the selection is empty
                if (comboBox.Text == null)
                    return;

                // Validates via regex
                if (!Regex.IsMatch(comboBox.Text, e.NewValue as string))
                {
                    comboBox.Text = oldText;
                    return;
                }

                // Sets the text as new old text value
                oldText = comboBox.Text;
            }));
        }));

        /// <summary>
        /// Gets the value of the <see cref="ValueRegexProperty"/> attached property.
        /// </summary>
        /// <param name="frameworkElement">The framework element which is used to get the value.</param>
        /// <returns>Returns the value of the attached property.</returns>
        public static string GetValueRegex(DependencyObject frameworkElement)
        {
            // Validates the argument
            if (frameworkElement == null)
                throw new ArgumentNullException(nameof(frameworkElement));

            object objectValue = frameworkElement.GetValue(ComboBox.ValueRegexProperty);
            return objectValue == DependencyProperty.UnsetValue ? "*" : (string)objectValue;
        }

        /// <summary>
        /// Sets the value of the <see cref="ValueRegexProperty"/> attached property.
        /// </summary>
        /// <param name="frameworkElement">The framework element which is used to set the value.</param>
        /// <param name="value">The value that is to be set.</param>
        public static void SetValueRegex(DependencyObject frameworkElement, string value)
        {
            // Validates the argument
            if (frameworkElement == null)
                throw new ArgumentNullException(nameof(frameworkElement));

            frameworkElement.SetValue(ComboBox.ValueRegexProperty, value);
        }

        #endregion
    }
}