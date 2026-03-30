using GlobalOptimization.Core.Function;

namespace Analysis;

public static class ObjectiveRegistry
{
    private static readonly IReadOnlyDictionary<string, ObjectiveDefinition> Definitions =
        new Dictionary<string, ObjectiveDefinition>(StringComparer.Ordinal)
        {
            ["function_v8"] = CreateFunctionV8()
        };

    public static ObjectiveDefinition Get(string objectiveId)
    {
        if (string.IsNullOrWhiteSpace(objectiveId))
            throw new ArgumentException("Objective id не может быть пустым.", nameof(objectiveId));

        if (!Definitions.TryGetValue(objectiveId, out var definition))
            throw new KeyNotFoundException($"Неизвестная целевая функция: '{objectiveId}'.");

        return definition;
    }

    private static ObjectiveDefinition CreateFunctionV8()
    {
        const string id = "function_v8";
        const string name = "Himmelblau function";
        const int dimension = 2;

        var searchSpace = new SearchSpace(
        [
            (-6.0, 6.0),
            (-6.0, 6.0)
        ]);

        var objectiveFunction = new DelegateObjectiveFunction(
            name,
            dimension,
            coordinates =>
            {
                var x = coordinates[0];
                var y = coordinates[1];

                return Math.Pow(x * x + y - 11.0, 2.0)
                       + Math.Pow(x + y * y - 7.0, 2.0);
            });

        return new ObjectiveDefinition(
            id,
            name,
            dimension,
            searchSpace,
            objectiveFunction,
            knownOptimalFitness: 0.0);
    }
}