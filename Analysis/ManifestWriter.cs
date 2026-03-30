using System.Text.Json;
using Analysis.Configs;

namespace Analysis;

public static class ManifestWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static void Write(
        string path,
        ExperimentSuiteConfig suiteConfig,
        int runCount,
        int historyRecordCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(suiteConfig);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var manifest = new
        {
            suite_id = suiteConfig.SuiteId,
            objective_id = suiteConfig.ObjectiveId,
            generated_at_utc = DateTime.UtcNow,
            seeds = suiteConfig.Seeds,
            case_ids = suiteConfig.Cases.Select(c => c.Id).ToArray(),
            budget = new
            {
                max_iterations = suiteConfig.Budget.MaxIterations,
                max_evaluations = suiteConfig.Budget.MaxEvaluations
            },
            metrics = new
            {
                known_optimal_fitness = suiteConfig.Metrics.KnownOptimalFitness,
                thresholds = suiteConfig.Metrics.Thresholds,
                log_error_epsilon = suiteConfig.Metrics.LogErrorEpsilon
            },
            recording = new
            {
                history_mode = suiteConfig.Recording.HistoryMode
            },
            run_count = runCount,
            history_record_count = historyRecordCount
        };

        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        File.WriteAllText(path, json);
    }
}