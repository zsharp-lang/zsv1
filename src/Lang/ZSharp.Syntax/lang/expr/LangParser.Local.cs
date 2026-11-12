using ZSharp.Parser;
using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public static partial class LangParser
    {
        public static LetExpression ParseLetExpression(Parser parser)
        {
            parser.Eat(Keywords.Let);

            var name = parser.Eat(TokenType.Identifier).Value;

            Expression? type = null;
            if (parser.Is(TokenType.Colon, eat: true))
                type = ParseType(parser);

            Expression? value = null;
            if (type is null)
            {
                parser.Eat(Symbols.Assign);
                value = parser.Parse<Expression>();
            }
            else if (parser.Is(Symbols.Assign, eat: true))
                value = parser.Parse<Expression>();

            return new()
            {
                Name = name,
                Type = type,
                Value = value
            };
        }

        public static VarExpression ParseVarExpression(Parser parser)
        {
            parser.Eat(Keywords.Var);

            var name = parser.Eat(TokenType.Identifier).Value;

            Expression? type = null;
            if (parser.Is(TokenType.Colon, eat: true))
                type = ParseType(parser);

            Expression? value = null;
            if (parser.Is(Symbols.Assign, eat: true))
                value = parser.Parse<Expression>();

            return new()
            {
                Name = name,
                Type = type,
                Value = value
            };
        }
    }
}
