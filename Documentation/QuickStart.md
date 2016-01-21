# Introduction

The MVVM Framework builds upon the work done by [ReactiveUI](https://github.com/reactiveui/ReactiveUI). It adds several utility functions, such
as application lifecycle management, navigation, and services most applications need. This makes it much easier to implement MVVM-based applications. Right now the
project only supports WPF projects, but this might change in the future and may libraries for mobile applications may be added.

Many parts of the source code were left out for brevity. The code samples have also been stripped of all comments. If you want to view the whole source code for the
sample application, you can find it [here](https://github.com/lecode-official/mvvm-framework/tree/master/System.Windows.Mvvm.Sample).

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

The sample application we are building is the canonical todo list app. First we start with the model, which is fairly easy. For the sake of briefness the model layer
of the application only consists of an in-memory store. In a real-world application a database or a file-based storage system should be used. The model layer consists
of `TodoListItem` class, which holds the information about a single todo list item:

```csharp
public class TodoListItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsFinished { get; set; }
}
```

For managing the todo list, there is the `TodoListItemsRepository`, which implements the repository pattern:

```csharp
public class TodoListItemsRepository
{
    private static List<TodoListItem> todoListItems = new List<TodoListItem>
    {
        new TodoListItem
        {
            Title = "Feed the dog",
            Description = "Don't forgot to buy Bingo's favorite dog chow."
        },
        new TodoListItem
        {
            Title = "Buy milk",
            Description = "Lactose free please."
        }
    };

    public IEnumerable<TodoListItem> GetTodoListItems() => TodoListItemsRepository.todoListItems;

    public TodoListItem GetTodoListItem(string id) => TodoListItemsRepository.todoListItems.FirstOrDefault(item => item.Id == id);

    public TodoListItem CreateTodoListItem(string title, string description)
    {
        TodoListItem todoListItem = new TodoListItem
        {
            Title = title,
            Description = description
        };
        TodoListItemsRepository.todoListItems.Add(todoListItem);
        return todoListItem;
    }

    public void RemoveTodoListItem(string id) => TodoListItemsRepository.todoListItems.Remove(this.GetTodoListItem(id));

    public void MarkTodoListItemAsFinished(string id) => this.GetTodoListItem(id).IsFinished = true;
}
```

## Dependency injection

The MVVM Framework has a powerful dependency injection mechanism, which automatically injects components into view models. The inversion of control container of
choice is [Ninject](https://github.com/ninject/Ninject). This is the only inversion of control container which is currently supported by the MVVM Framework, but
there plans to support a wide array of inversion of control containers in the future.

In order to make our `TodoListItemsRepository` application-wide, head over to the `App.xaml.cs` and implement the `OnStartedAsync` method. Then bind the
`TodoListItemsRepository` to the [Ninject](https://github.com/ninject/Ninject) kernel. `InSingletonScope` is used so that all our view models use the same instance
of the repository.

```csharp
protected override Task OnStartedAsync(ApplicationStartedEventArgs eventArguments)
{
    this.Kernel.Bind<TodoListItemsRepository>().ToSelf().InSingletonScope();
    return Task.FromResult(0);
}
```

Use the `OnStartedAsync` to bind all your services to the [Ninject](https://github.com/ninject/Ninject) kernel. I you haven't used Ninject (or dependency injection)
before, then head over to [Ninject.org](http://www.ninject.org/). They have a lot of comprehensive tutorials explaining the concepts behind inversion of control and
the usage of [Ninject](https://github.com/ninject/Ninject).

## The view model

Now we're ready to implement our first view model. View models have several lifecycle callback methods as well:

- **`OnActivateAsync`** - Is called when the view model is created (before the user is navigated to the view and before the `OnNavigateToAsync` event is called). After the view model was created, it is cached and reused until it is destroyed, therefore `OnActivateAsync` is only called once in the life cycle of a view model.
- **`OnNavigateToAsync`** - Is called before the view model is navigated to. Other than `OnActivateAsync`, `OnNavigateToAsync` is called everytime the user navigates to this view model.
- **`OnNavigateFromAsync`** - Is called before the view model is navigated away from. Other than `OnDeactivateAsync`, `OnNavigateFromAsync` is called everytime the user navigates away from this view model.
- **`OnDeactivateAsync`** - Is called when the view model gets deactivated. The view model only gets deactivated when the navigation stack of the window, that contains the view of this view model, is cleared, or when the windows, containing the view of this view model, is closed. Therefore `OnDeactivateAsync` is only called once in the lifecycle of a view model.

All view models have to implement the `IViewModel` interface. As mentioned before, the MVVM Framework builds upon [ReactiveUI](https://github.com/reactiveui/ReactiveUI),
so there is standard implementation for the use with [ReactiveUI](https://github.com/reactiveui/ReactiveUI): `ReactiveViewModel`. You should always derive your view
models from `ReactiveViewModel`, because it has a lot of additional utility functions. As far as the navigation sub-system of the MVVM Framework is concerned, the only
requirement of a view model is to implement `IViewModel`.

At first we have to create a view model for our todo list items. The todo list item view model is a reactive view on the model layer of out sample application. Since
this is just a container view model, no view model lifecycle management is needed. This is reflected by the fact, that the view model does not not derive from
`ReactiveViewModel` but `ReactiveObject` which is [ReactiveUI](https://github.com/reactiveui/ReactiveUI)'s UI of making reactive classes. If you are not yet familiar
with [ReactiveUI](https://github.com/reactiveui/ReactiveUI), then I recommend, that you head over to [RectiveUI.net](http://reactiveui.net/) and have a look at it
first, because the MVVM Framework is heavily influenced by and build on [ReactiveUI](https://github.com/reactiveui/ReactiveUI).

```csharp
public class TodoListItemViewModel : ReactiveObject
{
    private string id;
    public string Id
    {
        get { return this.id; }
        set { this.RaiseAndSetIfChanged(ref this.id, value); }
    }

    private string title;
    public string Title
    {
        get { return this.title; }
        set { this.RaiseAndSetIfChanged(ref this.title, value); }
    }

    private string description;
    public string Description
    {
        get { return this.description; }
        set { this.RaiseAndSetIfChanged(ref this.description, value); }
    }

    private bool isFinished;
    public bool IsFinished
    {
        get { return this.isFinished; }
        set { this.RaiseAndSetIfChanged(ref this.isFinished, value); }
    }
}
```

At first we want to implement a simple view model, which just loads all todo list items and makes it possible to mark them as finished. Please not that the
`TodoListItemsRepository` is passed to the view model via the constructor. The constructor arguments are automatically injected into the view model by the navigation
sub-system of the MVVM Framework.

```csharp
public class MainViewModel : ReactiveViewModel
{
    public MainViewModel(TodoListItemsRepository todoListItemsRepository)
    {
        this.todoListItemsRepository = todoListItemsRepository;
    }

    private readonly TodoListItemsRepository todoListItemsRepository;

    public ReactiveList<TodoListItemViewModel> TodoListItems { get; private set; } = new ReactiveList<TodoListItemViewModel> { ChangeTrackingEnabled = true };

    private TodoListItemViewModel selectedTodoListItem;
    public TodoListItemViewModel SelectedTodoListItem
    {
        get { return this.selectedTodoListItem; }
        set { this.RaiseAndSetIfChanged(ref this.selectedTodoListItem, value); }
    }

    public ReactiveCommand<Unit> MarkTodoListItemAsFinishedCommand { get; private set; }
    public ReactiveCommand<Unit> RemoveTodoListItemCommand { get; private set; }

    public override Task OnActivateAsync()
    {
        this.MarkTodoListItemAsFinishedCommand = ReactiveCommand.CreateAsyncTask(x =>
        {
            this.SelectedTodoListItem.IsFinished = true;
            this.todoListItemsRepository.MarkTodoListItemAsFinished(this.SelectedTodoListItem.Id);
            return Task.FromResult(Unit.Default);
        });
        this.MarkTodoListItemAsFinishedCommand = ReactiveCommand.CreateAsyncTask(x =>
        {
            this.TodoListItems.Remove(this.SelectedTodoListItem);
            this.todoListItemsRepository.RemoveTodoListItem(this.SelectedTodoListItem.Id);
            this.SelectedTodoListItem = null;
            return Task.FromResult(Unit.Default);
        });
        return Task.FromResult(0);
    }

    public override Task OnNavigateToAsync(NavigationEventArgs e)
    {
        this.TodoListItems.Clear();
        this.TodoListItems.AddRange(this.todoListItemsRepository.GetTodoListItems().Select(item => new TodoListItemViewModel
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            IsFinished = item.IsFinished
        }));
        return Task.FromResult(0);
    }
}
```

## The view

## Navigation

# The broader picture

There are many more features that come with the MVVM Framework that are out of scope of this quick start guide. If you want to dive deeper then head over to the
[Documentation](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Documentation.md), which has detailed instructions for all of the
features of the MVVM framework.