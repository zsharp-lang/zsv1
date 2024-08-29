using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static Function ParseFunctionExpression(Parser parser)
        {
            var funKeyword = parser.Eat(Keywords.Function);

            string? name = null;
            if (parser.Is(TokenType.Identifier))
                name = parser.Eat(TokenType.Identifier).Value;

            parser.Eat(TokenType.LParen);

            var signature = ParseSignature(parser);

            parser.Eat(TokenType.RParen);

            // TODO: add support for modifiers
            //var modifiers = ParseModifiers(parser); 

            Expression? returnType = null;
            if (parser.Is(TokenType.Colon, eat: true))
                returnType = parser.Parse<Expression>();

            var body = ParseBlockStatement(parser);

            return new(new()
            {
                FunKeyword = funKeyword,
            })
            {
                Body = body,
                Name = name,
                ReturnType = returnType,
                Signature = signature,
            };
        }

        public static ExpressionStatement ParseFunctionStatement(Parser parser)
            => new()
            {
                Expression = ParseFunctionExpression(parser),
            };
    }
}
