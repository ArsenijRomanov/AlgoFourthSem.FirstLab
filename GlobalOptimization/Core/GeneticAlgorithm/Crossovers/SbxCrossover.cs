using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;

namespace GlobalOptimization.Core.GeneticAlgorithm.Crossovers;

public sealed class SbxCrossover : ICrossover<RealGenome>
{
    private readonly Random _random;
    private readonly double _crossoverProbability;
    private readonly double _distributionIndex;

    public SbxCrossover(
        Random random,
        double crossoverProbability,
        double distributionIndex)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));

        if (crossoverProbability < 0.0 || crossoverProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(crossoverProbability));

        if (distributionIndex < 0.0)
            throw new ArgumentOutOfRangeException(nameof(distributionIndex));

        _crossoverProbability = crossoverProbability;
        _distributionIndex = distributionIndex;
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

        for (int i = 0; i < parent1.Length; i++)
        {
            double x1 = parent1[i];
            double x2 = parent2[i];

            if (Math.Abs(x1 - x2) < 1e-12)
            {
                child1[i] = x1;
                child2[i] = x2;
                continue;
            }

            if (x1 > x2)
                (x1, x2) = (x2, x1);

            double u = _random.NextDouble();
            double beta;

            if (u <= 0.5)
            {
                beta = Math.Pow(2.0 * u, 1.0 / (_distributionIndex + 1.0));
            }
            else
            {
                beta = Math.Pow(1.0 / (2.0 * (1.0 - u)), 1.0 / (_distributionIndex + 1.0));
            }

            double middle = 0.5 * (x1 + x2);
            double halfDistance = 0.5 * (x2 - x1);

            double c1 = middle - beta * halfDistance;
            double c2 = middle + beta * halfDistance;

            if (_random.NextDouble() < 0.5)
            {
                child1[i] = c1;
                child2[i] = c2;
            }
            else
            {
                child1[i] = c2;
                child2[i] = c1;
            }
        }

        return (child1, child2);
    }
}
