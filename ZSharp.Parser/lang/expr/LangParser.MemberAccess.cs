using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static BinaryExpression ParseMemberAccess(Parser parser, Expression target)
        {
            var @operator = parser.Eat(Symbols.MemberAccess).Value;

            if (parser.Is(TokenType.Identifier, out var name, eat: true))
                return new()
                {
                    Left = target,
                    Operator = @operator,
                    Right = new IdentifierExpression(new(name))//LiteralExpression.String(name.Value),
                };
            else if (parser.Is(TokenType.Number, out var index))
                throw new NotImplementedException("Member access by index");

            throw new ParseError();
        }
    }
}
