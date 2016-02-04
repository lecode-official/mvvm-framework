
namespace System.Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Represents a set of attached properties for scroll viewers.
    /// </summary>
    public static class ScrollViewer
    {
        #region Attached Properties

        /// <summary>
        /// Contains the dependency property that is an indicator for horizontal mouse wheel scrolling.
        /// </summary>
        public static readonly DependencyProperty IsHorizontalScrollingEnabledProperty = DependencyProperty.RegisterAttached(
            "IsHorizontalScrollingEnabled", typeof(bool), typeof(ScrollViewer), new UIPropertyMetadata(false, delegate (DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                // Checks if the sender is a scroll viewer
                System.Windows.Controls.ScrollViewer scrollViewer = sender as System.Windows.Controls.ScrollViewer;
                if (scrollViewer == null)
                    return;

                // Registers for the horizontal scroll event
                if (e.NewValue is bool && (bool)e.NewValue)
                    scrollViewer.PreviewMouseWheel += ScrollViewer.OnScrollViewerPreviewMouseWheel;
                else
                    scrollViewer.PreviewMouseWheel -= ScrollViewer.OnScrollViewerPreviewMouseWheel;
            }));

        /// <summary>
        /// Gets the value of the <see cref="IsHorizontalScrollingEnabled"/> attached property.
        /// </summary>
        /// <param name="scrollViewer">The scroll viewer which is used to get the value.</param>
        /// <returns>Returns the value of the attached property.</returns>
        public static bool GetIsHorizontalScrollingEnabled(DependencyObject scrollViewer)
        {
            // Validates the parameters
            if (scrollViewer == null)
                throw new ArgumentNullException(nameof(scrollViewer));

            return (bool)scrollViewer.GetValue(ScrollViewer.IsHorizontalScrollingEnabledProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="IsHorizontalScrollingEnabled"/> attached property.
        /// </summary>
        /// <param name="scrollViewer">The scroll viewer which is used to set the value.</param>
        /// <param name="value">The value that is to be set.</param>
        public static void SetIsHorizontalScrollingEnabled(DependencyObject scrollViewer, bool value)
        {
            // Validates the parameters
            if (scrollViewer == null)
                throw new ArgumentNullException(nameof(scrollViewer));

            scrollViewer.SetValue(ScrollViewer.IsHorizontalScrollingEnabledProperty, value);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Is called when the mouse wheel is used.
        /// </summary>
        /// <param name="sender">The scroll viewer that sent the event.</param>
        /// <param name="e">The event parameters.</param>
        private static void OnScrollViewerPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Checks if the sender is a scroll viewer
            System.Windows.Controls.ScrollViewer scrollViewer = sender as System.Windows.Controls.ScrollViewer;
            if (scrollViewer == null)
                return;

            // Handles the mouse wheel
            if (e.Delta > 0)
                scrollViewer.LineLeft();
            else
                scrollViewer.LineRight();
            e.Handled = true;
        }

        #endregion
    }
}