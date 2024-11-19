using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class ClassParser
        : ContextParser<OOPDefinition, Statement>
    {
        public MethodParser Method { get; } = new();

        public ClassParser()
        {
            AddKeywordParser(
                LangParser.Keywords.Let, 
                Utils.ExpressionStatement(LangParser.ParseLetExpression)
            );
            AddKeywordParser(
                LangParser.Keywords.Function,
                Utils.DefinitionStatement(Method.Parse)
            );
        }

        public override OOPDefinition Parse(Parser parser)
        {
            var type = parser.Eat(LangParser.Keywords.Class).Value;

            string? name = null;
            if (parser.Is(TokenType.Identifier))
                name = parser.Eat(TokenType.Identifier).Value;

            List<Expression>? bases = null;
            if (parser.Is(TokenType.Colon, eat: true))
            {
                bases = [];

                do
                {
                    bases.Add(parser.Parse<Expression>());
                } while (parser.Is(TokenType.Comma, eat: true));
            }

            BlockStatement? body = null;
            if (parser.Is(TokenType.LCurly))
                body = ParseClassBody(parser);
            else
                parser.Eat(TokenType.Semicolon); 

            return new()
            {
                Type = type,
                Bases = bases,
                Content = body,
                Name = name,
            };
        }

        protected override Statement ParseDefaultContextItem(Parser parser)
            => LangParser.ParseExpressionStatement(parser);

        private BlockStatement ParseClassBody(Parser parser)
        {
            List<Statement> body = [];

            parser.Eat(TokenType.LCurly);

            while (!parser.Is(TokenType.RCurly, eat: true))
                body.Add(ParseContextItem(parser));

            return new BlockStatement()
            {
                Statements = body
            };
        }
    }
}
