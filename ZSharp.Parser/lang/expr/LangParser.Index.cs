using ZSharp.AST;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static CallArgument ParseIndexArgument(Parser parser)
            => ParseCallArgument(parser);

        public static IndexExpression ParseIndexExpression(Parser parser, Expression target)
        {
            var lBracker = parser.Eat(Text.TokenType.LBracket);

            List<CallArgument> arguments = [];

            if (!parser.Is(Text.TokenType.RBracket))
            {
                arguments.Add(ParseCallArgument(parser));

                while (parser.Is(Text.TokenType.Comma, eat: true))
                    arguments.Add(ParseCallArgument(parser));
            }

            var rBracket = parser.Eat(Text.TokenType.RBracket);

            return new()
            {
                Arguments = arguments,
                Target = target,
            };
        }
    }
}
