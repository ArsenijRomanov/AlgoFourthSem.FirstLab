using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

namespace GlobalOptimization.Core.GeneticAlgorithm.Genomes;

public sealed class RealGenome : IGenome
{
    private readonly double[] _genes;

    public int Length => _genes.Length;

    public RealGenome(int length)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        _genes = new double[length];
    }

    public RealGenome(double[] genes, bool clone = true)
    {
        if (genes is null)
            throw new ArgumentNullException(nameof(genes));
        if (genes.Length == 0)
            throw new ArgumentException("Вещественный геном не может быть пустым.", nameof(genes));

        _genes = clone ? (double[])genes.Clone() : genes;
    }

    public double this[int index]
    {
        get
        {
            ValidateIndex(index);
            return _genes[index];
        }
        set
        {
            ValidateIndex(index);
            _genes[index] = value;
        }
    }

    public double GetGene(int index)
    {
        ValidateIndex(index);
        return _genes[index];
    }

    public void SetGene(int index, double value)
    {
        ValidateIndex(index);
        _genes[index] = value;
    }

    public RealGenome Clone()
    {
        return new RealGenome(_genes, clone: true);
    }

    public double[] ToArray()
    {
        return (double[])_genes.Clone();
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= _genes.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", _genes)}]";
    }
}
