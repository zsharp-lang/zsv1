using ZSharp.Parser;
using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public static partial class LangParser
    {
        public static IsExpression ParseIsExpression(Parser parser, Expression expression)
        {
            parser.Eat(Keywords.Is);

            return new()
            {
                Expression = expression,
                Pattern = ParsePattern(parser)
            };
        }

        /// <summary>
        /// A pattern: a name, which always says <c>let</c> or <c>var</c>, with the type it
        /// matches if it has one, or else an expression, as a value pattern.
        /// </summary>
        public static Pattern ParsePattern(Parser parser)
        {
            if (parser.Is(Keywords.Let) || parser.Is(Keywords.Var))
            {
                var isMutable = parser.Is(Keywords.Var);
                parser.Eat(isMutable ? Keywords.Var : Keywords.Let);

                var name = parser.Eat(TokenType.Identifier).Value;

                Expression? type = null;
                if (parser.Is(TokenType.Colon, eat: true))
                    type = ParseType(parser);

                return new NamePattern { IsMutable = isMutable, Name = name, Type = type };
            }

            return new ValuePattern { Value = parser.Parse<Expression>() };
        }
    }
}
