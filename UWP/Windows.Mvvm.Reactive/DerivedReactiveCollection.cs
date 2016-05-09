
#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

#endregion

namespace Windows.Mvvm.Reactive
{
    /// <summary>
    /// Represents a reactive collection, which derives from another reactive collection. Everytime the reactive collection, the derived collection is based on, changes, the content of the derived collection is updated.
    /// </summary>
    public class DerivedReactiveCollection<TInput, TOutput> : ReadOnlyReactiveCollection<TOutput>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DerivedReactiveCollection{TInput, TOutput}"/> instance.
        /// </summary>
        /// <param name="reactiveCollection">The reactive collection on which the derived reactive collection is based.</param>
        /// <param name="wherePredicate">The delegate, which is used to filter the source reactive collection.</param>
        /// <param name="selectPredicate">The delegate, which is used to map the elements of the source reactive collection to the target type of the derived reactive collection.</param>
        public DerivedReactiveCollection(IReactiveCollection<TInput> reactiveCollection, Func<TInput, bool> wherePredicate, Func<TInput, TOutput> selectPredicate)
        {
            // Stores the where predicate, if no where predicate was specified, then it defaults to a predicate, which does not filter out any elements
            this.wherePredicate = wherePredicate ?? (input => true);

            // Stores the select predicate, if no select predicate was specified, then it defaults to four predicates with falling precedence:
            // 1. If the output type is the same as the input type, then the select predicate, just returns the input item
            // 2. If the output type has a constructor that takes one parameter which is assignable from the input type, then the predicate creates a new output type by calling the constructor
            // 3. If the output type has a default constructor, then the predicate creates a new output type by calling the default constructor
            // 4. If none of the above apply, then the select predicate defaults to a delegate, which is just maps every input to null
            Type outputType = typeof(TOutput);
            ConstructorInfo outputTypeConstructorInfo = outputType.GetConstructors().Where(constructorInfo => !constructorInfo.ContainsGenericParameters && constructorInfo.GetParameters().Count() == 1 && constructorInfo.GetParameters().First().ParameterType.GetTypeInfo().IsAssignableFrom(typeof(TInput).GetTypeInfo())).FirstOrDefault();
            ConstructorInfo outputTypeDefaultConstructorInfo = outputType.GetConstructors().Where(constructorInfo => constructorInfo.GetParameters().Count() == 0).FirstOrDefault();
            if (selectPredicate != null)
                this.selectPredicate = selectPredicate;
            else if (typeof(TInput) == typeof(TOutput))
                this.selectPredicate = input => (TOutput)Convert.ChangeType(input, outputType);
            else if (outputTypeConstructorInfo != null)
                this.selectPredicate = input => (TOutput)outputTypeConstructorInfo.Invoke(new object[] { input });
            else if (outputTypeDefaultConstructorInfo != null)
                this.selectPredicate = input => (TOutput)outputTypeDefaultConstructorInfo.Invoke(new object[0]);
            else
                this.selectPredicate = input => default(TOutput);

            // Initializes the derived collection by adding the initial content of the collection retrieved as a constructor parameter
            List<TOutput> initialContent = new List<TOutput>();
            foreach (TInput item in reactiveCollection)
            {
                Guid initialInputItemGuid = Guid.NewGuid();
                this.inputItemMap.Add(initialInputItemGuid);
                if (this.wherePredicate(item))
                {
                    this.outputItemMap.Add(initialInputItemGuid);
                    initialContent.Add(this.selectPredicate(item));
                }
            }
            this.Collection = initialContent;

            // Everytime the input reactive collection changes, the derived collection changes
            (reactiveCollection as INotifyCollectionChanged).CollectionChanged += (sender, e) =>
            {
                // Creates some local variables that are needed in the algorithm
                TInput inputItem = default(TInput);
                Guid inputItemGuid = Guid.Empty;
                bool isInDerivedCollection = false;
                TOutput outputItem = default(TOutput);

                // Checks kind of change took place in the input reactive collection and performs the same operation on the derived reactive collection
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:

                        // Converts the new item that was added to the type TOutput
                        inputItem = (TInput)e.NewItems[0];
                        inputItemGuid = Guid.NewGuid();
                        isInDerivedCollection = this.wherePredicate(inputItem);
                        outputItem = isInDerivedCollection ? this.selectPredicate(inputItem) : default(TOutput);

                        // There are two cases when a new element was added, either the item was added to the end of the original reactive collection or it was inserted at a specific index
                        if (e.NewStartingIndex == -1)
                        {
                            if (isInDerivedCollection)
                            {
                                this.Add(outputItem);
                                this.outputItemMap.Add(inputItemGuid);
                            }
                            this.inputItemMap.Add(inputItemGuid);
                        }
                        else
                        {
                            // Checks if the new item is in the derived reactive collection, if so then it is inserted
                            if (isInDerivedCollection)
                            {
                                // At first the index has to be determined, this is done by finding the index of the first predecessor of the new item that is in the derived reactive collection
                                int newIndex = -1;
                                for (int i = e.NewStartingIndex; i >= 0; i--)
                                {
                                    newIndex = this.outputItemMap.IndexOf(this.inputItemMap[i]);
                                    if (newIndex != -1)
                                        break;
                                }
                                newIndex = newIndex == -1 ? 0 : newIndex + 1;

                                // Inserts the item at the index that was determined
                                this.Insert(newIndex, outputItem);
                                this.outputItemMap.Insert(newIndex, inputItemGuid);
                            }

                            // Adds the GUID that uniquely identifies the new input item in the input item map
                            this.inputItemMap.Insert(e.NewStartingIndex, inputItemGuid);
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        
                        // Gets the GUID, which uniquely identifies the input item that was removed
                        inputItemGuid = this.inputItemMap[e.OldStartingIndex];

                        // Removes the input item GUID from the input item map, because it is no longer in the input reactive collection
                        this.inputItemMap.Remove(inputItemGuid);

                        // Checks an output item was generated for the input item, if so then it is removed from the output item map and the derived collection
                        int outputItemIndex = this.outputItemMap.IndexOf(inputItemGuid);
                        if (outputItemIndex != -1)
                        {
                            this.outputItemMap.RemoveAt(outputItemIndex);
                            this.RemoveAt(outputItemIndex);
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:

                        // Converts the new item that was added to the type TOutput
                        inputItem = (TInput)e.NewItems[0];
                        inputItemGuid = Guid.NewGuid();
                        isInDerivedCollection = this.wherePredicate(inputItem);
                        outputItem = isInDerivedCollection ? this.selectPredicate(inputItem) : default(TOutput);

                        // Gets the index of the old item in the derived reactive collection
                        outputItemIndex = this.outputItemMap.IndexOf(this.inputItemMap[e.NewStartingIndex]);

                        // Checks if the old item is currently in the derived reactive collection, if not then it is added, otherwise it replaced
                        if (outputItemIndex == -1)
                        {
                            // Checks if the item, the old item is to be replaced with, should be added to the derived reactive collection, if so then it is inserted at the correct position
                            if (isInDerivedCollection)
                            {
                                // At first the index has to be determined, this is done by finding the index of the first predecessor of the new item that is in the derived reactive collection
                                int newIndex = -1;
                                for (int i = e.NewStartingIndex; i >= 0; i--)
                                {
                                    newIndex = this.outputItemMap.IndexOf(this.inputItemMap[i]);
                                    if (newIndex != -1)
                                        break;
                                }
                                newIndex = newIndex == -1 ? 0 : newIndex + 1;

                                // Inserts the item at the index that was determined
                                this.Insert(newIndex, outputItem);
                                this.outputItemMap.Insert(newIndex, inputItemGuid);
                            }
                        }
                        else
                        {
                            // Checks if the item, the old item is to be replaced with, should be added to the derived reactive collection, if so then it is replaced, otherwise the old item is removed
                            if (isInDerivedCollection)
                            {
                                base[outputItemIndex] = outputItem;
                                this.outputItemMap[outputItemIndex] = inputItemGuid;
                            }
                            else
                            {
                                this.RemoveAt(outputItemIndex);
                                this.outputItemMap.RemoveAt(outputItemIndex);
                            }
                        }

                        // Replaces the old item with the new one in the input item map
                        this.inputItemMap[e.NewStartingIndex] = inputItemGuid;
                        break;

                    case NotifyCollectionChangedAction.Reset:

                        // Clears the maps, which were used to map from input items to output items
                        this.inputItemMap.Clear();
                        this.outputItemMap.Clear();

                        // Clears the derived collection
                        this.Clear();

                        // Rebuilds the whole reactive collection
                        List<TOutput> newContent = new List<TOutput>();
                        foreach (TInput item in reactiveCollection)
                        {
                            Guid initialInputItemGuid = Guid.NewGuid();
                            this.inputItemMap.Add(initialInputItemGuid);
                            if (this.wherePredicate(item))
                            {
                                this.outputItemMap.Add(initialInputItemGuid);
                                newContent.Add(this.selectPredicate(item));
                            }
                        }
                        this.AddRange(newContent);
                        break;
                }
            };
        }

        /// <summary>
        /// Initializes a new <see cref="DerivedReactiveCollection{TInput, TOutput}"/> instance.
        /// </summary>
        /// <param name="reactiveCollection">The reactive collection on which the derived reactive collection is based.</param>
        /// <param name="wherePredicate">The delegate, which is used to filter the source reactive collection.</param>
        public DerivedReactiveCollection(IReactiveCollection<TInput> reactiveCollection, Func<TInput, bool> wherePredicate)
            : this(reactiveCollection, wherePredicate, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="DerivedReactiveCollection{TInput, TOutput}"/> instance.
        /// </summary>
        /// <param name="reactiveCollection">The reactive collection on which the derived reactive collection is based.</param>
        /// <param name="selectPredicate">The delegate, which is used to map the elements of the source reactive collection to the target type of the derived reactive collection.</param>
        public DerivedReactiveCollection(IReactiveCollection<TInput> reactiveCollection, Func<TInput, TOutput> selectPredicate)
            : this(reactiveCollection, null, selectPredicate)
        { }

        /// <summary>
        /// Initializes a new <see cref="DerivedReactiveCollection{TInput, TOutput}"/> instance.
        /// </summary>
        /// <param name="reactiveCollection">The reactive collection on which the derived reactive collection is based.</param>
        public DerivedReactiveCollection(IReactiveCollection<TInput> reactiveCollection)
            : this(reactiveCollection, null, null)
        { }

        #endregion

        #region Private Fields
        
        /// <summary>
        /// Contains a map, which contains for each input element a GUID, which uniquely identifies the item (this is needed, because the input collection may contain the same element multiple times are different indices).
        /// </summary>
        private readonly List<Guid> inputItemMap = new List<Guid>();

        /// <summary>
        /// Contains a map, which contains for each output element the GUID that uniquely identifies the input item it derived from.
        /// </summary>
        private readonly List<Guid> outputItemMap = new List<Guid>();

        /// <summary>
        /// Contains the delegate, which is used to filter the source reactive collection.
        /// </summary>
        private readonly Func<TInput, bool> wherePredicate;

        /// <summary>
        /// Contains the delegate, which is used to map the elements of the source reactive collection to the target type of the derived reactive collection.
        /// </summary>
        private readonly Func<TInput, TOutput> selectPredicate;

        #endregion
    }
}