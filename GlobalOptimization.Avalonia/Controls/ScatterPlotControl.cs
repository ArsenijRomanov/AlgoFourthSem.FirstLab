using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GlobalOptimization.Avalonia.Models;

namespace GlobalOptimization.Avalonia.Controls;

public sealed class ScatterPlotControl : Control
{
    private const double OuterLeftMargin = 28;
    private const double OuterTopMargin = 16;
    private const double OuterRightMargin = 16;
    private const double OuterBottomMargin = 16;

    private const double PlotLeftMargin = 56;
    private const double PlotTopMargin = 12;
    private const double PlotRightMargin = 18;
    private const double PlotBottomMargin = 42;

    private INotifyCollectionChanged? _collection;

    public static readonly StyledProperty<IEnumerable<ScatterPointModel>?> PointsProperty =
        AvaloniaProperty.Register<ScatterPlotControl, IEnumerable<ScatterPointModel>?>(nameof(Points));

    public static readonly StyledProperty<ScatterPointModel?> HighlightedPointProperty =
        AvaloniaProperty.Register<ScatterPlotControl, ScatterPointModel?>(nameof(HighlightedPoint));

    public static readonly StyledProperty<decimal> WorldXMinProperty =
        AvaloniaProperty.Register<ScatterPlotControl, decimal>(nameof(WorldXMin), -6m);

    public static readonly StyledProperty<decimal> WorldXMaxProperty =
        AvaloniaProperty.Register<ScatterPlotControl, decimal>(nameof(WorldXMax), 6m);

    public static readonly StyledProperty<decimal> WorldYMinProperty =
        AvaloniaProperty.Register<ScatterPlotControl, decimal>(nameof(WorldYMin), -6m);

    public static readonly StyledProperty<decimal> WorldYMaxProperty =
        AvaloniaProperty.Register<ScatterPlotControl, decimal>(nameof(WorldYMax), 6m);

    private double _zoom = 1.0;
    private Vector _pan = default;
    private bool _isDragging;
    private Point _lastPointerPosition;

    public IEnumerable<ScatterPointModel>? Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public ScatterPointModel? HighlightedPoint
    {
        get => GetValue(HighlightedPointProperty);
        set => SetValue(HighlightedPointProperty, value);
    }

    public decimal WorldXMin
    {
        get => GetValue(WorldXMinProperty);
        set => SetValue(WorldXMinProperty, value);
    }

    public decimal WorldXMax
    {
        get => GetValue(WorldXMaxProperty);
        set => SetValue(WorldXMaxProperty, value);
    }

    public decimal WorldYMin
    {
        get => GetValue(WorldYMinProperty);
        set => SetValue(WorldYMinProperty, value);
    }

    public decimal WorldYMax
    {
        get => GetValue(WorldYMaxProperty);
        set => SetValue(WorldYMaxProperty, value);
    }

    public ScatterPlotControl()
    {
        ClipToBounds = true;
        Focusable = true;
    }

    public void ZoomIn()
    {
        ApplyZoom(1.15, GetPlotRect().Center);
    }

    public void ZoomOut()
    {
        ApplyZoom(1.0 / 1.15, GetPlotRect().Center);
    }

