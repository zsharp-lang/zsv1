using ZSharp.Parser;
using ZSharp.AST;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public static partial class LangParser
    {
        public static ArrayLiteral ParseArrayLiteral(Parser parser)
        {
            var lBracket = parser.Eat(Text.TokenType.LBracket);

            List<Expression> items = [];
            while (!parser.Is(Text.TokenType.RBracket))
            {
                items.Add(parser.Parse<Expression>());

                if (!parser.Is(Text.TokenType.Comma, eat: true))
                    break;
            }

            var rBracket = parser.Eat(Text.TokenType.RBracket);

            return new()
            {
                Items = items
            };
        }
    }
}
