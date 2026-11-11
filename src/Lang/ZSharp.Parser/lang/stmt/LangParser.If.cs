using ZSharp.AST;
#if !ZSHARP_MINIMAL_PARENTHESIS
using ZSharp.Text;
#endif

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static IfStatement ParseIfStatement(Parser parser)
        {
            parser.Eat(Keywords.If);

#if !ZSHARP_MINIMAL_PARENTHESIS
			parser.Eat(TokenType.LParen);
#endif

            var condition = parser.Parse<Expression>();

#if !ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(TokenType.RParen);
#endif

#if ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(Symbols.ThenDo);
#endif

			var @if = parser.Parse<Statement>();

            Statement? @else = null;

            if (parser.Is(Keywords.Else, eat: true))
				@else = parser.Parse<Statement>();

            return new()
            {
                Condition = condition,
                If = @if,
                Else = @else,
            };
		}
	}
}
