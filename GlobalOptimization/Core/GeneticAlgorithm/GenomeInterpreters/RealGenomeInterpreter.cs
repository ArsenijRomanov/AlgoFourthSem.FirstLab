using GlobalOptimization.Core.Function;
using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;

namespace GlobalOptimization.Core.GeneticAlgorithm.GenomeInterpreters;

public sealed class RealGenomeInterpreter : IGenomeInterpreter<RealGenome>
{
    private readonly IObjectiveFunction _objectiveFunction;
    private readonly SearchSpace _searchSpace;

    public int EvaluationCount { get; private set; }

    public RealGenomeInterpreter(
        IObjectiveFunction objectiveFunction,
        SearchSpace searchSpace)
    {
        _objectiveFunction = objectiveFunction ?? throw new ArgumentNullException(nameof(objectiveFunction));
        _searchSpace = searchSpace ?? throw new ArgumentNullException(nameof(searchSpace));

        if (_objectiveFunction.Dimension != _searchSpace.Dimension)
            throw new ArgumentException("Размерность функции и области поиска должна совпадать.");
    }

    public double Evaluate(RealGenome genome)
    {
        if (genome is null)
            throw new ArgumentNullException(nameof(genome));

        var coordinates = GetCoordinates(genome);
        EvaluationCount++;
        return _objectiveFunction.Evaluate(coordinates);
    }

    public IReadOnlyList<double> GetCoordinates(RealGenome genome)
    {
        if (genome is null)
            throw new ArgumentNullException(nameof(genome));

        if (genome.Length != _searchSpace.Dimension)
        {
            throw new InvalidOperationException(
                $"Ожидалась длина вещественного генома {_searchSpace.Dimension}, получено {genome.Length}.");
        }

        var coordinates = new double[genome.Length];

        for (int i = 0; i < genome.Length; i++)
        {
            var (min, max) = _searchSpace.Bounds[i];
            double value = genome[i];

            if (value < min)
                value = min;
            else if (value > max)
                value = max;

            genome[i] = value;
            coordinates[i] = value;
        }

        return coordinates;
    }

    public Individual<RealGenome> CreateIndividual(RealGenome genome)
    {
        if (genome is null)
            throw new ArgumentNullException(nameof(genome));

        var fitness = Evaluate(genome);
        return new Individual<RealGenome>(genome, fitness);
    }
}