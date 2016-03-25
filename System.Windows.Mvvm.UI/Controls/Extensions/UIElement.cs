
#region Using Directives

using System.Windows.Input;

#endregion

namespace System.Windows.Controls.Extensions
{
    /// <summary>
    /// Represents a collection of attached properties for enhanced UI element features.
    /// </summary>
    public static class UIElement
    {
        #region Attached Properties

        /// <summary>
        /// Contains the dependency property that contains a list of input bindings, which can be set via style.
        /// </summary>
        public static readonly DependencyProperty InputBindingsProperty = DependencyProperty.RegisterAttached("InputBindings", typeof(InputBindingCollection), typeof(UIElement), new PropertyMetadata(null, (sender, e) =>
        {
            // Gets the UI element to which the property is attached
            System.Windows.UIElement uiElement = sender as System.Windows.UIElement;
            if (uiElement == null)
                return;

            // Clears the old input bindings
            uiElement.InputBindings.Clear();

            // Adds the new ones
            if (e.NewValue != null)
                uiElement.InputBindings.AddRange(e.NewValue as InputBindingCollection);
        }));

        /// <summary>
        /// Gets the value of the <see cref="InputBindingsProperty"/> attached property.
        /// </summary>
        /// <param name="uiElement">The UI element which is used to get the value.</param>
        /// <returns>Returns the value of the attached property.</returns>
        public static ICommand GetInputBindings(DependencyObject uiElement)
        {
            // Validates the argument
            if (uiElement == null)
                throw new ArgumentNullException(nameof(uiElement));

            return (ICommand)uiElement.GetValue(UIElement.InputBindingsProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="InputBindingsProperty"/> attached property.
        /// </summary>
        /// <param name="uiElement">The UI element which is used to set the value.</param>
        /// <param name="value">The value that is to be set.</param>
        public static void SetInputBindings(DependencyObject uiElement, ICommand value)
        {
            // Validates the argument
            if (uiElement == null)
                throw new ArgumentNullException(nameof(uiElement));

            uiElement.SetValue(UIElement.InputBindingsProperty, value);
        }

        #endregion
    }
}