    public void ResetView()
    {
        _zoom = 1.0;
        _pan = default;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;

        var controlBackground = new SolidColorBrush(Color.Parse("#07111D"));
        var controlBorderPen = new Pen(new SolidColorBrush(Color.Parse("#2C3E50")));
        var plotBackground = new SolidColorBrush(Color.Parse("#10161F"));
        var plotBorderPen = new Pen(new SolidColorBrush(Color.Parse("#34495E")));
        var gridPen = new Pen(new SolidColorBrush(Color.Parse("#2C3E50")));
        var axesPen = new Pen(new SolidColorBrush(Color.Parse("#BFC9CA")), 1.6);

        context.DrawRectangle(controlBackground, controlBorderPen, bounds);

        if (bounds.Width <= 1 || bounds.Height <= 1)
            return;

        var plotRect = GetPlotRect();
        context.DrawRectangle(plotBackground, plotBorderPen, plotRect);

        DrawGridAndAxes(context, plotRect, gridPen, axesPen);

        var points = Points?.ToList() ?? new List<ScatterPointModel>();

        foreach (var point in points)
            DrawPoint(context, plotRect, point);

        if (HighlightedPoint is not null)
            DrawHighlightedPoint(context, plotRect, HighlightedPoint);

        var world = GetWorldBounds();
        var footer = BuildText(
            $"X: [{world.Left:G4}; {world.Right:G4}]   Y: [{world.Top:G4}; {world.Bottom:G4}]",
            Brushes.Gainsboro);

        context.DrawText(footer, new Point(12, Bounds.Height - 24));
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

        if (change.Property == HighlightedPointProperty)
            InvalidateVisual();

        if (change.Property == WorldXMinProperty ||
            change.Property == WorldXMaxProperty ||
            change.Property == WorldYMinProperty ||
            change.Property == WorldYMaxProperty)
        {
            ResetView();
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        double factor = e.Delta.Y >= 0 ? 1.15 : 1.0 / 1.15;
        ApplyZoom(factor, e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _isDragging = true;
        _lastPointerPosition = e.GetPosition(this);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!_isDragging)
            return;

        _isDragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_isDragging)
            return;

        var position = e.GetPosition(this);
        var delta = position - _lastPointerPosition;
        _lastPointerPosition = position;

        _pan += delta;
        InvalidateVisual();
        e.Handled = true;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void ApplyZoom(double factor, Point screenCenter)
    {
        var plotRect = GetPlotRect();
        if (!plotRect.Contains(screenCenter))
            screenCenter = plotRect.Center;

        var before = ScreenToWorld(screenCenter);

        _zoom = Math.Clamp(_zoom * factor, 0.25, 50.0);

        var after = WorldToScreen(before.X, before.Y);
        _pan += screenCenter - after;

        InvalidateVisual();
    }

    private Rect GetWorldBounds()
    {
        var xMin = (double)WorldXMin;
        var xMax = (double)WorldXMax;
        var yMin = (double)WorldYMin;
        var yMax = (double)WorldYMax;

        if (xMax <= xMin)
            xMax = xMin + 1.0;

        if (yMax <= yMin)
            yMax = yMin + 1.0;

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private Rect GetPlotRect()
    {
        double availableLeft = OuterLeftMargin + PlotLeftMargin;
        double availableTop = OuterTopMargin + PlotTopMargin;
        double availableWidth = Math.Max(10, Bounds.Width - availableLeft - PlotRightMargin - OuterRightMargin);
        double availableHeight = Math.Max(10, Bounds.Height - availableTop - PlotBottomMargin - OuterBottomMargin);

        double side = Math.Max(10, Math.Min(availableWidth, availableHeight));

        double left = availableLeft + (availableWidth - side) / 2.0;
        double top = availableTop + (availableHeight - side) / 2.0;

        return new Rect(left, top, side, side);
    }

    private void DrawGridAndAxes(
        DrawingContext context,
        Rect plotRect,
        Pen gridPen,
        Pen axesPen)
    {
        var visible = GetVisibleWorldBounds(plotRect);

        double xStep = GetNiceStep(visible.Width / 10.0);
        double yStep = GetNiceStep(visible.Height / 10.0);

        double xStart = Math.Ceiling(visible.Left / xStep) * xStep;
        double yStart = Math.Ceiling(visible.Top / yStep) * yStep;

        for (double x = xStart; x <= visible.Right + 1e-9; x += xStep)
        {
            double sx = WorldToScreen(x, 0).X;

            if (sx < plotRect.Left - 1 || sx > plotRect.Right + 1)
                continue;

            bool isAxis = Math.Abs(x) < 1e-9;
            context.DrawLine(
                isAxis ? axesPen : gridPen,
                new Point(sx, plotRect.Top),
                new Point(sx, plotRect.Bottom));

            var label = BuildText(FormatAxisValue(x), Brushes.Gainsboro);
            context.DrawText(label, new Point(sx - label.Width / 2.0, plotRect.Bottom + 6));
        }

        for (double y = yStart; y <= visible.Bottom + 1e-9; y += yStep)
        {
            double sy = WorldToScreen(0, y).Y;

            if (sy < plotRect.Top - 1 || sy > plotRect.Bottom + 1)
                continue;

            bool isAxis = Math.Abs(y) < 1e-9;
            context.DrawLine(
                isAxis ? axesPen : gridPen,
                new Point(plotRect.Left, sy),
                new Point(plotRect.Right, sy));

            var label = BuildText(FormatAxisValue(y), Brushes.Gainsboro);
            context.DrawText(label, new Point(plotRect.Left - label.Width - 8, sy - label.Height / 2.0));
        }
    }

    private void DrawPoint(DrawingContext context, Rect plotRect, ScatterPointModel point)
    {
        var position = WorldToScreen(point.X, point.Y);

        if (!plotRect.Contains(position))
            return;

        double radius = point.IsBest ? 4.5 : 3.0;

        IBrush fill = point.IsBest
            ? Brushes.Gold
            : new SolidColorBrush(Color.Parse("#5DADE2"));

        context.DrawEllipse(fill, null, position, radius, radius);
    }

    private void DrawHighlightedPoint(DrawingContext context, Rect plotRect, ScatterPointModel point)
    {
        var position = WorldToScreen(point.X, point.Y);

        if (!plotRect.Contains(position))
            return;

        IBrush fill = new SolidColorBrush(Color.Parse("#E74C3C"));
        var pen = new Pen(Brushes.White, 1.5);

        context.DrawEllipse(fill, pen, position, 7, 7);
        context.DrawLine(pen, new Point(position.X - 10, position.Y), new Point(position.X + 10, position.Y));
        context.DrawLine(pen, new Point(position.X, position.Y - 10), new Point(position.X, position.Y + 10));

        var text = BuildText($"best = ({point.X:G6}; {point.Y:G6})", Brushes.White);
        context.DrawText(text, new Point(position.X + 10, position.Y - 24));
    }

    private Rect GetVisibleWorldBounds(Rect plotRect)
    {
        var topLeft = ScreenToWorld(new Point(plotRect.Left, plotRect.Top));
        var bottomRight = ScreenToWorld(new Point(plotRect.Right, plotRect.Bottom));

        double xMin = Math.Min(topLeft.X, bottomRight.X);
        double xMax = Math.Max(topLeft.X, bottomRight.X);
        double yMin = Math.Min(bottomRight.Y, topLeft.Y);
        double yMax = Math.Max(bottomRight.Y, topLeft.Y);

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private Point WorldToScreen(double x, double y)
    {
        var world = GetWorldBounds();
        var plotRect = GetPlotRect();

        double baseScale = Math.Min(plotRect.Width / world.Width, plotRect.Height / world.Height);
        if (double.IsNaN(baseScale) || double.IsInfinity(baseScale) || baseScale <= 0)
            baseScale = 1.0;

        double scale = baseScale * _zoom;

        double worldCenterX = world.Left + world.Width / 2.0;
        double worldCenterY = world.Top + world.Height / 2.0;

        double screenCenterX = plotRect.Left + plotRect.Width / 2.0 + _pan.X;
        double screenCenterY = plotRect.Top + plotRect.Height / 2.0 + _pan.Y;

        return new Point(
            screenCenterX + (x - worldCenterX) * scale,
            screenCenterY - (y - worldCenterY) * scale);
    }

    private Point ScreenToWorld(Point point)
    {
        var world = GetWorldBounds();
        var plotRect = GetPlotRect();

        double baseScale = Math.Min(plotRect.Width / world.Width, plotRect.Height / world.Height);
        if (double.IsNaN(baseScale) || double.IsInfinity(baseScale) || baseScale <= 0)
            baseScale = 1.0;

        double scale = baseScale * _zoom;

        double worldCenterX = world.Left + world.Width / 2.0;
        double worldCenterY = world.Top + world.Height / 2.0;

        double screenCenterX = plotRect.Left + plotRect.Width / 2.0 + _pan.X;
        double screenCenterY = plotRect.Top + plotRect.Height / 2.0 + _pan.Y;

        return new Point(
            worldCenterX + (point.X - screenCenterX) / scale,
            worldCenterY - (point.Y - screenCenterY) / scale);
    }

    private static double GetNiceStep(double rawStep)
    {
        if (rawStep <= 0 || double.IsNaN(rawStep) || double.IsInfinity(rawStep))
            return 1.0;

        double exponent = Math.Floor(Math.Log10(rawStep));
        double fraction = rawStep / Math.Pow(10.0, exponent);

        double niceFraction;
        if (fraction <= 1.0)
            niceFraction = 1.0;
        else if (fraction <= 2.0)
            niceFraction = 2.0;
        else if (fraction <= 5.0)
            niceFraction = 5.0;
        else
            niceFraction = 10.0;

        return niceFraction * Math.Pow(10.0, exponent);
    }

    private static string FormatAxisValue(double value)
    {
        if (Math.Abs(value) < 1e-12)
            value = 0;

        return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
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