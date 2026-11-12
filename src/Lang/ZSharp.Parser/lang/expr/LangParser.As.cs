using ZSharp.AST;

namespace ZSharp.Parser
{
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
