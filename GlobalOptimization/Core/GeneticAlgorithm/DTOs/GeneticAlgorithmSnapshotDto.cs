namespace GlobalOptimization.Core.GeneticAlgorithm.DTOs;

public sealed class GeneticAlgorithmSnapshotDto
{
    public int Iteration { get; }
    public int EvaluationCount { get; }
    public PointDto BestPoint { get; }
    public IReadOnlyList<PointDto> Population { get; }

    public GeneticAlgorithmSnapshotDto(
        int iteration,
        int evaluationCount,
        PointDto? bestPoint,
        IReadOnlyList<PointDto> population)
    {
        Iteration = iteration;
        EvaluationCount = evaluationCount;
        BestPoint = bestPoint ?? throw new ArgumentNullException(nameof(bestPoint));
        Population = population ?? throw new ArgumentNullException(nameof(population));
    }
}