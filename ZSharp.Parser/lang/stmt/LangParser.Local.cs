using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static (Token, Expression?, Expression) ParseLetWithoutKeyword(Parser parser)
        {
            var name = parser.Eat(TokenType.Identifier);

            Expression? type = null;
            if (parser.Is(TokenType.Colon, eat: true))
                type = parser.Parse<Expression>();

            parser.Eat(Symbols.Assign);

            var value = parser.Parse<Expression>();

            return (name, type, value);
        }

        public static LetStatement ParseLetStatement(Parser parser)
        {
            parser.Eat(Keywords.Let);

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
                    new()
                    {
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
}
