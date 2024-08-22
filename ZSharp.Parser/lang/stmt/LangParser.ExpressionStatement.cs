using ZSharp.AST;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static ExpressionStatement ParseExpressionStatement(Parser parser)
        {
            var expression = parser.Parse<Expression>();

            return new(new()
                {
                    Semicolon = parser.Eat(Text.TokenType.Semicolon)
                })
            {
                Expression = expression
            };
        }
    }
}
