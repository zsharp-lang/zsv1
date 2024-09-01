using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class FunctionParser
        : ContextParser<Function, Statement>
    {
        public FunctionParser()
        {
            AddKeywordParser(
                LangParser.Keywords.Return,
                LangParser.ParseReturnStatement
            );
        }

        public override Function Parse(Parser parser)
        {
            var funKeyword = parser.Eat(LangParser.Keywords.Function);

            string? name = null;
            if (parser.Is(TokenType.Identifier))
                name = parser.Eat(TokenType.Identifier).Value;

            parser.Eat(TokenType.LParen);

            var signature = LangParser.ParseSignature(parser);

            parser.Eat(TokenType.RParen);

            // TODO: add support for modifiers
            //var modifiers = ParseModifiers(parser); 

            Expression? returnType = null;
            if (parser.Is(TokenType.Colon, eat: true))
                returnType = parser.Parse<Expression>();

            var body = ParseFunctionBody(parser);

            return new(new()
            {
                FunKeyword = funKeyword,
            })
            {
                Body = body,
                Name = name,
                ReturnType = returnType,
                Signature = signature,
            };
        }

        protected override Statement ParseDefaultContextItem(Parser parser)
            => LangParser.ParseExpressionStatement(parser);

        private Statement ParseFunctionBody(Parser parser)
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
