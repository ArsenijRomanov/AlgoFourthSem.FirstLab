namespace GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

public interface IVelocityLimiter
{
    void Apply(
        PsoContext context,
        Particle particle);
}
