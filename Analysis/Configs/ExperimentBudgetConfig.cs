using System.Text.Json.Serialization;

namespace Analysis.Configs;

public sealed class ExperimentBudgetConfig
{
    [JsonPropertyName("max_iterations")]
    public int MaxIterations { get; init; }

    [JsonPropertyName("max_evaluations")]
    public int? MaxEvaluations { get; init; }

    public void Validate()
    {
        if (MaxIterations <= 0)
            throw new InvalidOperationException("MaxIterations должен быть больше 0.");

        if (MaxEvaluations is <= 0)
            throw new InvalidOperationException("MaxEvaluations должен быть null или больше 0.");
    }
}