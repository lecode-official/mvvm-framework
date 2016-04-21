# Introduction

Many parts of the source code were left out for brevity. The code samples have also been stripped of all comments. If you want to view the whole source code for the
sample application, you can find it [here](https://github.com/lecode-official/mvvm-framework/tree/master/System.Windows.Mvvm.Sample).

# Using the MVVM Framework

## Creating a New Project

This quick guide is for kick-starting you on developing applications using WPF. Both flavors of the MVVM Framework share a pretty similar API surface but are not the
same. Both of them look and feel very similar, but each of them acknowledges the differences between the platforms by providing APIs that are tailor-made for for each
platform. A version of the quick start guide for the Windows universal app platform (UWP) is coming soon. To get started using the framework you should open Visual
Studio and create a new C# WPF project.

![Project creation dialog](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/ProjectCreationDialog.jpg "Creating a new C# WPF project")

To install the MVVM Framework you can use the NuGet package manager or the NuGet package manager console:

```batch
PM> Install-Package System.Windows.Mvvm
```

## Application Lifecycle Management

In order to properly manage the lifecycle of the MVVM application, the MVVM Framework provides a base class for your application: `MvvmApplication`. Therefore you
have to open the `App.xaml` and perform a couple of changes. First of all the navigation is performed by the framework, so you can remove the `StartupUri` property.
Secondly the application class has to derive from `MvvmApplication`. After making those changes, your `App.xaml` should look like this:

```xaml
<mvvm:MvvmApplication x:Class="System.Windows.Mvvm.Sample.App"
                      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                      xmlns:local="clr-namespace:System.Windows.Mvvm.Sample"
                      xmlns:mvvm="clr-namespace:System.Windows.Mvvm.Application;assembly=System.Windows.Mvvm.Application">
    <mvvm:MvvmApplication.Resources>
        
    </mvvm:MvvmApplication.Resources>
</mvvm:MvvmApplication>
```

Please make sure that you also adapt your `App.xaml.cs` so that it also derives from `MvvmApplication`.

```csharp
public partial class App : MvvmApplication
{
}
```

The `MvvmApplication` has several lifecycle callback methods, which you can override:

- **`OnStartedAsync`** - Gets called after the application startup. This can be overridden to implement custom startup logic and display views.
- **`OnExitAsync`** - Gets called right before the application quits. This can be overridden to implement custom shutdown logic.
- **`OnUnhandledExceptionAsync`** - Gets called if an exception was thrown that was not handled by user-code.

All callback methods are asynchronous methods and can therefore call asynchronous methods. Additionally `MvvmApplication` implements the `IDisposable` interface
and there is a `Dispose` method which you can override.

At the very least you'll have to implement the `OnStartedAsync` method, which is the entry-point to the application and should be used to set up the application
and navigate to the initial view of the application.

## The Model

The sample application we are building is the canonical todo list app. First we start with the model, which is fairly easy. For the sake of briefness the model layer
of the application only consists of an in-memory store. In a real-world application a database or a file-based storage system should be used. The model layer consists
of the `TodoListItem` class, which holds the information about a single todo list item:

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

## Dependency Injection

The MVVM Framework has a powerful dependency injection mechanism, which automatically injects components into view models. In order to support multiple different
IoC containers, the MVVM Framework uses an abstraction layer. This allows you to choose an IoC container, which suits your needs.

In order to make our `TodoListItemsRepository` available application-wide, head over to the `App.xaml.cs` and implement the `OnStartedAsync` method. Then bind the
`TodoListItemsRepository` to the IoC container. `InSingletonScope` is used so that all our view models use the same instance of the repository.

```csharp
protected override Task OnStartedAsync(ApplicationStartedEventArgs eventArguments)
{
    this.iocContainer = new SimpleIocContainer();
    this.iocContainer.RegisterType<TodoListItemsRepository>(Scope.Singleton);
    return Task.FromResult(0);
}
```

Use the `OnStartedAsync` to bind all your services to the IoC container. If you haven't used dependency injection before, then head over to
[Ninject.org](http://www.ninject.org/). They have a lot of comprehensive tutorials explaining the concepts behind inversion of control. Alternatively you could take
a look at our [Simple IoC](https://github.com/lecode-official/simple-ioc), which has a very simple, yet comprehensive
[sample](https://github.com/lecode-official/simple-ioc/tree/master/Samples/SimpleIoc.Samples.Console) that showcases some of the most seen use cases of dependency
injection.

## The View Model

Now we're ready to implement our first view model. View models have several lifecycle callback methods as well:

- **`OnActivateAsync`** - Is called when the view model is created (before the user is navigated to the view and before the `OnNavigateToAsync` event is called). After the view model was created, it is cached and reused until it is destroyed, therefore `OnActivateAsync` is only called once in the lifecycle of a view model.
- **`OnNavigateToAsync`** - Is called before the view model is navigated to. Other than `OnActivateAsync`, `OnNavigateToAsync` is called everytime the user navigates to this view model.
- **`OnNavigateFromAsync`** - Is called before the view model is navigated away from. Other than `OnDeactivateAsync`, `OnNavigateFromAsync` is called everytime the user navigates away from the view model.
- **`OnDeactivateAsync`** - Is called when the view model gets deactivated. The view model only gets deactivated when the navigation stack of the window, that contains the view of this view model, is cleared, or when the window, containing the view of this view model, is closed. Therefore `OnDeactivateAsync` is only called once in the lifecycle of a view model.

All view models have to implement the `IViewModel` interface. As mentioned before, the MVVM Framework has strong ties to [ReactiveUI](https://github.com/reactiveui/ReactiveUI),
so there is standard implementation for the use with [ReactiveUI](https://github.com/reactiveui/ReactiveUI): `ReactiveViewModel`. You should always derive your view
models from `ReactiveViewModel`, because it has a lot of additional utility functions. As far as the navigation sub-system of the MVVM Framework is concerned, the only
requirement of a view model is to implement `IViewModel`.

At first we have to create a view model for our todo list items. The todo list item view model is a reactive view on the model layer of our sample application. Since
this is just a container view model, no view model lifecycle management is needed. This is reflected by the fact, that the view model does not derive from
`ReactiveViewModel` but `ReactiveObject` which is [ReactiveUI](https://github.com/reactiveui/ReactiveUI)'s way of making a class reactive. If you are not yet familiar
with [ReactiveUI](https://github.com/reactiveui/ReactiveUI), we would recommend that you head over to [RectiveUI.net](http://reactiveui.net/) and have a look at it.

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

Next we want to implement a simple view model, which just loads all todo list items and makes it possible to remove them or mark them as finished. Please note that the
`TodoListItemsRepository` is passed to the view model via constructor injection. The constructor arguments are automatically injected into the view model by the navigation
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
        this.MarkTodoListItemAsFinishedCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedTodoListItem).Select(x => this.SelectedTodoListItem != null), x =>
        {
            this.SelectedTodoListItem.IsFinished = true;
            this.todoListItemsRepository.MarkTodoListItemAsFinished(this.SelectedTodoListItem.Id);
            return Task.FromResult(Unit.Default);
        });

        this.RemoveTodoListItemCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedTodoListItem).Select(x => this.SelectedTodoListItem != null), x =>
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
        foreach (TodoListItem todoListItem in this.todoListItemsRepository.GetTodoListItems().ToList())
        {
            this.TodoListItems.Add(new TodoListItemViewModel
            {
                Id = todoListItem.Id,
                Title = todoListItem.Title,
                Description = todoListItem.Description,
                IsFinished = todoListItem.IsFinished
            });
        }

        return Task.FromResult(0);
    }
}
```

## The View

The MVVM Framework supports a wide array of view scenarios from single-window applications, where only one main window exists that hosts all views, to
multi-window applications where each view is a different window. Of course a mixture of both paradigms is possible as well. Our sample application will
have one main window, which will host our views. The views are hosted within a `Frame` that is contained in the window. This makes scenarios possible, where
the window can have its own UI components. The navigation sub-system of the MVVM Framework automatically detects the `Frame` within a window. If no `Frame` is
found, then the window does not support navigation. So please open the `MainWindow.xaml` that was created when the project was created and add a frame as its
only control:

```xaml
<Window x:Class="System.Windows.Mvvm.Sample.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:System.Windows.Mvvm.Sample"
        mc:Ignorable="d" Title="MVVM Framework Sample" Height="350" Width="525">
    <Frame />
</Window>
```

Now we have to create the main view, which displays our todo list. All views have to be of type `Page` (or be derived from `Page`). So please go ahead and add the
`MainView.xaml`. At first you have to tell the MVVM Framework which view model belongs to the view, so that the correct view model can be instantiated when navigating
to the view. This is done via the `ViewModelAttribute`:

```csharp
[ViewModel(typeof(MainViewModel))]
public partial class MainView : Page
{
    public MainView()
    {
        InitializeComponent();
    }
}
```

Now go ahead and open the `MainView.xaml` view and add the following content:

```xaml
<Page x:Class="System.Windows.Mvvm.Sample.Views.MainView"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
      xmlns:local="clr-namespace:System.Windows.Mvvm.Sample.Views"
      mc:Ignorable="d" d:DesignHeight="300" d:DesignWidth="300">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <ListBox ItemsSource="{Binding Path=TodoListItems}" SelectedItem="{Binding Path=SelectedTodoListItem}" IsSynchronizedWithCurrentItem="True" Grid.Row="0">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Vertical">
                        <TextBlock FontSize="26" FontWeight="Bold" Text="{Binding Path=Title}" Visibility="{Binding Path=IsFinished, Converter={StaticResource InvertedBooleanToVisibilityConverter}}" />
                        <TextBlock FontSize="26" FontWeight="Bold" Text="{Binding Path=Title}" TextDecorations="Strikethrough" Visibility="{Binding Path=IsFinished, Converter={StaticResource BooleanToVisibilityConverter}}" />

                        <TextBlock FontSize="18" Text="{Binding Path=Description}" Visibility="{Binding Path=IsFinished, Converter={StaticResource InvertedBooleanToVisibilityConverter}}" />
                        <TextBlock FontSize="18" Text="{Binding Path=Description}" TextDecorations="Strikethrough" Visibility="{Binding Path=IsFinished, Converter={StaticResource BooleanToVisibilityConverter}}" />
                    </StackPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>

        <StackPanel Orientation="Horizontal" Grid.Row="1" HorizontalAlignment="Right">
            <Button Command="{Binding Path=MarkTodoListItemAsFinishedCommand}" Padding="5" Margin="10">Mark as finished</Button>
            <Button Command="{Binding Path=RemoveTodoListItemCommand}" Padding="5" Margin="10">Remove</Button>
        </StackPanel>
    </Grid>
</Page>
```

As you can see, we're using some value converters. The MVVM Framework comes with a small selection of built-in value converters. In order to be able to use them
application-wide, you'll have to add them to the application resources, so go ahead, open the `App.xaml` file and apply the following changes (do not forgot to add the
new namespace):

```xaml
<mvvm:MvvmApplication x:Class="System.Windows.Mvvm.Sample.App"
                      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                      xmlns:local="clr-namespace:System.Windows.Mvvm.Sample"
                      xmlns:mvvm="clr-namespace:System.Windows.Mvvm.Application;assembly=System.Windows.Mvvm.Application"
                      xmlns:controls="clr-namespace:System.Windows.Controls;assembly=System.Windows.Mvvm.UI">
    <mvvm:MvvmApplication.Resources>
        <ResourceDictionary>

            <BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" />
            <controls:InvertedBooleanToVisibilityConverter x:Key="InvertedBooleanToVisibilityConverter" />

        </ResourceDictionary>
    </mvvm:MvvmApplication.Resources>
</mvvm:MvvmApplication>
```

Finally we have to make an initial navigation to the view, which has to be done in the `OnStartedAsync` callback method in the `App.xaml.cs`. You have to specify the
type of the view and the window to which you want to navigate. The navigation sub-system automatically figures out how to instantiate the window, the page, and the
view model. It also automatically injects all services that the view model requires in its constructor. To use the navigation sub-system, the `WindowNavigationService`
has to be bound to the IoC container. When navigating an (anonymous) object with parameters can be passed to the view model. The properties in this object are automatically
matched by property name and assigned to the public properties of the view model. The second parameter of the `NavigateAsync` method determines whether the window is
the main window of the application. The main window has the same lifetime as the application, once the main window is closed, the application is shut down as well
(this can also be configured in the `App.xaml` via the `ShutdownMode` property).

```csharp
protected override async Task OnStartedAsync(ApplicationStartedEventArgs eventArguments)
{
    this.iocContainer = new SimpleIocContainer();

    this.iocContainer.RegisterType<IReadOnlyIocContainer>(() => this.iocContainer);
    this.iocContainer.RegisterType<TodoListItemsRepository>(Scope.Singleton);
    this.iocContainer.RegisterType<WindowNavigationService>(Scope.Singleton);

    WindowNavigationService windowNavigationService = this.iocContainer.GetInstance<WindowNavigationService>();
    await windowNavigationService.NavigateAsync<MainWindow, MainView>(null, true);
}
```

As you have probably noticed, we are binding the IoC container to itself. The navigation sub-system actually does not care which IoC container we are using, therefore,
the IoC container itself is injected into it. In order to be able to instantiate the `WindowNavigationService`, the IoC container has to be bound so that it can be
injected into the navigation sub-system.

Now we are ready to test our sample application for the first time. After starting the application, you should see something like this:

![Sample application main view](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/SampleApplicationMainView.jpg "Sample application main view")

## The Navigation Service

The navigation sub-system of the MVVM Framework is comprised of two services: `WindowNavigationService` and `NavigationService`. The `WindowNavigationService` is for creating
new windows and opening them. When a window supports navigation (which is when it contains a `Frame`), then the window is assigned an instance of `NavigationService`,
which can be used for navigation between views *within* the window.

The navigation sub-system is also the component that takes care of instantiating view models, and injecting any dependencies into it. It also automatically injects the correct
`NavigationService` for the current window into the view model of the corresponding `Page` being displayed in the window, if required. This makes navigation extremely easy, if
you want to navigate to a new window, then just use the `WindowNavigationService`, if you want to navigate to another view within the current window, then use the `NavigationService`.

Lets say we want to create a second view, which lets the user create new todo list items. Then we would create a new view model (`CreateTodoListItemViewModel.cs`) for the view.
In order to use the `NavigationService` for the current window, we would let the navigation sub-system inject it into the constructor of our new view model:

```csharp
public class CreateTodoListItemViewModel : ReactiveViewModel
{
    public CreateTodoListItemViewModel(NavigationService navigationService, TodoListItemsRepository todoListItemsRepository)
    {
        this.navigationService = navigationService;
        this.todoListItemsRepository = todoListItemsRepository;
    }

    private readonly NavigationService navigationService;
    private readonly TodoListItemsRepository todoListItemsRepository;

    private string title;
    public string Title
    {
        get {  return this.title; }
        set { this.RaiseAndSetIfChanged(ref this.title, value); }
    }

    private string description;
    public string Description
    {
        get { return this.description; }
        set { this.RaiseAndSetIfChanged(ref this.description, value); }
    }

    public ReactiveCommand<NavigationResult> CancelCommand { get; private set; }
    public ReactiveCommand<NavigationResult> SaveCommand { get; private set; }

    public override Task OnActivateAsync()
    {
        this.CancelCommand = ReactiveCommand.CreateAsyncTask(async x => await this.navigationService.NavigateBackAsync());

        this.SaveCommand = ReactiveCommand.CreateAsyncTask(async x =>
        {
            this.todoListItemsRepository.CreateTodoListItem(this.Title, this.Description);
            return await this.navigationService.NavigateBackAsync();
        });

        return Task.FromResult(0);
    }
}
```

In this sample we use the `NavigateBackAsync` to navigate back to the main view of the application. Now we need to create a new view for creating new todo list items:

```xaml
<Page x:Class="System.Windows.Mvvm.Sample.Views.CreateTodoListItemView"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
      xmlns:local="clr-namespace:System.Windows.Mvvm.Sample.Views"
      mc:Ignorable="d" d:DesignHeight="130" d:DesignWidth="300">

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <TextBlock FontSize="18" Grid.Column="0" Grid.ColumnSpan="2" Grid.Row="0">Create new todo list item</TextBlock>

        <TextBlock Grid.Column="0" Grid.Row="1" HorizontalAlignment="Left" VerticalAlignment="Center">Title</TextBlock>
        <TextBox Grid.Column="1" Grid.Row="1" Margin="2" Text="{Binding Path=Title}"></TextBox>

        <TextBlock Grid.Column="0" Grid.Row="2" HorizontalAlignment="Left" VerticalAlignment="Center">Description</TextBlock>
        <TextBox Grid.Column="1" Grid.Row="2" Margin="2" Text="{Binding Path=Description}"></TextBox>

        <StackPanel Orientation="Horizontal" Grid.Column="0" Grid.ColumnSpan="2" Grid.Row="4" HorizontalAlignment="Right">
            <Button Command="{Binding Path=CancelCommand}" Padding="5" Margin="10">Cancel</Button>
            <Button Command="{Binding Path=SaveCommand}" Padding="5" Margin="10">Save</Button>
        </StackPanel>
    </Grid>
</Page>
```

Don't forget to specify the view model for the view in the `CreateTodoListItemView.xaml.cs`:

```csharp
[ViewModel(typeof(CreateTodoListItemViewModel))]
public partial class CreateTodoListItemView : Page
{
    public CreateTodoListItemView()
    {
        InitializeComponent();
    }
}
```

Now we just need to add the code for the navigation to the `MainViewModel`. We need to adapt the constructor, so that we can get a hold of the `NavigationService` for
the window in which the main view is displayed:

```csharp
public MainViewModel(NavigationService navigationService, TodoListItemsRepository todoListItemsRepository)
{
    this.navigationService = navigationService;
    this.todoListItemsRepository = todoListItemsRepository;
}

private readonly NavigationService navigationService;
private readonly TodoListItemsRepository todoListItemsRepository;
```

Then you need to add another command to the `MainViewModel`:

```csharp
public ReactiveCommand<NavigationResult> CreateTodoListItemCommand { get; private set; }

public override Task OnActivateAsync()
{
    this.MarkTodoListItemAsFinishedCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedTodoListItem).Select(x => this.SelectedTodoListItem != null), x =>
    {
        this.SelectedTodoListItem.IsFinished = true;
        this.todoListItemsRepository.MarkTodoListItemAsFinished(this.SelectedTodoListItem.Id);
        return Task.FromResult(Unit.Default);
    });

    this.RemoveTodoListItemCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedTodoListItem).Select(x => this.SelectedTodoListItem != null), x =>
    {
        this.TodoListItems.Remove(this.SelectedTodoListItem);
        this.todoListItemsRepository.RemoveTodoListItem(this.SelectedTodoListItem.Id);
        this.SelectedTodoListItem = null;
        return Task.FromResult(Unit.Default);
    });

    this.CreateTodoListItemCommand = ReactiveCommand.CreateAsyncTask(async x =>  await this.navigationService.NavigateAsync<CreateTodoListItemView>());

    return Task.FromResult(0);
}
```

Here we use the `NavigateAsync` method. You have to specify the type of the view as the generic parameter. Optionally you could pass an (anonymous) object with parameters to the
`NavigateAsync` method. The properties in this object are automatically matched by property name and assigned to the public properties of the view model.

Finally, don't forget to add a new button to the main view, which navigates the user to the creation view:

```xaml
<StackPanel Orientation="Horizontal" Grid.Row="1" HorizontalAlignment="Right">
    <Button Command="{Binding Path=CreateTodoListItemCommand}" Padding="5" Margin="10">Create new item</Button>
    <Button Command="{Binding Path=MarkTodoListItemAsFinishedCommand}" Padding="5" Margin="10">Mark as finished</Button>
    <Button Command="{Binding Path=RemoveTodoListItemCommand}" Padding="5" Margin="10">Remove</Button>
