using ZSharp.Parser;
using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

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
