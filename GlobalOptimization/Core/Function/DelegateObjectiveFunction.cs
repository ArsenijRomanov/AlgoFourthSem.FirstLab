namespace GlobalOptimization.Core.Function;

public sealed class DelegateObjectiveFunction : IObjectiveFunction
{
    private readonly Func<IReadOnlyList<double>, double> _evaluate;

    public string Name { get; }
    public int Dimension { get; }

    public DelegateObjectiveFunction(
        string name,
        int dimension,
        Func<IReadOnlyList<double>, double> evaluate)
    {
        Name = name;
        Dimension = dimension;
        _evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
    }

    public double Evaluate(IReadOnlyList<double> coordinates)
    {
        if (coordinates.Count != Dimension)
            throw new ArgumentException("Неверная размерность точки.", nameof(coordinates));

        return _evaluate(coordinates);
    }
}
