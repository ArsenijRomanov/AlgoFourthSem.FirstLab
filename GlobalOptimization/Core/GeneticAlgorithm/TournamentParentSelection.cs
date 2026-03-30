using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

namespace GlobalOptimization.Core.GeneticAlgorithm;

public sealed class TournamentParentSelection<TGenome> : IParentSelection<TGenome>
    where TGenome : IGenome
{
    private readonly Random _random;
    private readonly int _tournamentSize;

    public TournamentParentSelection(Random random, int tournamentSize)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));

        if (tournamentSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(tournamentSize));

        _tournamentSize = tournamentSize;
    }

    public Individual<TGenome> Select(IReadOnlyList<Individual<TGenome>> population)
    {
        ArgumentNullException.ThrowIfNull(population);

        if (population.Count == 0)
            throw new InvalidOperationException("Популяция пуста.");

        if (_tournamentSize > population.Count)
        {
            throw new InvalidOperationException(
                $"Размер турнира ({_tournamentSize}) не может быть больше размера популяции ({population.Count}).");
        }

        var usedIndices = new HashSet<int>();
        Individual<TGenome>? best = null;

        while (usedIndices.Count < _tournamentSize)
        {
            int index = _random.Next(population.Count);

            if (!usedIndices.Add(index))
                continue;

            var candidate = population[index];

            if (best is null || candidate.Fitness < best.Fitness)
                best = candidate;
        }

        return best!;
    }
}