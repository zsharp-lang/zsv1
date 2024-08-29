using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static LetExpression ParseLetExpression(Parser parser)
        {
            parser.Eat(Keywords.Let);

            var name = parser.Eat(TokenType.Identifier).Value;

            Expression? type = null;
            if (parser.Is(TokenType.Colon, eat: true))
                type = parser.Parse<Expression>();

            parser.Eat(Symbols.Assign);

            var value = parser.Parse<Expression>();

            return new()
            {
                Name = name,
                Type = type,
                Value = value
            };
        }
    }
}
