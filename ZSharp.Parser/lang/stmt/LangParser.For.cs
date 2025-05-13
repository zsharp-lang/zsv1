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

            var currentItem = ParseForValue(parser);

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
                Value = currentItem,
                Source = iterable,
                Body = @for,
                Else = @else,
            };
		}

        private static ForValue ParseForValue(Parser parser)
        {
            if (parser.Is(Keywords.Let))
                return ParseLetForValue(parser);

            throw new ParseError($"Expected 'let', got {parser.Token}");
        }

        private static LetForValue ParseLetForValue(Parser parser)
        {
            parser.Eat(Keywords.Let);

            var name = parser.Eat(TokenType.Identifier).Value;

            Expression? type = null;
            if (parser.Is(TokenType.Colon, eat: true))
                type = parser.Parse<Expression>();

            return new()
            {
                Name = name,
                Type = type
            };
        }
	}
}
