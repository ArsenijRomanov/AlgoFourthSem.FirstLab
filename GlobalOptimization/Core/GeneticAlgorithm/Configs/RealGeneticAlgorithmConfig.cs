namespace GlobalOptimization.Core.GeneticAlgorithm.Configs;

public sealed class RealGeneticAlgorithmConfig : GeneticAlgorithmConfig
{
    public RealCrossoverType CrossoverType { get; init; }

    public double GeneMutationProbability { get; init; } = 0.1;
    public double Sigma { get; init; } = 0.1;

    public double SbxDistributionIndex { get; init; } = 2.0;
    
    public override void Validate()
    {
        base.Validate();

        if (GeneMutationProbability is < 0.0 or > 1.0)
            throw new InvalidOperationException("Вероятность мутации гена должна быть в диапазоне [0, 1].");

        if (Sigma < 0.0)
            throw new InvalidOperationException("Sigma не может быть отрицательной.");

        if (CrossoverType == RealCrossoverType.Sbx && SbxDistributionIndex < 0.0)
            throw new InvalidOperationException("Индекс распределения SBX не может быть отрицательным.");
    }
}
