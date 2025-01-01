using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static BreakStatement ParseBreakStatement(Parser parser)
        {
            parser.Eat(Keywords.Break);

            Expression? value = null;
            if (!parser.Is(TokenType.Semicolon))
                value = parser.Parse<Expression>();

            parser.Eat(TokenType.Semicolon);

            return new()
            {
                Value = value
            };
        }
	}
}
