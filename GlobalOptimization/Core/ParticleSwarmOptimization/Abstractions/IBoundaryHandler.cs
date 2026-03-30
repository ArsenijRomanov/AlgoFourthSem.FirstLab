namespace GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

public interface IBoundaryHandler
{
    void Apply(
        PsoContext context,
        Particle particle);
}
