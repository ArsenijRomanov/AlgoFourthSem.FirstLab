using System.Globalization;
using System.Linq.Expressions;

namespace GlobalOptimization.Avalonia.Services;

public static class ExpressionCompiler
{
    public static Func<double, double, double> Compile(string expressionText)
    {
        if (string.IsNullOrWhiteSpace(expressionText))
            throw new ExpressionParseException("Строка функции пуста.");

        var tokens = Tokenize(expressionText);
        var rpn = ToReversePolishNotation(tokens);
        return BuildDelegate(rpn);
    }

    private static IReadOnlyList<Token> Tokenize(string text)
    {
        var tokens = new List<Token>();
        int i = 0;

        while (i < text.Length)
        {
            char c = text[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                int start = i;
                bool dotSeen = c == '.';
                i++;

                while (i < text.Length)
                {
                    if (char.IsDigit(text[i]))
                    {
                        i++;
                        continue;
                    }

                    if (text[i] == '.')
                    {
                        if (dotSeen)
                            throw new ExpressionParseException($"Некорректное число возле позиции {i + 1}.");

                        dotSeen = true;
                        i++;
                        continue;
                    }

                    break;
                }

                string raw = text[start..i];
                if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                    throw new ExpressionParseException($"Не удалось распознать число '{raw}'.");

                tokens.Add(Token.NumberToken(number));
                continue;
            }

            if (char.IsLetter(c))
            {
                int start = i;
                i++;

                while (i < text.Length && char.IsLetter(text[i]))
                    i++;

                string identifier = text[start..i].ToLowerInvariant();

                tokens.Add(Token.Identifier(identifier));
                continue;
            }

            tokens.Add(c switch
            {
                '+' => Token.Operator("+"),
                '-' => Token.Operator("-"),
                '*' => Token.Operator("*"),
                '/' => Token.Operator("/"),
                '^' => Token.Operator("^"),
                '(' => Token.LeftParenthesis(),
                ')' => Token.RightParenthesis(),
                _ => throw new ExpressionParseException(
                    $"Недопустимый символ '{c}' возле позиции {i + 1}.")
            });

            i++;
        }

        return tokens;
    }

    private static IReadOnlyList<Token> ToReversePolishNotation(IReadOnlyList<Token> tokens)
    {
        var output = new List<Token>();
        var operators = new Stack<Token>();

        Token? previous = null;

        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case TokenKind.Number:
                    output.Add(token);
                    break;

                case TokenKind.Identifier:
                    if (IsVariable(token.Text!) || IsConstant(token.Text!))
                    {
                        output.Add(token);
                    }
                    else if (IsFunction(token.Text!))
                    {
                        operators.Push(token);
                    }
                    else
                    {
                        throw new ExpressionParseException($"Неизвестный идентификатор '{token.Text}'.");
                    }

                    break;

                case TokenKind.Operator:
                    var opToken = token;

                    if (token.Text == "-" && IsUnaryContext(previous))
                        opToken = Token.Operator("neg");

                    while (operators.Count > 0 &&
                           operators.Peek().Kind is TokenKind.Operator or TokenKind.Identifier &&
                           ShouldPopBeforePushing(operators.Peek(), opToken))
                    {
                        output.Add(operators.Pop());
                    }

                    operators.Push(opToken);
                    break;

                case TokenKind.LeftParenthesis:
                    operators.Push(token);
                    break;

                case TokenKind.RightParenthesis:
                    bool foundLeftParenthesis = false;

                    while (operators.Count > 0)
                    {
                        var top = operators.Pop();

                        if (top.Kind == TokenKind.LeftParenthesis)
                        {
                            foundLeftParenthesis = true;
                            break;
                        }

                        output.Add(top);
                    }

                    if (!foundLeftParenthesis)
                        throw new ExpressionParseException("Обнаружена лишняя закрывающая скобка.");

                    if (operators.Count > 0 &&
                        operators.Peek().Kind == TokenKind.Identifier &&
                        IsFunction(operators.Peek().Text!))
                    {
                        output.Add(operators.Pop());
                    }

                    break;

                default:
                    throw new ExpressionParseException("Неизвестный токен.");
            }

