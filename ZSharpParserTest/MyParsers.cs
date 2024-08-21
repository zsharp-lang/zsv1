using ZSharp.AST;
using ZSharp.Parser;
using ZSharp.Text;

internal static class MyParsers
{
    public static LetExpression ParseLetExpression(Parser parser)
    {
        if (!parser.Eat("let", out var _)) throw new Exception("parse error: expected keyword 'let'");

        var name = parser.Eat(TokenType.Identifier);

        Expression? type = null;
        if (parser.Is(TokenType.Colon, eat: true))
            type = parser.Parse<Expression>();

        parser.Eat("=");

        var value = parser.Parse<Expression>();

        return new(name.Value)
        {
            Value = value,
            Type = type,
        };
    }

    public static ParserFunction<Statement> ExpressionStatement(Func<Parser, Expression> fn)
        => parser => 
        {
            var expression = fn(parser);
            parser.Eat(TokenType.Semicolon);
            return new ExpressionStatement(expression);
        };
}