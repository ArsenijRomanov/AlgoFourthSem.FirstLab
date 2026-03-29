using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

namespace GlobalOptimization.Core.GeneticAlgorithm;

public sealed class Individual<TGenome> where TGenome : IGenome
{
    public TGenome Genome { get; }
    public double Fitness { get; }

    public Individual(TGenome genome, double fitness)
    {
        Genome = genome ?? throw new ArgumentNullException(nameof(genome));
        Fitness = fitness;
    }

    public override string ToString()
    {
        return $"Fitness = {Fitness}";
    }
}
