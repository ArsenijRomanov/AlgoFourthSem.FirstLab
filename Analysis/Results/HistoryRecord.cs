namespace Analysis.Results;

public sealed class HistoryRecord
{
    public string SuiteId { get; init; } = string.Empty;
    public string CaseId { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public int Seed { get; init; }

    public int Iteration { get; init; }
    public int EvaluationCount { get; init; }

    public double BestFitness { get; init; }
    public double BestError { get; init; }

    public double BestX { get; init; }
    public double BestY { get; init; }
}