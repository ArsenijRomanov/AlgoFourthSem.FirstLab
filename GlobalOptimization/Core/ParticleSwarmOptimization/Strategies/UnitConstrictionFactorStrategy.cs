using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

namespace GlobalOptimization.Core.ParticleSwarmOptimization.Strategies;

public sealed class UnitConstrictionFactorStrategy : IConstrictionFactorStrategy
{
    public double GetFactor(PsoContext context, Particle particle)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(particle);

        return 1.0;
    }
}
