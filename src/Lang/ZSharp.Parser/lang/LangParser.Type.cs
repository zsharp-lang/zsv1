using ZSharp.AST;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public const int TypeExpressionMinimumBindingPower = 20;

        public static Expression ParseType(Parser parser)
            => ParseType(parser, TypeExpressionMinimumBindingPower);

        public static Expression ParseType(Parser parser, int bindingPower)
            => (parser.GetParserFor<Expression>() as ExpressionParser ?? throw new()).ParseExpression(parser, bindingPower);
    }
}
