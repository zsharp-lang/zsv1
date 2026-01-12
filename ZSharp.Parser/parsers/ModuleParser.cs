using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class ModuleParser
        : ContextParser<Module, Statement>
    {
        public override Module Parse(Parser parser)
        {
            var moduleKeyword = parser.Eat(LangParser.Keywords.Module);

            var name = parser.Eat(TokenType.Identifier);

            List<Statement> body;
            if (parser.Is(TokenType.LCurly, eat: true))
                body = ParseModuleBody(parser, TokenType.RCurly);
            else
            {
                parser.Eat(TokenType.Semicolon);
                body = ParseModuleBody(parser, TokenType.EOF);
            }

            return new(new()
            {
                ModuleKeyword = moduleKeyword,
                Name = name,
            })
            {
                Body = body,
                Name = name.Value,
            };
        }

        protected override Statement ParseDefaultContextItem(Parser parser)
            => LangParser.ParseExpressionStatement(parser);

        private List<Statement> ParseModuleBody(Parser parser, TokenType endToken)
        {
            var statements = new List<Statement>();

            while (!parser.Is(endToken, eat: true))
                statements.Add(ParseContextItem(parser));

            return statements;
        }
    }
}
