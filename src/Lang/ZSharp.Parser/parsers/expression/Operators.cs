using ZSharp.AST;

namespace ZSharp.Parser
{
    /// <summary>
    /// How an expression parser reads an operator: before its operand, or between two,
    /// binding to the left or to the right.
    /// </summary>
    public static class Operators
    {
        /// <summary>
        /// An operator written before its operand, which binds it as tightly as the power
        /// given: what follows it, up to an operator that binds less tightly.
        /// </summary>
        public static NudFunction Prefix(string @operator, int bindingPower)
            => parser =>
            {
                parser.Eat(@operator);

                var operand =
                (parser.GetParserFor<Expression>() as ExpressionParser ?? throw new ParseError())
                .ParseExpression(parser, bindingPower);

                return new UnaryExpression()
                {
                    Operator = @operator,
                    Operand = operand
                };
            };

        public static LedFunction InfixL(string @operator, int bindingPower)
            => (parser, left) =>
            {
                parser.Eat(@operator);

                var right =
                (parser.GetParserFor<Expression>() as ExpressionParser ?? throw new ParseError())
                .ParseExpression(parser, bindingPower);

                return new BinaryExpression()
                {
                    Left = left,
                    Right = right,
                    Operator = @operator
                };
            };

        public static LedFunction InfixR(string @operator, int bindingPower)
            => InfixL(@operator, bindingPower - 1);
    }
}