</StackPanel>
```

The end result should look something like this:

![Sample application create todo list item view](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/SampleApplicationCreateTodoListItemView.jpg "Sample application create todo list item view")

## The Application Service

The `WindowNavigationService` and the `NavigationService` are not the only services that come with the MVVM Framework. The MVVM Framework provides a wide array of services,
that are helpful to all kinds of applications. One such service is the `ApplicationService`, which provides helpful lifecycle events, that can be subscribed to from anywhere
in the application. It also provides some utility methods to shutdown and restart the application.

We'll use the `ApplicationService` to add a quit button to the application. This is also the perfect moment to introduce you to another helpful feature of the MVVM Framework:
window view models. Windows may, just as views, have their own view model. As stated above, windows may have more UI than just the `Frame` for hosting views. We'll add the
quit button directly to the `MainWindow` because from there it is accessible anywhere in the application.

At first we have to add a new view model for the `MainWindow` to the project - `MainWindowViewModel`:

```csharp
public class MainWindowViewModel : ReactiveViewModel
{
    public MainWindowViewModel(ApplicationService applicationService)
    {
        this.applicationService = applicationService;
    }

    private readonly ApplicationService applicationService;

    public ReactiveCommand<Unit> ShutdownApplicationCommand { get; private set; }

    public override Task OnActivateAsync()
    {
        this.ShutdownApplicationCommand = ReactiveCommand.CreateAsyncTask(x =>
        {
            this.applicationService.Shutdown();
            return Task.FromResult(Unit.Default);
        });
        return Task.FromResult(0);
    }
}
```

In order to be able to get the `ApplicationService` injected, it must be bound to the IoC container, so head over to the `App.xaml.cs` and add the following line to the
`OnStartedAsync` method:

```csharp
this.iocContainer.RegisterType<ApplicationService>(Scope.Singleton);
```

Finally we have to add the quit button to the `MainWindow`:

```xaml
<Window x:Class="System.Windows.Mvvm.Sample.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:System.Windows.Mvvm.Sample"
        mc:Ignorable="d" Title="MVVM Framework Sample" Height="350" Width="525">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <Frame Grid.Row="0" />

        <StackPanel Orientation="Horizontal" Grid.Row="1" HorizontalAlignment="Right">
            <Button Command="{Binding Path=ShutdownApplicationCommand}" Padding="5" Margin="10">Quit</Button>
        </StackPanel>
    </Grid>
