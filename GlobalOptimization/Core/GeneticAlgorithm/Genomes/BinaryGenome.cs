using GlobalOptimization.Core.GeneticAlgorithm.Abstractions;

namespace GlobalOptimization.Core.GeneticAlgorithm.Genomes;

public sealed class BinaryGenome : IGenome
{
    private readonly bool[] _bits;

    public int Length => _bits.Length;

    public BinaryGenome(int length)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        _bits = new bool[length];
    }

    public BinaryGenome(bool[] bits, bool clone = true)
    {
        if (bits is null)
            throw new ArgumentNullException(nameof(bits));
        if (bits.Length == 0)
            throw new ArgumentException("Бинарный геном не может быть пустым.", nameof(bits));

        _bits = clone ? (bool[])bits.Clone() : bits;
    }

    public bool this[int index]
    {
        get
        {
            ValidateIndex(index);
            return _bits[index];
        }
        set
        {
            ValidateIndex(index);
            _bits[index] = value;
        }
    }

    public bool GetBit(int index)
    {
        ValidateIndex(index);
        return _bits[index];
    }

    public void SetBit(int index, bool value)
    {
        ValidateIndex(index);
        _bits[index] = value;
    }

    public void FlipBit(int index)
    {
        ValidateIndex(index);
        _bits[index] = !_bits[index];
    }

    public BinaryGenome Clone()
    {
        return new BinaryGenome(_bits, clone: true);
    }

    public bool[] ToArray()
    {
        return (bool[])_bits.Clone();
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= _bits.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
    }

    public override string ToString()
    {
        return string.Concat(_bits.Select(bit => bit ? '1' : '0'));
    }
}
