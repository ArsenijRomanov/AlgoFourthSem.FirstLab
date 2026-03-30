namespace GlobalOptimization.Avalonia.Models;

public sealed class ResultPointRowViewModel
{
    public int Rank { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Fitness { get; init; }
    public bool IsBest { get; init; }
}
