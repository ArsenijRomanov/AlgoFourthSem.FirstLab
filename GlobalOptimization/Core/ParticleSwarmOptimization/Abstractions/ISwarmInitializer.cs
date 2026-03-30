using GlobalOptimization.Core.Function;

namespace GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;

public interface ISwarmInitializer
{
    IReadOnlyList<Particle> InitializeSwarm(
        int swarmSize,
        SearchSpace searchSpace);
}
