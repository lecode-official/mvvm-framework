
#region Using Directives

using System;
using System.Windows;
using System.Windows.Input;

#endregion

namespace System.Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Represents a set of attached properties for controls.
    /// </summary>
    public static class Control
    {
        #region Attached Properties

        /// <summary>
        /// Contains the attached property, which determinse whether the control to which the property is attached currently has the focus.
        /// </summary>
        public static readonly DependencyProperty HasFocusProperty = DependencyProperty.RegisterAttached(
            "HasFocus", typeof(bool), typeof(Control), new UIPropertyMetadata(false, delegate (DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                // Validates that the sender is a control
                System.Windows.Controls.Control control = sender as System.Windows.Controls.Control;
                if (control == null)
                    return;

                // If the parameter is true, the control is focused, otherwise the focus is move to the next control
                if (e.NewValue is bool && (bool)e.NewValue)
                    control.Focus();
                else
                    control.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }));

        /// <summary>
        /// Gets a value that determines whether the control to which the <see cref="HasFocusProperty"/> attached property has been attached, currently has the focus.
        /// </summary>
        /// <param name="control">The control for which is to be determined whether is has the focus or not.</param>
        /// <returns>Returns <c>true</c> if the specified control has the focus and <c>false</c> otherwise.</returns>
        public static bool GetHasFocus(DependencyObject control)
        {
            // Validates the parameters
            if (control == null)
                throw new ArgumentNullException("control");

            // Gets the value of the attached property
            return (bool)control.GetValue(Control.HasFocusProperty);
        }

        /// <summary>
        /// Sets a value that determines whether the control to which the <see cref="HasFocusProperty"/> attached property has been attached, currently has the focus.
        /// </summary>
        /// <param name="control">The control whose <see cref="HasFocusProperty"/> is to be set.</param>
        /// <param name="value">The new value for the <see cref="HasFocusProperty"/>. If the value is <c>true</c>, then the control is focused, otherwise the focus is move to the next control.</param>
        public static void SetHasFocus(DependencyObject control, bool value)
        {
            // Validates the parameters
            if (control == null)
                throw new ArgumentNullException("control");

            // Sets the value of the attached property
            control.SetValue(Control.HasFocusProperty, value);
        }
        
        /// <summary>
        /// Contains the dependency property that contains the a value that determines whether the content is valid.
        /// </summary>
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.RegisterAttached("IsValid", typeof(bool), typeof(Control), new PropertyMetadata(false, (sender, e) =>
        {
            // Sets the new state
            VisualStateManager.GoToState(sender as FrameworkElement, (bool)e.NewValue ? "IsValid" : "IsInvalid", true);
        }));

        /// <summary>
        /// Gets the value of the <see cref="IsValidProperty"/> attached property.
        /// </summary>
        /// <param name="frameworkElement">The framework element which is used to get the value.</param>
        /// <returns>Returns the value of the attached property.</returns>
        public static bool GetIsValid(DependencyObject frameworkElement)
        {
            // Validates the argument
            if (frameworkElement == null)
                throw new ArgumentNullException("frameworkElement");

            return (bool)frameworkElement.GetValue(Control.IsValidProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="IsValidProperty"/> attached property.
        /// </summary>
        /// <param name="frameworkElement">The framework element which is used to set the value.</param>
        /// <param name="value">The value that is to be set.</param>
        public static void SetIsValid(DependencyObject frameworkElement, bool value)
        {
            // Validates the argument
            if (frameworkElement == null)
                throw new ArgumentNullException("frameworkElement");

            frameworkElement.SetValue(Control.IsValidProperty, value);
        }

        #endregion
    }
}