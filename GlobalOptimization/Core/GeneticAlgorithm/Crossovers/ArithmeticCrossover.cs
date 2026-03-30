using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;

namespace GlobalOptimization.Core.GeneticAlgorithm.Crossovers;

public sealed class ArithmeticCrossover : ICrossover<RealGenome>
{
    private readonly Random _random;
    private readonly double _crossoverProbability;

    public ArithmeticCrossover(Random random, double crossoverProbability)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));

        if (crossoverProbability < 0.0 || crossoverProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(crossoverProbability));

        _crossoverProbability = crossoverProbability;
    }

    public (RealGenome Child1, RealGenome Child2) Cross(
        RealGenome parent1,
        RealGenome parent2)
    {
        ArgumentNullException.ThrowIfNull(parent1);
        ArgumentNullException.ThrowIfNull(parent2);

        if (parent1.Length != parent2.Length)
            throw new InvalidOperationException("Длины вещественных геномов должны совпадать.");

        var child1 = parent1.Clone();
        var child2 = parent2.Clone();

        if (_random.NextDouble() > _crossoverProbability)
            return (child1, child2);

        double alpha = _random.NextDouble();

        for (int i = 0; i < parent1.Length; i++)
        {
            double x1 = parent1[i];
            double x2 = parent2[i];

            child1[i] = alpha * x1 + (1.0 - alpha) * x2;
            child2[i] = alpha * x2 + (1.0 - alpha) * x1;
        }

        return (child1, child2);
    }
}