using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

namespace GlobalOptimization.Core.ParticleSwarmOptimization.Strategies;

public sealed class CanonicalConstrictionFactorStrategy : IConstrictionFactorStrategy
{
    public double GetFactor(PsoContext context, Particle particle)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(particle);

        double phi = context.C1 + context.C2;

        if (phi <= 4.0)
        {
            throw new InvalidOperationException(
                "Для канонического коэффициента сжатия требуется c1 + c2 > 4.");
        }

        return 2.0 / Math.Abs(2.0 - phi - Math.Sqrt(phi * phi - 4.0 * phi));
    }
}
