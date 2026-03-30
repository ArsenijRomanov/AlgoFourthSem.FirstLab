using System.Text.Json.Serialization;

namespace Analysis.Configs;

public sealed class ExperimentMetricsConfig
{
    [JsonPropertyName("known_optimal_fitness")]
    public double KnownOptimalFitness { get; init; }

    [JsonPropertyName("thresholds")]
    public IReadOnlyList<double> Thresholds { get; init; } = [];

    [JsonPropertyName("log_error_epsilon")]
    public double LogErrorEpsilon { get; init; } = 1e-16;

    public void Validate()
    {
        if (Thresholds.Count == 0)
            throw new InvalidOperationException("Список thresholds не может быть пустым.");

        if (Thresholds.Any(t => t <= 0.0))
            throw new InvalidOperationException("Все thresholds должны быть больше 0.");

        for (int i = 1; i < Thresholds.Count; i++)
        {
            if (Thresholds[i] >= Thresholds[i - 1])
            {
                throw new InvalidOperationException(
                    "Thresholds должны быть строго убывающими, например [0.1, 0.01, ...].");
            }
        }

        if (LogErrorEpsilon <= 0.0)
            throw new InvalidOperationException("LogErrorEpsilon должен быть больше 0.");
    }
}