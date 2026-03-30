using System.Text.Json;
using Analysis.Adapters;
using Analysis.Configs;
using GlobalOptimization.Core.GeneticAlgorithm;
using GlobalOptimization.Core.GeneticAlgorithm.Configs;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;
using GlobalOptimization.Core.ParticleSwarmOptimization;

namespace Analysis;

public static class OptimizationMethodFactory
{
    public static IOptimizationAdapter Create(
        ExperimentCaseConfig caseConfig,
        ObjectiveDefinition objective,
        ExperimentBudgetConfig budget,
        int seed)
    {
        ArgumentNullException.ThrowIfNull(caseConfig);
        ArgumentNullException.ThrowIfNull(objective);
        ArgumentNullException.ThrowIfNull(budget);

        return caseConfig.Id switch
        {
            CaseIds.GaBinaryOnePoint => CreateBinaryGa(
                caseConfig,
                objective,
                budget,
                seed,
                BinaryCrossoverType.OnePoint),

            CaseIds.GaBinaryUniform => CreateBinaryGa(
                caseConfig,
                objective,
                budget,
                seed,
                BinaryCrossoverType.Uniform),

            CaseIds.GaRealArithmetic => CreateRealGa(
                caseConfig,
                objective,
                budget,
                seed,
                RealCrossoverType.Arithmetic),

            CaseIds.GaRealSbx => CreateRealGa(
                caseConfig,
                objective,
                budget,
                seed,
                RealCrossoverType.Sbx),

            CaseIds.PsoUnitChi => CreatePso(
                caseConfig,
                objective,
                budget,
                seed,
                ConstrictionFactorType.Unit),

            CaseIds.PsoCanonicalChi => CreatePso(
                caseConfig,
                objective,
                budget,
                seed,
                ConstrictionFactorType.Canonical),

            _ => throw new InvalidOperationException(
                $"Неизвестный case id: '{caseConfig.Id}'.")
        };
    }

    private static IOptimizationAdapter CreateBinaryGa(
        ExperimentCaseConfig caseConfig,
        ObjectiveDefinition objective,
        ExperimentBudgetConfig budget,
        int seed,
        BinaryCrossoverType expectedCrossoverType)
    {
        EnsureFamily(caseConfig, "ga");
        EnsureRepresentation(caseConfig, "binary");

        var parameters = caseConfig.Parameters;

        var config = new BinaryGeneticAlgorithmConfig
        {
            PopulationSize = GetRequiredInt(parameters, "population_size"),
            TournamentSize = GetRequiredInt(parameters, "tournament_size"),
            EliteCount = GetRequiredInt(parameters, "elite_count"),
            RandomSeed = seed,
            CrossoverProbability = GetRequiredDouble(parameters, "crossover_probability"),
            BitsPerCoordinate = GetRequiredInt(parameters, "bits_per_coordinate"),
            BitMutationProbability = GetRequiredDouble(parameters, "bit_mutation_probability"),
            CrossoverType = GetRequiredEnum<BinaryCrossoverType>(parameters, "crossover_type")
        };

        if (config.CrossoverType != expectedCrossoverType)
        {
            throw new InvalidOperationException(
                $"Case '{caseConfig.Id}': crossover_type = {config.CrossoverType}, ожидался {expectedCrossoverType}.");
        }

        var algorithm = GeneticAlgorithmFactory.Create(
            objective.ObjectiveFunction,
            objective.SearchSpace,
            config);

        return new GeneticAlgorithmAdapter<BinaryGenome>(
            caseConfig.Id,
            caseConfig.Family,
            caseConfig.Variant,
            algorithm,
            budget);
    }

    private static IOptimizationAdapter CreateRealGa(
        ExperimentCaseConfig caseConfig,
        ObjectiveDefinition objective,
        ExperimentBudgetConfig budget,
        int seed,
        RealCrossoverType expectedCrossoverType)
    {
        EnsureFamily(caseConfig, "ga");
        EnsureRepresentation(caseConfig, "real");

        var parameters = caseConfig.Parameters;

        var config = new RealGeneticAlgorithmConfig
        {
            PopulationSize = GetRequiredInt(parameters, "population_size"),
            TournamentSize = GetRequiredInt(parameters, "tournament_size"),
            EliteCount = GetRequiredInt(parameters, "elite_count"),
            RandomSeed = seed,
            CrossoverProbability = GetRequiredDouble(parameters, "crossover_probability"),
            GeneMutationProbability = GetRequiredDouble(parameters, "gene_mutation_probability"),
            Sigma = GetRequiredDouble(parameters, "sigma"),
            SbxDistributionIndex = GetRequiredDouble(parameters, "sbx_distribution_index"),
            CrossoverType = GetRequiredEnum<RealCrossoverType>(parameters, "crossover_type")
        };

        if (config.CrossoverType != expectedCrossoverType)
        {
            throw new InvalidOperationException(
                $"Case '{caseConfig.Id}': crossover_type = {config.CrossoverType}, ожидался {expectedCrossoverType}.");
        }

        var algorithm = GeneticAlgorithmFactory.Create(
            objective.ObjectiveFunction,
            objective.SearchSpace,
            config);

        return new GeneticAlgorithmAdapter<RealGenome>(
            caseConfig.Id,
            caseConfig.Family,
            caseConfig.Variant,
            algorithm,
            budget);
    }

