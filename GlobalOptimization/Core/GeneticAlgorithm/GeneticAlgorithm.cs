using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;
using GlobalOptimization.Core.GeneticAlgorithm.DTOs;

namespace GlobalOptimization.Core.GeneticAlgorithm;

public sealed class GeneticAlgorithm<TGenome> where TGenome : IGenome
{
    private readonly int _populationSize;
    private readonly IPopulationBuilder<TGenome> _populationBuilder;
    private readonly IParentSelection<TGenome> _parentSelection;
    private readonly ICrossover<TGenome> _crossover;
    private readonly IMutation<TGenome> _mutation;
    private readonly IGenomeInterpreter<TGenome> _genomeInterpreter;
    private readonly IReplacementStrategy<TGenome> _replacementStrategy;

    private List<Individual<TGenome>> _population = new();
    public Individual<TGenome>? BestIndividual { get; private set; }
    public int Iteration { get; private set; }
    public bool IsInitialized { get; private set; }

    public int EvaluationCount => _genomeInterpreter.EvaluationCount;

    public GeneticAlgorithm(
        int populationSize,
        IPopulationBuilder<TGenome> populationBuilder,
        IParentSelection<TGenome> parentSelection,
        ICrossover<TGenome> crossover,
        IMutation<TGenome> mutation,
        IGenomeInterpreter<TGenome> genomeEvaluator,
        IReplacementStrategy<TGenome> replacementStrategy)
    {
        if (populationSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(populationSize));

        _populationBuilder = populationBuilder ?? throw new ArgumentNullException(nameof(populationBuilder));
        _parentSelection = parentSelection ?? throw new ArgumentNullException(nameof(parentSelection));
        _crossover = crossover ?? throw new ArgumentNullException(nameof(crossover));
        _mutation = mutation ?? throw new ArgumentNullException(nameof(mutation));
        _genomeInterpreter = genomeEvaluator ?? throw new ArgumentNullException(nameof(genomeEvaluator));
        _replacementStrategy = replacementStrategy ?? throw new ArgumentNullException(nameof(replacementStrategy));

        _populationSize = populationSize;
    }

    public void Initialize()
    {
        var initialPopulation = _populationBuilder.BuildPopulation(_populationSize);

        if (initialPopulation is null)
            throw new InvalidOperationException("Построитель популяции вернул null.");

        if (initialPopulation.Count != _populationSize)
        {
            throw new InvalidOperationException(
                $"Построитель популяции вернул {initialPopulation.Count} особей вместо {_populationSize}.");
        }

        _population = new List<Individual<TGenome>>(initialPopulation);
        BestIndividual = FindBest(_population);
        Iteration = 0;
        IsInitialized = true;
    }

    public void Step()
    {
        EnsureInitialized();

        var offspring = new List<Individual<TGenome>>(_populationSize);

        while (offspring.Count < _populationSize)
        {
            var parent1 = _parentSelection.Select(_population);
            var parent2 = _parentSelection.Select(_population);

            if (parent1 is null || parent2 is null)
                throw new InvalidOperationException("Стратегия выбора родителей вернула null.");

            var (childGenome1, childGenome2) = _crossover.Cross(parent1.Genome, parent2.Genome);

            if (childGenome1 is null || childGenome2 is null)
                throw new InvalidOperationException("Кроссинговер вернул null-геном.");

            _mutation.Mutate(childGenome1);
            _mutation.Mutate(childGenome2);

            offspring.Add(CreateIndividual(childGenome1));

            if (offspring.Count < _populationSize)
                offspring.Add(CreateIndividual(childGenome2));
        }

        var nextPopulation = _replacementStrategy.CreateNextPopulation(
            _population,
            offspring,
            _populationSize);

        if (nextPopulation is null)
            throw new InvalidOperationException("Стратегия формирования нового поколения вернула null.");

        if (nextPopulation.Count != _populationSize)
        {
            throw new InvalidOperationException(
                $"Стратегия формирования нового поколения вернула {nextPopulation.Count} особей вместо {_populationSize}.");
        }

        _population = new List<Individual<TGenome>>(nextPopulation);

        var currentBest = FindBest(_population);
        if (BestIndividual is null || currentBest.Fitness < BestIndividual.Fitness)
            BestIndividual = currentBest;

        Iteration++;
    }

    public void Step(int iterations)
    {
        if (iterations < 0)
            throw new ArgumentOutOfRangeException(nameof(iterations));

        for (int i = 0; i < iterations; i++)
            Step();
    }

    public PointDto GetPointDto(Individual<TGenome> individual)
    {
        if (individual is null)
            throw new ArgumentNullException(nameof(individual));

        var coordinates = _genomeInterpreter.GetCoordinates(individual.Genome);
        return new PointDto(coordinates, individual.Fitness);
    }

    public IReadOnlyList<PointDto> GetPopulationDto()
    {
        EnsureInitialized();

        var result = new List<PointDto>(_population.Count);

        foreach (var individual in _population)
            result.Add(GetPointDto(individual));

        return result.AsReadOnly();
    }

    public GeneticAlgorithmSnapshotDto GetSnapshot()
    {
        EnsureInitialized();

        var populationDto = GetPopulationDto();
        var bestDto = BestIndividual is null ? null : GetPointDto(BestIndividual);

        return new GeneticAlgorithmSnapshotDto(
            Iteration,
            EvaluationCount,
            bestDto,
            populationDto);
    }
    
    public void Reset()
    {
        _population = new List<Individual<TGenome>>();
        BestIndividual = null;
        Iteration = 0;
        IsInitialized = false;
        _genomeInterpreter.ResetStatistics();
    }

    private Individual<TGenome> CreateIndividual(TGenome genome)
        => _genomeInterpreter.CreateIndividual(genome);

    private static Individual<TGenome> FindBest(IReadOnlyList<Individual<TGenome>> population)
    {
        if (population.Count == 0)
            throw new InvalidOperationException("Популяция пуста.");

        var best = population[0];

        for (int i = 1; i < population.Count; i++)
        {
            if (population[i].Fitness < best.Fitness)
                best = population[i];
        }

        return best;
    }

    private void EnsureInitialized()
    {
        if (!IsInitialized)
            throw new InvalidOperationException(
                "Алгоритм не инициализирован. Сначала вызови Initialize().");
    }
}