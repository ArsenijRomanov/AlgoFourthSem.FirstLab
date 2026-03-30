using GlobalOptimization.Avalonia.Models;
using GlobalOptimization.Core.Function;
using GlobalOptimization.Core.GeneticAlgorithm;
using GlobalOptimization.Core.GeneticAlgorithm.Configs;
using GlobalOptimization.Core.GeneticAlgorithm.DTOs;
using GlobalOptimization.Core.GeneticAlgorithm.Genomes;
using GlobalOptimization.Core.ParticleSwarmOptimization;

namespace GlobalOptimization.Avalonia.Services;

public sealed class OptimizationSession
{
    private const int InternalPsoMaxIterations = 1_000_000;

    private OptimizationSessionSignature? _signature;

    private GeneticAlgorithm<BinaryGenome>? _binaryGa;
    private GeneticAlgorithm<RealGenome>? _realGa;
    private ParticleSwarmOptimization? _pso;

    public OptimizationSessionSignature? CurrentSignature => _signature;

    public OptimizationRunResult Run(OptimizationRunRequest request)
    {
        ValidateRequest(request);

        var signature = OptimizationSessionSignature.FromRequest(request);

        if (_signature != signature || !HasInitializedAlgorithm())
            BuildSession(request, signature);

        if (_binaryGa is not null)
        {
            if (!_binaryGa.IsInitialized)
                _binaryGa.Initialize();

            _binaryGa.Step(request.IterationsToRun);
            return BuildBinaryGaResult(_binaryGa);
        }

        if (_realGa is not null)
        {
            if (!_realGa.IsInitialized)
                _realGa.Initialize();

            _realGa.Step(request.IterationsToRun);
            return BuildRealGaResult(_realGa);
        }

        if (_pso is not null)
        {
            if (!_pso.IsInitialized)
                _pso.Initialize();

            _pso.Step(request.IterationsToRun);
            return BuildPsoResult(_pso);
        }

        throw new InvalidOperationException("Алгоритм не был создан.");
    }

    public void Reset()
    {
        _binaryGa?.Reset();
        _realGa?.Reset();
        _pso?.Reset();
    }

    private bool HasInitializedAlgorithm()
        => _binaryGa is not null || _realGa is not null || _pso is not null;

    private void BuildSession(OptimizationRunRequest request, OptimizationSessionSignature signature)
    {
        Reset();
        _binaryGa = null;
        _realGa = null;
        _pso = null;

        var objective = BuildObjectiveFunction(request.FunctionExpression);
        var searchSpace = new SearchSpace(new List<(double Min, double Max)>
        {
            (request.XMin, request.XMax),
            (request.YMin, request.YMax)
        });

        switch (request.AlgorithmKind)
        {
            case OptimizationAlgorithmKind.GeneticAlgorithm:
                if (request.GenomeEncoding == GenomeEncodingKind.Binary)
                {
                    var config = new BinaryGeneticAlgorithmConfig
                    {
                        PopulationSize = request.PopulationSize,
                        BitsPerCoordinate = request.BitsPerCoordinate,
                        CrossoverType = request.BinaryCrossoverType
                    };

                    _binaryGa = GeneticAlgorithmFactory.Create(objective, searchSpace, config);
                }
                else
                {
                    var config = new RealGeneticAlgorithmConfig
                    {
                        PopulationSize = request.PopulationSize,
                        CrossoverType = request.RealCrossoverType
                    };

                    _realGa = GeneticAlgorithmFactory.Create(objective, searchSpace, config);
                }

                break;

            case OptimizationAlgorithmKind.ParticleSwarmOptimization:
                var psoConfig = new ParticleSwarmOptimizationConfig
                {
                    SwarmSize = request.SwarmSize,
                    MaxIterations = InternalPsoMaxIterations,
                    ConstrictionFactorType = request.ConstrictionFactorType
                };

                _pso = PsoFactory.Create(objective, searchSpace, psoConfig);
                break;

            default:
                throw new InvalidOperationException("Неизвестный алгоритм.");
        }

        _signature = signature;
    }

