using ZSharp.AST;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static CallArgument ParseCallArgument(Parser parser)
        {
            string? name = null;

            if (parser.Is(Text.TokenType.Identifier))
                using (var lookAhead = parser.LookAhead())
                {
                    name = parser.Eat(Text.TokenType.Identifier).Value;
                    if (parser.Is(Text.TokenType.Colon, eat: true)) lookAhead.Commit();
                    else name = null;
                }

            var expression = parser.Parse<Expression>();

            return new()
            {
                Name = name,
                Value = expression
            };
        }

        public static CallExpression ParseCallExpression(Parser parser, Expression callee)
        {
            var lParen = parser.Eat(Text.TokenType.LParen);

            List<CallArgument> arguments = [];

            while (!parser.Is(Text.TokenType.RParen))
            {
                arguments.Add(ParseCallArgument(parser));

                while (!parser.Is(Text.TokenType.RParen) && parser.Is(Text.TokenType.Comma, eat: true))
                    arguments.Add(ParseCallArgument(parser));
            }

            var rParen = parser.Eat(Text.TokenType.RParen);

            return new(new()
            {
                LParen = lParen,
                RParen = rParen
            })
            {
                Arguments = arguments,
                Callee = callee,
            };
        }
    }
}
