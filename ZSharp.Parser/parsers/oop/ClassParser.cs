using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class ClassParser
        : ContextParser<OOPDefinition, Statement>
    {
        public MethodParser Method { get; } = new();

        public ConstructorParser Constructor { get; } = new();

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
            AddKeywordParser(
                LangParser.Keywords.New,
                Utils.DefinitionStatement(Constructor.Parse)
            );
        }

        public override OOPDefinition Parse(Parser parser)
        {
            var type = parser.Eat(LangParser.Keywords.Class);

            Token? name = null;
            if (parser.Is(TokenType.Identifier))
                name = parser.Eat(TokenType.Identifier);

            Signature? signature = null;
            if (parser.Is(TokenType.LParen, eat: true))
            {
                signature = LangParser.ParseSignature(parser);

                parser.Eat(TokenType.RParen);
            }

            Expression? of = null;
            if (parser.Is(LangParser.Keywords.Of, out var ofKeyword, eat: true))
                of = parser.Parse<Expression>();

            List<Expression>? bases = null;
            if (parser.Is(TokenType.Colon, out var basesSeparator, eat: true))
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

            return new(new()
            {
                BasesSeparator = basesSeparator,
                Name = name,
                OfKeyword = ofKeyword,
                Type = type,
            })
            {
                Type = type.Value,
                Signature = signature,
                Of = of,
                Bases = bases,
                Content = body,
                Name = name?.Value ?? string.Empty,
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
