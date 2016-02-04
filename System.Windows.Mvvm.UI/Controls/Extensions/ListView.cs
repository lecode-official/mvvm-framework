
#region Using Directives

using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

#endregion

namespace System.Windows.Controls.Extensions
{
    /// <summary>
    /// Represents an attached property for advanced list view features.
    /// </summary>
    public static class ListView
    {
        #region Attached Properties

        /// <summary>
        /// Contains the dependency property that contains the command for the activated item.
        /// </summary>
        public static readonly DependencyProperty ItemActivatedCommandProperty = DependencyProperty.RegisterAttached("ItemActivatedCommand", typeof(ICommand), typeof(ListView), new PropertyMetadata(null, (sender, e) =>
        {
            // Gets the list view to which the property is attached
            System.Windows.Controls.ListView listView = sender as System.Windows.Controls.ListView;
            if (listView == null)
                return;

            // Removes the old event handler
            listView.MouseDoubleClick -= ListView.ListViewDoubleMouseClick;

            // Registers the new event handler
            listView.MouseDoubleClick += ListView.ListViewDoubleMouseClick;
        }));

        /// <summary>
        /// Gets the value of the <see cref="ItemActivatedCommandProperty"/> attached property.
        /// </summary>
        /// <param name="frameworkElement">The framework element which is used to get the value.</param>
        /// <returns>Returns the value of the attached property.</returns>
        public static ICommand GetItemActivatedCommand(DependencyObject frameworkElement)
        {
            // Validates the argument
            if (frameworkElement == null)
                throw new ArgumentNullException(nameof(frameworkElement));

            return (ICommand)frameworkElement.GetValue(ListView.ItemActivatedCommandProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="ItemActivatedCommandProperty"/> attached property.
        /// </summary>
        /// <param name="frameworkElement">The framework element which is used to set the value.</param>
        /// <param name="value">The value that is to be set.</param>
        public static void SetItemActivatedCommand(DependencyObject frameworkElement, ICommand value)
        {
            // Validates the argument
            if (frameworkElement == null)
                throw new ArgumentNullException(nameof(frameworkElement));

            frameworkElement.SetValue(ListView.ItemActivatedCommandProperty, value);
        }

        /// <summary>
        /// Contains the dependency property that contains the command for the activated item.
        /// </summary>
        public static readonly DependencyProperty IsItemContextMenuProperty = DependencyProperty.RegisterAttached("IsItemContextMenu", typeof(bool), typeof(ListView), new PropertyMetadata(false, (sender, e) =>
        {
            // Gets the list view to which the property is attached
            System.Windows.Controls.ListView listView = sender as System.Windows.Controls.ListView;
            if (listView == null)
                return;

            // Removes the old event handler
            listView.PreviewMouseRightButtonDown -= ListView.ListViewPreviewMouseRightButtonDown;

            // Registers the new event handler
            if ((bool)e.NewValue)
                listView.PreviewMouseRightButtonDown += ListView.ListViewPreviewMouseRightButtonDown;
        }));

        /// <summary>
        /// Gets the value of the <see cref="IsItemContextMenuProperty"/> attached property.
        /// </summary>
        /// <param name="frameworkElement">The framework element which is used to get the value.</param>
        /// <returns>Returns the value of the attached property.</returns>
        public static bool GetIsItemContextMenu(DependencyObject frameworkElement)
        {
            // Validates the argument
            if (frameworkElement == null)
                throw new ArgumentNullException(nameof(frameworkElement));

            return (bool)frameworkElement.GetValue(ListView.IsItemContextMenuProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="IsItemContextMenuProperty"/> attached property.
        /// </summary>
        /// <param name="frameworkElement">The framework element which is used to set the value.</param>
        /// <param name="value">The value that is to be set.</param>
        public static void SetIsItemContextMenu(DependencyObject frameworkElement, bool value)
        {
            // Validates the argument
            if (frameworkElement == null)
                throw new ArgumentNullException(nameof(frameworkElement));

            frameworkElement.SetValue(ListView.IsItemContextMenuProperty, value);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Tries to find the visual parent of the depencency object.
        /// </summary>
        /// <param name="dependencyObject">The object that is the starting point for the search.</param>
        /// <returns>Returns the list view item that has been found.</returns>
        private static System.Windows.Controls.ListViewItem FindListViewItem(DependencyObject dependencyObject)
        {
            // Goes through the parent objects to find the list view item
            while (dependencyObject != null && !(dependencyObject is System.Windows.Controls.ListViewItem))
            {
                if (dependencyObject is System.Windows.Controls.ListView)
                    return null;

                if (!(dependencyObject is Visual))
                    dependencyObject = LogicalTreeHelper.GetParent(dependencyObject);
                else
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            // Returns the found list view item
            return dependencyObject as System.Windows.Controls.ListViewItem;
        }

        /// <summary>
        /// Tries to find the visual parent of the depencency object.
        /// </summary>
        /// <param name="dependencyObject">The object that is the starting point for the search.</param>
        /// <returns>Returns the list view item that has been found.</returns>
        private static System.Windows.Controls.ListView FindListView(DependencyObject dependencyObject)
        {
            // Goes through the parent objects to find the list view item
            while (dependencyObject != null && !(dependencyObject is System.Windows.Controls.ListView))
            {
                if (dependencyObject is Run)
                    dependencyObject = LogicalTreeHelper.GetParent(dependencyObject);
                else
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            // Returns the found list view item
            return dependencyObject as System.Windows.Controls.ListView;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Is called when the list view is double clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments</param>
        private static void ListViewDoubleMouseClick(object sender, MouseButtonEventArgs e)
        {
            // Gets the sender
            System.Windows.Controls.ListView listView = sender as System.Windows.Controls.ListView;

            // Gets the clicked list view item
            System.Windows.Controls.ListViewItem item = ListView.FindListViewItem(e.OriginalSource as DependencyObject);
            if (item == null)
                return;

            // Gets the command
            ICommand command = listView.GetValue(ListView.ItemActivatedCommandProperty) as ICommand;
            if (command == null)
                return;

            // Executes the command
            if (command != null)
                command.Execute(item.DataContext);
        }

        /// <summary>
        /// Is called when the list view is right clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments</param>
        private static void ListViewPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Gets the sender
            System.Windows.Controls.ListView listView = sender as System.Windows.Controls.ListView;

            // Gets the clicked list view item
            System.Windows.Controls.ListViewItem item = ListView.FindListViewItem(e.OriginalSource as DependencyObject);
            if (item == null)
                listView.SetValue(ContextMenuService.IsEnabledProperty, false);
            else
                listView.SetValue(ContextMenuService.IsEnabledProperty, true);
        }

        #endregion
    }
}