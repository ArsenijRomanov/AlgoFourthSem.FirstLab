namespace GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

public interface IMutation<TGenome> where TGenome : IGenome
{
    void Mutate(TGenome genome);
}
