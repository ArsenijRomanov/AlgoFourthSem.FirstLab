using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;

namespace GlobalOptimization.Core.GeneticAlgorithm.Mutations;

public sealed class GaussianMutation : IMutation<RealGenome>
{
    private readonly Random _random;
    private readonly double _geneMutationProbability;
    private readonly double _sigma;

    public GaussianMutation(Random random, double geneMutationProbability, double sigma)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));

        if (geneMutationProbability is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(geneMutationProbability));

        ArgumentOutOfRangeException.ThrowIfLessThan(sigma, 0.0);

        _geneMutationProbability = geneMutationProbability;
        _sigma = sigma;
    }

    public void Mutate(RealGenome genome)
    {
        ArgumentNullException.ThrowIfNull(genome);

        for (var i = 0; i < genome.Length; i++)
        {
            if (_random.NextDouble() < _geneMutationProbability)
                genome[i] += NextGaussian() * _sigma;
        }
    }

    private double NextGaussian()
    {
        double u1 = 1.0 - _random.NextDouble();
        double u2 = 1.0 - _random.NextDouble();

        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}