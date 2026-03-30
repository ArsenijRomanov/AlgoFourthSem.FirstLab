using System.Globalization;
using System.Text;
using Analysis.Results;

namespace Analysis;

public static class CsvWriter
{
    public static void WriteRuns(string path, IReadOnlyList<RunResult> runs, IReadOnlyList<double> thresholds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(runs);
        ArgumentNullException.ThrowIfNull(thresholds);

        EnsureDirectory(path);

        using var writer = new StreamWriter(path, false, Encoding.UTF8);

        var header = BuildRunsHeader(thresholds);
        writer.WriteLine(string.Join(",", header));

        foreach (var run in runs)
        {
            var row = BuildRunsRow(run, thresholds);
            writer.WriteLine(string.Join(",", row));
        }
    }

    public static void WriteHistory(string path, IReadOnlyList<HistoryRecord> history)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(history);

        EnsureDirectory(path);

        using var writer = new StreamWriter(path, false, Encoding.UTF8);

        writer.WriteLine(
            "suite_id,case_id,run_id,seed,iteration,evaluation_count,best_fitness,best_error,best_x,best_y");

        foreach (var record in history)
        {
            var row = new[]
            {
                Escape(record.SuiteId),
                Escape(record.CaseId),
                Escape(record.RunId),
                record.Seed.ToString(CultureInfo.InvariantCulture),
                record.Iteration.ToString(CultureInfo.InvariantCulture),
                record.EvaluationCount.ToString(CultureInfo.InvariantCulture),
                FormatDouble(record.BestFitness),
                FormatDouble(record.BestError),
                FormatDouble(record.BestX),
                FormatDouble(record.BestY)
            };

            writer.WriteLine(string.Join(",", row));
        }
    }

    private static IReadOnlyList<string> BuildRunsHeader(IReadOnlyList<double> thresholds)
    {
        var header = new List<string>
        {
            "suite_id",
            "case_id",
            "run_id",
            "seed",
            "status",
            "iterations_completed",
            "evaluation_count",
            "elapsed_ms",
            "final_best_fitness",
            "final_best_error",
            "best_x",
            "best_y"
        };

        foreach (var threshold in thresholds)
            header.Add($"reached_{FormatThresholdSuffix(threshold)}");

        foreach (var threshold in thresholds)
            header.Add($"evals_to_{FormatThresholdSuffix(threshold)}");

        header.Add("auc_log_error");

        return header;
    }

    private static IReadOnlyList<string> BuildRunsRow(RunResult run, IReadOnlyList<double> thresholds)
    {
        var row = new List<string>
        {
            Escape(run.SuiteId),
            Escape(run.CaseId),
            Escape(run.RunId),
            run.Seed.ToString(CultureInfo.InvariantCulture),
            Escape(run.Status.ToString()),
            run.IterationsCompleted.ToString(CultureInfo.InvariantCulture),
            run.EvaluationCount.ToString(CultureInfo.InvariantCulture),
            run.ElapsedMs.ToString(CultureInfo.InvariantCulture),
            FormatDouble(run.FinalBestFitness),
            FormatDouble(run.FinalBestError),
            FormatDouble(run.BestX),
            FormatDouble(run.BestY)
        };

        foreach (var threshold in thresholds)
        {
            run.ReachedThresholds.TryGetValue(threshold, out var reached);
            row.Add(reached ? "true" : "false");
        }

        foreach (var threshold in thresholds)
        {
            run.EvaluationsToThreshold.TryGetValue(threshold, out var evaluations);
            row.Add(evaluations?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
        }

        row.Add(FormatDouble(run.AucLogError));

        return row;
    }

    private static void EnsureDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }

    private static string FormatDouble(double value)
    {
        if (double.IsNaN(value))
            return "NaN";

        if (double.IsPositiveInfinity(value))
            return "Infinity";

        if (double.IsNegativeInfinity(value))
            return "-Infinity";

        return value.ToString("G17", CultureInfo.InvariantCulture);
    }

    private static string FormatThresholdSuffix(double threshold)
    {
        return threshold.ToString("0.###############################", CultureInfo.InvariantCulture)
            .Replace('-', 'm')
            .Replace('.', '_');
    }

    private static string Escape(string? value)
    {
        value ??= string.Empty;

        if (!value.Contains('"') && !value.Contains(',') && !value.Contains('\n') && !value.Contains('\r'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}