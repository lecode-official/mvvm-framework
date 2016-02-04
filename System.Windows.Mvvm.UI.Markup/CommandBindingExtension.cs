
#region Using Directives

using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

#endregion

namespace System.Windows.Mvvm.UI.Markup
{
    /// <summary>
    /// Represents a custom markup extension, which is used to bind commands to events.
    /// </summary>
    public class CommandBindingExtension : MarkupExtension
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CommandBindingExtension"/> instance. No command is set.
        /// </summary>
        public CommandBindingExtension()
            : this(null, false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CommandBindingExtension"/> instance. The event arguments are not passed to the command.
        /// </summary>
        /// <param name="commandPath">The command to which the event is to be bound.</param>
        public CommandBindingExtension(PropertyPath commandPath)
            : this(commandPath, false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CommandBindingExtension"/> instance.
        /// </summary>
        /// <param name="commandPath">The command to which the event is to be bound.</param>
        /// <param name="passEventArgumentsToCommand">Determines whether the arguments of the event to which the command gets bound should be passed to the command as a parameter.</param>
        public CommandBindingExtension(PropertyPath commandPath, bool passEventArgumentsToCommand)
        {
            this.CommandPath = commandPath;
            this.PassEventArgumentsToCommand = passEventArgumentsToCommand;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the command to which the event is to be bound.
        /// </summary>
        [ConstructorArgument("commandPath")]
        public PropertyPath CommandPath { get; set; }

        /// <summary>
        /// Gets or sets a value that determines whether the event arguments of the event to which the command is bound are passed to it as parameters.
        /// </summary>
        [ConstructorArgument("passEventArgumentsToCommand")]
        public bool PassEventArgumentsToCommand { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Provides the value of the <see cref="CommandBindingExtension"/>. It generates an event handler that is passed to the event in which the<see cref="CommandBindingExtension"/> is used.
        /// </summary>
        /// <param name="serviceProvider">A service provider which gives access to the target object and target value of the <see cref="CommandBindingExtension"/></param>
        /// <exception cref="ArgumentNullException">If the service provider argument is null an <see cref="ArgumentNullException"/> exception is thrown.</exception>
        /// <exception cref="InvalidOperationException">
        /// An <see cref="InvalidOperationException" /> exception is raised when the target property (the event to which the command is to be bound) could not be retrieved or when the target property is no event.
        /// </exception>
        /// <returns>Returns an event handler, which invokes the given command.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Validates the argument
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            // Uses the service provider to retrieve a target provider, which is able to provide the target property to which the command is to be bound, if the target provider is null, an invalid operation exception is thrown
            IProvideValueTarget targetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (targetProvider == null)
                throw new InvalidOperationException(Resources.Localization.CommandBindingExtension.TargetProviderNullExceptionMessage);

            // The target property could either be a plain old .NET event or a WPF routed event, in case of the .NET event, the target property is the event itself, in case of the WPF routed event, the target property is the AddHandler
            // method of the routed event, both target properties are retrieved, the one that is not null is the right target property
            EventInfo targetEventInfo = targetProvider.TargetProperty as EventInfo;
            MethodInfo targetAddHandlerMethodInfo = targetProvider.TargetProperty as MethodInfo;

            // Checks whether the target property is a .NET event or a WPF routed event and retrieves the type of the delegate for the event handler accordingly, which
            // is used to generate a delegate, that is returned
            Type delegateType;
            if (targetEventInfo != null)
            {
                // The target property is a .NET event, so retrieve the delegate type accordingly
                delegateType = targetEventInfo.EventHandlerType;
            }
            else if (targetAddHandlerMethodInfo != null)
            {
                // The target property is a WPF routed event, so retrieve a delegate tye
                ParameterInfo[] parameters = targetAddHandlerMethodInfo.GetParameters();
                delegateType = parameters[1].ParameterType;
            }
            else
            {
                // The target property is not event at all, so an invalid operation exception is raised
                throw new InvalidOperationException(Resources.Localization.CommandBindingExtension.TargetPropertyNoEventExceptionMessage);
            }

            // Creates the delegate, which is returned as the value
            MethodInfo methodInfo = this.GetType().GetMethod("InvokeCommandEventHandler", BindingFlags.NonPublic | BindingFlags.Instance);
            return Delegate.CreateDelegate(delegateType, this, methodInfo);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// The event handler, which is provided as a value for the target value of the <see cref="CommandBindingExtension"/>. It executes the provided command.
        /// </summary>
        /// <param name="sender">The object that invoked the event. It this case it is the target object of the <see cref="CommandBindingExtension"/>.</param>
        /// <param name="e">The event arguments, that contain more information about the event that is raised.</param>
        private void InvokeCommandEventHandler(object sender, EventArgs e)
        {
            // Checks if the command that is to be invoked is null, if so nothing is done (if a binding target does not exist, then nothing should be done, just like in normal binding, otherwise an application that uses the command
            // binding in design mode would crash)
            if (this.CommandPath == null)
                return;

            // PropertyPath does not provide a public method to provide the value of the object it points to, so we have to get it indirectly via a binding
            Binding binding = new Binding
            {
                Path = this.CommandPath,
                Source = (sender as FrameworkElement).DataContext
            };
            FrameworkElement frameworkElement = new FrameworkElement();
            BindingOperations.SetBinding(frameworkElement, FrameworkElement.TagProperty, binding);
            object commandValue = frameworkElement.Tag;
            BindingOperations.ClearBinding(frameworkElement, FrameworkElement.DataContextProperty);

            // Converts the command value to the actual command, if the value is not a command, nothing is done, because if a binding target does not exist, nothing should be done
            ICommand command = commandValue as ICommand;
            if (command == null)
                return;

            // Checks if the commands should be passed to the command, if so then get the parameters, otherwise set the parameters to null
            object parameter = null;
            if (this.PassEventArgumentsToCommand)
                parameter = e;

            // Checks if the command can be executed, if not nothing is done
            if (!command.CanExecute(parameter))
                return;

            // Executes the command
            command.Execute(parameter);
        }

        #endregion
    }
}