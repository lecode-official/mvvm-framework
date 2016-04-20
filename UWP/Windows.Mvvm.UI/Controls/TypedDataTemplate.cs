
#region Using Directives

using System;
using Windows.UI.Xaml;

#endregion

namespace Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Represents a data template for a specific data type.
    /// </summary>
    public class TypedDataTemplate : DataTemplate
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the data type for which the data template should be used.
        /// </summary>
        public Type DataType { get; set; }

        #endregion
    }
}