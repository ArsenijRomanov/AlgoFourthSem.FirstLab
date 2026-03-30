namespace GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

public interface IConstrictionFactorStrategy
{
    double GetFactor(
        PsoContext context,
        Particle particle);
}