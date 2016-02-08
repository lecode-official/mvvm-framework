# Project Structure

The MVVM Framework project is divided up into several lightweight projects, which each make up a small part of the functionality of the project. In this quick quide, 
we will introduce you to the different projects and their function within the framework.

## System.Windows.Mvvm.Application

The application project contains the class `MvvmApplication`, which should be the starting point for most applications. `MvvmApplication` derives from `System.Windows.Application`
and enhances it with several lifecycle callbacks.

- **`OnStartedAsAdditionalInstanceAsync`** - Gets called when there already is another instance of the same application running. This callback method is called before `OnStartedAsync` is invoked.
- **`OnStartedAsync`** - Gets called after the application startup. This can be overridden to implement custom startup logic and display views.
- **`OnExitAsync`** - Gets called right before the application quits. This can be overridden to implement custom shutdown logic.
- **`OnUnhandledExceptionAsync`** - Gets called if an exception was thrown that was not handled by user-code.

The `MvvmApplication` also implements the `IDisposable` interface and makes it possible to override the `Dispose` method for custom disposal logic.

Although this project contains functionality, which is used by most MVVM Framework applications, it is still optional, because there are some use-cases where the `MvvmApplication` class is not
needed, e.g. when writing a Microsoft Office plugin.

## System.Windows.Mvvm.Configuration

This project contains a simple API for managing application configuration. The API has multiple advantages over the built-in configuration functionality of the .NET Framework: for one it
encapsulates the whole configuration logic, without directly dependending on any any framework functionality, which makes it possible to use the same API across many platforms. Also the API supports
multiple data formats for storing the configuration data. Finally, the API makes it possible to decouple the configuration from all other components, because the `ConfigContext` can be injected
other components via dependency injection, which makes them unit-testable (on the other hand, the .NET Framework configuration mechanism is encapsulated into a static class, which is a nightmare
when it comes to unit testing).

## System.Windows.Mvvm.Reactive

As mentioned before in other parts of the documentation, the MVVM Framework has strong ties to [ReactiveUI](https://github.com/reactiveui/ReactiveUI), because it was originally conceived as an
extension to it. Now, as the framework matures, the bridge to [ReactiveUI](https://github.com/reactiveui/ReactiveUI) has evolved to an component, which is recommend, but optional to use. This
project contains all reactive components, which make it easier to work together with the MVVM Framework as well as some extensions, which makes writing reactive applications even more simple. Most
notably, the project contains the `ReactiveViewModel` class, which is a view model implementation based on [ReactiveUI](https://github.com/reactiveui/ReactiveUI).

## System.Windows.Mvvm.Services.Application

This project contains the `ApplicationService` class, which is a nice API to interface with the MVVM application. Since it is implemented as a service, it can be easily injected into all components
of the application, without having to directly reference the application object. It contains useful methods for shutting down and restarting the application, application life-cycle events, as well
as references to the current culture and the current dispatcher.

## System.Windows.Mvvm.Services.Dialog

The dialog project contains a service, which encapsulates the dialog mechanism of the operating system. This makes it easier to decouple view models from the operating system and thus makes it
really simple to unit-test them. The `DialogService` class is the heart of this project and offers methods for displaying message boxes, open file dialogs, save file dialogs, and folder browser
dialogs.

## System.Windows.Mvvm.Services.Navigation

You could call the navigation project the heart of the MVVM Framework, because it contains the most important functionality of the framework: navigation. The core of the project are the classes
`WindowNavigationService` and `NavigationService`. As the names suggest, both are implemented as services, which makes it possible to use dependenca injection to inject them into view models, which
in turn makes it very easy to unit-test these view models. The `WindowNavigationService` makes it possible to navigate between windows, while the `NavigationService` makes it possible to navigate
between views within a window. Splitting up the functionality into these two components makes the navigation sub-system of the framework very flexible when building all kinds of applications, from
muli-window applications to single-window applications and everything in between.

The navigation sub-system contains a very powerful view model resolution and dependency injection mechanism, which automatically finds the corrent view models for the views and windows, instantiates
them, injects any dependencies, and manages their lifecycle.

## System.Windows.Mvvm.Services.OperatingSystem

This project contains the `OperatingSystemService` that contains functionality concerning the operating system. It contains the `Shutdown` method that shuts down the operating system as well as
the `SessionEnding` event, which is called when an operating system session ends (i.e. the user is signing out or the operating system is being shut down).

## System.Windows.Mvvm.UI

The UI project contains some helpful extensions for the UI layer that are useful for all kinds of applications. Most notably, the project contains several value converters, extensions for some
built-in controls which add much needed functionality, like simple validation, and the `CommandBindingExtension` markup extension, which makes it possible to use XAML data binding to bind commands
to an event of a control.

## MvvmFramework.Samples.Wpf

This project contains a sample application, which showcases the usage of the MVVM Framework. It implements the canonical Todo list application. If you want more information on the inner workings
of the sample application, please consult the [quick start guide](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/QuickStart.md), which goes through code step by step
and explains some of the most important features of the MVVM Framework.