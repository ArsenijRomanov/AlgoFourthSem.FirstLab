using GlobalOptimization.Core.Function;

namespace GlobalOptimization.Core.ParticleSwarmOptimization;

public sealed class PsoContext
{
    private readonly double[] _globalBestPosition;

    public int Iteration { get; }
    public int MaxIterations { get; }

    public IReadOnlyList<Particle> Particles { get; }
    public IReadOnlyList<double> GlobalBestPosition { get; }

    public double GlobalBestFitness { get; }

    public SearchSpace SearchSpace { get; }

    public double C1 { get; }
    public double C2 { get; }

    public PsoContext(
        int iteration,
        int maxIterations,
        IReadOnlyList<Particle> particles,
        IReadOnlyList<double> globalBestPosition,
        double globalBestFitness,
        SearchSpace searchSpace,
        double c1,
        double c2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(iteration);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxIterations);

        ArgumentNullException.ThrowIfNull(particles);
        ArgumentNullException.ThrowIfNull(globalBestPosition);
        ArgumentNullException.ThrowIfNull(searchSpace);

        if (particles.Count == 0)
            throw new ArgumentException("Рой не может быть пустым.", nameof(particles));

        if (globalBestPosition.Count != searchSpace.Dimension)
        {
            throw new ArgumentException(
                "Размерность глобально лучшей точки должна совпадать с размерностью пространства поиска.",
                nameof(globalBestPosition));
        }

        foreach (var particle in particles)
        {
            if (particle is null)
                throw new ArgumentException("Рой не должен содержать null-частицы.", nameof(particles));

            if (particle.Dimension != searchSpace.Dimension)
            {
                throw new ArgumentException(
                    "Размерность частицы должна совпадать с размерностью пространства поиска.",
                    nameof(particles));
            }
        }

        Iteration = iteration;
        MaxIterations = maxIterations;

        Particles = particles;
        _globalBestPosition = globalBestPosition.ToArray();
        GlobalBestPosition = Array.AsReadOnly(_globalBestPosition);

        GlobalBestFitness = globalBestFitness;

        SearchSpace = searchSpace;

        C1 = c1;
        C2 = c2;
    }
}
