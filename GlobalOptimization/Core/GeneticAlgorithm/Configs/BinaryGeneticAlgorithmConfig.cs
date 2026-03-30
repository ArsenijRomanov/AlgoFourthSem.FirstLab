namespace GlobalOptimization.Core.GeneticAlgorithm.Configs;

public sealed class BinaryGeneticAlgorithmConfig : GeneticAlgorithmConfig
{
    public int BitsPerCoordinate { get; init; } = 16;
    public BinaryCrossoverType CrossoverType { get; init; }

    public double BitMutationProbability { get; init; } = 0.01;
    
    public override void Validate()
    {
        base.Validate();

        if (BitsPerCoordinate <= 0)
            throw new InvalidOperationException("Число бит на координату должно быть больше 0.");

        if (BitMutationProbability is < 0.0 or > 1.0)
            throw new InvalidOperationException("Вероятность побитовой мутации должна быть в диапазоне [0, 1].");
    }
}
