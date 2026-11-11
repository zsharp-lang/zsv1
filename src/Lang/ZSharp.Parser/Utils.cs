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
                parser => new DefinitionStatement() { Definition = defParser.Parse(parser) }
            );

        public static Parser<Statement> DefinitionStatement<T>(ParserFunction<T> defParser)
            where T : Expression
            => new FunctionalParser<Statement>(
                parser => new DefinitionStatement() { Definition = defParser(parser) }
            );

        public static ParserFunction<Statement> ExpressionStatement(Func<Parser, Expression> fn, bool semicolon = true)
            => parser =>
            {
                var expression = fn(parser);
                if (semicolon)
                    parser.Eat(TokenType.Semicolon);
                return new ExpressionStatement
                {
                    Expression = expression
                };
            };

        /// <summary>
        /// An operator written before its operand, which binds it as tightly as the power
        /// given: what follows it, up to an operator that binds less tightly.
        /// </summary>
        public static NudFunction Prefix(string @operator, int bindingPower)
            => parser =>
            {
                parser.Eat(@operator);

                var operand =
                (parser.GetParserFor<Expression>() as ExpressionParser ?? throw new ParseError())
                .ParseExpression(parser, bindingPower);

                return new UnaryExpression()
                {
                    Operator = @operator,
                    Operand = operand
                };
            };

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
