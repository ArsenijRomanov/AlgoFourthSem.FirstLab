namespace Analysis;

public static class CaseIds
{
    public const string GaBinaryOnePoint = "ga_binary_one_point";
    public const string GaBinaryUniform = "ga_binary_uniform";
    public const string GaRealArithmetic = "ga_real_arithmetic";
    public const string GaRealSbx = "ga_real_sbx";
    public const string PsoUnitChi = "pso_unit_chi";
    public const string PsoCanonicalChi = "pso_canonical_chi";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        GaBinaryOnePoint,
        GaBinaryUniform,
        GaRealArithmetic,
        GaRealSbx,
        PsoUnitChi,
        PsoCanonicalChi
    };
}