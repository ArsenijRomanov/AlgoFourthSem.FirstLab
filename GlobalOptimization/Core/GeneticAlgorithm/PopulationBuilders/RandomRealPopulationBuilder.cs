using GlobalOptimization.Core.Function;
using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;

namespace GlobalOptimization.Core.GeneticAlgorithm.PopulationBuilders;

public sealed class RandomRealPopulationBuilder(
    Random random,
    IGenomeInterpreter<RealGenome> interpreter,
    SearchSpace searchSpace)
    : IPopulationBuilder<RealGenome>
{
    private readonly Random _random = random ?? throw new ArgumentNullException(nameof(random));
    private readonly IGenomeInterpreter<RealGenome> _interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));
    private readonly SearchSpace _searchSpace = searchSpace ?? throw new ArgumentNullException(nameof(searchSpace));

    public IReadOnlyList<Individual<RealGenome>> BuildPopulation(int populationSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(populationSize);

        var population = new List<Individual<RealGenome>>(populationSize);

        for (var i = 0; i < populationSize; i++)
        {
            var genes = new double[_searchSpace.Dimension];

            for (var j = 0; j < _searchSpace.Dimension; j++)
            {
                var (min, max) = _searchSpace.Bounds[j];
                genes[j] = min + _random.NextDouble() * (max - min);
            }

            var genome = new RealGenome(genes, clone: false);
            population.Add(_interpreter.CreateIndividual(genome));
        }

        return population.AsReadOnly();
    }
}