using System.Text.Json.Serialization;

namespace Analysis.Configs;

public sealed class ExperimentRecordingConfig
{
    public const string EveryIteration = "every_iteration";

    [JsonPropertyName("history_mode")]
    public string HistoryMode { get; init; } = EveryIteration;

    public void Validate()
    {
        if (!string.Equals(HistoryMode, EveryIteration, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Поддерживается только history_mode = '{EveryIteration}'.");
    }
}