    private static IObjectiveFunction BuildObjectiveFunction(string expressionText)
    {
        var compiled = ExpressionCompiler.Compile(expressionText);

        return new DelegateObjectiveFunction(
            "UserFunction",
            2,
            coordinates => compiled(coordinates[0], coordinates[1]));
    }

    private static OptimizationRunResult BuildBinaryGaResult(GeneticAlgorithm<BinaryGenome> algorithm)
    {
        var snapshot = algorithm.GetSnapshot();
        var sortedPoints = snapshot.Population
            .OrderBy(point => point.Fitness)
            .ToList();

        return BuildGaResult(snapshot.Iteration, snapshot.EvaluationCount, snapshot.BestPoint, sortedPoints);
    }

    private static OptimizationRunResult BuildRealGaResult(GeneticAlgorithm<RealGenome> algorithm)
    {
        var snapshot = algorithm.GetSnapshot();
        var sortedPoints = snapshot.Population
            .OrderBy(point => point.Fitness)
            .ToList();

        return BuildGaResult(snapshot.Iteration, snapshot.EvaluationCount, snapshot.BestPoint, sortedPoints);
    }

    private static OptimizationRunResult BuildGaResult(
        int iteration,
        int evaluationCount,
        PointDto bestPoint,
        IReadOnlyList<PointDto> sortedPoints)
    {
        var resultPoints = new List<ResultPoint>(sortedPoints.Count);

        for (int i = 0; i < sortedPoints.Count; i++)
        {
            var point = sortedPoints[i];
            resultPoints.Add(new ResultPoint(
                point.Coordinates[0],
                point.Coordinates[1],
                point.Fitness,
                i == 0));
        }

        return new OptimizationRunResult
        {
            Iteration = iteration,
            EvaluationCount = evaluationCount,
            BestFitness = bestPoint.Fitness,
            BestX = bestPoint.Coordinates[0],
            BestY = bestPoint.Coordinates[1],
            Points = resultPoints,
            HighlightedPoint = new ScatterPointModel(
                bestPoint.Coordinates[0],
                bestPoint.Coordinates[1],
                bestPoint.Fitness,
                true)
        };
    }

    private static OptimizationRunResult BuildPsoResult(ParticleSwarmOptimization algorithm)
    {
        var snapshot = algorithm.GetSnapshot();

        var sortedPoints = snapshot.Particles
            .OrderBy(particle => particle.Fitness)
            .Select((particle, index) => new ResultPoint(
                particle.Position[0],
                particle.Position[1],
                particle.Fitness,
                index == 0))
            .ToList();

        return new OptimizationRunResult
        {
            Iteration = snapshot.Iteration,
            EvaluationCount = snapshot.EvaluationCount,
            BestFitness = snapshot.GlobalBestFitness,
            BestX = snapshot.GlobalBestPosition[0],
            BestY = snapshot.GlobalBestPosition[1],
            Points = sortedPoints,
            HighlightedPoint = new ScatterPointModel(
                snapshot.GlobalBestPosition[0],
                snapshot.GlobalBestPosition[1],
                snapshot.GlobalBestFitness,
                true)
        };
    }

    private static void ValidateRequest(OptimizationRunRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionExpression))
            throw new InvalidOperationException("Введите функцию.");

        if (request.XMin >= request.XMax)
            throw new InvalidOperationException("Для диапазона X должно выполняться xmin < xmax.");

        if (request.YMin >= request.YMax)
            throw new InvalidOperationException("Для диапазона Y должно выполняться ymin < ymax.");

        if (request.IterationsToRun < 0)
            throw new InvalidOperationException("Число итераций не может быть отрицательным.");

        if (request.PopulationSize <= 0)
            throw new InvalidOperationException("Размер популяции должен быть больше 0.");

        if (request.SwarmSize <= 0)
            throw new InvalidOperationException("Размер роя должен быть больше 0.");

        if (request.BitsPerCoordinate <= 0)
            throw new InvalidOperationException("Число бит должно быть больше 0.");
    }
}
