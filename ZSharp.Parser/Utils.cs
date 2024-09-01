using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public delegate Expression TerminalFunction(Token token);

    public static class Utils
    {
        public static Parser<Statement> DefinitionStatement<T>(Parser<T> defParser)
            where T : Expression
            => new FunctionalParser<Statement>(
                parser => new ExpressionStatement() { Expression = defParser.Parse(parser) }
            );

        public static Parser<Statement> DefinitionStatement<T>(ParserFunction<T> defParser)
            where T : Expression
            => new FunctionalParser<Statement>(
                parser => new ExpressionStatement() { Expression = defParser(parser) }
            );

        //public static ParserFunction<Statement> ExpressionStatement(Func<Parser, Expression> fn)
        //    => parser =>
        //    {
        //        var expression = fn(parser);
        //        parser.Eat(TokenType.Semicolon);
        //        return new ExpressionStatement(expression);
        //    };

        public static LedFunction InfixL(string @operator, int bindingPower)
            => (parser, left) =>
            {
                parser.Eat(@operator);

                var right =
                (parser.GetParserFor<Expression>() as ExpressionParser ?? throw new ParseError())
                .ParseExpression(parser, bindingPower);

                return new BinaryExpression()
                {
                    Left = left,
                    Right = right,
                    Operator = @operator
                };
            };

        public static LedFunction InfixR(string @operator, int bindingPower)
            => InfixL(@operator, bindingPower - 1);
    }
}
