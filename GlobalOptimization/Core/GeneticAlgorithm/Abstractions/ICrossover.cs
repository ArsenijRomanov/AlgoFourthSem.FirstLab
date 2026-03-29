namespace GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

public interface ICrossover<TGenome> where TGenome : IGenome
{
    (TGenome Child1, TGenome Child2) Cross(
        TGenome parent1,
        TGenome parent2);
}
