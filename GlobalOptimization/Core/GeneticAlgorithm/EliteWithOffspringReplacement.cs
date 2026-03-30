using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

namespace GlobalOptimization.Core.GeneticAlgorithm;

public sealed class EliteWithOffspringReplacement<TGenome> : IReplacementStrategy<TGenome>
    where TGenome : IGenome
{
    private readonly int _eliteCount;

    public EliteWithOffspringReplacement(int eliteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(eliteCount);

        _eliteCount = eliteCount;
    }

    public IReadOnlyList<Individual<TGenome>> CreateNextPopulation(
        IReadOnlyList<Individual<TGenome>> currentPopulation,
        IReadOnlyList<Individual<TGenome>> offspring,
        int targetPopulationSize)
    {
        ArgumentNullException.ThrowIfNull(currentPopulation);
        ArgumentNullException.ThrowIfNull(offspring);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetPopulationSize);

        if (currentPopulation.Count == 0)
            throw new InvalidOperationException("Текущая популяция пуста.");

        if (_eliteCount > targetPopulationSize)
        {
            throw new InvalidOperationException(
                $"Размер элиты ({_eliteCount}) не может быть больше размера популяции ({targetPopulationSize}).");
        }

        if (_eliteCount > currentPopulation.Count)
        {
            throw new InvalidOperationException(
                $"Размер элиты ({_eliteCount}) не может быть больше размера текущей популяции ({currentPopulation.Count}).");
        }

        int offspringNeeded = targetPopulationSize - _eliteCount;

        if (offspring.Count < offspringNeeded)
        {
            throw new InvalidOperationException(
                $"Недостаточно потомков для формирования нового поколения: требуется {offspringNeeded}, получено {offspring.Count}.");
        }

        var nextPopulation = new List<Individual<TGenome>>(targetPopulationSize);

        var elites = currentPopulation
            .OrderBy(individual => individual.Fitness)
            .Take(_eliteCount);

        nextPopulation.AddRange(elites);

        var bestOffspring = offspring
            .OrderBy(individual => individual.Fitness)
            .Take(offspringNeeded);

        nextPopulation.AddRange(bestOffspring);

        return nextPopulation.AsReadOnly();
    }
}
