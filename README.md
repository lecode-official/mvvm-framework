# MVVM Framework

![MVVM Framework Logo](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/LogoHeader.png "MVVM Framework Logo")

This MVVM Framework was build on top of [ReactiveUI](https://github.com/reactiveui/ReactiveUI). It enhances [ReactiveUI](https://github.com/reactiveui/ReactiveUI)
with a lot of utility functions such as windowing and navigation services. This project aims at building on those features that
[ReactiveUI](https://github.com/reactiveui/ReactiveUI) shines in and provide extra functionality to make implementing an application on the basis of the MVVM
pattern easier.

## Acknowledgments

My special thanks goes out to [Lukas Rögner](https://github.com/lukasroegner) who wrote a lot of the code and did a great deal of refactoring. Without him the
project would not have happened.

## Using the project

The project is available on NuGet: https://www.nuget.org/packages/System.Windows.Mvvm/0.1.3.

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