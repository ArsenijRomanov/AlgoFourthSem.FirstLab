namespace GlobalOptimization.Core.ParticleSwarmOptimization;

public sealed class Particle
{
    private readonly double[] _position;
    private readonly double[] _velocity;
    private readonly double[] _bestPosition;

    public int Dimension => _position.Length;

    public IReadOnlyList<double> Position { get; }
    public IReadOnlyList<double> Velocity { get; }
    public IReadOnlyList<double> BestPosition { get; }

    public double Fitness { get; private set; }
    public double BestFitness { get; private set; }

    public Particle(
        double[] position,
        double[] velocity,
        double fitness)
    {
        ArgumentNullException.ThrowIfNull(position);

        ArgumentNullException.ThrowIfNull(velocity);

        if (position.Length == 0)
            throw new ArgumentException("Позиция частицы не может быть пустой.", nameof(position));

        if (position.Length != velocity.Length)
        {
            throw new ArgumentException(
                "Размерности позиции и скорости частицы должны совпадать.");
        }

        _position = (double[])position.Clone();
        _velocity = (double[])velocity.Clone();
        _bestPosition = (double[])position.Clone();

        Position = Array.AsReadOnly(_position);
        Velocity = Array.AsReadOnly(_velocity);
        BestPosition = Array.AsReadOnly(_bestPosition);

        Fitness = fitness;
        BestFitness = fitness;
    }

    public double GetPosition(int index)
    {
        ValidateIndex(index);
        return _position[index];
    }

    public void SetPosition(int index, double value)
    {
        ValidateIndex(index);
        _position[index] = value;
    }

    public double GetVelocity(int index)
    {
        ValidateIndex(index);
        return _velocity[index];
    }

    public void SetVelocity(int index, double value)
    {
        ValidateIndex(index);
        _velocity[index] = value;
    }

    public double GetBestPosition(int index)
    {
        ValidateIndex(index);
        return _bestPosition[index];
    }

    public void SetFitness(double fitness)
    {
        Fitness = fitness;
    }

    public bool UpdatePersonalBest()
    {
        if (Fitness >= BestFitness)
            return false;

        BestFitness = Fitness;
        Array.Copy(_position, _bestPosition, Dimension);
        return true;
    }

    public double[] GetPositionCopy()
    {
        return (double[])_position.Clone();
    }

    public double[] GetVelocityCopy()
    {
        return (double[])_velocity.Clone();
    }

    public double[] GetBestPositionCopy()
    {
        return (double[])_bestPosition.Clone();
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= Dimension)
            throw new ArgumentOutOfRangeException(nameof(index));
    }
}