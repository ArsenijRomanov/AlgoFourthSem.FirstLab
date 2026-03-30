namespace GlobalOptimization.Avalonia.Models;

public sealed record ScatterPointModel(
    double X,
    double Y,
    double Fitness,
    bool IsBest = false);
