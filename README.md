# MVVM Framework
This MVVM framework was build on top of ReactiveUI. It enhances ReactiveUI with a lot of utility functions such as windowing and navigation services. This project
aims at building on those features that ReactiveUI shines in and provide extra functionality to make implementing an application on the basis of the MVVM pattern
easier.

## Acknowledgments
My special thanks goes out to Lukas Rögner who wrote a lot of the code and did a great deal of refactoring. Without him the project would not have happened.

## Using the project
The project is available on NuGet: https://www.nuget.org/packages/System.Windows.Mvvm/0.1.0.

```batch
PM> Install-Package System.Windows.Mvvm
```

If you want to you can download and manually build the solution. The project was built using Visual Studio 2015. Basically any version of Visual Studio 2015 will
suffice, no extra plugins or tools are needed. Just clone the Git repository open the solution using Visual Studio and build the solution.

```batch
git pull https://github.com/lecode-official/mvvm-framework.git
```

The solution is split up in several light-weight projects, which each contain a small portion of the functionality.

For more information, please refer to the documentation (coming soon).