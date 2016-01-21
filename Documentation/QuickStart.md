# Introduction

The MVVM Framework builds upon the work done by [ReactiveUI](https://github.com/reactiveui/ReactiveUI). It adds several utility functions, such
as application lifecycle management, navigation, and services most applications need. This makes it much easier to implement MVVM-based applications. Right now the
project only supports WPF projects, but this might change in the future and may libraries for mobile applications may be added.

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

