using GlobalOptimization.Core.GeneticAlgorithm.Configs;
using GlobalOptimization.Core.ParticleSwarmOptimization;

namespace GlobalOptimization.Avalonia.Models;

public sealed record OptimizationRunRequest(
    string FunctionExpression,
    double XMin,
    double XMax,
    double YMin,
    double YMax,
    int IterationsToRun,
    OptimizationAlgorithmKind AlgorithmKind,
    int PopulationSize,
    GenomeEncodingKind GenomeEncoding,
    BinaryCrossoverType BinaryCrossoverType,
    RealCrossoverType RealCrossoverType,
    int BitsPerCoordinate,
    int SwarmSize,
    ConstrictionFactorType ConstrictionFactorType);
