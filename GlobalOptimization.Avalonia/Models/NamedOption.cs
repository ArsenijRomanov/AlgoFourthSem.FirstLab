namespace GlobalOptimization.Avalonia.Models;

public sealed record NamedOption<T>(string Name, T Value)
{
    public override string ToString() => Name;
}
