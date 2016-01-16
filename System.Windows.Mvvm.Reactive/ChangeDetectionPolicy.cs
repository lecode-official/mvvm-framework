
#region Using Directives

using System;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents the detection policies for changes in properties of entities. This flag defines whether observables should take care of changes on navigation properties.
    /// </summary>
    [Flags]
    public enum ChangeDetectionPolicy
    {
        /// <summary>
        /// Changes in simple properties are detected.
        /// </summary>
        Property = 1,

        /// <summary>
        /// Changes in navigation properties (in a one-to-x relation) are detected.
        /// </summary>
        NavigationProperty = 2,

        /// <summary>
        /// Changes in navigation properties (in a many-to-x relation) are detected.
        /// </summary>
        CollectionNavigationProperty = 4
    }
}