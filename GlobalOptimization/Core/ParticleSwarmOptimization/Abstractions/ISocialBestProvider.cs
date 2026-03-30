namespace GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

public interface ISocialBestProvider
{
    IReadOnlyList<double> GetBestPosition(
        PsoContext context,
        Particle particle);
}
