using System.Collections.ObjectModel;
using System.Windows.Input;
using GlobalOptimization.Avalonia.Infrastructure;
using GlobalOptimization.Avalonia.Models;
using GlobalOptimization.Avalonia.Services;
using GlobalOptimization.Core.GeneticAlgorithm.Configs;
using GlobalOptimization.Core.ParticleSwarmOptimization;

namespace GlobalOptimization.Avalonia.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly OptimizationSession _session;

    private string _functionExpression = "(x^2 + y - 11)^2 + (x + y^2 - 7)^2";

    private decimal _xMin = -6;
    private decimal _xMax = 6;
    private decimal _yMin = -6;
    private decimal _yMax = 6;

    private decimal _iterationsPerRun = 10;
    private decimal _populationSize = 100;
    private decimal _bitsPerCoordinate = 16;
    private decimal _swarmSize = 100;

    private NamedOption<OptimizationAlgorithmKind> _selectedAlgorithm;
    private NamedOption<GenomeEncodingKind> _selectedGenomeEncoding;
    private NamedOption<BinaryCrossoverType> _selectedBinaryCrossover;
    private NamedOption<RealCrossoverType> _selectedRealCrossover;
    private NamedOption<ConstrictionFactorType> _selectedConstrictionFactorType;

    private int _currentIteration;
    private int _evaluationCount;
    private string _bestFitnessText = "—";
    private string _bestPointText = "—";
    private string _statusMessage = "Готово к запуску.";

    private IReadOnlyList<ScatterPointModel> _plotPoints = Array.Empty<ScatterPointModel>();
    private ScatterPointModel? _highlightedPoint;
    private OptimizationSessionSignature? _lastSignature;

    public MainWindowViewModel()
        : this(new OptimizationSession())
    {
    }

    public MainWindowViewModel(OptimizationSession session)
    {
        _session = session;

        AlgorithmOptions =
        [
            new NamedOption<OptimizationAlgorithmKind>("Генетический алгоритм", OptimizationAlgorithmKind.GeneticAlgorithm),
            new NamedOption<OptimizationAlgorithmKind>("Роевой алгоритм", OptimizationAlgorithmKind.ParticleSwarmOptimization)
        ];

        GenomeEncodingOptions =
        [
            new NamedOption<GenomeEncodingKind>("Вещественное", GenomeEncodingKind.Real),
            new NamedOption<GenomeEncodingKind>("Бинарное", GenomeEncodingKind.Binary)
        ];

        BinaryCrossoverOptions =
        [
            new NamedOption<BinaryCrossoverType>("Одноточечный", BinaryCrossoverType.OnePoint),
            new NamedOption<BinaryCrossoverType>("Равномерный", BinaryCrossoverType.Uniform)
        ];

        RealCrossoverOptions =
        [
            new NamedOption<RealCrossoverType>("Арифметический", RealCrossoverType.Arithmetic),
            new NamedOption<RealCrossoverType>("SBX", RealCrossoverType.Sbx)
        ];

        ConstrictionFactorOptions =
        [
            new NamedOption<ConstrictionFactorType>("Без сжатия", ConstrictionFactorType.Unit),
            new NamedOption<ConstrictionFactorType>("Канонический", ConstrictionFactorType.Canonical),
            new NamedOption<ConstrictionFactorType>("Динамический", ConstrictionFactorType.Dynamic)
        ];

        _selectedAlgorithm = AlgorithmOptions[0];
        _selectedGenomeEncoding = GenomeEncodingOptions[0];
        _selectedBinaryCrossover = BinaryCrossoverOptions[0];
        _selectedRealCrossover = RealCrossoverOptions[0];
        _selectedConstrictionFactorType = ConstrictionFactorOptions[1];

        RunCommand = new RelayCommand(Run);
        ResetCommand = new RelayCommand(Reset);
    }

    public IReadOnlyList<NamedOption<OptimizationAlgorithmKind>> AlgorithmOptions { get; }
    public IReadOnlyList<NamedOption<GenomeEncodingKind>> GenomeEncodingOptions { get; }
    public IReadOnlyList<NamedOption<BinaryCrossoverType>> BinaryCrossoverOptions { get; }
    public IReadOnlyList<NamedOption<RealCrossoverType>> RealCrossoverOptions { get; }
    public IReadOnlyList<NamedOption<ConstrictionFactorType>> ConstrictionFactorOptions { get; }

    public ObservableCollection<ResultPointRowViewModel> TablePoints { get; } = new();
    public ObservableCollection<FitnessHistoryEntry> History { get; } = new();

    public ICommand RunCommand { get; }
    public ICommand ResetCommand { get; }

    public string FunctionExpression
    {
        get => _functionExpression;
        set => SetProperty(ref _functionExpression, value);
    }

    public decimal XMin
    {
        get => _xMin;
        set => SetProperty(ref _xMin, value);
    }

    public decimal XMax
    {
        get => _xMax;
        set => SetProperty(ref _xMax, value);
    }

    public decimal YMin
    {
        get => _yMin;
        set => SetProperty(ref _yMin, value);
    }

    public decimal YMax
    {
        get => _yMax;
        set => SetProperty(ref _yMax, value);
    }

    public decimal IterationsPerRun
    {
        get => _iterationsPerRun;
        set => SetProperty(ref _iterationsPerRun, value);
    }

    public decimal PopulationSize
    {
        get => _populationSize;
        set => SetProperty(ref _populationSize, value);
    }

    public decimal BitsPerCoordinate
    {
        get => _bitsPerCoordinate;
        set => SetProperty(ref _bitsPerCoordinate, value);
    }

    public decimal SwarmSize
    {
        get => _swarmSize;
        set => SetProperty(ref _swarmSize, value);
    }

    public NamedOption<OptimizationAlgorithmKind> SelectedAlgorithm
    {
        get => _selectedAlgorithm;
        set
        {
            if (!SetProperty(ref _selectedAlgorithm, value))
                return;

            RaisePropertyChanged(nameof(IsGeneticAlgorithm));
            RaisePropertyChanged(nameof(IsParticleSwarmOptimization));
        }
    }

    public NamedOption<GenomeEncodingKind> SelectedGenomeEncoding
    {
        get => _selectedGenomeEncoding;
        set
        {
            if (!SetProperty(ref _selectedGenomeEncoding, value))
                return;

            RaisePropertyChanged(nameof(IsBinaryEncoding));
            RaisePropertyChanged(nameof(IsRealEncoding));
        }
    }

    public NamedOption<BinaryCrossoverType> SelectedBinaryCrossover
    {
        get => _selectedBinaryCrossover;
        set => SetProperty(ref _selectedBinaryCrossover, value);
    }

    public NamedOption<RealCrossoverType> SelectedRealCrossover
    {
        get => _selectedRealCrossover;
        set => SetProperty(ref _selectedRealCrossover, value);
    }

    public NamedOption<ConstrictionFactorType> SelectedConstrictionFactorType
    {
        get => _selectedConstrictionFactorType;
        set => SetProperty(ref _selectedConstrictionFactorType, value);
    }

    public bool IsGeneticAlgorithm => SelectedAlgorithm.Value == OptimizationAlgorithmKind.GeneticAlgorithm;
    public bool IsParticleSwarmOptimization => SelectedAlgorithm.Value == OptimizationAlgorithmKind.ParticleSwarmOptimization;
    public bool IsBinaryEncoding => SelectedGenomeEncoding.Value == GenomeEncodingKind.Binary;
    public bool IsRealEncoding => SelectedGenomeEncoding.Value == GenomeEncodingKind.Real;

    public int CurrentIteration
    {
        get => _currentIteration;
        private set => SetProperty(ref _currentIteration, value);
    }

    public int EvaluationCount
    {
        get => _evaluationCount;
        private set => SetProperty(ref _evaluationCount, value);
    }

    public string BestFitnessText
    {
        get => _bestFitnessText;
        private set => SetProperty(ref _bestFitnessText, value);
    }

    public string BestPointText
    {
        get => _bestPointText;
        private set => SetProperty(ref _bestPointText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public IReadOnlyList<ScatterPointModel> PlotPoints
    {
        get => _plotPoints;
        private set => SetProperty(ref _plotPoints, value);
    }

    public ScatterPointModel? HighlightedPoint
    {
        get => _highlightedPoint;
        private set => SetProperty(ref _highlightedPoint, value);
    }

    public bool HasHistory => History.Count > 0;

    private void Run()
    {
        try
        {
            var request = BuildRequest();
            var signature = OptimizationSessionSignature.FromRequest(request);

            if (_lastSignature != signature)
            {
                History.Clear();
                TablePoints.Clear();
                _session.Reset();
            }

            var result = _session.Run(request);

            ApplyResult(result);

            History.Add(new FitnessHistoryEntry(result.Iteration, result.BestFitness));
            RaisePropertyChanged(nameof(HasHistory));

            _lastSignature = signature;
            StatusMessage = "Расчёт выполнен успешно.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private void Reset()
    {
        _session.Reset();
        _lastSignature = null;

        CurrentIteration = 0;
        EvaluationCount = 0;
        BestFitnessText = "—";
        BestPointText = "—";
        PlotPoints = Array.Empty<ScatterPointModel>();
        HighlightedPoint = null;

        TablePoints.Clear();
        History.Clear();
        RaisePropertyChanged(nameof(HasHistory));

        StatusMessage = "Состояние алгоритма сброшено.";
    }

    private OptimizationRunRequest BuildRequest()
    {
        return new OptimizationRunRequest(
            FunctionExpression,
            (double)XMin,
            (double)XMax,
            (double)YMin,
            (double)YMax,
            decimal.ToInt32(IterationsPerRun),
            SelectedAlgorithm.Value,
            decimal.ToInt32(PopulationSize),
            SelectedGenomeEncoding.Value,
            SelectedBinaryCrossover.Value,
            SelectedRealCrossover.Value,
            decimal.ToInt32(BitsPerCoordinate),
            decimal.ToInt32(SwarmSize),
            SelectedConstrictionFactorType.Value);
    }

    private void ApplyResult(OptimizationRunResult result)
    {
        CurrentIteration = result.Iteration;
        EvaluationCount = result.EvaluationCount;
        BestFitnessText = result.BestFitness.ToString("G10");
        BestPointText = $"({result.BestX:G8}; {result.BestY:G8})";

        PlotPoints = result.Points
            .Select(point => new ScatterPointModel(point.X, point.Y, point.Fitness, point.IsBestCurrentPoint))
            .ToList();

        HighlightedPoint = result.HighlightedPoint;

        TablePoints.Clear();

        for (int i = 0; i < result.Points.Count; i++)
        {
            var point = result.Points[i];
            TablePoints.Add(new ResultPointRowViewModel
            {
                Rank = i + 1,
                X = point.X,
                Y = point.Y,
                Fitness = point.Fitness,
                IsBest = i == 0
            });
        }
    }
}
