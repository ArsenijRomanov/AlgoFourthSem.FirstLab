namespace GlobalOptimization.Avalonia.Models;

public sealed class OptimizationRunResult
{
    public int Iteration { get; init; }
    public int EvaluationCount { get; init; }

    public double BestFitness { get; init; }
    public double BestX { get; init; }
    public double BestY { get; init; }

    public IReadOnlyList<ResultPoint> Points { get; init; } = Array.Empty<ResultPoint>();
    public ScatterPointModel? HighlightedPoint { get; init; }
}
