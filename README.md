# MVVM Framework

![MVVM Framework Logo](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/Banner.png "MVVM Framework Logo")

> **IMPORTANT** This project is no longer actively in development.

The MVVM Framework is a compact and simple, yet powerful implementation of the MVVM pattern. It provides a strong model for managing the application lifecycle
and the navigation. It was build on three important core principles:

1. Decoupling operating system functionality from the application logic
2. Asynchronous from top to bottom using the task awaitable pattern
3. Playing nice with other frameworks

The MVVM framework decouples operation system functionality from the application logic by providing a powerful service system, which abstracts away any platform
specific code from the application logic. Everything in the framework was build with the task awaitable pattern in mind, from application lifecycle management to
navigation. The framework plays nice with all kinds of frameworks, for example the dependency injection mechanism build into the framework does not care what
kind of inversion of control container you are using.

Right now the MVVM Framework is available in two flavors: one for WPF and one for the universal Windows app platform UWP. Both share a pretty similar API surface
but are not the same. Both flavors look and feel very similar, but each of them acknowledges the differences between the platforms by providing APIs that are
tailor-made for for each platform.

## Acknowledgments

My special thanks goes out to [Lukas Rögner](https://github.com/lukasroegner) who wrote a lot of the code and did a great deal of refactoring. Without him the
project would not have happened.

## Using the Project

The project is available on NuGet. You can find the WPF version here: https://www.nuget.org/packages/System.Windows.Mvvm.

```batch
PM> Install-Package System.Windows.Mvvm
```

The UWP version is available here: https://www.nuget.org/packages/Windows.Mvvm.

```batch
PM> Install-Package Windows.Mvvm 
```

If you want to you can download and manually build the solution. The project was built using Visual Studio 2015. Basically any version of Visual Studio 2015 will
suffice, no extra plugins or tools are needed (except for the `System.Windows.Mvvm.nuproj` project, which needs the
[NuBuild Project System](https://visualstudiogallery.msdn.microsoft.com/3efbfdea-7d51-4d45-a954-74a2df51c5d0) Visual Studio extension for building the NuGet
package). Just clone the Git repository, open the solution in Visual Studio, and build the solution.

```batch
git clone https://github.com/lecode-official/mvvm-framework
```

The solution is split up into several light-weight projects, each of them containing a small portion of the functionality.

For more information, please refer to the [Documentation](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Documentation.md).

## Contributions

Currently we are not accepting any contributors, but if you want to help, we would greatly appreciate feedback and bug reports. To file a bug, please use GitHub's
issue system. Alternatively, you can clone the repository and send us a pull request.
