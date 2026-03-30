using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

namespace GlobalOptimization.Core.ParticleSwarmOptimization.Strategies;

public sealed class GlobalBestProvider : ISocialBestProvider
{
    public IReadOnlyList<double> GetBestPosition(PsoContext context, Particle particle)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(particle);

        return context.GlobalBestPosition;
    }
}
