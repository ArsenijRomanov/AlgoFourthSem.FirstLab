namespace GlobalOptimization.Avalonia.Models;

public sealed record ResultPoint(
    double X,
    double Y,
    double Fitness,
    bool IsBestCurrentPoint);
