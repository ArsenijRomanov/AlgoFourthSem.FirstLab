using System.Text.Json;
using Analysis.Configs;

namespace Analysis;

public static class SuiteLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static ExperimentSuiteConfig LoadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Путь к конфигу не может быть пустым.", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException("Файл конфига не найден.", path);

        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<ExperimentSuiteConfig>(json, JsonOptions)
                     ?? throw new InvalidOperationException("Не удалось десериализовать suite config.");

        config.Validate();
        return config;
    }
}