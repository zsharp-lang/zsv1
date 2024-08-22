using ZSharp.AST;
using ZSharp.Parser;
using ZSharp.Text;


internal static partial class MyParsers
{
    public static (Token, Expression?, Expression) ParseLetWithoutKeyword(Parser parser)
    {
        var name = parser.Eat(TokenType.Identifier);

        Expression? type = null;
        if (parser.Is(TokenType.Colon, eat: true))
            type = parser.Parse<Expression>();

        parser.Eat("=");

        var value = parser.Parse<Expression>();

        return (name, type, value);
    }

    public static LetExpression ParseLetExpression(Parser parser)
    {
        if (!parser.Eat("let", out var _)) throw new Exception("parse error: expected keyword 'let'");

        var (name, type, value) = ParseLetWithoutKeyword(parser);

        return new(name.Value)
        {
            Value = value,
            Type = type,
        };
    }

    public static LetStatement ParseLetStatement(Parser parser)
    {
        if (!parser.Eat("let", out var _)) throw new ParseError();

        var (name, type, value) = ParseLetWithoutKeyword(parser);

        List<LetStatementDefinition> definitions = [
            new() {
                Name = name.Value,
                Type = type,
                Value = value
            }
        ];

        while (parser.Is(TokenType.Comma, eat: true))
        {
            (name, type, value) = ParseLetWithoutKeyword(parser);

            definitions.Add(
                new() {
                    Name = name.Value,
                    Type = type,
                    Value = value
                }
            );
        }

        parser.Eat(TokenType.Semicolon);

        return new()
        {
            Definitions = definitions
        };
    }
}