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
            if (!parser.Is(TokenType.Semicolon) && !parser.Is(Keywords.From))
                value = parser.Parse<Expression>();

            var from = ParseLoopName(parser);

            parser.Eat(TokenType.Semicolon);

            return new()
            {
                Value = value,
                From = from
            };
        }

        public static ContinueStatement ParseContinueStatement(Parser parser)
        {
            parser.Eat(Keywords.Continue);

            var from = ParseLoopName(parser);

            parser.Eat(TokenType.Semicolon);

            return new()
            {
                From = from
            };
        }

        /// <summary>
        /// The loop named by <c>from name</c>, if there is one.
        /// </summary>
        private static string? ParseLoopName(Parser parser)
            => parser.Is(Keywords.From, eat: true) ? parser.Eat(TokenType.Identifier).Value : null;
	}
}
