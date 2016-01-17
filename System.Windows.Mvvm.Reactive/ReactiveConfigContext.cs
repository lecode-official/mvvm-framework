﻿
#region Using Directives

using System.Windows.Mvvm.Configuration;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a configuration context with reactive methods to determine changes in the data set.
    /// </summary>
    public class ReactiveConfigContext : ConfigContext
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ReactiveConfigContext"/> instance.
        /// </summary>
        /// <param name="fileName">The name of the configuration file.</param>
        public ReactiveConfigContext(string fileName)
            : base(fileName) { }

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Contains the object that observes whether items are to be added, removed or changed.
        /// </summary>
        private ReplaySubject<ReactiveConfigContext> beforeChanged = new ReplaySubject<ReactiveConfigContext>();

        /// <summary>
        /// Gets an object that observes whether items are to be added, removed or changed.
        /// </summary>
        public IObservable<ReactiveConfigContext> BeforeChanged
        {
            get
            {
                return this.beforeChanged;
            }
        }

        /// <summary>
        /// Contains the object that observes whether items have been added, removed or changed.
        /// </summary>
        private ReplaySubject<ReactiveConfigContext> changed = new ReplaySubject<ReactiveConfigContext>();

        /// <summary>
        /// Gets an object that observes whether items have been added, removed or changed.
        /// </summary>
        public IObservable<ReactiveConfigContext> Changed
        {
            get
            {
                return this.changed;
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Saves all changes made to the context to the configuration file.
        /// </summary>
        public override async Task SaveChangesAsync()
        {
            // Peeks the observable
            this.beforeChanged.OnNext(this);

            // Saves the changes
            await base.SaveChangesAsync();

            // Peeks the observable
            this.changed.OnNext(this);
        }

        #endregion
    }
}