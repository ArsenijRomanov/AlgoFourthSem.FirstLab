namespace GlobalOptimization.Avalonia.Services;

public sealed class ExpressionParseException : Exception
{
    public ExpressionParseException(string message) : base(message)
    {
    }
}
