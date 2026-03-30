using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;

namespace GlobalOptimization.Core.GeneticAlgorithm.Crossovers;

public sealed class OnePointBinaryCrossover : ICrossover<BinaryGenome>
{
    private readonly Random _random;
    private readonly double _crossoverProbability;

    public OnePointBinaryCrossover(Random random, double crossoverProbability)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));

        if (crossoverProbability is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(crossoverProbability));

        _crossoverProbability = crossoverProbability;
    }

    public (BinaryGenome Child1, BinaryGenome Child2) Cross(
        BinaryGenome parent1,
        BinaryGenome parent2)
    {
        ArgumentNullException.ThrowIfNull(parent1);
        ArgumentNullException.ThrowIfNull(parent2);

        if (parent1.Length != parent2.Length)
            throw new InvalidOperationException("Длины бинарных геномов должны совпадать.");

        var child1 = parent1.Clone();
        var child2 = parent2.Clone();

        if (parent1.Length < 2 || _random.NextDouble() > _crossoverProbability)
            return (child1, child2);

        int cutPoint = _random.Next(1, parent1.Length);

        for (int i = cutPoint; i < parent1.Length; i++)
        {
            (child1[i], child2[i]) = (child2[i], child1[i]);
        }

        return (child1, child2);
    }
}