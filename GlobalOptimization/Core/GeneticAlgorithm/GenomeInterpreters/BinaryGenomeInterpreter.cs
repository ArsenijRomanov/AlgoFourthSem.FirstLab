using GlobalOptimization.Core.Function;
using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;

namespace GlobalOptimization.Core.GeneticAlgorithm.GenomeInterpreters;

public sealed class BinaryGenomeInterpreter : IGenomeInterpreter<BinaryGenome>
{
    private readonly IObjectiveFunction _objectiveFunction;
    private readonly SearchSpace _searchSpace;
    private readonly int _bitsPerCoordinate;

    public int EvaluationCount { get; private set; }

    public BinaryGenomeInterpreter(
        IObjectiveFunction objectiveFunction,
        SearchSpace searchSpace,
        int bitsPerCoordinate)
    {
        _objectiveFunction = objectiveFunction ?? throw new ArgumentNullException(nameof(objectiveFunction));
        _searchSpace = searchSpace ?? throw new ArgumentNullException(nameof(searchSpace));

        if (bitsPerCoordinate <= 0)
            throw new ArgumentOutOfRangeException(nameof(bitsPerCoordinate));

        if (_objectiveFunction.Dimension != _searchSpace.Dimension)
            throw new ArgumentException("Размерность функции и области поиска должна совпадать.");

        _bitsPerCoordinate = bitsPerCoordinate;
    }

    public double Evaluate(BinaryGenome genome)
    {
        if (genome is null)
            throw new ArgumentNullException(nameof(genome));

        var coordinates = GetCoordinates(genome);
        EvaluationCount++;
        return _objectiveFunction.Evaluate(coordinates);
    }

    public IReadOnlyList<double> GetCoordinates(BinaryGenome genome)
    {
        if (genome is null)
            throw new ArgumentNullException(nameof(genome));

        int dimension = _searchSpace.Dimension;
        int expectedLength = dimension * _bitsPerCoordinate;

        if (genome.Length != expectedLength)
        {
            throw new InvalidOperationException(
                $"Ожидалась длина бинарного генома {expectedLength}, получено {genome.Length}.");
        }

        var coordinates = new double[dimension];
        double maxEncodedValue = Math.Pow(2, _bitsPerCoordinate) - 1.0;

        for (int dim = 0; dim < dimension; dim++)
        {
            double encodedValue = 0.0;
            int offset = dim * _bitsPerCoordinate;

            for (int bit = 0; bit < _bitsPerCoordinate; bit++)
            {
                if (genome[offset + bit])
                    encodedValue += Math.Pow(2, _bitsPerCoordinate - 1 - bit);
            }

            var (min, max) = _searchSpace.Bounds[dim];
            coordinates[dim] = min + (encodedValue / maxEncodedValue) * (max - min);
        }

        return coordinates;
    }

    public Individual<BinaryGenome> CreateIndividual(BinaryGenome genome)
    {
        if (genome is null)
            throw new ArgumentNullException(nameof(genome));

        var fitness = Evaluate(genome);
        return new Individual<BinaryGenome>(genome, fitness);
    }
    
    public void ResetStatistics()
        => EvaluationCount = 0;
}