</Window>
```

Do not forget to tell the navigation sub-system which view model to use for the `MainWindow`:

```csharp
[ViewModel(typeof(MainWindowViewModel))]
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

The finished result should look like this:

![Sample application main window](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/SampleApplicationMainWindow.jpg "Sample application main window")

## The Dialog Service

There is another useful service that the MVVM Framework has in store for you: the `DialogService`. The `DialogService` provides an easy to use API for the
various dialogs, e.g. message box, open file dialog, and open directory dialog, that Windows offers.

In this last section we want to use the dialog service to ask users if they really want to cancel the creation of a todo list item. By now you probably know the
drill: in order to use the `DialogService` you have to bind it to the IoC container in the `OnStartedAsync` method of the `App` class. Just add the following line to the
`OnStartedAsync` method:

```csharp
this.iocContainer.RegisterType<DialogService>(Scope.Singleton);
```

Now we can get it injected into the constructor of the `CreateTodoListItemViewModel`:

```csharp
public CreateTodoListItemViewModel(NavigationService navigationService, DialogService dialogService, TodoListItemsRepository todoListItemsRepository)
{
    this.navigationService = navigationService;
    this.dialogService = dialogService;
    this.todoListItemsRepository = todoListItemsRepository;
}

private readonly NavigationService navigationService;
private readonly DialogService dialogService;
private readonly TodoListItemsRepository todoListItemsRepository;
```

