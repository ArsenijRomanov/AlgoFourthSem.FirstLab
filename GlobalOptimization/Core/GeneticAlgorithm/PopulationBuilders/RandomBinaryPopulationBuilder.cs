using GlobalOptimization.Core.Function;
using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;

namespace GlobalOptimization.Core.GeneticAlgorithm.PopulationBuilders;

public sealed class RandomBinaryPopulationBuilder : IPopulationBuilder<BinaryGenome>
{
    private readonly Random _random;
    private readonly IGenomeInterpreter<BinaryGenome> _interpreter;
    private readonly int _genomeLength;

    public RandomBinaryPopulationBuilder(
        Random random,
        IGenomeInterpreter<BinaryGenome> interpreter,
        SearchSpace searchSpace,
        int bitsPerCoordinate)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));

        ArgumentNullException.ThrowIfNull(searchSpace);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitsPerCoordinate);

        _genomeLength = searchSpace.Dimension * bitsPerCoordinate;
    }

    public IReadOnlyList<Individual<BinaryGenome>> BuildPopulation(int populationSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(populationSize);

        var population = new List<Individual<BinaryGenome>>(populationSize);

        for (var i = 0; i < populationSize; i++)
        {
            var bits = new bool[_genomeLength];

            for (var j = 0; j < _genomeLength; j++)
                bits[j] = _random.NextDouble() < 0.5;

            var genome = new BinaryGenome(bits, clone: false);
            population.Add(_interpreter.CreateIndividual(genome));
        }

        return population.AsReadOnly();
    }
}
