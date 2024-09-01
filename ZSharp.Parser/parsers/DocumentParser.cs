using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class DocumentParser
        : ContextParser<Document, Statement>
    {
        public override Document Parse(Parser parser)
        {
            List<Statement> content = [];

            while (parser.HasTokens && !parser.Is(Text.TokenType.EOF))
                content.Add(ParseContextItem(parser));

            return new()
            {
                Statements = content
            };
        }

        protected override Statement ParseDefaultContextItem(Parser parser)
            => LangParser.ParseExpressionStatement(parser);
    }
}
