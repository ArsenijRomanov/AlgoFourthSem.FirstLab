namespace Analysis.Adapters;

public interface IOptimizationAdapter
{
    string CaseId { get; }
    string Family { get; }
    string Variant { get; }

    bool IsInitialized { get; }
    bool IsFinished { get; }

    int Iteration { get; }
    int EvaluationCount { get; }

    double BestFitness { get; }
    IReadOnlyList<double> BestPoint { get; }

    void Initialize();
    void Step();
}