
#region Using Directives

using System.Threading.Tasks;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents an object that provides several properties for the <see cref="DbSet"/> that can be observed.
    /// </summary>
    public abstract class ReactiveDbSet
    {
        #region Public Methods

        /// <summary>
        /// Detects all changes before saving them to the set.
        /// </summary>
        public abstract Task BeforeDetectChangesAsync();

        /// <summary>
        /// Detects all changes after saving them to the set.
        /// </summary>
        public abstract Task DetectChangesAsync();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the context for which the observable is generated.
        /// </summary>
        public ReactiveDbContext DbContext { get; protected set; }

        /// <summary>
        /// Gets the change detection policy that is used by the set.
        /// </summary>
        public ChangeDetectionPolicy Policy { get; protected set; }

        #endregion
    }
}