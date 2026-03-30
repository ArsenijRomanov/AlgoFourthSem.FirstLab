using GlobalOptimization.Core.Function;
using GlobalOptimization.Core.ParticleSwarmOptimization.Abstractions;
using GlobalOptimization.Core.ParticleSwarmOptimization.DTOs;

namespace GlobalOptimization.Core.ParticleSwarmOptimization;

public sealed class ParticleSwarmOptimization
{
    private readonly int _swarmSize;
    private readonly int _maxIterations;
    private readonly double _c1;
    private readonly double _c2;

    private readonly IObjectiveFunction _objectiveFunction;
    private readonly SearchSpace _searchSpace;
    private readonly ISwarmInitializer _swarmInitializer;
    private readonly IInertiaWeightStrategy _inertiaWeightStrategy;
    private readonly IConstrictionFactorStrategy _constrictionFactorStrategy;
    private readonly IVelocityLimiter _velocityLimiter;
    private readonly IBoundaryHandler _boundaryHandler;
    private readonly ISocialBestProvider _socialBestProvider;
    private readonly Random _random;

    private List<Particle> _particles = [];
    private double[] _globalBestPosition = [];

    public int Iteration { get; private set; }
    public int EvaluationCount { get; private set; }
    public bool IsInitialized { get; private set; }

    public double GlobalBestFitness { get; private set; } = double.PositiveInfinity;
    public IReadOnlyList<double> GlobalBestPosition => Array.AsReadOnly(_globalBestPosition);

    public ParticleSwarmOptimization(
        int swarmSize,
        int maxIterations,
        double c1,
        double c2,
        IObjectiveFunction objectiveFunction,
        SearchSpace searchSpace,
        ISwarmInitializer swarmInitializer,
        IInertiaWeightStrategy inertiaWeightStrategy,
        IConstrictionFactorStrategy constrictionFactorStrategy,
        IVelocityLimiter velocityLimiter,
        IBoundaryHandler boundaryHandler,
        ISocialBestProvider socialBestProvider,
        Random random)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(swarmSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxIterations);
        ArgumentOutOfRangeException.ThrowIfLessThan(c1, 0.0);
        ArgumentOutOfRangeException.ThrowIfLessThan(c2, 0.0);

        _objectiveFunction = objectiveFunction ?? throw new ArgumentNullException(nameof(objectiveFunction));
        _searchSpace = searchSpace ?? throw new ArgumentNullException(nameof(searchSpace));
        _swarmInitializer = swarmInitializer ?? throw new ArgumentNullException(nameof(swarmInitializer));
        _inertiaWeightStrategy = inertiaWeightStrategy ?? throw new ArgumentNullException(nameof(inertiaWeightStrategy));
        _constrictionFactorStrategy = constrictionFactorStrategy ?? throw new ArgumentNullException(nameof(constrictionFactorStrategy));
        _velocityLimiter = velocityLimiter ?? throw new ArgumentNullException(nameof(velocityLimiter));
        _boundaryHandler = boundaryHandler ?? throw new ArgumentNullException(nameof(boundaryHandler));
        _socialBestProvider = socialBestProvider ?? throw new ArgumentNullException(nameof(socialBestProvider));
        _random = random ?? throw new ArgumentNullException(nameof(random));

        if (_objectiveFunction.Dimension != _searchSpace.Dimension)
            throw new ArgumentException("Размерность функции и области поиска должна совпадать.");

