using Avalonia.Controls;
using Avalonia.Interactivity;
using GlobalOptimization.Avalonia.Controls;
using GlobalOptimization.Avalonia.ViewModels;

namespace GlobalOptimization.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private ScatterPlotControl? GetScatterPlot()
    {
        return this.FindControl<ScatterPlotControl>("ScatterPlot");
    }

    private void ZoomInPlotClick(object? sender, RoutedEventArgs e)
    {
        GetScatterPlot()?.ZoomIn();
    }

    private void ZoomOutPlotClick(object? sender, RoutedEventArgs e)
    {
        GetScatterPlot()?.ZoomOut();
    }

    private void ResetPlotViewClick(object? sender, RoutedEventArgs e)
    {
        GetScatterPlot()?.ResetView();
    }

    private void OpenFitnessHistoryClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel || !viewModel.HasHistory)
            return;

        var window = new FitnessHistoryWindow
        {
            DataContext = viewModel
        };

        window.Show(this);
    }
}