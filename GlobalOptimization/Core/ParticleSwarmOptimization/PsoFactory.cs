using GlobalOptimization.Core.Function;
using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;
using GlobalOptimization.Core.ParticleSwarmOptimization.Strategies;

namespace GlobalOptimization.Core.ParticleSwarmOptimization;

public static class PsoFactory
{
    public static ParticleSwarmOptimization Create(
        IObjectiveFunction objectiveFunction,
        SearchSpace searchSpace,
        ParticleSwarmOptimizationConfig config)
    {
        ArgumentNullException.ThrowIfNull(objectiveFunction);
        ArgumentNullException.ThrowIfNull(searchSpace);
        ArgumentNullException.ThrowIfNull(config);

        config.Validate();
        ValidateProblem(objectiveFunction, searchSpace);

        var random = new Random(config.RandomSeed);

        var swarmInitializer = new RandomSwarmInitializer(
            random,
            config.VelocityLimitRatio);

        var inertiaWeightStrategy = new ConstantInertiaWeightStrategy(
            config.ConstantInertiaWeight);

        IConstrictionFactorStrategy constrictionFactorStrategy =
            config.ConstrictionFactorType switch
            {
                ConstrictionFactorType.Unit =>
                    new UnitConstrictionFactorStrategy(),

                ConstrictionFactorType.Canonical =>
                    new CanonicalConstrictionFactorStrategy(),

                ConstrictionFactorType.Dynamic =>
                    new DynamicConstrictionFactorStrategy(
                        config.DynamicConstrictionChiMax),

                _ => throw new InvalidOperationException(
                    $"Неизвестный тип коэффициента сжатия: {config.ConstrictionFactorType}.")
            };

        var velocityLimiter = new RangeBasedVelocityLimiter(
            config.VelocityLimitRatio);

        var boundaryHandler = new ClampBoundaryHandler();

        var socialBestProvider = new GlobalBestProvider();

        return new ParticleSwarmOptimization(
            config.SwarmSize,
            config.MaxIterations,
            config.C1,
            config.C2,
            objectiveFunction,
            searchSpace,
            swarmInitializer,
            inertiaWeightStrategy,
            constrictionFactorStrategy,
            velocityLimiter,
            boundaryHandler,
            socialBestProvider,
            random);
    }

    private static void ValidateProblem(
        IObjectiveFunction objectiveFunction,
        SearchSpace searchSpace)
    {
        if (objectiveFunction.Dimension != searchSpace.Dimension)
            throw new ArgumentException("Размерность функции и области поиска должна совпадать.");
    }
}