
namespace System.Windows.Mvvm.Services.Dialog
{
    /// <summary>
    /// Represents the result of a dialog, which not only returns the result itself (what the user did, e.g. click okay or cancel), but also a result value.
    /// </summary>
    /// <typeparam name="T">The type of the result value, which is returned.</typeparam>
    public class DialogResult<T>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="Result"/> instance.
        /// </summary>
        /// <param name="result">The result of the dialog invocation.</param>
        /// <param name="resultValue">The resulting value of the dialog invocation.</param>
        public DialogResult(DialogResult result, T resultValue)
        {
            this.Result = result;
            this.ResultValue = resultValue;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the result of the dialog invocation.
        /// </summary>
        public DialogResult Result { get; set; }

        /// <summary>
        /// Gets or sets the resulting value of the dialog invocation.
        /// </summary>
        public T ResultValue { get; set; }

        #endregion
    }
}