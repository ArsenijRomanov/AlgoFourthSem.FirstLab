namespace GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

public interface IInertiaWeightStrategy
{
    double GetWeight(
        PsoContext context,
        Particle particle);
}