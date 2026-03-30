namespace GlobalOptimization.Core.ParticleSwarmOptimization.DTOs;

public class PsoSnapshotDto
{
    public int Iteration { get; }
    public int EvaluationCount { get; }

    public IReadOnlyList<double> GlobalBestPosition { get; }
    public double GlobalBestFitness { get; }

    public IReadOnlyList<ParticleDto> Particles { get; }

    public PsoSnapshotDto(
        int iteration,
        int evaluationCount,
        IEnumerable<double> globalBestPosition,
        double globalBestFitness,
        IReadOnlyList<ParticleDto> particles)
    {
        ArgumentNullException.ThrowIfNull(globalBestPosition);

        ArgumentNullException.ThrowIfNull(particles);

        var globalBestPositionCopy = globalBestPosition.ToArray();

        if (globalBestPositionCopy.Length == 0)
        {
            throw new ArgumentException(
                "Глобально лучшая позиция не может быть пустой.",
                nameof(globalBestPosition));
        }

        Iteration = iteration;
        EvaluationCount = evaluationCount;

        GlobalBestPosition = Array.AsReadOnly(globalBestPositionCopy);
        GlobalBestFitness = globalBestFitness;

        Particles = particles;
    }
}
