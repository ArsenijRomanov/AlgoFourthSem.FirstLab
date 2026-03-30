using System.Diagnostics;
using Analysis.Adapters;
using Analysis.Configs;
using Analysis.Results;

namespace Analysis;

public sealed class ExperimentRunner
{
    public ExperimentRunOutput Run(ExperimentSuiteConfig suiteConfig)
    {
        ArgumentNullException.ThrowIfNull(suiteConfig);
        suiteConfig.Validate();

        var objective = ObjectiveRegistry.Get(suiteConfig.ObjectiveId);

        if (Math.Abs(objective.KnownOptimalFitness - suiteConfig.Metrics.KnownOptimalFitness) > 1e-12)
        {
            throw new InvalidOperationException(
                $"KnownOptimalFitness в suite ({suiteConfig.Metrics.KnownOptimalFitness}) " +
                $"не совпадает с реестром функции ({objective.KnownOptimalFitness}).");
        }

        var runs = new List<RunResult>();
        var history = new List<HistoryRecord>();

        foreach (var caseConfig in suiteConfig.Cases)
        {
            foreach (var seed in suiteConfig.Seeds)
            {
                var runOutput = RunSingleCase(suiteConfig, caseConfig, objective, seed);
                runs.Add(runOutput.Run);
                history.AddRange(runOutput.History);
            }
        }

        return new ExperimentRunOutput(runs, history);
    }

