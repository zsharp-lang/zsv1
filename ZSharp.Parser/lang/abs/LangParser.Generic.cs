using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static GenericParameter ParseGenericParameter(Parser parser)
        {
            var name = parser.Eat(TokenType.Identifier).Value;

            Expression? constraint = null;
            if (parser.Is(TokenType.Colon))
            {
                parser.Eat(TokenType.Colon);
                constraint = parser.Parse<Expression>();
            }

            return new GenericParameter
            {
                Name = name,
                Constraint = constraint
            };
        }
    }
}
