
namespace System.Windows.Mvvm.UI.Controls
{
    /// <summary>
    /// Represents an entry of the data template selector.
    /// </summary>
    public struct Template
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the template for the type.
        /// </summary>
        public DataTemplate DataTemplate { get; set; }

        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        public Type Type { get; set; }

        #endregion
    }
}