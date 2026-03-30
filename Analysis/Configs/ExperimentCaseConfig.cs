using System.Text.Json;
using System.Text.Json.Serialization;

namespace Analysis.Configs;

public sealed class ExperimentCaseConfig
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("family")]
    public string Family { get; init; } = string.Empty;

    [JsonPropertyName("representation")]
    public string? Representation { get; init; }

    [JsonPropertyName("variant")]
    public string Variant { get; init; } = string.Empty;

    [JsonPropertyName("parameters")]
    public JsonElement Parameters { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("Case.Id не может быть пустым.");

        if (string.IsNullOrWhiteSpace(Family))
            throw new InvalidOperationException($"Case '{Id}': Family не может быть пустым.");

        if (string.IsNullOrWhiteSpace(Variant))
            throw new InvalidOperationException($"Case '{Id}': Variant не может быть пустым.");

        if (Parameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            throw new InvalidOperationException($"Case '{Id}': Parameters не заданы.");

        if (Parameters.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Case '{Id}': Parameters должны быть JSON-объектом.");
    }
}