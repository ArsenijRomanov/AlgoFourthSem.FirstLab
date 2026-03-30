using GlobalOptimization.Core.GeneticAlgorithm.Configs;
using GlobalOptimization.Core.ParticleSwarmOptimization;

namespace GlobalOptimization.Avalonia.Models;

public sealed record OptimizationSessionSignature(
    string FunctionExpression,
    double XMin,
    double XMax,
    double YMin,
    double YMax,
    OptimizationAlgorithmKind AlgorithmKind,
    int PopulationSize,
    GenomeEncodingKind GenomeEncoding,
    BinaryCrossoverType BinaryCrossoverType,
    RealCrossoverType RealCrossoverType,
    int BitsPerCoordinate,
    int SwarmSize,
    ConstrictionFactorType ConstrictionFactorType)
{
    public static OptimizationSessionSignature FromRequest(OptimizationRunRequest request)
        => new OptimizationSessionSignature(
            request.FunctionExpression,
            request.XMin,
            request.XMax,
            request.YMin,
            request.YMax,
            request.AlgorithmKind,
            request.PopulationSize,
            request.GenomeEncoding,
            request.BinaryCrossoverType,
            request.RealCrossoverType,
            request.BitsPerCoordinate,
            request.SwarmSize,
            request.ConstrictionFactorType);
}