    private SingleRunOutput RunSingleCase(
        ExperimentSuiteConfig suiteConfig,
        ExperimentCaseConfig caseConfig,
        ObjectiveDefinition objective,
        int seed)
    {
        var runId = $"{caseConfig.Id}__seed_{seed}";
        var history = new List<HistoryRecord>();
        var thresholdReached = suiteConfig.Metrics.Thresholds.ToDictionary(t => t, _ => false);
        var evalsToThreshold = suiteConfig.Metrics.Thresholds.ToDictionary<double, double, int?>(t => t, _ => null);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var adapter = OptimizationMethodFactory.Create(caseConfig, objective, suiteConfig.Budget, seed);

            adapter.Initialize();

            AddHistoryRecord(
                suiteConfig,
                adapter,
                runId,
                seed,
                objective.KnownOptimalFitness,
                history);

            UpdateThresholds(
                suiteConfig.Metrics.Thresholds,
                thresholdReached,
                evalsToThreshold,
                adapter.BestFitness,
                objective.KnownOptimalFitness,
                adapter.EvaluationCount);

            while (!adapter.IsFinished)
            {
                adapter.Step();

                AddHistoryRecord(
                    suiteConfig,
                    adapter,
                    runId,
                    seed,
                    objective.KnownOptimalFitness,
                    history);

                UpdateThresholds(
                    suiteConfig.Metrics.Thresholds,
                    thresholdReached,
                    evalsToThreshold,
                    adapter.BestFitness,
                    objective.KnownOptimalFitness,
                    adapter.EvaluationCount);
            }

            stopwatch.Stop();

            var status = ResolveStatus(adapter, suiteConfig.Budget);

            var (bestX, bestY) = GetBestCoordinates(adapter.BestPoint);
            var finalError = CalculateBestError(adapter.BestFitness, objective.KnownOptimalFitness);
            var aucLogError = CalculateAucLogError(history, suiteConfig.Metrics.LogErrorEpsilon);

            var runResult = new RunResult
            {
                SuiteId = suiteConfig.SuiteId,
                CaseId = caseConfig.Id,
                RunId = runId,
                Seed = seed,
                Status = status,
                IterationsCompleted = adapter.Iteration,
                EvaluationCount = adapter.EvaluationCount,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                FinalBestFitness = adapter.BestFitness,
                FinalBestError = finalError,
                BestX = bestX,
                BestY = bestY,
                ReachedThresholds = new Dictionary<double, bool>(thresholdReached),
                EvaluationsToThreshold = new Dictionary<double, int?>(evalsToThreshold),
                AucLogError = aucLogError
            };

            return new SingleRunOutput(runResult, history);
        }
        catch
        {
            stopwatch.Stop();

            var failedResult = new RunResult
            {
                SuiteId = suiteConfig.SuiteId,
                CaseId = caseConfig.Id,
                RunId = runId,
                Seed = seed,
                Status = RunStatus.Failed,
                IterationsCompleted = history.Count == 0 ? 0 : history[^1].Iteration,
                EvaluationCount = history.Count == 0 ? 0 : history[^1].EvaluationCount,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                FinalBestFitness = double.PositiveInfinity,
                FinalBestError = double.PositiveInfinity,
                BestX = double.NaN,
                BestY = double.NaN,
                ReachedThresholds = new Dictionary<double, bool>(thresholdReached),
                EvaluationsToThreshold = new Dictionary<double, int?>(evalsToThreshold),
                AucLogError = double.PositiveInfinity
            };

            return new SingleRunOutput(failedResult, history);
        }
    }

    private static void AddHistoryRecord(
        ExperimentSuiteConfig suiteConfig,
        IOptimizationAdapter adapter,
        string runId,
        int seed,
        double knownOptimalFitness,
        List<HistoryRecord> history)
    {
        var (bestX, bestY) = GetBestCoordinates(adapter.BestPoint);
        var bestError = CalculateBestError(adapter.BestFitness, knownOptimalFitness);

        history.Add(new HistoryRecord
        {
            SuiteId = suiteConfig.SuiteId,
            CaseId = adapter.CaseId,
            RunId = runId,
            Seed = seed,
            Iteration = adapter.Iteration,
            EvaluationCount = adapter.EvaluationCount,
            BestFitness = adapter.BestFitness,
            BestError = bestError,
            BestX = bestX,
            BestY = bestY
        });
    }

    private static void UpdateThresholds(
        IReadOnlyList<double> thresholds,
        IDictionary<double, bool> thresholdReached,
        IDictionary<double, int?> evalsToThreshold,
        double bestFitness,
        double knownOptimalFitness,
        int evaluationCount)
    {
        var bestError = CalculateBestError(bestFitness, knownOptimalFitness);

        foreach (var threshold in thresholds)
        {
            if (thresholdReached[threshold])
                continue;

            if (bestError <= threshold)
            {
                thresholdReached[threshold] = true;
                evalsToThreshold[threshold] = evaluationCount;
            }
        }
    }

    private static RunStatus ResolveStatus(IOptimizationAdapter adapter, ExperimentBudgetConfig budget)
    {
        if (budget.MaxEvaluations.HasValue && adapter.EvaluationCount >= budget.MaxEvaluations.Value)
            return RunStatus.StoppedByEvaluationLimit;

        if (adapter.Iteration >= budget.MaxIterations)
            return RunStatus.StoppedByIterationLimit;

        return RunStatus.Completed;
    }

    private static double CalculateBestError(double bestFitness, double knownOptimalFitness)
    {
        var error = bestFitness - knownOptimalFitness;
        return error < 0.0 ? 0.0 : error;
    }

    private static (double X, double Y) GetBestCoordinates(IReadOnlyList<double> point)
    {
        if (point.Count >= 2)
            return (point[0], point[1]);

        return (double.NaN, double.NaN);
    }

    private static double CalculateAucLogError(
        IReadOnlyList<HistoryRecord> history,
        double logErrorEpsilon)
    {
        if (history.Count < 2)
            return 0.0;

        double area = 0.0;

        for (int i = 1; i < history.Count; i++)
        {
            var left = history[i - 1];
            var right = history[i];

            var x1 = left.EvaluationCount;
            var x2 = right.EvaluationCount;

            if (x2 <= x1)
                continue;

            var y1 = Math.Log10(left.BestError + logErrorEpsilon);
            var y2 = Math.Log10(right.BestError + logErrorEpsilon);

            area += (x2 - x1) * (y1 + y2) * 0.5;
        }

        return area;
    }

    private sealed class SingleRunOutput(RunResult run, IReadOnlyList<HistoryRecord> history)
    {
        public RunResult Run { get; } = run ?? throw new ArgumentNullException(nameof(run));
        public IReadOnlyList<HistoryRecord> History { get; } = history ?? throw new ArgumentNullException(nameof(history));
    }
}