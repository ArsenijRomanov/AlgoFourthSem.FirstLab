using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

namespace GlobalOptimization.Core.ParticleSwarmOptimization.Strategies;

public sealed class ClampBoundaryHandler : IBoundaryHandler
{
    public void Apply(PsoContext context, Particle particle)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(particle);

        for (var i = 0; i < particle.Dimension; i++)
        {
            var (min, max) = context.SearchSpace.Bounds[i];
            var position = particle.GetPosition(i);

            if (position < min)
                particle.SetPosition(i, min);
            else if (position > max)
                particle.SetPosition(i, max);
        }
    }
}
