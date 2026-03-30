using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;

namespace GlobalOptimization.Core.GeneticAlgorithm.Mutations;

public sealed class BitFlipMutation : IMutation<BinaryGenome>
{
    private readonly Random _random;
    private readonly double _bitMutationProbability;

    public BitFlipMutation(Random random, double bitMutationProbability)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));

        if (bitMutationProbability is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(bitMutationProbability));

        _bitMutationProbability = bitMutationProbability;
    }

    public void Mutate(BinaryGenome genome)
    {
        ArgumentNullException.ThrowIfNull(genome);

        for (var i = 0; i < genome.Length; i++)
        {
            if (_random.NextDouble() < _bitMutationProbability)
                genome.FlipBit(i);
        }
    }
}
