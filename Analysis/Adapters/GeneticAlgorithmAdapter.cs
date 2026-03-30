using Analysis.Configs;
using GlobalOptimization.Core.GeneticAlgorithm;
using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

namespace Analysis.Adapters;

public sealed class GeneticAlgorithmAdapter<TGenome> : IOptimizationAdapter
    where TGenome : IGenome
{
    private readonly GeneticAlgorithm<TGenome> _algorithm;
    private readonly ExperimentBudgetConfig _budget;

    public string CaseId { get; }
    public string Family { get; }
    public string Variant { get; }

    public bool IsInitialized => _algorithm.IsInitialized;

    public bool IsFinished
    {
        get
        {
            if (!IsInitialized)
                return false;

            if (Iteration >= _budget.MaxIterations)
                return true;

            if (_budget.MaxEvaluations.HasValue && EvaluationCount >= _budget.MaxEvaluations.Value)
                return true;

            return false;
        }
    }

    public int Iteration => _algorithm.Iteration;
    public int EvaluationCount => _algorithm.EvaluationCount;

    public double BestFitness => _algorithm.BestIndividual?.Fitness ?? double.PositiveInfinity;

    public IReadOnlyList<double> BestPoint
    {
        get
        {
            if (_algorithm.BestIndividual is null)
                return Array.Empty<double>();

            return _algorithm.GetPointDto(_algorithm.BestIndividual).Coordinates;
        }
    }

    public GeneticAlgorithmAdapter(
        string caseId,
        string family,
        string variant,
        GeneticAlgorithm<TGenome> algorithm,
        ExperimentBudgetConfig budget)
    {
        if (string.IsNullOrWhiteSpace(caseId))
            throw new ArgumentException("CaseId не может быть пустым.", nameof(caseId));

        if (string.IsNullOrWhiteSpace(family))
            throw new ArgumentException("Family не может быть пустым.", nameof(family));

        if (string.IsNullOrWhiteSpace(variant))
            throw new ArgumentException("Variant не может быть пустым.", nameof(variant));

        _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
        _budget = budget ?? throw new ArgumentNullException(nameof(budget));

        CaseId = caseId;
        Family = family;
        Variant = variant;
    }

    public void Initialize()
    {
        _algorithm.Reset();
        _algorithm.Initialize();
    }

    public void Step()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Алгоритм не инициализирован.");

        if (IsFinished)
            return;

        _algorithm.Step();
    }
}