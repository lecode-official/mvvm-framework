# MVVM Framework

![MVVM Framework Logo](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/LogoHeader.png "MVVM Framework Logo")

This MVVM Framework was build on top of [ReactiveUI](https://github.com/reactiveui/ReactiveUI). It enhances [ReactiveUI](https://github.com/reactiveui/ReactiveUI)
with a lot of utility functions such as windowing and navigation services. This project aims at building on those features that
[ReactiveUI](https://github.com/reactiveui/ReactiveUI) shines in and provide extra functionality to make implementing an application on the basis of the MVVM
pattern easier.

## Acknowledgments

My special thanks goes out to [Lukas Rögner](https://github.com/lukasroegner) who wrote a lot of the code and did a great deal of refactoring. Without him the
project would not have happened.

Also this project would not be possible without the great contributions of the open source community. The MVVM Framework was build using these awesome open source
projects:

- **[Ninject](https://github.com/ninject/Ninject)** - Ninject is a lightning-fast, ultra-lightweight dependency injector for .NET applications. It helps you split your application into a collection of loosely-coupled, highly-cohesive pieces, and then glue them back together in a flexible manner. By using Ninject to support your software's architecture, your code will become easier to write, reuse, test, and modify.
- **[ReactiveUI](https://github.com/reactiveui/ReactiveUI)** - A MVVM framework that integrates with the Reactive Extensions for .NET to create elegant, testable User Interfaces that run on any mobile or desktop platform. Supports Xamarin.iOS, Xamarin.Android, Xamarin.Mac, Xamarin Forms, WPF, Windows Forms, Windows Phone 8, Windows Store and Universal Windows Platform (UWP).

## Using the project

The project is available on NuGet: https://www.nuget.org/packages/System.Windows.Mvvm/0.1.5.

```batch
PM> Install-Package System.Windows.Mvvm
```

If you want to you can download and manually build the solution. The project was built using Visual Studio 2015. Basically any version of Visual Studio 2015 will
suffice, no extra plugins or tools are needed. Just clone the Git repository open the solution using Visual Studio and build the solution.

```batch
git pull https://github.com/lecode-official/mvvm-framework.git
```

The solution is split up in several light-weight projects, which each contain a small portion of the functionality.

For more information, please refer to the [Documentation](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Documentation.md).

## Contributions

Currently we are accepting no official contributors, but you want to help anyway, we would greatly appreciate feedback and bug reports. To file a bug, please use
GitHub's issue system. Alternatively you can clone the repository and send us a pull request.