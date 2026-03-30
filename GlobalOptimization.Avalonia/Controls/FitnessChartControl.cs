using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GlobalOptimization.Avalonia.Models;

namespace GlobalOptimization.Avalonia.Controls;

public sealed class FitnessChartControl : Control
{

    private INotifyCollectionChanged? _collection;


    public static readonly StyledProperty<IEnumerable<FitnessHistoryEntry>?> PointsProperty =
        AvaloniaProperty.Register<FitnessChartControl, IEnumerable<FitnessHistoryEntry>?>(nameof(Points));

    public IEnumerable<FitnessHistoryEntry>? Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        context.DrawRectangle(
            new SolidColorBrush(Color.Parse("#10161F")),
            new Pen(new SolidColorBrush(Color.Parse("#7F8C8D"))),
            bounds);

        var points = Points?.OrderBy(p => p.Iteration).ToList() ?? new List<FitnessHistoryEntry>();

        if (points.Count == 0)
        {
            var text = BuildText("История фитнеса пока пуста.", Brushes.Gainsboro);
            context.DrawText(text, new Point(20, 20));
            return;
        }

        double left = 55;
        double top = 20;
        double right = Math.Max(left + 10, bounds.Width - 20);
        double bottom = Math.Max(top + 10, bounds.Height - 45);

        var plotRect = new Rect(left, top, right - left, bottom - top);

        var xMin = points[0].Iteration;
        var xMax = Math.Max(points[^1].Iteration, xMin + 1);

        double yMin = points.Min(p => p.BestFitness);
        double yMax = points.Max(p => p.BestFitness);

        if (Math.Abs(yMax - yMin) < 1e-12)
        {
            yMin -= 1.0;
            yMax += 1.0;
        }
        else
        {
            double padding = (yMax - yMin) * 0.1;
            yMin -= padding;
            yMax += padding;
        }

        var gridPen = new Pen(new SolidColorBrush(Color.Parse("#2C3E50")));
        var axisPen = new Pen(new SolidColorBrush(Color.Parse("#95A5A6")), 1.5);
        var linePen = new Pen(new SolidColorBrush(Color.Parse("#58D68D")), 2.0);

        DrawGrid(context, plotRect, gridPen, axisPen, xMin, xMax, yMin, yMax);

        Point? previous = null;
        foreach (var entry in points)
        {
            var point = Map(entry.Iteration, entry.BestFitness, plotRect, xMin, xMax, yMin, yMax);

            if (previous is not null)
                context.DrawLine(linePen, previous.Value, point);

            context.DrawEllipse(Brushes.Gold, null, point, 4, 4);
            previous = point;
        }

        var last = points[^1];
        var lastPoint = Map(last.Iteration, last.BestFitness, plotRect, xMin, xMax, yMin, yMax);
        context.DrawText(
            BuildText($"iter = {last.Iteration}, best = {last.BestFitness:G8}", Brushes.White),
            new Point(lastPoint.X + 8, lastPoint.Y - 20));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PointsProperty)
        {
            if (_collection is not null)
                _collection.CollectionChanged -= OnCollectionChanged;

            _collection = change.NewValue as INotifyCollectionChanged;

            if (_collection is not null)
                _collection.CollectionChanged += OnCollectionChanged;

            InvalidateVisual();
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private static void DrawGrid(
        DrawingContext context,
        Rect plotRect,
        Pen gridPen,
        Pen axisPen,
        int xMin,
        int xMax,
        double yMin,
        double yMax)
    {
        const int horizontalLines = 5;
        const int verticalLines = 5;

        for (int i = 0; i <= verticalLines; i++)
        {
            double t = i / (double)verticalLines;
            double x = plotRect.Left + plotRect.Width * t;
            context.DrawLine(gridPen, new Point(x, plotRect.Top), new Point(x, plotRect.Bottom));

            int xValue = (int)Math.Round(xMin + (xMax - xMin) * t);
            context.DrawText(BuildText(xValue.ToString(), Brushes.Gainsboro), new Point(x - 10, plotRect.Bottom + 6));
        }

        for (int i = 0; i <= horizontalLines; i++)
        {
            double t = i / (double)horizontalLines;
            double y = plotRect.Top + plotRect.Height * t;
            context.DrawLine(gridPen, new Point(plotRect.Left, y), new Point(plotRect.Right, y));

            double yValue = yMax - (yMax - yMin) * t;
            context.DrawText(BuildText(yValue.ToString("G5"), Brushes.Gainsboro), new Point(6, y - 8));
        }

        context.DrawRectangle(null, axisPen, plotRect);
    }

    private static Point Map(
        int iteration,
        double fitness,
        Rect plotRect,
        int xMin,
        int xMax,
        double yMin,
        double yMax)
    {
        double xRatio = (iteration - xMin) / (double)(xMax - xMin);
        double yRatio = (fitness - yMin) / (yMax - yMin);

        return new Point(
            plotRect.Left + plotRect.Width * xRatio,
            plotRect.Bottom - plotRect.Height * yRatio);
    }

    private static FormattedText BuildText(string text, IBrush brush)
    {
        return new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default),
            12,
            brush);
    }
}
