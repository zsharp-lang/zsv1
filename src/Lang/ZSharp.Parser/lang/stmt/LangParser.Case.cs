using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static CaseStatement ParseCaseStatement(Parser parser)
        {
            parser.Eat(Keywords.Case);

            Expression? value = null;
#if !ZSHARP_MINIMAL_PARENTHESIS
            if (parser.Is(TokenType.LParen, eat: true))
            {
                value = parser.Parse<Expression>();
                parser.Eat(TokenType.RParen);
            }
#else
            if (!parser.Is(Keywords.Of) && !parser.Is(TokenType.LCurly))
                value = parser.Parse<Expression>();
#endif

            Expression? of = null;
            if (parser.Is(Keywords.Of, eat: true))
            {
#if !ZSHARP_MINIMAL_PARENTHESIS
                parser.Eat(TokenType.LParen);
#endif

                if (parser.Is(TokenType.Operator))
                    of = new OperatorExpression
                    {
                        Operator = parser.Eat(TokenType.Operator).Value
                    };
                else
                    of = parser.Parse<Expression>();

#if !ZSHARP_MINIMAL_PARENTHESIS
                parser.Eat(TokenType.RParen);
#endif
            }

            parser.Eat(TokenType.LCurly);

            var whenClauses = new List<WhenClauseStatement>();

            while (!parser.Is(TokenType.RCurly, eat: true))
                whenClauses.Add(ParseWhenClauseStatement(parser));

            Statement? @else = null;
            if (parser.Is(Keywords.Else, eat: true))
                @else = parser.Parse<Statement>();

            return new()
            {
                Else = @else,
                Of = of,
                Value = value,
                WhenClauses = whenClauses,
            };
        }

        public static WhenClauseStatement ParseWhenClauseStatement(Parser parser)
        {
            parser.Eat(Keywords.When);

#if !ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(TokenType.LParen);
#endif

            var value = parser.Parse<Expression>();

#if !ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(TokenType.RParen);
#endif

#if ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(Symbols.ThenDo);
#endif

            var body = parser.Parse<Statement>();

            return new()
            {
                Value = value,
                Body = body,
            };
        }
    }
}
