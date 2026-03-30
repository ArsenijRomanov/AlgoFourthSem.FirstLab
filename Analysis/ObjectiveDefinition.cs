using GlobalOptimization.Core.Function;

namespace Analysis;

public sealed class ObjectiveDefinition
{
    public string Id { get; }
    public string Name { get; }
    public int Dimension { get; }
    public SearchSpace SearchSpace { get; }
    public IObjectiveFunction ObjectiveFunction { get; }
    public double KnownOptimalFitness { get; }

    public ObjectiveDefinition(
        string id,
        string name,
        int dimension,
        SearchSpace searchSpace,
        IObjectiveFunction objectiveFunction,
        double knownOptimalFitness)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id не может быть пустым.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name не может быть пустым.", nameof(name));

        ArgumentNullException.ThrowIfNull(searchSpace);
        ArgumentNullException.ThrowIfNull(objectiveFunction);

        if (dimension <= 0)
            throw new ArgumentOutOfRangeException(nameof(dimension));

        if (searchSpace.Dimension != dimension)
            throw new ArgumentException("Размерность SearchSpace должна совпадать с dimension.", nameof(searchSpace));

        if (objectiveFunction.Dimension != dimension)
            throw new ArgumentException("Размерность ObjectiveFunction должна совпадать с dimension.", nameof(objectiveFunction));

        Id = id;
        Name = name;
        Dimension = dimension;
        SearchSpace = searchSpace;
        ObjectiveFunction = objectiveFunction;
        KnownOptimalFitness = knownOptimalFitness;
    }
}
