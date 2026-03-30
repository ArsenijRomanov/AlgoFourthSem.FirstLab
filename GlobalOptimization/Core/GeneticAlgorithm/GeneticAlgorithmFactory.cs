using GlobalOptimization.Core.Function;
using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.Configs;
using GlobalOptimization.Core.GeneticAlgorithm.Crossovers;
using GlobalOptimization.Core.GeneticAlgorithm.GenomeInterpreters;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;
using GlobalOptimization.Core.GeneticAlgorithm.Mutations;
using GlobalOptimization.Core.GeneticAlgorithm.PopulationBuilders;

namespace GlobalOptimization.Core.GeneticAlgorithm;

public static class GeneticAlgorithmFactory
{
    public static GeneticAlgorithm<BinaryGenome> Create(
        IObjectiveFunction objectiveFunction,
        SearchSpace searchSpace,
        BinaryGeneticAlgorithmConfig config)
    {
        ArgumentNullException.ThrowIfNull(objectiveFunction);
        ArgumentNullException.ThrowIfNull(searchSpace);
        ArgumentNullException.ThrowIfNull(config);

        config.Validate();
        ValidateProblem(objectiveFunction, searchSpace);

        var random = new Random(config.RandomSeed);

        var interpreter = new BinaryGenomeInterpreter(
            objectiveFunction,
            searchSpace,
            config.BitsPerCoordinate);

        var populationBuilder = new RandomBinaryPopulationBuilder(
            random,
            interpreter,
            searchSpace,
            config.BitsPerCoordinate);

        var parentSelection = new TournamentParentSelection<BinaryGenome>(
            random,
            config.TournamentSize);

        ICrossover<BinaryGenome> crossover = config.CrossoverType switch
        {
            BinaryCrossoverType.OnePoint =>
                new OnePointBinaryCrossover(random, config.CrossoverProbability),

            BinaryCrossoverType.Uniform =>
                new UniformBinaryCrossover(random, config.CrossoverProbability),

            _ => throw new InvalidOperationException(
                $"Неизвестный тип бинарного кроссинговера: {config.CrossoverType}.")
        };

        var mutation = new BitFlipMutation(
            random,
            config.BitMutationProbability);

        var replacementStrategy = new EliteWithOffspringReplacement<BinaryGenome>(
            config.EliteCount);

        return new GeneticAlgorithm<BinaryGenome>(
            config.PopulationSize,
            populationBuilder,
            parentSelection,
            crossover,
            mutation,
            interpreter,
            replacementStrategy);
    }

    public static GeneticAlgorithm<RealGenome> Create(
        IObjectiveFunction objectiveFunction,
        SearchSpace searchSpace,
        RealGeneticAlgorithmConfig config)
    {
        ArgumentNullException.ThrowIfNull(objectiveFunction);
        ArgumentNullException.ThrowIfNull(searchSpace);
        ArgumentNullException.ThrowIfNull(config);

        config.Validate();
        ValidateProblem(objectiveFunction, searchSpace);

        var random = new Random(config.RandomSeed);

        var interpreter = new RealGenomeInterpreter(
            objectiveFunction,
            searchSpace);

        var populationBuilder = new RandomRealPopulationBuilder(
            random,
            interpreter,
            searchSpace);

        var parentSelection = new TournamentParentSelection<RealGenome>(
            random,
            config.TournamentSize);

        ICrossover<RealGenome> crossover = config.CrossoverType switch
        {
            RealCrossoverType.Arithmetic =>
                new ArithmeticCrossover(random, config.CrossoverProbability),

            RealCrossoverType.Sbx =>
                new SbxCrossover(
                    random,
                    config.CrossoverProbability,
                    config.SbxDistributionIndex),

            _ => throw new InvalidOperationException(
                $"Неизвестный тип вещественного кроссинговера: {config.CrossoverType}.")
        };

        var mutation = new GaussianMutation(
            random,
            config.GeneMutationProbability,
            config.Sigma);

        var replacementStrategy = new EliteWithOffspringReplacement<RealGenome>(
            config.EliteCount);

        return new GeneticAlgorithm<RealGenome>(
            config.PopulationSize,
            populationBuilder,
            parentSelection,
            crossover,
            mutation,
            interpreter,
            replacementStrategy);
    }

    private static void ValidateProblem(
        IObjectiveFunction objectiveFunction,
        SearchSpace searchSpace)
    {
        if (objectiveFunction.Dimension != searchSpace.Dimension)
            throw new ArgumentException("Размерность функции и области поиска должна совпадать.");
    }
}
