namespace GlobalOptimization.Core.GeneticAlgorithm.DTOs;

public sealed class PointDto
{
    public IReadOnlyList<double> Coordinates { get; }
    public double Fitness { get; }

    public PointDto(IEnumerable<double> coordinates, double fitness)
    {
        if (coordinates is null)
            throw new ArgumentNullException(nameof(coordinates));

        var copy = coordinates.ToArray();

        if (copy.Length == 0)
            throw new ArgumentException("Точка не может быть пустой.", nameof(coordinates));

        Coordinates = Array.AsReadOnly(copy);
        Fitness = fitness;
    }

    public override string ToString()
    {
        return $"({string.Join(", ", Coordinates)}) -> {Fitness}";
    }
}