using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static Signature ParseSignature(Parser parser)
        {
            List<Parameter>? args = null;
            Parameter? varArgs = null;
            List<Parameter>? kwArgs = null;
            Parameter? varKwArgs = null;

            if (parser.Is(TokenType.RParen))
                return new Signature
                {
                    Args = args,
                    VarArgs = varArgs,
                    KwArgs = kwArgs,
                    VarKwArgs = varKwArgs,
                };

            if (StartsParameter(parser))
            {
                args = [ParseParameter(parser)];

                while (parser.Is(TokenType.Comma, eat: true) && StartsParameter(parser))
					args.Add(ParseParameter(parser));
			}

            if (parser.Is(Symbols.VarPack, eat: true))
            {
				varArgs = ParseParameter(parser);

                parser.Is(TokenType.Comma, eat: true);
			}

            if (parser.Is(TokenType.LCurly, eat: true))
            {
                if (StartsParameter(parser))
				{
					kwArgs = [ParseParameter(parser)];

					while (parser.Is(TokenType.Comma, eat: true) && StartsParameter(parser))
						kwArgs.Add(ParseParameter(parser));
				}

                if (parser.Is(Symbols.VarPack, eat: true))
                    varKwArgs = ParseParameter(parser);

                parser.Eat(TokenType.RCurly);
			}

            return new()
            {
                Args = args,
                VarArgs = varArgs,
                KwArgs = kwArgs,
                VarKwArgs = varKwArgs,
            };
		}

        private static bool StartsParameter(Parser parser)
            => parser.Is(TokenType.Identifier) || parser.Is(Symbols.Decorator);

        public static Parameter ParseParameter(Parser parser)
        {
            var decorators = new List<Expression>();

            while (parser.Is(Symbols.Decorator, eat: true))
                decorators.Add((parser.GetParserFor<Expression>() as ExpressionParser ?? throw new ParseError())
                    .ParseExpression(parser, DecoratorBindingPower));

			var name = parser.Eat(TokenType.Identifier);

            Token? alias = null;
            if (parser.Is(Keywords.As, eat: true))
                alias = parser.Eat(TokenType.Identifier);

			Expression? type = null;
            if (parser.Is(TokenType.Colon, out var typeSeparator, eat: true))
				type = ParseType(parser);

            Expression? initializer = null;
			if (parser.Is(Symbols.Assign, out var valueSeparator, eat: true))
				initializer = parser.Parse<Expression>();

            return new(new()
            {
                AsKeyword = alias,
                Name = name,
                TypeSeparator = typeSeparator,
                ValueSeparator = valueSeparator,
            })
            {
                Alias = alias?.Value,
                Name = name.Value,
                Type = type,
				Initializer = initializer,
                Decorators = decorators,
			};
		}
    }
}
