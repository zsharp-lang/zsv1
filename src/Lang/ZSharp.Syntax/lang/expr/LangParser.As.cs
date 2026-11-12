using ZSharp.Parser;
using ZSharp.AST;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public static partial class LangParser
    {
        public static CastExpression ParseCastExpression(Parser parser, Expression expression)
        {
            var asKeyword = parser.Eat(Keywords.As);

            var targetType = parser.Parse<Expression>();

            return new()
            {
                Expression = expression,
                TargetType = targetType,
            };
        }
    }
}
