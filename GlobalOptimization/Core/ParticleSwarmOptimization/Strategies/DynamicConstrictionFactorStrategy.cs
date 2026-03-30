using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

namespace GlobalOptimization.Core.ParticleSwarmOptimization.Strategies;

public sealed class DynamicConstrictionFactorStrategy : IConstrictionFactorStrategy
{
    private readonly double _chiMax;

    public DynamicConstrictionFactorStrategy(double chiMax)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(chiMax, 0.0);

        _chiMax = chiMax;
    }

    public double GetFactor(PsoContext context, Particle particle)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(particle);

        var phi = context.C1 + context.C2;

        if (phi <= 4.0)
        {
            throw new InvalidOperationException(
                "Для динамического коэффициента сжатия требуется c1 + c2 > 4.");
        }

        var chiMin = 2.0 / Math.Abs(2.0 - phi - Math.Sqrt(phi * phi - 4.0 * phi));

        return _chiMax - (double)context.Iteration / context.MaxIterations * (_chiMax - chiMin);
    }
}