Finally we can adapt the `CancelCommand` to use the dialog service:

```csharp
public ReactiveCommand<Unit> CancelCommand { get; private set; }
public ReactiveCommand<NavigationResult> SaveCommand { get; private set; }

public override Task OnActivateAsync()
{
    this.CancelCommand = ReactiveCommand.CreateAsyncTask(async x =>
    {
        if (await this.dialogService.ShowMessageBoxDialogAsync("Do you really want to cancel the creation of the todo list item.", "Confirm cancellation", Services.Dialog.MessageBoxButton.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            await this.navigationService.NavigateBackAsync();
    });

    this.SaveCommand = ReactiveCommand.CreateAsyncTask(async x =>
    {
        this.todoListItemsRepository.CreateTodoListItem(this.Title, this.Description);
        return await this.navigationService.NavigateBackAsync();
    });

    return Task.FromResult(0);
}
```

The finished result should look like this:

![Demonstrating the dialog service](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Images/DialogService.jpg "Demonstrating the dialog service")

# The Broader Picture

Now you should be equipped to start developing applications using the MVVM Framework. But there are many more features that come with the MVVM Framework that are out
of scope of this quick start guide. If you want to dive deeper, head over to the [Documentation](https://github.com/lecode-official/mvvm-framework/blob/master/Documentation/Documentation.md),
which has detailed instructions for all features of the MVVM Framework.