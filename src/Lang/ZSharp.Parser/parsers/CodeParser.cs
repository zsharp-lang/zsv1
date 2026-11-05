using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class CodeParser
        : ContextParser<Statement, Statement>
    {
        public Parser<Statement> DefaultContextItemParser { get; set; } 
            = new FunctionalParser<Statement>(LangParser.ParseExpressionStatement);

        public override Statement Parse(Parser parser)
        {
            if (parser.Is(Text.TokenType.LCurly))
                return LangParser.ParseBlockStatement(parser);

            return ParseContextItem(parser);
        }

        protected override Statement ParseDefaultContextItem(Parser parser)
            => DefaultContextItemParser.Parse(parser);
    }
}