        _swarmSize = swarmSize;
        _maxIterations = maxIterations;
        _c1 = c1;
        _c2 = c2;
    }

    public void Initialize()
    {
        ResetState();

        var initialSwarm = _swarmInitializer.InitializeSwarm(_swarmSize, _searchSpace);

        if (initialSwarm is null)
            throw new InvalidOperationException("Инициализатор роя вернул null.");

        if (initialSwarm.Count != _swarmSize)
        {
            throw new InvalidOperationException(
                $"Инициализатор роя вернул {initialSwarm.Count} частиц вместо {_swarmSize}.");
        }

        _particles = new List<Particle>(_swarmSize);

        foreach (var particle in initialSwarm)
        {
            if (particle is null)
                throw new InvalidOperationException("Инициализатор роя вернул null-частицу.");

            if (particle.Dimension != _searchSpace.Dimension)
            {
                throw new InvalidOperationException(
                    $"Размерность частицы ({particle.Dimension}) не совпадает с размерностью задачи ({_searchSpace.Dimension}).");
            }

            var fitness = Evaluate(particle.Position);
            particle.SetFitness(fitness);
            particle.UpdatePersonalBest();

            _particles.Add(particle);
        }

        UpdateGlobalBestFromParticles();
        IsInitialized = true;
    }

    public void Reset()
    {
        ResetState();
    }

    public void Step()
    {
        EnsureInitialized();

        if (Iteration >= _maxIterations)
            return;

        var context = BuildContext();

        foreach (var particle in _particles)
        {
            var w = _inertiaWeightStrategy.GetWeight(context, particle);
            var chi = _constrictionFactorStrategy.GetFactor(context, particle);
            var socialBest = _socialBestProvider.GetBestPosition(context, particle);

            for (var i = 0; i < particle.Dimension; i++)
            {
                var r1 = _random.NextDouble();
                var r2 = _random.NextDouble();

                var currentVelocity = particle.GetVelocity(i);
                var currentPosition = particle.GetPosition(i);
                var personalBest = particle.GetBestPosition(i);

                var cognitive = _c1 * r1 * (personalBest - currentPosition);
                var social = _c2 * r2 * (socialBest[i] - currentPosition);

                var newVelocity = chi * (w * currentVelocity + cognitive + social);
                particle.SetVelocity(i, newVelocity);
            }

            _velocityLimiter.Apply(context, particle);

            for (var i = 0; i < particle.Dimension; i++)
            {
                var newPosition = particle.GetPosition(i) + particle.GetVelocity(i);
                particle.SetPosition(i, newPosition);
            }

            _boundaryHandler.Apply(context, particle);

            var fitness = Evaluate(particle.Position);
            particle.SetFitness(fitness);
            particle.UpdatePersonalBest();
        }

        UpdateGlobalBestFromParticles();
        Iteration++;
    }

    public void Step(int iterations)
    {
        if (iterations >= 0)
            for (var i = 0; i < iterations; i++)
            {
                if (Iteration >= _maxIterations)
                    break;

                Step();
            }
        else
            throw new ArgumentOutOfRangeException(nameof(iterations));
    }

    private void ResetState()
    {
        _particles = [];
        _globalBestPosition = [];

        Iteration = 0;
        EvaluationCount = 0;
        IsInitialized = false;
        GlobalBestFitness = double.PositiveInfinity;
    }
    
    public ParticleDto GetParticleDto(Particle particle)
    {
        ArgumentNullException.ThrowIfNull(particle);

        return new ParticleDto(
            particle.Position,
            particle.Velocity,
            particle.Fitness,
            particle.BestPosition,
            particle.BestFitness);
    }
    
    public IReadOnlyList<ParticleDto> GetParticlesDto()
    {
        EnsureInitialized();

        var result = new List<ParticleDto>(_particles.Count);
        result.AddRange(_particles.Select(GetParticleDto));

        return result.AsReadOnly();
    }
    
    public PsoSnapshotDto GetSnapshot()
    {
        EnsureInitialized();

        var particlesDto = GetParticlesDto();

        return new PsoSnapshotDto(
            Iteration,
            EvaluationCount,
            _globalBestPosition,
            GlobalBestFitness,
            particlesDto);
    }

    private double Evaluate(IReadOnlyList<double> coordinates)
    {
        EvaluationCount++;
        return _objectiveFunction.Evaluate(coordinates);
    }

    private PsoContext BuildContext()
    {
        return new PsoContext(
            Iteration,
            _maxIterations,
            _particles,
            _globalBestPosition,
            GlobalBestFitness,
            _searchSpace,
            _c1,
            _c2);
    }

    private void UpdateGlobalBestFromParticles()
    {
        Particle? bestParticle = null;

        foreach (var particle in _particles
                     .Where(particle => bestParticle is null || particle.BestFitness < bestParticle.BestFitness))
        {
            bestParticle = particle;
        }

        GlobalBestFitness = bestParticle!.BestFitness;
        _globalBestPosition = bestParticle.GetBestPositionCopy();
    }

    private void EnsureInitialized()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Роевой алгоритм не инициализирован.");
    }
}
