namespace GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

public interface IGenomeInterpreter<TGenome> where TGenome : IGenome
{
    int EvaluationCount { get; }

    double Evaluate(TGenome genome);
    IReadOnlyList<double> GetCoordinates(TGenome genome);
    Individual<TGenome> CreateIndividual(TGenome genome);
}
