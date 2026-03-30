using Analysis.Configs;

namespace Analysis;

public static class ExperimentRunnerFacade
{
    public static void RunToDirectory(ExperimentSuiteConfig suiteConfig, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(suiteConfig);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        var runner = new ExperimentRunner();
        var output = runner.Run(suiteConfig);

        CsvWriter.WriteRuns(
            Path.Combine(outputDirectory, "runs.csv"),
            output.Runs,
            suiteConfig.Metrics.Thresholds);

        CsvWriter.WriteHistory(
            Path.Combine(outputDirectory, "history.csv"),
            output.History);

        ManifestWriter.Write(
            Path.Combine(outputDirectory, "manifest.json"),
            suiteConfig,
            output.Runs.Count,
            output.History.Count);
    }
}