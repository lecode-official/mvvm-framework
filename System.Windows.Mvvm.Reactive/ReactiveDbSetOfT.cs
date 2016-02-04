
#region Using Directives

using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

#endregion

namespace System.Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents an object that provides several properties for the <see cref="DbSet"/> that can be observed.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class ReactiveDbSet<T> : ReactiveDbSet where T : class
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DbSetObservable"/> instance.
        /// </summary>
        /// <param name="dbContext">The context for which the observable is generated.</param>
        /// <param name="policy">The change detection policy that should be used by the set.</param>
        internal ReactiveDbSet(ReactiveDbContext dbContext, ChangeDetectionPolicy policy)
        {
            this.DbContext = dbContext;
            this.Policy = policy;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains all detected changes from the tracker so that they can be stored for the observables that are invoked after the database change.
        /// </summary>
        private List<Action> detectedChanges = new List<Action>();

        #endregion

        #region Public Methods


        /// <summary>
        /// Detects all changes before saving them to the set.
        /// </summary>
        public override Task BeforeDetectChangesAsync()
        {
            return Task.Run(() =>
            {
                // Checks whether any changes have been applied to the context
                this.DbContext.ChangeTracker.DetectChanges();

                // Gets the list of changes from the context
                List<ObjectStateEntry> changes = new List<ObjectStateEntry>(this.DbContext.ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified).Where(objectStateEntry => objectStateEntry.IsRelationship || objectStateEntry.Entity is T));
                List<ObjectStateEntry> handledChanges = new List<ObjectStateEntry>();
                this.detectedChanges.Clear();

                // Checks whether any changes have been made
                bool itemsAdded = false;
                bool itemsChanged = false;
                bool itemsRemoved = false;

                // Cycles through all change entries
                while (changes.Count > 0)
                {
                    // Gets the first item in the changes
                    ObjectStateEntry entry = changes.First();

                    // Checks which observable should be invoked based on basic entry changes
                    if ((this.Policy & ChangeDetectionPolicy.Property) == ChangeDetectionPolicy.Property && !entry.IsRelationship)
                    {
                        T entity = entry.Entity as T;
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                itemsAdded = true;
                                this.beforeItemAdded.OnNext(entity);
                                this.detectedChanges.Add(() => this.itemAdded.OnNext(entity));
                                break;
                            case EntityState.Modified:
                                itemsChanged = true;
                                this.beforeItemChanged.OnNext(entity);
                                this.detectedChanges.Add(() => this.itemChanged.OnNext(entity));
                                break;
                            case EntityState.Deleted:
                                itemsRemoved = true;
                                this.beforeItemRemoved.OnNext(entity);
                                this.detectedChanges.Add(() => this.itemRemoved.OnNext(entity));
                                break;
                        }
                    }

                    // Checks which observable should be invoked based on relation entry changes
                    if (((this.Policy & ChangeDetectionPolicy.NavigationProperty) == ChangeDetectionPolicy.NavigationProperty || (this.Policy & ChangeDetectionPolicy.CollectionNavigationProperty) == ChangeDetectionPolicy.CollectionNavigationProperty) && entry.IsRelationship)
                    {
                        // Tries to get the entities from the original values
                        try
                        {
                            // This method of change detection can only handle relationships with two ends
                            if (entry.OriginalValues.FieldCount != 2)
                                throw new InvalidOperationException();

                            // Gets the entity of the first end
                            ObjectStateEntry firstRelatedEntry = this.DbContext.ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.OriginalValues[0]);
                            RelationshipMultiplicity firstRelationType = (entry.EntitySet as AssociationSet).AssociationSetEnds[1].CorrespondingAssociationEndMember.RelationshipMultiplicity;
                            T firstEntity = firstRelatedEntry.Entity as T;

                            // Checks whether this change should be detected, which is the case if
                            // - This set is responsible for the type of the entity
                            // - The entity is not deleted or added
                            // - The correct type of navigation property is set as policy
                            if (firstEntity != null && (firstRelatedEntry.State == EntityState.Unchanged || firstRelatedEntry.State == EntityState.Modified) && (((this.Policy & ChangeDetectionPolicy.NavigationProperty) == ChangeDetectionPolicy.NavigationProperty && (firstRelationType == RelationshipMultiplicity.One || firstRelationType == RelationshipMultiplicity.ZeroOrOne)) || ((this.Policy & ChangeDetectionPolicy.CollectionNavigationProperty) == ChangeDetectionPolicy.CollectionNavigationProperty && firstRelationType == RelationshipMultiplicity.Many)))
                            {
                                itemsChanged = true;
                                this.beforeItemChanged.OnNext(firstEntity);
                                this.detectedChanges.Add(() => this.itemChanged.OnNext(firstEntity));
                            }

                            // Gets the entity of the first end
                            ObjectStateEntry secondRelatedEntry = this.DbContext.ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.OriginalValues[1]);
                            RelationshipMultiplicity secondRelationType = (entry.EntitySet as AssociationSet).AssociationSetEnds[0].CorrespondingAssociationEndMember.RelationshipMultiplicity;
                            T secondEntity = secondRelatedEntry.Entity as T;

                            // Checks whether this change should be detected, which is the case if
                            // - This set is responsible for the type of the entity
                            // - The entity is not deleted or added
                            // - The correct type of navigation property is set as policy
                            if (secondEntity != null && (secondRelatedEntry.State == EntityState.Unchanged || secondRelatedEntry.State == EntityState.Modified) && (((this.Policy & ChangeDetectionPolicy.NavigationProperty) == ChangeDetectionPolicy.NavigationProperty && (secondRelationType == RelationshipMultiplicity.One || secondRelationType == RelationshipMultiplicity.ZeroOrOne)) || ((this.Policy & ChangeDetectionPolicy.CollectionNavigationProperty) == ChangeDetectionPolicy.CollectionNavigationProperty && secondRelationType == RelationshipMultiplicity.Many)))
                            {
                                itemsChanged = true;
                                this.beforeItemChanged.OnNext(secondEntity);
                                this.detectedChanges.Add(() => this.itemChanged.OnNext(secondEntity));
                            }
                        }
                        catch (InvalidOperationException) { }

                        // Tries to get the entities from the new values
                        try
                        {
                            // This method of change detection can only handle relationships with two ends
                            if (entry.CurrentValues.FieldCount != 2)
                                throw new InvalidOperationException();

                            // Gets the entity of the first end
                            ObjectStateEntry firstRelatedEntry = this.DbContext.ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.CurrentValues[0]);
                            RelationshipMultiplicity firstRelationType = (entry.EntitySet as AssociationSet).AssociationSetEnds[1].CorrespondingAssociationEndMember.RelationshipMultiplicity;
                            T firstEntity = firstRelatedEntry.Entity as T;

                            // Checks whether this change should be detected, which is the case if
                            // - This set is responsible for the type of the entity
                            // - The entity is not deleted or added
                            // - The correct type of navigation property is set as policy
                            if (firstEntity != null && (firstRelatedEntry.State == EntityState.Unchanged || firstRelatedEntry.State == EntityState.Modified) && (((this.Policy & ChangeDetectionPolicy.NavigationProperty) == ChangeDetectionPolicy.NavigationProperty && (firstRelationType == RelationshipMultiplicity.One || firstRelationType == RelationshipMultiplicity.ZeroOrOne)) || ((this.Policy & ChangeDetectionPolicy.CollectionNavigationProperty) == ChangeDetectionPolicy.CollectionNavigationProperty && firstRelationType == RelationshipMultiplicity.Many)))
                            {
                                itemsChanged = true;
                                this.beforeItemChanged.OnNext(firstEntity);
                                this.detectedChanges.Add(() => this.itemChanged.OnNext(firstEntity));
                            }

                            // Gets the entity of the first end
                            ObjectStateEntry secondRelatedEntry = this.DbContext.ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.CurrentValues[1]);
                            RelationshipMultiplicity secondRelationType = (entry.EntitySet as AssociationSet).AssociationSetEnds[0].CorrespondingAssociationEndMember.RelationshipMultiplicity;
                            T secondEntity = secondRelatedEntry.Entity as T;

                            // Checks whether this change should be detected, which is the case if
                            // - This set is responsible for the type of the entity
                            // - The entity is not deleted or added
                            // - The correct type of navigation property is set as policy
                            if (secondEntity != null && (secondRelatedEntry.State == EntityState.Unchanged || secondRelatedEntry.State == EntityState.Modified) && (((this.Policy & ChangeDetectionPolicy.NavigationProperty) == ChangeDetectionPolicy.NavigationProperty && (secondRelationType == RelationshipMultiplicity.One || secondRelationType == RelationshipMultiplicity.ZeroOrOne)) || ((this.Policy & ChangeDetectionPolicy.CollectionNavigationProperty) == ChangeDetectionPolicy.CollectionNavigationProperty && secondRelationType == RelationshipMultiplicity.Many)))
                            {
                                itemsChanged = true;
                                this.beforeItemChanged.OnNext(secondEntity);
                                this.detectedChanges.Add(() => this.itemChanged.OnNext(secondEntity));
                            }
                        }
                        catch (InvalidOperationException) { }
                    }

                    // Removes the item from the list as it has been handled
                    handledChanges.Add(entry);
                    changes.Remove(entry);

                    // Checks whether any changes have been applied to the context
                    this.DbContext.ChangeTracker.DetectChanges();

                    // Adds all new changes that might have happened due to invoking the observable
                    changes.AddRange(this.DbContext.ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified).Where(objectStateEntry => (objectStateEntry.IsRelationship || objectStateEntry.Entity is T) && !changes.Contains(objectStateEntry) && !handledChanges.Contains(objectStateEntry)));
                }

                // Invokes the generic observables for actions
                if (itemsAdded || itemsRemoved)
                {
                    this.beforeChanged.OnNext(this);
                    this.detectedChanges.Add(() => this.changed.OnNext(this));
                }
                if (itemsAdded || itemsChanged || itemsRemoved)
                {
                    this.beforeAny.OnNext(this);
                    this.detectedChanges.Add(() => this.any.OnNext(this));
                }
            });
        }

        /// <summary>
        /// Detects all changes after saving them to the set.
        /// </summary>
        public override Task DetectChangesAsync()
        {
            return Task.Run(() =>
            {
                // Invokes all stored actions
                foreach (Action detectedChange in this.detectedChanges)
                    detectedChange();
            });
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Contains the object that observes whether an item in the set is to be changed.
        /// </summary>
        private Subject<T> beforeItemChanged = new Subject<T>();

        /// <summary>
        /// Gets an object that observes whether an item in the set is to be changed.
        /// </summary>
        public IObservable<T> BeforeItemChanged
        {
            get
            {
                return this.beforeItemChanged;
            }
        }

        /// <summary>
        /// Contains the object that observes whether an item in the set has been changed.
        /// </summary>
        private Subject<T> itemChanged = new Subject<T>();

        /// <summary>
        /// Gets an object that observes whether an item in the set has been changed.
        /// </summary>
        public IObservable<T> ItemChanged
        {
            get
            {
                return this.itemChanged;
            }
        }

        /// <summary>
        /// Contains the object that observes whether an item is to be added to the set.
        /// </summary>
        private Subject<T> beforeItemAdded = new Subject<T>();

        /// <summary>
        /// Gets an object that observes whether an item is to be added to the set.
        /// </summary>
        public IObservable<T> BeforeItemAdded
        {
            get
            {
                return this.beforeItemAdded;
            }
        }

        /// <summary>
        /// Contains the object that observes whether an item has been added to the set.
        /// </summary>
        private Subject<T> itemAdded = new Subject<T>();

        /// <summary>
        /// Gets an object that observes whether an item has been added to the set.
        /// </summary>
        public IObservable<T> ItemAdded
        {
            get
            {
                return this.itemAdded;
            }
        }

        /// <summary>
        /// Contains the object that observes whether an item is to be removed from the set.
        /// </summary>
        private Subject<T> beforeItemRemoved = new Subject<T>();

        /// <summary>
        /// Gets an object that observes whether an item is to be removed from the set.
        /// </summary>
        public IObservable<T> BeforeItemRemoved
        {
            get
            {
                return this.beforeItemRemoved;
            }
        }

        /// <summary>
        /// Contains the object that observes whether an item has been removed from the set.
        /// </summary>
        private Subject<T> itemRemoved = new Subject<T>();

        /// <summary>
        /// Gets an object that observes whether an item has been removed from the set.
        /// </summary>
        public IObservable<T> ItemRemoved
        {
            get
            {
                return this.itemRemoved;
            }
        }

        /// <summary>
        /// Contains the object that observes whether items are to be added or removed.
        /// </summary>
        private Subject<ReactiveDbSet<T>> beforeChanged = new Subject<ReactiveDbSet<T>>();

        /// <summary>
        /// Gets an object that observes whether items are to be added or removed.
        /// </summary>
        public IObservable<ReactiveDbSet<T>> BeforeChanged
        {
            get
            {
                return this.beforeChanged;
            }
        }

        /// <summary>
        /// Contains the object that observes whether items have been added or removed.
        /// </summary>
        private Subject<ReactiveDbSet<T>> changed = new Subject<ReactiveDbSet<T>>();

        /// <summary>
        /// Gets an object that observes whether items have been added or removed.
        /// </summary>
        public IObservable<ReactiveDbSet<T>> Changed
        {
            get
            {
                return this.changed;
            }
        }

        /// <summary>
        /// Contains the object that observes whether any changes are to be made.
        /// </summary>
        private Subject<ReactiveDbSet<T>> beforeAny = new Subject<ReactiveDbSet<T>>();

        /// <summary>
        /// Gets an object that observes whether any changes are to be made.
        /// </summary>
        public IObservable<ReactiveDbSet<T>> BeforeAny
        {
            get
            {
                return this.beforeAny;
            }
        }

        /// <summary>
        /// Contains the object that observes whether any changes have been made.
        /// </summary>
        private Subject<ReactiveDbSet<T>> any = new Subject<ReactiveDbSet<T>>();

        /// <summary>
        /// Gets an object that observes whether any changes have been made.
        /// </summary>
        public IObservable<ReactiveDbSet<T>> Any
        {
            get
            {
                return this.any;
            }
        }

        #endregion
    }
}