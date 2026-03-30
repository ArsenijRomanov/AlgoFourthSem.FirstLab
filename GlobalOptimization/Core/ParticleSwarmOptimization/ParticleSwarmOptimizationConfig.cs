namespace GlobalOptimization.Core.ParticleSwarmOptimization;

public sealed class ParticleSwarmOptimizationConfig
{
    public int SwarmSize { get; init; } = 100;
    public int MaxIterations { get; init; } = 200;
    public int RandomSeed { get; init; } = 1;

    public double C1 { get; init; } = 2.05;
    public double C2 { get; init; } = 2.05;

    public double ConstantInertiaWeight { get; init; } = 1.0;

    public double DynamicConstrictionChiMax { get; init; } = 1.0;

    public double VelocityLimitRatio { get; init; } = 0.2;
    
    public ConstrictionFactorType ConstrictionFactorType { get; init; } =
        ConstrictionFactorType.Unit;

    public void Validate()
    {
        if (SwarmSize <= 0)
            throw new InvalidOperationException("Размер роя должен быть больше 0.");

        if (MaxIterations <= 0)
            throw new InvalidOperationException("Максимальное число итераций должно быть больше 0.");

        if (C1 <= 0.0)
            throw new InvalidOperationException("Коэффициент c1 не может быть отрицательным.");

        if (C2 <= 0.0)
            throw new InvalidOperationException("Коэффициент c2 не может быть отрицательным.");

        if (ConstantInertiaWeight <= 0.0)
            throw new InvalidOperationException("Постоянный вес инерции не может быть отрицательным.");

        if (DynamicConstrictionChiMax <= 0.0)
            throw new InvalidOperationException("ChiMax не может быть отрицательным.");

        if (VelocityLimitRatio <= 0.0)
            throw new InvalidOperationException("Коэффициент ограничения скорости не может быть отрицательным.");

        if (ConstrictionFactorType is ConstrictionFactorType.Canonical or ConstrictionFactorType.Dynamic
            && C1 + C2 <= 4.0)
        {
            throw new InvalidOperationException(
                "Для коэффициента сжатия требуется c1 + c2 > 4.");
        }
    }
}
