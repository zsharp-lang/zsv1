using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class ConstructorParser
        : ContextParser<Constructor, Statement>
    {
        public ConstructorParser()
        {
            //AddKeywordParser(
            //    LangParser.Keywords.Return,
            //    LangParser.ParseReturnStatement
            //); // NOT YET
            AddKeywordParser(
                LangParser.Keywords.Let,
                Utils.ExpressionStatement(LangParser.ParseLetExpression)
            );
        }

        public override Constructor Parse(Parser parser)
        {
            var newKeyword = parser.Eat(LangParser.Keywords.New);

            Token? name = null;
            if (parser.Is(TokenType.Identifier))
                name = parser.Eat(TokenType.Identifier);

            parser.Eat(TokenType.LParen);

            var signature = LangParser.ParseSignature(parser);

            parser.Eat(TokenType.RParen);

            // TODO: add support for modifiers
            //var modifiers = ParseModifiers(parser); 

            List<Expression>? initializers = null;
            //if (parser.Is(TokenType.Colon, eat: true))
            //    returnType = parser.Parse<Expression>();

            var body = ParseConstructorBody(parser);

            return new(new()
            {
                Name = name,
                NewKeyword = newKeyword,
            })
            {
                Body = body,
                Name = name?.Value ?? string.Empty,
                Initializers = initializers,
                Signature = signature,
            };
        }

        protected override Statement ParseDefaultContextItem(Parser parser)
            => LangParser.ParseExpressionStatement(parser);

        private Statement ParseConstructorBody(Parser parser)
        {
            if (parser.Is(LangParser.Symbols.ThenDo, eat: true))
                return ParseContextItem(parser);

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
