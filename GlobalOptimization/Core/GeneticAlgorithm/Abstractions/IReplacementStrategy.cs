namespace GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

public interface IReplacementStrategy<TGenome> where TGenome : IGenome
{
    IReadOnlyList<Individual<TGenome>> CreateNextPopulation(
        IReadOnlyList<Individual<TGenome>> currentPopulation,
        IReadOnlyList<Individual<TGenome>> offspring,
        int targetPopulationSize);
}