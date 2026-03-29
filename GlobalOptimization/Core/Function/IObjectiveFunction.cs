namespace GlobalOptimization.Core.Function;

public interface IObjectiveFunction
{
    string Name { get; }
    int Dimension { get; }
    double Evaluate(IReadOnlyList<double> coordinates);
}
