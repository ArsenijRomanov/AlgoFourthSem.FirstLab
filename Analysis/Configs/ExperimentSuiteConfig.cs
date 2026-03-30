using System.Text.Json.Serialization;

namespace Analysis.Configs;

public sealed class ExperimentSuiteConfig
{
    [JsonPropertyName("suite_id")]
    public string SuiteId { get; init; } = string.Empty;

    [JsonPropertyName("objective_id")]
    public string ObjectiveId { get; init; } = string.Empty;

    [JsonPropertyName("seeds")]
    public IReadOnlyList<int> Seeds { get; init; } = [];

    [JsonPropertyName("budget")]
    public ExperimentBudgetConfig Budget { get; init; } = new();

    [JsonPropertyName("metrics")]
    public ExperimentMetricsConfig Metrics { get; init; } = new();

    [JsonPropertyName("recording")]
    public ExperimentRecordingConfig Recording { get; init; } = new();

    [JsonPropertyName("cases")]
    public IReadOnlyList<ExperimentCaseConfig> Cases { get; init; } = [];

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SuiteId))
            throw new InvalidOperationException("SuiteId не может быть пустым.");

        if (string.IsNullOrWhiteSpace(ObjectiveId))
            throw new InvalidOperationException("ObjectiveId не может быть пустым.");

        if (Seeds.Count == 0)
            throw new InvalidOperationException("Список seeds не может быть пустым.");

        if (Seeds.Any(seed => seed < 0))
            throw new InvalidOperationException("Seeds не могут быть отрицательными.");

        Budget.Validate();
        Metrics.Validate();
        Recording.Validate();

        if (Cases.Count == 0)
            throw new InvalidOperationException("Список cases не может быть пустым.");

        var duplicateCaseId = Cases
            .GroupBy(c => c.Id, StringComparer.Ordinal)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateCaseId is not null)
            throw new InvalidOperationException($"Обнаружен дублирующийся case id: {duplicateCaseId.Key}.");

        foreach (var experimentCase in Cases)
            experimentCase.Validate();
    }
}