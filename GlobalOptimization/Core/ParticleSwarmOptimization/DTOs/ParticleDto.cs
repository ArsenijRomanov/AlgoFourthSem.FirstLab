namespace GlobalOptimization.Core.ParticleSwarmOptimization.DTOs;

public sealed class ParticleDto
{
    public IReadOnlyList<double> Position { get; }
    public IReadOnlyList<double> Velocity { get; }
    public double Fitness { get; }

    public IReadOnlyList<double> BestPosition { get; }
    public double BestFitness { get; }

    public ParticleDto(
        IEnumerable<double> position,
        IEnumerable<double> velocity,
        double fitness,
        IEnumerable<double> bestPosition,
        double bestFitness)
    {
        ArgumentNullException.ThrowIfNull(position);

        ArgumentNullException.ThrowIfNull(velocity);

        ArgumentNullException.ThrowIfNull(bestPosition);

        var positionCopy = position.ToArray();
        var velocityCopy = velocity.ToArray();
        var bestPositionCopy = bestPosition.ToArray();

        if (positionCopy.Length == 0)
            throw new ArgumentException("Позиция частицы не может быть пустой.", nameof(position));

        if (velocityCopy.Length != positionCopy.Length)
            throw new ArgumentException("Размерности позиции и скорости частицы должны совпадать.", nameof(velocity));

        if (bestPositionCopy.Length != positionCopy.Length)
            throw new ArgumentException(
                "Размерность лучшей позиции частицы должна совпадать с размерностью текущей позиции.", nameof(bestPosition));

        Position = Array.AsReadOnly(positionCopy);
        Velocity = Array.AsReadOnly(velocityCopy);
        Fitness = fitness;

        BestPosition = Array.AsReadOnly(bestPositionCopy);
        BestFitness = bestFitness;
    }
}
