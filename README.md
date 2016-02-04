# MVVM Framework

![MVVM Framework Logo](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/Banner.png "MVVM Framework Logo")

The MVVM Framework was build on top of [ReactiveUI](https://github.com/reactiveui/ReactiveUI). It enhances [ReactiveUI](https://github.com/reactiveui/ReactiveUI)
with a lot of utility functions such as windowing and navigation services. This project aims at building on those features that
[ReactiveUI](https://github.com/reactiveui/ReactiveUI) shines at and provides extra functionality to make implementing an application on the basis of the MVVM
pattern easier.

## Acknowledgments

My special thanks goes out to [Lukas Rögner](https://github.com/lukasroegner) who wrote a lot of the code and did a great deal of refactoring. Without him the
project would not have happened.

**[ReactiveUI](https://github.com/reactiveui/ReactiveUI)** - A MVVM framework that integrates with the Reactive Extensions for .NET to create elegant, testable
User Interfaces that run on any mobile or desktop platform. Supports Xamarin.iOS, Xamarin.Android, Xamarin.Mac, Xamarin Forms, WPF, Windows Forms, Windows Phone 8,
Windows Store and Universal Windows Platform (UWP).

## Using the Project

The project is available on NuGet: https://www.nuget.org/packages/System.Windows.Mvvm.

```batch
PM> Install-Package System.Windows.Mvvm
```

If you want to you can download and manually build the solution. The project was built using Visual Studio 2015. Basically any version of Visual Studio 2015 will
suffice, no extra plugins or tools are needed (except for the `System.Windows.Mvvm.Nuget` project, which needs the
[NuBuild Project System](https://visualstudiogallery.msdn.microsoft.com/3efbfdea-7d51-4d45-a954-74a2df51c5d0) Visual Studio extension for building the NuGet
package). Just clone the Git repository, open the solution in Visual Studio, and build the solution.

```batch
git pull https://github.com/lecode-official/mvvm-framework.git
```

The solution is split up into several light-weight projects, each of them containing a small portion of the functionality.

For more information, please refer to the [Documentation](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Documentation.md).

## Contributions

Currently we are not accepting any contributors, but if you want to help, we would greatly appreciate feedback and bug reports. To file a bug, please use GitHub's
issue system. Alternatively, you can clone the repository and send us a pull request.