    private static IOptimizationAdapter CreatePso(
        ExperimentCaseConfig caseConfig,
        ObjectiveDefinition objective,
        ExperimentBudgetConfig budget,
        int seed,
        ConstrictionFactorType expectedConstrictionFactorType)
    {
        EnsureFamily(caseConfig, "pso");

        var parameters = caseConfig.Parameters;

        var config = new ParticleSwarmOptimizationConfig
        {
            SwarmSize = GetRequiredInt(parameters, "swarm_size"),
            MaxIterations = budget.MaxIterations,
            RandomSeed = seed,
            C1 = GetRequiredDouble(parameters, "c1"),
            C2 = GetRequiredDouble(parameters, "c2"),
            ConstantInertiaWeight = GetRequiredDouble(parameters, "constant_inertia_weight"),
            DynamicConstrictionChiMax = GetRequiredDouble(parameters, "dynamic_constriction_chi_max"),
            VelocityLimitRatio = GetRequiredDouble(parameters, "velocity_limit_ratio"),
            ConstrictionFactorType = GetRequiredEnum<ConstrictionFactorType>(parameters, "constriction_factor_type")
        };

        if (config.ConstrictionFactorType != expectedConstrictionFactorType)
        {
            throw new InvalidOperationException(
                $"Case '{caseConfig.Id}': constriction_factor_type = {config.ConstrictionFactorType}, ожидался {expectedConstrictionFactorType}.");
        }

        var algorithm = PsoFactory.Create(
            objective.ObjectiveFunction,
            objective.SearchSpace,
            config);

        return new PsoAdapter(
            caseConfig.Id,
            caseConfig.Family,
            caseConfig.Variant,
            algorithm,
            budget);
    }

    private static void EnsureFamily(ExperimentCaseConfig caseConfig, string expectedFamily)
    {
        if (!string.Equals(caseConfig.Family, expectedFamily, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Case '{caseConfig.Id}': family = '{caseConfig.Family}', ожидалось '{expectedFamily}'.");
        }
    }

    private static void EnsureRepresentation(ExperimentCaseConfig caseConfig, string expectedRepresentation)
    {
        if (!string.Equals(caseConfig.Representation, expectedRepresentation, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Case '{caseConfig.Id}': representation = '{caseConfig.Representation}', ожидалось '{expectedRepresentation}'.");
        }
    }

    private static int GetRequiredInt(JsonElement parameters, string propertyName)
    {
        var property = GetRequiredProperty(parameters, propertyName);

        if (!property.TryGetInt32(out var value))
            throw new InvalidOperationException($"Параметр '{propertyName}' должен быть int.");

        return value;
    }

    private static double GetRequiredDouble(JsonElement parameters, string propertyName)
    {
        var property = GetRequiredProperty(parameters, propertyName);

        if (!property.TryGetDouble(out var value))
            throw new InvalidOperationException($"Параметр '{propertyName}' должен быть double.");

        return value;
    }

    private static TEnum GetRequiredEnum<TEnum>(JsonElement parameters, string propertyName)
        where TEnum : struct, Enum
    {
        var property = GetRequiredProperty(parameters, propertyName);

        if (property.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException($"Параметр '{propertyName}' должен быть строкой.");

        var rawValue = property.GetString();
        if (string.IsNullOrWhiteSpace(rawValue))
            throw new InvalidOperationException($"Параметр '{propertyName}' не может быть пустым.");

        if (!Enum.TryParse<TEnum>(rawValue, ignoreCase: true, out var value))
        {
            throw new InvalidOperationException(
                $"Параметр '{propertyName}' имеет недопустимое значение '{rawValue}'.");
        }

        return value;
    }

    private static JsonElement GetRequiredProperty(JsonElement parameters, string propertyName)
    {
        if (parameters.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Parameters должен быть JSON-объектом.");

        if (!parameters.TryGetProperty(propertyName, out var property))
            throw new InvalidOperationException($"Отсутствует обязательный параметр '{propertyName}'.");

        return property;
    }
}