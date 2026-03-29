namespace GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

public interface IPopulationBuilder<TGenome> where TGenome : IGenome
{
    IReadOnlyList<Individual<TGenome>> BuildPopulation(int populationSize);
}
