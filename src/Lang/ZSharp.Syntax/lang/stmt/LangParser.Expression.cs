using ZSharp.Parser;
using ZSharp.AST;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public static partial class LangParser
    {
        public static ExpressionStatement ParseExpressionStatement(Parser parser)
        {
            var expression = parser.Parse<Expression>();

            parser.Eat(Text.TokenType.Semicolon);

            return new()
            {
                Expression = expression,
            };
        }
    }
}
