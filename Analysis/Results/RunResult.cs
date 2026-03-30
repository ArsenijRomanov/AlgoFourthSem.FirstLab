namespace Analysis.Results;

public sealed class RunResult
{
    public string SuiteId { get; init; } = string.Empty;
    public string CaseId { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public int Seed { get; init; }

    public RunStatus Status { get; init; }

    public int IterationsCompleted { get; init; }
    public int EvaluationCount { get; init; }
    public long ElapsedMs { get; init; }

    public double FinalBestFitness { get; init; }
    public double FinalBestError { get; init; }

    public double BestX { get; init; }
    public double BestY { get; init; }

    public IReadOnlyDictionary<double, bool> ReachedThresholds { get; init; } =
        new Dictionary<double, bool>();

    public IReadOnlyDictionary<double, int?> EvaluationsToThreshold { get; init; } =
        new Dictionary<double, int?>();

    public double AucLogError { get; init; }
}