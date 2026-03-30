using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

namespace GlobalOptimization.Core.ParticleSwarmOptimization.Strategies;

public sealed class ConstantInertiaWeightStrategy : IInertiaWeightStrategy
{
    private readonly double _weight;

    public ConstantInertiaWeightStrategy(double weight)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0.0);

        _weight = weight;
    }

    public double GetWeight(PsoContext context, Particle particle)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(particle);

        return _weight;
    }
}
