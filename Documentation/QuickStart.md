# Introduction

The MVVM Framework builds upon the work done by [ReactiveUI](https://github.com/reactiveui/ReactiveUI). It adds several utility functions, such
as application lifecycle management, navigation, and services most applications need. This makes it much easier to implement MVVM-based applications. Right now the
project only supports WPF projects, but this might change in the future and may libraries for mobile applications may be added.

Many parts of the source code were left out for brevity. If you want to view the whole source code for the sample application, you can find it
[here](https://github.com/lecode-official/mvvm-framework/tree/master/System.Windows.Mvvm.Sample).

# Using the MVVM Framework

## Creating a new project

Right now the MVVM Framework does not support any framework other than WPF. This might change in the future. To get started using the framework you should start by
opening Visual Studio and creating a new C# WPF project.

![Project creation dialog](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/ProjectCreationDialog.jpg "Creating a new C# WPF project")

To install the MVVM Framework you can use the NuGet package manager or use the NuGet package manager console:

```batch
PM> Install-Package System.Windows.Mvvm
```

## Application lifecycle management

In order to properly manage the lifecycle of the MVVM application, the MVVM Framework provides a base class for your application: `MvvmApplication`. Therefore you
have to open the `App.xaml` and perform a couple of changes. First of all the navigation is performed by the framework, so you can remove the `StartupUri` property.
Secondly the application class has to derive from `MvvmApplication`. After making those changes, your `App.xaml` should look like this:

```xaml
<mvvm:MvvmApplication x:Class="System.Windows.Mvvm.Sample.App"
                      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                      xmlns:local="clr-namespace:System.Windows.Mvvm.Sample"
                      xmlns:mvvm="clr-namespace:System.Windows.Mvvm.Application;assembly=System.Windows.Mvvm.Application">
    <Application.Resources>

    </Application.Resources>
</mvvm:MvvmApplication>
```

Please make sure that you also adapt your `App.xaml.cs` so that it also derives from `MvvmApplication`.

```csharp
public partial class App : MvvmApplication
{
}
```

The `MvvmApplication` has several lifecycle callback methods, which you can override:

- **`OnStartedAsAdditionalInstanceAsync`** - Gets called when there already is another instance of the same application running. This callback method is called before `OnStartedAsync` is invoked.
- **`OnStartedAsync`** - Gets called after the application startup. This can be overridden to implement custom startup logic and displaying views.
- **`OnExitAsync`** - Gets called right before the application quits. This can be overridden to implement custom shutdown logic.
- **`OnUnhandledExceptionAsync`** - Gets called if an exception was thrown that was not handled by user-code.

All the callback methods are asynchronous methods and can therefore call asynchronous methods. Additionally `MvvmApplication` implements the `IDisposable` interface
and there is a `Dispose` method which you can override.

At the very least you'll have to implement the `OnStartedAsync` method, which is the entry-point to the application and should be used to set up the Application
and navigate to the initial view of the application.

## The model

## The view model

## The view

## navigation

# The broader picture

There are many more features that come with the MVVM Framework that are out of scope of this quick start guide. If you want to dive deeper then head over to the
[Documentation](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Documentation.md), which has detailed instructions for all of the
features of the MVVM framework.