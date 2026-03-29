namespace GlobalOptimization.Core.Function;

public sealed class SearchSpace(IReadOnlyList<(double Min, double Max)> bounds)
{
    public IReadOnlyList<(double Min, double Max)> Bounds { get; } = bounds;

    public int Dimension => Bounds.Count;
}