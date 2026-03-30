namespace GlobalOptimization.Core.GeneticAlgorithm.Configs;

public abstract class GeneticAlgorithmConfig
{
    public int PopulationSize { get; init; } = 100;
    public int TournamentSize { get; init; } = 3;
    public int EliteCount { get; init; } = 2;
    public int RandomSeed { get; init; } = 1;

    public double CrossoverProbability { get; init; } = 1.0;
    
    public virtual void Validate()
    {
        if (PopulationSize <= 0)
            throw new InvalidOperationException("Размер популяции должен быть больше 0.");

        if (TournamentSize <= 0)
            throw new InvalidOperationException("Размер турнира должен быть больше 0.");

        if (EliteCount < 0)
            throw new InvalidOperationException("Размер элиты не может быть отрицательным.");

        if (EliteCount > PopulationSize)
            throw new InvalidOperationException("Размер элиты не может быть больше размера популяции.");

        if (TournamentSize > PopulationSize)
            throw new InvalidOperationException("Размер турнира не может быть больше размера популяции.");

        if (CrossoverProbability is <= 0.0 or > 1.0)
            throw new InvalidOperationException("Вероятность кроссинговера должна быть в диапазоне [0, 1].");
    }
}