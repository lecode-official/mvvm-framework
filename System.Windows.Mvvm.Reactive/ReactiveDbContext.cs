
#region Using Directives

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a database context with reactive methods to determine changes in the data set.
    /// </summary>
    public class ReactiveDbContext : DbContext
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveDbContext"/> instance.
        /// </summary>
        public ReactiveDbContext()
            : base()
        {
            this.ObjectContext = (this as IObjectContextAdapter).ObjectContext;
        }

        /// <summary>
        /// Initializes a new <see cref="ReactiveDbContext"/> instance.
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        public ReactiveDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            this.ObjectContext = (this as IObjectContextAdapter).ObjectContext;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains all observables for the sets of the database, so that they can be recycled.
        /// </summary>
        private IDictionary<Type, IDictionary<ChangeDetectionPolicy, ReactiveDbSet>> reactiveDbSets = new Dictionary<Type, IDictionary<ChangeDetectionPolicy, ReactiveDbSet>>();

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets the object context of the underlying database context.
        /// </summary>
        internal ObjectContext ObjectContext { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Detects changes to the database before they have been saved.
        /// </summary>
        private async Task BeforeDetectChangesAsync()
        {
            // Calls the detection method of each observable
            foreach (ReactiveDbSet reactiveDbSet in this.reactiveDbSets.Values)
                await reactiveDbSet.BeforeDetectChangesAsync();
        }

        /// <summary>
        /// Detects changes to the database after they have been saved.
        /// </summary>
        private async Task DetectChangesAsync()
        {
            // Calls the detection method of each observable
            foreach (ReactiveDbSet reactiveDbSet in this.reactiveDbSets.Values)
                await reactiveDbSet.DetectChangesAsync();
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Saves all changes back to the database.
        /// </summary>
        /// <returns>Returns the number of affected rows.</returns>
        public override async Task<int> SaveChangesAsync()
        {
            // Calls the detection method
            await this.BeforeDetectChangesAsync();

            // Saves the changes
            int result = await base.SaveChangesAsync();

            // Applies the detected changes
            await this.DetectChangesAsync();
            return result;
        }

        /// <summary>
        /// Saves all changes back to the database.
        /// </summary>
        /// <param name="cancellationToken">The token that can be used to cancel the operation.</param>
        /// <returns>Returns the number of affected rows.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            // Calls the detection method
            await this.BeforeDetectChangesAsync();

            // Saves the changes
            int result = await base.SaveChangesAsync(cancellationToken);

            // Applies the detected changes
            await this.DetectChangesAsync();
            return result;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets an observable object for the specified entity type.
        /// </summary>
        /// <param name="policy">The detection policy that should be applied to the observable.</param>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <returns>Returns the observable object that can be used to detect changes.</returns>
        public ReactiveDbSet<T> ObservableFor<T>() where T : class => this.ObservableFor<T>(ChangeDetectionPolicy.Property | ChangeDetectionPolicy.NavigationProperty | ChangeDetectionPolicy.CollectionNavigationProperty);

        /// <summary>
        /// Gets an observable object for the specified entity type.
        /// </summary>
        /// <param name="policy">The detection policy that should be applied to the observable.</param>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <returns>Returns the observable object that can be used to detect changes.</returns>
        public ReactiveDbSet<T> ObservableFor<T>(ChangeDetectionPolicy policy) where T : class
        {
            // Checks if the observable already exists, if not, adds it to the list
            if (!this.reactiveDbSets.ContainsKey(typeof(T)))
                this.reactiveDbSets[typeof(T)] = new Dictionary<ChangeDetectionPolicy, ReactiveDbSet>();
            if (!this.reactiveDbSets[typeof(T)].ContainsKey(policy))
                this.reactiveDbSets[typeof(T)][policy] = new ReactiveDbSet<T>(this, policy);

            // Returns the observable object
            return this.reactiveDbSets[typeof(T)][policy] as ReactiveDbSet<T>;
        }

        #endregion
    }
}