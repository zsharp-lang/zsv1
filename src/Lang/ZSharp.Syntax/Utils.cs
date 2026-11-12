using ZSharp.Parser;
using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

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
    }
}
