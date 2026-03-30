using GlobalOptimization.Core.Function;
using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

namespace GlobalOptimization.Core.ParticleSwarmOptimization.Strategies;

public sealed class RandomSwarmInitializer : ISwarmInitializer
{
    private readonly Random _random;
    private readonly double _initialVelocityRatio;

    public RandomSwarmInitializer(Random random, double initialVelocityRatio = 0.2)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));

        ArgumentOutOfRangeException.ThrowIfLessThan(initialVelocityRatio, 0.0);

        _initialVelocityRatio = initialVelocityRatio;
    }

    public IReadOnlyList<Particle> InitializeSwarm(
        int swarmSize,
        SearchSpace searchSpace)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(swarmSize);
        ArgumentNullException.ThrowIfNull(searchSpace);

        var particles = new List<Particle>(swarmSize);

        for (var i = 0; i < swarmSize; i++)
        {
            var position = new double[searchSpace.Dimension];
            var velocity = new double[searchSpace.Dimension];

            for (var j = 0; j < searchSpace.Dimension; j++)
            {
                var (min, max) = searchSpace.Bounds[j];
                var range = max - min;
                var vmax = _initialVelocityRatio * range;

                position[j] = min + _random.NextDouble() * range;
                velocity[j] = -vmax + _random.NextDouble() * (2.0 * vmax);
            }

            particles.Add(new Particle(position, velocity, double.PositiveInfinity));
        }

        return particles.AsReadOnly();
    }
}
