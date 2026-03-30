using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

namespace GlobalOptimization.Core.ParticleSwarmOptimization.Strategies;

public sealed class RangeBasedVelocityLimiter : IVelocityLimiter
{
    private readonly double _ratio;

    public RangeBasedVelocityLimiter(double ratio)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ratio, 0.0);

        _ratio = ratio;
    }

    public void Apply(PsoContext context, Particle particle)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(particle);

        for (var i = 0; i < particle.Dimension; i++)
        {
            var (min, max) = context.SearchSpace.Bounds[i];
            var vmax = _ratio * (max - min);

            var velocity = particle.GetVelocity(i);

            if (velocity > vmax)
                particle.SetVelocity(i, vmax);
            else if (velocity < -vmax)
                particle.SetVelocity(i, -vmax);
        }
    }
}
