namespace Analysis.Results;

public sealed class ExperimentRunOutput(
    IReadOnlyList<RunResult> runs,
    IReadOnlyList<HistoryRecord> history)
{
    public IReadOnlyList<RunResult> Runs { get; } = runs ?? throw new ArgumentNullException(nameof(runs));
    public IReadOnlyList<HistoryRecord> History { get; } = history ?? throw new ArgumentNullException(nameof(history));
}