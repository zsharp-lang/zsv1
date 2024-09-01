using ZSharp.AST;

namespace ZSharp.Parser
{

    public sealed partial class ExpressionParser
        : ContextParser<Expression, Expression>
    {
        public override Expression Parse(Parser parser)
        {
            return ParseContextItem(parser);
        }

        protected override Expression ParseDefaultContextItem(Parser parser)
        {
            return ParseExpression(parser);
        }

        public Expression ParseExpression(Parser parser, int bindingPower = 0)
        {
            var subParser = GetSubParserFor(parser.Token) ?? throw new ParseError();

            var left = (subParser.Nud ?? throw new ParseError()).Invoke(parser);

            if (subParser.BindingPower == -1) return left;

            while ((subParser = GetSubParserFor(parser.Token)) is not null && bindingPower < subParser.BindingPower)
                left = (subParser.Led ?? throw new ParseError()).Invoke(parser, left);

            return left;
        }
     }
}
