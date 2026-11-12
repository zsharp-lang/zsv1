using ZSharp.AST;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static IsOfExpression ParseIsOfExpression(Parser parser, Expression expression)
        {
            var isKeyword = parser.Eat(Keywords.Is);

            string? name = null;
            if (!parser.Is(Keywords.Of))
                name = parser.Eat(Text.TokenType.Identifier).Value;

            var ofKeyword = parser.Eat(Keywords.Of);

            var ofType = parser.Parse<Expression>();

            return new()
            {
                Expression = expression,
                Name = name,
                OfType = ofType
            };
        }
    }
}
