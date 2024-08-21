using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class ExpressionParser
        : Parser<Expression>
    {
        public override Expression Parse(Parser parser)
        {
            if (parser.Is(Text.TokenType.String, out var token, eat: true))
                return new Literal(token.Value, LiteralType.String);

            throw new NotImplementedException();
        }
    }
}
