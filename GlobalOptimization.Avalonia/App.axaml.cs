using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GlobalOptimization.Avalonia.Services;
using GlobalOptimization.Avalonia.ViewModels;
using GlobalOptimization.Avalonia.Views;

namespace GlobalOptimization.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(new OptimizationSession())
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
