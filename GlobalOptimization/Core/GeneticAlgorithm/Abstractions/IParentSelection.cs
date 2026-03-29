namespace GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

public interface IParentSelection<TGenome> where TGenome : IGenome
{
    Individual<TGenome> Select(IReadOnlyList<Individual<TGenome>> population);
}