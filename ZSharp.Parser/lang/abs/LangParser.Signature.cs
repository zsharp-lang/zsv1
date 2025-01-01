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

            if (parser.Is(TokenType.Identifier))
            {
                args = [ParseParameter(parser)];

                while (parser.Is(TokenType.Comma, eat: true) && parser.Is(TokenType.Identifier))
					args.Add(ParseParameter(parser));
			}

            if (parser.Is(Symbols.VarPack, eat: true))
            {
				varArgs = ParseParameter(parser);

                parser.Is(TokenType.Comma, eat: true);
			}

            if (parser.Is(TokenType.LCurly, eat: true))
            {
                if (parser.Is(TokenType.Identifier))
				{
					kwArgs = [ParseParameter(parser)];

					while (parser.Is(TokenType.Comma, eat: true) && parser.Is(TokenType.Identifier))
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

        public static Parameter ParseParameter(Parser parser)
        {
			var name = parser.Eat(TokenType.Identifier).Value;

            string? alias = null;
            if (parser.Is(Keywords.As, eat: true))
                alias = parser.Eat(TokenType.Identifier).Value;

			Expression? type = null;
            if (parser.Is(TokenType.Colon, eat: true))
				type = ParseType(parser);

            Expression? initializer = null;
			if (parser.Is(Symbols.Assign, eat: true))
				initializer = parser.Parse<Expression>();

            return new()
            {
                Alias = alias,
                Name = name,
                Type = type,
				Initializer = initializer,
			};
		}
    }
}
