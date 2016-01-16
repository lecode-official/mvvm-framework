
#region Using Directives

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

#endregion

namespace System.Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Represents an attached property for text box validation and input.
    /// </summary>
    public static class TextBox
    {
        #region Attached Properties

        /// <summary>
        /// Contains the dependency property that contains a regex that validates the input.
        /// </summary>
        public static readonly DependencyProperty ValueRegexProperty = DependencyProperty.RegisterAttached("ValueRegex", typeof(string), typeof(TextBox), new UIPropertyMetadata("*", (sender, e) =>
        {
            // Gets the text box to which the property is attached
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null)
                return;

            string oldText = textBox.Text;
            int oldCaretIndex = textBox.CaretIndex;
            textBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler((x, y) =>
            {
                // Checks if the selection is empty
                if (textBox.Text == null)
                    return;

                // Validates via regex
                if (!Regex.IsMatch(textBox.Text, e.NewValue as string))
                {
                    // Sets the old text
                    int temporaryCaretIndex = oldCaretIndex;
                    textBox.Text = oldText;
                    oldCaretIndex = temporaryCaretIndex;

                    // Sets the old caret
                    textBox.CaretIndex = Math.Min(oldText.Length, oldCaretIndex);
                    return;
                }

                // Sets the text as new old text value
                oldText = textBox.Text;
                oldCaretIndex = textBox.CaretIndex;
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
                throw new ArgumentNullException("frameworkElement");

            object objectValue = frameworkElement.GetValue(TextBox.ValueRegexProperty);
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
                throw new ArgumentNullException("frameworkElement");

            frameworkElement.SetValue(TextBox.ValueRegexProperty, value);
        }

        #endregion
    }
}