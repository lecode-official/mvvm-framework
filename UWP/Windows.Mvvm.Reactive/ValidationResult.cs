
#region Using Directives

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represets a validation error, when a <see cref="ReactiveProperty{T}"/> has an erroneous value.
    /// </summary>
    public class ValidationResult
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ValidationResult"/> instance.
        /// </summary>
        /// <param name="errors">A list of all the errors that occurred.</param>
        public ValidationResult(IEnumerable<string> errors)
        {
            this.HasErrors = errors != null && errors.Any();
            this.Errors = errors ?? new List<string>();
        }

        /// <summary>
        /// Initializes a new <see cref="ValidationResult"/> instance.
        /// </summary>
        /// <param name="error">The error that has occurred.</param>
        public ValidationResult(string error)
            : this(string.IsNullOrWhiteSpace(error) ? null : new List<string> { error })
        { }

        /// <summary>
        /// Initializes a new <see cref="ValidationResult"/> instance.
        /// </summary>
        /// <param name="hasErrors">Determines whether there is a validation error or not.</param>
        public ValidationResult(bool hasErrors)
        {
            this.HasErrors = hasErrors;
            this.Errors = new List<string>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value that determines whether there is a validation error or not.
        /// </summary>
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Gets a list of all the errors that occurred.
        /// </summary>
        public IEnumerable<string> Errors { get; private set; }

        #endregion
    }
}