using ZSharp.AST;
#if !ZSHARP_MINIMAL_PARENTHESIS
using ZSharp.Text;
#endif

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static ForStatement ParseForStatement(Parser parser)
        {
            parser.Eat(Keywords.For);

#if !ZSHARP_MINIMAL_PARENTHESIS
			parser.Eat(TokenType.LParen);
#endif

            var currentItem = parser.Parse<Expression>();

            var inKeyword = parser.Eat(Keywords.In);

            var iterable = parser.Parse<Expression>();

#if !ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(TokenType.RParen);
#endif

#if ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(Symbols.ThenDo);
#endif

			var @for = parser.Parse<Statement>();

            Statement? @else = null;

            if (parser.Is(Keywords.Else, eat: true))
				@else = parser.Parse<Statement>();

            return new()
            {
                CurrentItem = currentItem,
                Iterable = iterable,
                Body = @for,
                Else = @else,
            };
		}
	}
}