            previous = token;
        }

        while (operators.Count > 0)
        {
            var token = operators.Pop();

            if (token.Kind is TokenKind.LeftParenthesis or TokenKind.RightParenthesis)
                throw new ExpressionParseException("Скобки не сбалансированы.");

            output.Add(token);
        }

        return output;
    }

    private static Func<double, double, double> BuildDelegate(IReadOnlyList<Token> rpn)
    {
        var xParameter = Expression.Parameter(typeof(double), "x");
        var yParameter = Expression.Parameter(typeof(double), "y");
        var stack = new Stack<Expression>();

        foreach (var token in rpn)
        {
            switch (token.Kind)
            {
                case TokenKind.Number:
                    stack.Push(Expression.Constant(token.NumericValue, typeof(double)));
                    break;

                case TokenKind.Identifier:
                    if (IsFunction(token.Text!))
                        stack.Push(BuildOperatorExpression(token.Text!, stack));
                    else
                        stack.Push(BuildIdentifierExpression(token.Text!, xParameter, yParameter));
                    break;

                case TokenKind.Operator:
                    stack.Push(BuildOperatorExpression(token.Text!, stack));
                    break;

                default:
                    throw new ExpressionParseException("Некорректная запись выражения.");
            }
        }

        if (stack.Count != 1)
            throw new ExpressionParseException("Не удалось собрать корректное выражение.");

        var lambda = Expression.Lambda<Func<double, double, double>>(stack.Pop(), xParameter, yParameter);
        return lambda.Compile();
    }

    private static Expression BuildIdentifierExpression(
        string identifier,
        ParameterExpression xParameter,
        ParameterExpression yParameter)
    {
        return identifier switch
        {
            "x" => xParameter,
            "y" => yParameter,
            "pi" => Expression.Constant(Math.PI),
            "e" => Expression.Constant(Math.E),
            "sin" or "cos" or "exp" or "sqrt" =>
                throw new ExpressionParseException($"Функция '{identifier}' должна иметь аргумент в скобках."),
            _ => throw new ExpressionParseException($"Неизвестный идентификатор '{identifier}'.")
        };
    }

    private static Expression BuildOperatorExpression(string op, Stack<Expression> stack)
    {
        if (op == "neg")
        {
            EnsureStackCount(stack, 1, "унарного минуса");
            return Expression.Negate(stack.Pop());
        }

        if (IsFunction(op))
        {
            EnsureStackCount(stack, 1, $"функции '{op}'");
            var argument = stack.Pop();

            return op switch
            {
                "sin" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Sin), new[] { typeof(double) })!, argument),
                "cos" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Cos), new[] { typeof(double) })!, argument),
                "exp" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Exp), new[] { typeof(double) })!, argument),
                "sqrt" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Sqrt), new[] { typeof(double) })!, argument),
                _ => throw new ExpressionParseException($"Неизвестная функция '{op}'.")
            };
        }

        EnsureStackCount(stack, 2, $"оператора '{op}'");

        var right = stack.Pop();
        var left = stack.Pop();

        return op switch
        {
            "+" => Expression.Add(left, right),
            "-" => Expression.Subtract(left, right),
            "*" => Expression.Multiply(left, right),
            "/" => Expression.Divide(left, right),
            "^" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) })!, left, right),
            _ => throw new ExpressionParseException($"Неизвестный оператор '{op}'.")
        };
    }

    private static bool ShouldPopBeforePushing(Token stackTop, Token current)
    {
        if (stackTop.Kind == TokenKind.Identifier && IsFunction(stackTop.Text!))
            return true;

        if (stackTop.Kind != TokenKind.Operator)
            return false;

        int left = GetPrecedence(stackTop.Text!);
        int right = GetPrecedence(current.Text!);

        if (IsRightAssociative(current.Text!))
            return left > right;

        return left >= right;
    }

    private static int GetPrecedence(string op) => op switch
    {
        "+" or "-" => 1,
        "*" or "/" => 2,
        "neg" => 3,
        "^" => 4,
        _ when IsFunction(op) => 5,
        _ => throw new ExpressionParseException($"Неизвестный оператор '{op}'.")
    };

    private static bool IsRightAssociative(string op)
        => op is "^" or "neg";

    private static bool IsUnaryContext(Token? previous)
    {
        return previous is null ||
               previous.Kind == TokenKind.Operator ||
               previous.Kind == TokenKind.LeftParenthesis;
    }

    private static void EnsureStackCount(Stack<Expression> stack, int expected, string target)
    {
        if (stack.Count < expected)
            throw new ExpressionParseException($"Недостаточно аргументов для {target}.");
    }

    private static bool IsVariable(string identifier)
        => identifier is "x" or "y";

    private static bool IsConstant(string identifier)
        => identifier is "pi" or "e";

    private static bool IsFunction(string identifier)
        => identifier is "sin" or "cos" or "exp" or "sqrt";

    private enum TokenKind
    {
        Number,
        Identifier,
        Operator,
        LeftParenthesis,
        RightParenthesis
    }

    private sealed record Token(
        TokenKind Kind,
        string? Text = null,
        double NumericValue = 0.0)
    {
        public static Token NumberToken(double value) => new Token(TokenKind.Number, NumericValue: value);
        public static Token Identifier(string name) => new Token(TokenKind.Identifier, Text: name);
        public static Token Operator(string op) => new Token(TokenKind.Operator, Text: op);
        public static Token LeftParenthesis() => new Token(TokenKind.LeftParenthesis);
        public static Token RightParenthesis() => new Token(TokenKind.RightParenthesis);
    }
}
